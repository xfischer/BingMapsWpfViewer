using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using BingMapsWPFViewer.Framework.IOC;
using BingMapsWPFViewer.Model.Services;
using BingMapsWPFViewer.Services.Mocks;
using BingMapsWPFViewer.Framework;
using BingMapsWPFViewer.ViewModel;
using Microsoft.Maps.MapControl.WPF;
using BingMapsWPFViewer.Views;
using BingMapsWPFViewer.Model;
using BingMapsWPFViewer.Tools.TinyHttpHost;
using BingMapsWPFViewer.Services;
using BingMapsWPFViewer.Tools;

namespace BingMapsWPFViewer.Main
{
	/// <summary>
	/// Logique d'interaction pour App.xaml
	/// </summary>
	public partial class App : Application
	{
		private static IBingMapsView _bingMapsContainer;

		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			StartupWindow wndStartup = new StartupWindow();
			wndStartup.Show();

			TinyHttpHost.Start();

			RegisterServices();
			RegisterViewModels();
			RegisterViews();
			
			_bingMapsContainer = new MainWindow();

			wndStartup.Close();
			((MainWindow)_bingMapsContainer).Show();

		}

		protected override void OnExit(ExitEventArgs e)
		{
			base.OnExit(e);

			TinyHttpHost.Stop();
		}

		private void RegisterViews()
		{
			ViewTypeLocator.RegisterWindow("NewLayerView", typeof(NewLayerView));
			ViewTypeLocator.RegisterWindow("EditLayerView", typeof(EditLayerView));
			ViewTypeLocator.RegisterWindow("MapInfoView", typeof(MapInfoView));
			ViewTypeLocator.RegisterWindow("SetupView", typeof(SetupView));
			
		}

		private static void RegisterServices()
		{
			ServiceLocator.Instance.Register<ILayersService>(new LayersServiceMock());
			ServiceLocator.Instance.Register<IDialogService>(new DialogService());
			ServiceLocator.Instance.Register<ISqlServerInfoSchemaService>(new SqlServerInfoSchemaService());
			ServiceLocator.Instance.Register<IMessenger>(new Messenger());
			//ServiceLocator.Instance.Register<INancyModule>(new TileModule());

			// Feature service
			ServiceLocator.Instance.Register<IFeatureServiceBase<LayerBase, SpatialQueryCriterion>>(new FeatureSpatialService());
		}
		private static void RegisterViewModels()
		{
			ViewModelTypeLocator.Register(enLayerType.SQLServerLayer.ToString(), typeof(SQLServerLayerViewModel));
			ViewModelTypeLocator.Register(enLayerType.ShapeFileLayer.ToString(), typeof(ShapeFileLayerViewModel));
			ViewModelTypeLocator.Register(enLayerType.TileLayerXYZ.ToString(), typeof(TileLayerXYZViewModel));
			ViewModelTypeLocator.Register(enLayerType.TileGridLayerVector.ToString(), typeof(TileGridLayerViewModel));
			ViewModelTypeLocator.Register(enLayerType.TileGridLayerImage.ToString(), typeof(TileGridLayerImageViewModel));
			//ViewModelLocator.RegisterViewModel<MainViewModel>(new MainViewModel(() => _bingMapsContainer.MapInstance));
		}
	}
}
