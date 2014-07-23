using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BingMapsWPFViewer.Model;
using BingMapsWPFViewer.Framework;
using BingMapsWPFViewer.Model.Services;
using BingMapsWPFViewer.Framework.IOC;
using System.ComponentModel;
using System.Windows.Media.Imaging;

namespace BingMapsWPFViewer.ViewModel
{
	public class LayerBaseViewModel : ValidationViewModelBase, IEditableObject
	{
		private ILayersService _layerModelService;

		public ProxyCommand<LayerBaseViewModel> SaveCommand { get; private set; }

		protected override void InitCommands()
		{
			SaveCommand = new ProxyCommand<LayerBaseViewModel>((param) => this.SaveLayer(param as LayerBase));
		}

		private void SaveLayer(LayerBase layerBase)
		{
			_layerModelService.Create(layerBase);
		}

		public virtual string IconName
		{
			get { return "file.png"; }
		}

		private BitmapImage _iconImage;
		public BitmapImage IconImage
		{
			get
			{
				if (_iconImage == null && !string.IsNullOrWhiteSpace(IconName))
				{
					_iconImage = new BitmapImage();
					_iconImage.BeginInit();
					_iconImage.UriSource = new Uri(@"pack://application:,,,/BingMapsWPFViewer.ViewModel;component/Icons/" + IconName, UriKind.Absolute);
					_iconImage.CacheOption = BitmapCacheOption.OnLoad;
					_iconImage.EndInit();
				}

				return _iconImage;
			}
		}


		protected override void InitServices()
		{
			_layerModelService = ServiceLocator.Instance.Retrieve<ILayersService>();
		}

		internal LayerBase LayerModel { get; set; }

		internal LayerBaseViewModel(LayerBase p_LayerBase)
		{
			LayerModel = p_LayerBase;
		}

		public string DisplayName
		{
			get { return LayerModel.DisplayName; }
			set
			{
				if (LayerModel.DisplayName != value)
				{
					LayerModel.DisplayName = value;
					RaisePropertyChanged<string>(() => DisplayName);
				}
			}
		}

		public int ZIndex
		{
			get { return LayerModel.ZIndex; }
			set
			{
				if (LayerModel.ZIndex != value)
				{
					LayerModel.ZIndex = value;
					RaisePropertyChanged<int>(() => ZIndex);
				}
			}

		}

		public enLayerType LayerType
		{
			get { return LayerModel.LayerType; }
		}

		private bool _isLoading = false;
		public bool IsLoading
		{
			get { return _isLoading; }
			set
			{
				_isLoading = value;
				RaisePropertyChanged<string>(() => LoadingText);
			}
		}
		public string LoadingText
		{
			get { return _isLoading ? "Loading..." : null; }
		}

		private bool _IsEnabled = true;
		public bool IsEnabled
		{
			get { return _IsEnabled; }
			set { _IsEnabled = value; }
		}

		#region IEditableObject Membres

		public virtual void BeginEdit()
		{
			this.LayerModel.BeginEdit();
			this.LayerModel.IsReallyEditing = true; // Allows model objects without INotifyPropertyChanged implementation
		}

		public virtual void CancelEdit()
		{
			this.LayerModel.CancelEdit();
			RaiseEntirePropertyChanged();
		}

		public virtual void EndEdit()
		{
			this.LayerModel.EndEdit();
		}

		#endregion
	}
}
