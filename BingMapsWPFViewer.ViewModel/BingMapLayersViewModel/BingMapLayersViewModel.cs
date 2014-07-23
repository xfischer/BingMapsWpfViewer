using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using BingMapsWPFViewer.Framework;
using BingMapsWPFViewer.Framework.IOC;
using BingMapsWPFViewer.Model;
using BingMapsWPFViewer.Model.Features;
using BingMapsWPFViewer.Model.Services;
using BingMapsWPFViewer.Tools;
using Microsoft.Maps.MapControl.WPF;
using Microsoft.SqlServer.Types;
using BingMapsWPFViewer.ViewModel.Properties;

namespace BingMapsWPFViewer.ViewModel
{
	/// <summary>
	/// ViewModel responsible for BingMaps stateless interaction and management
	/// Inherits from a list of layer view model and reflects all changes
	/// to Map
	/// </summary>
	public class BingMapLayersViewModel : ListViewModelBase<LayerBaseViewModel>
	{

		#region Members

		private Map _map;

		private IMessenger _messenger;
		private IFeatureServiceBase<LayerBase, SpatialQueryCriterion> _spatialFeatureService;


		/// <summary>
		/// Returns all useful information about current viewport
		/// </summary>
		public MapViewport Viewport
		{
			get
			{

				if (_map == null)
					return null;

				BoundingBox bbox = new BoundingBox(_map.BoundingRectangle.West, _map.BoundingRectangle.South, _map.BoundingRectangle.East, _map.BoundingRectangle.North);
				string bboxWKT = bbox.ToSqlGeometry().ToString();
				return new MapViewport(bbox, _map.ViewportSize.Width, _map.ViewportSize.Height, _map.ZoomLevel);
			}
		}

		/// <summary>
		/// Map resolution is useful for geometry reduction algorithms
		/// gives the units/pixel value
		/// </summary>
		public double MapResolution
		{
			get { return _map.BoundingRectangle.Height / _map.ViewportSize.Height; }
		}

		/// <summary>
		/// Map Zoom level (1 to 23)
		/// </summary>
		public double ZoomLevel
		{
			get { return _map.ZoomLevel; }
			set
			{
				if (_map.ZoomLevel != value && _map.TargetZoomLevel != value)
				{
					_map.ZoomLevel = value;
					RaisePropertyChanged<double>(() => ZoomLevel);
				}
			}
		}

		private Feature _selectedFeature;
		public Feature SelectedFeature 
		{
			get { return _selectedFeature; }
			set
			{
				_selectedFeature = value;
				RaisePropertyChanged<Feature>(() => SelectedFeature);
			}
		}

		#endregion

		protected override void InitServices()
		{
			_messenger = ServiceLocator.Instance.Retrieve<IMessenger>();
			_spatialFeatureService = ServiceLocator.Instance.Retrieve<IFeatureServiceBase<LayerBase, SpatialQueryCriterion>>();
		}

		public ProxyCommand<BingMapLayersViewModel> CancelDataQueryCommand { get; private set; }
		

		protected override void InitCommands()
		{
			CancelDataQueryCommand = new ProxyCommand<BingMapLayersViewModel>((_) =>
				{
					if (!_dataQueryCancelled)
					{
						DebugHelper.WriteLine(this, "Canceling");
						this.CancelDataQuery();
					}
				});
		
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="mapInstance">Map control instance</param>
		/// <param name="layersVM">Collection of layers</param>
		public BingMapLayersViewModel(Map mapInstance, ObservableCollection<LayerBaseViewModel> layersVM)
			: base()
		{
			_map = mapInstance;
			this.InitMap();

			this.Items = layersVM;
			this.AddLayers(layersVM);


			_messenger.Register<LayerBaseViewModel>(Events.LAYER_ADDED, (layer) =>
			{
				this.AddLayer(layer);

			});

			_messenger.Register<LayerBaseViewModel>(Events.LAYER_REMOVED, (layer) =>
			{
				this.RemoveLayer(layer);
			});

			_messenger.Register<IEnumerable<LayerBaseViewModel>>(Events.LAYER_ZINDEX_CHANGED, (layers) =>
			{
				foreach (var layer in layers)
				{
					UIElement uie = this.UIElementFromVM(layer);
					uie.SetValue(Canvas.ZIndexProperty, layer.LayerModel.ZIndex);

				}
			});

		}

		#region Layer management

		/// <summary>
		/// Add given layers to the Map
		/// Create Bing Map specific layers
		/// </summary>
		/// <param name="layers"></param>
		private void AddLayers(IEnumerable<LayerBaseViewModel> layers)
		{
			if (layers == null) return;

			foreach (LayerBaseViewModel layerVM in layers)
			{
				this.AddLayer(layerVM);
			}
		}

		private void AddLayer(LayerBaseViewModel layer)
		{
			if (layer == null) return;


			UIElement newLayer = null;
			if (layer.LayerModel.IsVectorLayer)
			{
				newLayer = new MapVectorLayer((VectorLayerBase)layer.LayerModel);
				_map.Children.Add(newLayer);
				RefreshData(newLayer);
			}
			else
			{
				newLayer = new MapTileLayerXYZ((TileLayerBase)layer.LayerModel);
				_map.Children.Add(newLayer);
			}


			// Set ZIndex
			if (newLayer != null)
				Canvas.SetZIndex(newLayer, layer.ZIndex);
		}

		/// <summary>
		/// Remove layer from Map
		/// </summary>
		/// <param name="layers"></param>
		private void RemoveLayers(IEnumerable<LayerBaseViewModel> layers)
		{
			if (layers == null) return;

			// Retrieve UI element from view model via IBingMapsWPFViewerLayer interface
			List<UIElement> layersInMapToDelete = this.UIElementFromVM(layers);

			foreach (UIElement layer2Delete in layersInMapToDelete)
				_map.Children.Remove(layer2Delete);

		}

		private void RemoveLayer(LayerBaseViewModel layer)
		{
			if (layer == null) return;

			// Retrieve UI element from view model via IBingMapsWPFViewerLayer interface
			UIElement layerInMapToDelete = this.UIElementFromVM(layer);
			if (layerInMapToDelete != null)
				_map.Children.Remove(layerInMapToDelete);

		}

		#region Layer retrieval

		/// <summary>
		/// Given a list of layer view model, retrieve corresponding Bing Map layers
		/// </summary>
		/// <param name="layers">List of layer view model</param>
		/// <returns>List of UI layers</returns>
		/// <remarks>All Bing Map custom layers must implement IBingMapsWPFViewerLayer for easy
		/// retrieval</remarks>
		private List<UIElement> UIElementFromVM(IEnumerable<LayerBaseViewModel> layers)
		{
			return (from layerInMap in _map.Children.OfType<IBingMapsWPFViewerLayer>()
							join inputLayer in layers
							on layerInMap.Id equals inputLayer.LayerModel.Id
							select (UIElement)layerInMap).ToList();
		}

		/// <summary>
		/// Given a layer view model, retrieve corresponding Bing Map layer
		/// </summary>
		/// <param name="layer">Layer view model</param>
		/// <returns>UI layer</returns>
		/// <remarks>All Bing Map custom layers must implement IBingMapsWPFViewerLayer for easy
		/// retrieval</remarks>
		private UIElement UIElementFromVM(LayerBaseViewModel layer)
		{
			return (from layerInMap in _map.Children.OfType<IBingMapsWPFViewerLayer>()
							where layerInMap.Id == layer.LayerModel.Id
							select (UIElement)layerInMap).FirstOrDefault();
		}

		private LayerBaseViewModel ViewModelFromLayerModel(LayerBase layer)
		{
			foreach (var internalLayer in this.Items)
				if (internalLayer.LayerModel == layer)
					return internalLayer;

			return null;
		}


		#endregion

		#endregion Layer management

		#region Bing Maps specific

		/// <summary>
		/// Initialize map credentials and events
		/// </summary>
		private void InitMap()
		{
			SetMapCredentials();
			_map.AnimationLevel = AnimationLevel.UserInput;
			_map.UseInertia = false;

			#region View changed

			// This handler prevents refreshing the view while the user pans and stops while panning
			// _refreshOnMouseUp flag is set when view changes and left mouse button is down
			_map.ViewChangeEnd += (o, e) =>
			{
				_panning = false;

				if (_mouseUp)
					RefreshData();
				else
					_refreshOnMouseUp = true;
			};

			_map.SizeChanged += (o, e) => RefreshData();
			_map.Loaded += (o, e) => RefreshData();

			_map.ViewChangeOnFrame += (o, e) =>
			{
				_panning = true;
				RaisePropertyChanged<double>(() => ZoomLevel);
			};

			#endregion View changed

			_map.PreviewMouseDown += (o, e) => _map.Focus(); // Sets focus to map when clicked
			_map.PreviewMouseWheel += (o, e) => { e.Handled = true; _map.ZoomLevel = Math.Floor(_map.ZoomLevel) + Math.Sign(e.Delta); };

			// Handles feature click
			_map.MouseLeftButtonDown += _map_MouseLeftButtonDown;
			_map.MouseLeftButtonUp += _map_MouseLeftButtonUp;

			// For better performance, all layers on map are hidden when view starts changing
			_map.ViewChangeStart += (o, e) =>
			{
				foreach (UIElement obj in _map.Children)
				{
					if (obj is MapLayer)
						obj.Visibility = Visibility.Collapsed;
				}
			};
		}

		public void SetMapCredentials()
		{
			string apiKey = Settings.Default.BingKey;
			_map.CredentialsProvider = new ApplicationIdCredentialsProvider(apiKey);
		}

		#region Mouse left button handlers (for click)

		private bool _mouseUp = true;
		private bool _panning = false;
		private bool _refreshOnMouseUp = false;
		void _map_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			_mouseUp = false;
		}

		void _map_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			if (_mouseUp) return; // mouse was down out of the map

			// If flag has been set because users was panning and mouse stopped
			// we refresh the view
			if (_refreshOnMouseUp)
			{
				_refreshOnMouseUp = false;
				RefreshData();
			}
			else
			{
				if (!_panning)
				{
					Location mouseLocation = null;
					if (_map.TryViewportPointToLocation(e.GetPosition(_map), out mouseLocation))
					{
						this.HandleMouseClick(mouseLocation);
					}
				}
			}

			_mouseUp = true;
		}

		private void HandleMouseClick(Location mouseLocation)
		{
			// Get features under mouse
			Dictionary<LayerBase, List<Feature>> featuresUnderMouse = this.GetFeaturesAtLocation(mouseLocation);

			// Set the top level selected feature
			if (featuresUnderMouse.Count > 0)
				this.SelectedFeature = featuresUnderMouse.Values.First().FirstOrDefault();
		}

		/// <summary>
		/// Get features list for a given location, by layer (zIndex sorted)
		/// </summary>
		/// <param name="mouseLocation"></param>
		/// <returns></returns>
		private Dictionary<LayerBase, List<Feature>> GetFeaturesAtLocation(Location mouseLocation)
		{
			Dictionary<LayerBase, List<Feature>> ret = new Dictionary<LayerBase, List<Feature>>();
			SqlGeometry geomPoint = SqlGeometry.Point(mouseLocation.Longitude, mouseLocation.Latitude, 4326);
			
			
			Point ptMouse =  _map.LocationToViewportPoint(mouseLocation);
			Location loc =  _map.ViewportPointToLocation(new Point(ptMouse.X + 2, ptMouse.Y));
			double buffer = Math.Abs(loc.Longitude - mouseLocation.Longitude);
			geomPoint = geomPoint.STBuffer(buffer);
			

			foreach (MapVectorLayer vectorLayer in _map.Children
																							.OfType<MapVectorLayer>()
																							.OrderByDescending(l=> l.VectorLayerBase.ZIndex))
			{
				if (vectorLayer.VectorLayerBase != null
					&& vectorLayer.VectorLayerBase.Features != null)
				{
					List<Feature> intersectingFeatures = vectorLayer.VectorLayerBase.Features
																							.Where(f=> f.Geometry.STIntersects(geomPoint).Value)
																							.ToList();

					if (intersectingFeatures.Count > 0)
						ret.Add(vectorLayer.VectorLayerBase, intersectingFeatures);

				}
			}

			return ret;
		}

		#endregion Mouse left button handlers (for click)

		#endregion Bing Maps specific

		#region Load data

		/// <summary>
		/// Set to true to cancel data query and data fetching/conversion
		/// </summary>
		private bool _dataQueryCancelled = false;

		/// <summary>
		/// Refreshes data for all databound layers
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void RefreshData()
		{
			if (!_map.IsLoaded)
				return;

			if (_map.BoundingRectangle.Width == 0 || _map.BoundingRectangle.Height == 0)
				return;

			this.RaisePropertyChanged<MapViewport>(() => Viewport);

			// Cancel any pending query
			this.CancelDataQuery();
			_dataQueryCancelled = false;

			foreach (UIElement element in _map.Children)
			{
				RefreshData(element);
				element.Visibility = Visibility.Visible;
			}


		}

		/// <summary>
		/// Refreshes data for a specific databound layer
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void RefreshData(UIElement layer)
		{
			if (!_map.IsLoaded)
				return;

			if (layer is MapVectorLayer )
			{

				MapVectorLayer vectorLayer = (MapVectorLayer)layer;
				VectorLayerBase layerModel = ((MapVectorLayer)layer).VectorLayerBase;


				LayerBaseViewModel baseVM = this.ViewModelFromLayerModel(layerModel);
				if (baseVM != null && !baseVM.IsEnabled)
				{
					layerModel.Features = new List<Feature>();
					vectorLayer.RefreshView();
					return;
				}

				Exception loadException = null;
				try
				{
					double yResolution = _map.BoundingRectangle.Height / _map.ViewportSize.Height;
					SpatialQueryCriterion criterion = new SpatialQueryCriterion(this.Viewport, layerModel.Attributes, new List<FeatureQueryFilter>() { layerModel.QueryFilter }, 5000);

					Stopwatch stopwatch = Stopwatch.StartNew();
					
					// Notify loading in progress
					_messenger.NotifyColleagues(Events.LAYER_LOADING, layerModel.Id);

					_spatialFeatureService.LoadAsync(layerModel, criterion, (response, results) =>
					{
						stopwatch.Stop();
						DebugHelper.WriteLine(this, string.Format("{1} ({0}) : query done in {2} ms", layerModel.DisplayName, layerModel.GetType().Name, stopwatch.ElapsedMilliseconds));

						if (response.HasError)
						{
							layerModel.Features = new List<Feature>();
							vectorLayer.RefreshView();
							this.ShowError(vectorLayer, response.ErrorMessage);
						}
						else
						{
							layerModel.Features = results;
							vectorLayer.RefreshView();
						}

						// Notify loading in progress
						_messenger.NotifyColleagues(Events.LAYER_LOADED, layerModel.Id);
					});

				}
				catch (Exception e)
				{
					loadException = e;
					layerModel.Features = new List<Feature>();
					vectorLayer.RefreshView();
				}
				finally
				{
					//vectorLayer.RefreshView();
					if (loadException != null)
						this.ShowError(vectorLayer, loadException.ToString());
				}


			}

		}

		private void CancelDataQuery()
		{
			_dataQueryCancelled = true;
			_spatialFeatureService.Cancel();
			foreach (UIElement element in _map.Children)
			{
				CancelDataQuery(element);
				element.Visibility = Visibility.Visible;
			}
		}

		private void CancelDataQuery(UIElement layer)
		{
			if (!_map.IsLoaded)
				return;

			if (layer is MapVectorLayer)
			{
				MapVectorLayer vectorLayer = (MapVectorLayer)layer;
				vectorLayer.CancelRefresh();
			}

		}

		#endregion

		protected override ObservableCollection<LayerBaseViewModel> LoadItems()
		{
			return new ObservableCollection<LayerBaseViewModel>();
		}

		#region UI Error message

		private void ShowError(MapVectorLayer vectorLayer, Exception ex)
		{
			this.ShowError(vectorLayer, ex.ToString());
		}

		private void ShowError(MapVectorLayer vectorLayer, string errorMessage)
		{
			TextBlock txtBlock = new TextBlock();
			//txtBlock.Width = 400;
			txtBlock.HorizontalAlignment = HorizontalAlignment.Stretch;
			txtBlock.TextWrapping = TextWrapping.Wrap;
			txtBlock.Text = errorMessage;
			txtBlock.FontWeight = FontWeights.Bold;
			txtBlock.Foreground = new SolidColorBrush(Colors.Red);
			txtBlock.Background = new SolidColorBrush(Color.FromArgb(128, 255, 255, 255));

			vectorLayer.AddChild(txtBlock, _map.BoundingRectangle.Center, PositionOrigin.Center);
		}

		#endregion UI Error message


		internal void ZoomIn()
		{
			_map.ZoomLevel = Math.Floor(_map.ZoomLevel) + 1;
		}

		internal void ZoomOut()
		{
			_map.ZoomLevel = Math.Floor(_map.ZoomLevel) - 1;
		}
	}
}
