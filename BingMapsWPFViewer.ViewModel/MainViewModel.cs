using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Maps.MapControl.WPF;
using System.Configuration;
using BingMapsWPFViewer.Model;
using BingMapsWPFViewer.Framework;
using BingMapsWPFViewer.Framework.IOC;
using System.Collections.ObjectModel;
using System.Windows;
using System.Reflection;
using BingMapsWPFViewer.Model.Services;
using BingMapsWPFViewer.Tools;
using BingMapsWPFViewer.ViewModel.Properties;

namespace BingMapsWPFViewer.ViewModel
{
	/// <summary>
	/// MainViewModel
	/// Shell view model
	/// </summary>
	public class MainViewModel : ListViewModelBase<LayerBaseViewModel>
	{
		#region Members

		private IDialogService _windowServices;
		private ILayersService _layerModelService;

		//Mediator
		private IMessenger _messenger;

		//Commands
		public ProxyCommand<MainViewModel> QuitApplicationCommand { get; private set; }
		public ProxyCommand<MainViewModel> SetupCommand { get; private set; }
		public ProxyCommand<MainViewModel> AboutCommand { get; private set; }
		public ProxyCommand<MainViewModel> ClearCacheCommand { get; private set; }
		public ProxyCommand<MainViewModel> ViewInfoCommand { get; private set; }

		public ProxyCommand<MainViewModel> ZoomInCommand { get; private set; }
		public ProxyCommand<MainViewModel> ZoomOutCommand { get; private set; }


		#endregion

		#region Init commands / services
		protected override void InitCommands()
		{
			QuitApplicationCommand = new ProxyCommand<MainViewModel>((_) =>
				{
					//if (_windowServices.DemanderConfirmation(
					//    "You are going to close application",
					//    "Are you sure ?"))
					//{ Environment.Exit(0);}
					//Environment.Exit(0);
					Application.Current.Shutdown();
				});

			AboutCommand = new ProxyCommand<MainViewModel>((_) =>
							_windowServices.DisplayInformation("About",
							String.Format(@"BingMapsWPFViewer
                {0}{1}Xavier FISCHER",
							Environment.NewLine, Environment.NewLine))
							);

			ViewInfoCommand = new ProxyCommand<MainViewModel>(param =>
				{
					_windowServices.OpenWindow<BingMapLayersViewModel>("Map Information", this.BingMapLayersViewModel, param as Type, true, false, false, true);
					//_windowServices.DisplayInformation("View Info",
					//		this.BingMapLayersViewModel.Viewport.ToString());
				});

			ClearCacheCommand = new ProxyCommand<MainViewModel>((_) =>
				{
					if (NetHelper.TryClearCache())
						_windowServices.DisplayInformation("Clear IE cache", "Cache cleared.");
					else
						_windowServices.DisplayWarning("Clear IE cache", "Error while clearing cache.");

				});

			SetupCommand = new ProxyCommand<MainViewModel>((viewTypeName) =>
				{
					SetupViewModel setupVM = new SetupViewModel();
					_windowServices.OpenDialogWindow<SetupViewModel>("Bing Maps API Key setup", setupVM, viewTypeName as Type, (_) =>
					{
						this.RaisePropertyChanged<bool>(() => IsKeyInvalid);
						BingMapLayersViewModel.SetMapCredentials();
						return true;
					}, null);

				});

			ZoomInCommand = new ProxyCommand<MainViewModel>((_) =>
			{
				this.BingMapLayersViewModel.ZoomIn();
			});
			ZoomOutCommand = new ProxyCommand<MainViewModel>((_) =>
			{
				this.BingMapLayersViewModel.ZoomOut();
			});
		}

		protected override void InitServices()
		{
			_windowServices = ServiceLocator.Instance.Retrieve<IDialogService>();
			_layerModelService = ServiceLocator.Instance.Retrieve<ILayersService>();
			_messenger = ServiceLocator.Instance.Retrieve<IMessenger>();
		}
		#endregion

		#region Properties (DataBinding)

		public bool IsKeyInvalid
		{
			get
			{
				return !new SetupViewModel().IsAPIKeyValid;
			}
		}

		#endregion

		/// <summary>
		/// Layer control view model
		/// </summary>
		public LayerControlViewModel LayerControlViewModel { get; private set; }

		/// <summary>
		/// BingMaps view model
		/// </summary>
		public BingMapLayersViewModel BingMapLayersViewModel { get; private set; }


		public MainViewModel(Map mapInstance)
		{
			RaisePropertyChanged<bool>(() => IsKeyInvalid);


			this.LayerControlViewModel = new LayerControlViewModel(this.Items);
			this.BingMapLayersViewModel = new BingMapLayersViewModel(mapInstance, this.Items);

			this.BingMapLayersViewModel.PropertyChanged += BingMapLayersViewModel_PropertyChanged;
		}

		protected override ObservableCollection<LayerBaseViewModel> LoadItems()
		{
			// Load layers
			ObservableCollection<LayerBaseViewModel> layersVM = new ObservableCollection<LayerBaseViewModel>();
			foreach (LayerBase layer in _layerModelService.Load(CriterionLayer.Empty))
			{
				Type vmType = ViewModelTypeLocator.Retrieve(layer.LayerType.ToString());

				LayerBaseViewModel layerVM = Activator.CreateInstance(vmType) as LayerBaseViewModel;
				layerVM.LayerModel = layer;

				if (layerVM == null)
					layerVM = new LayerBaseViewModel(layer);

				layersVM.Add(layerVM);

				//if (layer is SqlServerLayer)
				//	layersVM.Add(new SQLServerLayerViewModel(layer as SqlServerLayer));
				//else if (layer is TileLayerXYZ)
				//	layersVM.Add(new TileLayerXYZViewModel(layer as TileLayerXYZ));
				//else
				//	layersVM.Add(new LayerBaseViewModel(layer));
			}

			return layersVM;

		}

		void BingMapLayersViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "ZoomLevel")
			{
				if (_statusMessages.Count == 1)
					_statusMessages.Add(string.Empty);

				_statusMessages[1] = this.BingMapLayersViewModel.ZoomLevel.ToString("0.##");
			}
		}

		private ObservableCollection<string> _statusMessages
				 = new ObservableCollection<string>() { "Application started" };
		public ObservableCollection<string> StatusMessages
		{
			get { return _statusMessages; }
			set
			{
				_statusMessages = value;
				RaisePropertyChanged<ObservableCollection<string>>(() => StatusMessages);
			}
		}

	}
}
