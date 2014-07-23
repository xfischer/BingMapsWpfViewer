using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BingMapsWPFViewer.Model;
using BingMapsWPFViewer.Framework;
using BingMapsWPFViewer.Framework.IOC;
using BingMapsWPFViewer.Tools.Geometry.SpatialReference;
using BingMapsWPFViewer.Tools;

namespace BingMapsWPFViewer.ViewModel
{
	public class ShapeFileLayerViewModel : LayerBaseViewModel
	{
		#region Commands / Services

		private IDialogService _windowsService;
		public ProxyCommand<ShapeFileLayerViewModel> OpenShapefileCommand { get; set; }
		public ProxyCommand<ShapeFileLayerViewModel> ShowCoordSysCommand { get; set; }

		protected override void InitCommands()
		{
			OpenShapefileCommand = new ProxyCommand<ShapeFileLayerViewModel>((_) =>
			{
				this.ShapeFilePath = _windowsService.OpenFileDialog("Select Shapefile", "Shapefiles (*.shp)|*.shp");
			});

			ShowCoordSysCommand = new ProxyCommand<ShapeFileLayerViewModel>((_) =>
				{
					_windowsService.DisplayInformation("Coordinate System", ShapefileHelper.GetProjection(this.Layer.ShapeFileName));
				});
		}

		protected override void InitServices()
		{
			_windowsService = ServiceLocator.Instance.Retrieve<IDialogService>();
		}

		#endregion

		public string ShapeFilePath
		{
			get { return this.Layer.ShapeFileName; }
			set
			{
				this.Layer.ShapeFileName = value;
				RaisePropertyChanged<string>(() => ShapeFilePath);

				RaisePropertyChanged<string>(() => CoordSysInfo);
			}
		}

		public string CoordSysInfo
		{
			get
			{
				this.IsCoordSysValid = false;

				if (this.Layer == null || this.Layer.ShapeFileName == null)
					return null;

				try
				{
					string proj = ShapefileHelper.GetProjection(this.Layer.ShapeFileName);

					this.IsCoordSysValid = ShapefileHelper.IsValidCoordSys(proj);
					if (this.IsCoordSysValid)
						return "OK"; //. (" + proj + ")";
					else
						return "Coordinate system not recognized or invalid";
				}
				catch (Exception ex)
				{
					return "Error: " + ex.Message;
				}
			}
		}

		private bool _isCoordSysValid;
		public bool IsCoordSysValid
		{
			get
			{
				return _isCoordSysValid;
			}
			set
			{
				_isCoordSysValid = value;
				this.RaisePropertyChanged<bool>(() => IsCoordSysValid);
			}
		}

		public ShapeFileLayer Layer
		{
			get { return (ShapeFileLayer)base.LayerModel; }
			set { base.LayerModel = value; }
		}

		public ShapeFileLayerViewModel()
			: base(new ShapeFileLayer(null, 0))
		{
		}

		public ShapeFileLayerViewModel(ShapeFileLayer layer)
			: base(layer)
		{
		}
	}
}
