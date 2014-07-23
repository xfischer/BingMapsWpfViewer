using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BingMapsWPFViewer.Framework;
using BingMapsWPFViewer.Model;
using BingMapsWPFViewer.Model.Services;
using BingMapsWPFViewer.Framework.IOC;

namespace BingMapsWPFViewer.ViewModel
{
	/// <summary>
	/// ViewModel used when creating a new layer
	/// New layer can be of any type inheriting LayerBase
	/// This class exposes the correct view model for a given layer type
	/// </summary>
	public class EditLayerViewModel : ValidationViewModelBase
	{
		public ProxyCommand<EditLayerViewModel> ChangeLayerTypeCommand { get; set; }
		public ProxyCommand<EditLayerViewModel> SaveLayerCommand { get; set; }
		public ProxyCommand<EditLayerViewModel> CloseCommand { get; set; }

		public ProxyCommand<EditLayerViewModel> GenerateDisplayNameCommand { get; set; }

		private ILayersService _layerService;
		private bool _createMode = false;

		public EditLayerViewModel(string closeCaption, string saveCaption, bool createMode = false)
		{
			this.CloseCaption = closeCaption;
			this.SaveCaption = saveCaption;
			_createMode = createMode;
		}

		#region Init commands / services

		protected override void InitCommands()
		{
			ChangeLayerTypeCommand = new ProxyCommand<EditLayerViewModel>(paramtype =>
			{
				this.ActivateLayerType(((LayerType)paramtype).UnderlyingType);

			}
			, this);

			SaveLayerCommand = new ProxyCommand<EditLayerViewModel>((_) =>
																			{
																				if (_createMode)
																					this.DialogResult = _layerService.Create(this.CurrentLayer.LayerModel);
																				else
																					this.DialogResult = _layerService.Update(this.CurrentLayer.LayerModel);
																			});

			CloseCommand = new ProxyCommand<EditLayerViewModel>((_) =>
			{
				this.DialogResult = false;
			});

			GenerateDisplayNameCommand = new ProxyCommand<EditLayerViewModel>((_) =>
				{
					if (this.CurrentLayer != null && this.CurrentLayer.LayerModel != null)
					{
						string nameProp = this.CurrentLayer.LayerModel.GenerateDisplayNameProposal();
						if (nameProp != null)
							this.LayerName = nameProp;
					}
				});
		}

		protected override void InitServices()
		{
			_layerService = ServiceLocator.Instance.Retrieve<ILayersService>();
		}

		#endregion Init commands / services

		#region Properties

		private LayerBaseViewModel _currentLayer;
		public LayerBaseViewModel CurrentLayer
		{
			get { return _currentLayer; }
			set
			{
				_currentLayer = value;

				RaisePropertyChanged<LayerBaseViewModel>(() => CurrentLayer);
				RaisePropertyChanged<string>(() => LayerName);

			}
		}

		void CurrentLayer_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "DisplayName")
			{
				RaisePropertyChanged<string>(() => LayerName);
			}
		}

		private enLayerType? _layerType;
		public enLayerType? LayerType
		{
			get { return _layerType; }
			set
			{
				if (_layerType.HasValue && _layerType.Value == value)
					return;

				_layerType = value;
			}
		}

		public string LayerName
		{
			get
			{
				if (_currentLayer == null)
					return null;
				else
					return _currentLayer.DisplayName;
			}
			set
			{
				if (_currentLayer == null)
					return;
				else
				{
					_currentLayer.DisplayName = value;
					RaisePropertyChanged<string>(() => LayerName);
				}
			}

		}

		public new bool IsValid
		{
			get
			{
				if (this.CurrentLayer == null)
					return false;
				else
					return this.CurrentLayer.ValidPropertiesCount == this.CurrentLayer.TotalPropertiesWithValidationCount;
			}
		}

		private bool? _dialogResult;
		public bool? DialogResult
		{
			get
			{
				return _dialogResult;
			}
			set
			{
				_dialogResult = value;
				RaisePropertyChanged<bool?>(() => DialogResult);
			}
		}

		public string CloseCaption { get; set; }
		public string SaveCaption { get; set; }

		#endregion

		private void ActivateLayerType(enLayerType enLayerType)
		{
			this.LayerType = enLayerType;

			if (_layerType == null)
				return;

			Type vmType = ViewModelTypeLocator.Retrieve(enLayerType.ToString());
			this.CurrentLayer = Activator.CreateInstance(vmType) as LayerBaseViewModel;

			if (this.CurrentLayer == null)
				this.CurrentLayer = new LayerBaseViewModel(null);
		}
	}
}

