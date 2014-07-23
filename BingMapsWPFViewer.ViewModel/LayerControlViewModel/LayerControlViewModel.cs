using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using BingMapsWPFViewer.Framework;
using BingMapsWPFViewer.Framework.IOC;
using BingMapsWPFViewer.Model.Services;

namespace BingMapsWPFViewer.ViewModel
{
	/// <summary>
	/// ViewModel 
	/// </summary>
	public class LayerControlViewModel : ListViewModelBase<LayerBaseViewModel>
	{
		private IDialogService _windowServices;
		private IMessenger _messenger;

		public LayerControlViewModel(ObservableCollection<LayerBaseViewModel> layersViewModel)
			: base()
		{
			this.Items = layersViewModel;

			_messenger.Register<Guid>(Events.LAYER_LOADING, (layerId) =>
			{
				 LayerBaseViewModel vm = this.Items.Where(i => i.LayerModel.Id == layerId).FirstOrDefault();
				 if (vm == null)
					 return;

				 vm.IsLoading = true;
				
			});

			_messenger.Register<Guid>(Events.LAYER_LOADED, (layerId) =>
			{
				LayerBaseViewModel vm = this.Items.Where(i => i.LayerModel.Id == layerId).FirstOrDefault();
				if (vm == null)
					return;

				vm.IsLoading = false;

			});
		}

		public ICollectionView ZIndexView
		{
			get
			{
				ICollectionView view = CollectionViewSource.GetDefaultView(Items);
				view.SortDescriptions.Add(new SortDescription("ZIndex", ListSortDirection.Descending));
				return view;
			}
		}

		public new ObservableCollection<LayerBaseViewModel> Items
		{
			get
			{
				return base.Items;
			}
			set
			{
				base.Items = value;
			}
		}

		#region Init commands / services
		protected override void InitServices()
		{
			_windowServices = ServiceLocator.Instance.Retrieve<IDialogService>();
			_messenger = ServiceLocator.Instance.Retrieve<IMessenger>();
		}

		protected override void InitCommands()
		{
			NewLayerCommand = new ProxyCommand<LayerControlViewModel>(param =>
			{
				EditLayerViewModel viewModel = new EditLayerViewModel("Close", "Create layer", true);
				_windowServices.OpenDialogWindow<EditLayerViewModel>("Create new layer", viewModel, param as Type, (layerVM) =>
				{
					this.EnsureNoZIndexOverlap(viewModel.CurrentLayer);

					this.Items.Add(viewModel.CurrentLayer);

					// Notify other view models
					_messenger.NotifyColleagues(Events.LAYER_ADDED, viewModel.CurrentLayer);

					return true;
				}
					, null);
			});

			EditLayerCommand = new ProxyCommand<LayerControlViewModel>(param =>
			{
				if (this.CurrentItem == null)
					return;

				EditLayerViewModel viewModel = new EditLayerViewModel("Cancel", "Save layer");
				viewModel.CurrentLayer = this.CurrentItem;
				this.CurrentItem.BeginEdit();
				_windowServices.OpenDialogWindow<EditLayerViewModel>("Edit layer", viewModel, param as Type
					, (layerVM) => // OK Action
						{
							//// Notify other view models
							//_messenger.NotifyColleagues(Events.LAYER_ADDED, viewModel.CurrentLayer);

							//this.Items.Where(i =>i.LayerModel.Id == viewModel.CurrentLayer.Id).Re .Add(viewModel.CurrentLayer);

							this.CurrentItem.EndEdit();
							return true;
						}
				, (layerVM) => // Cancel Action
					{
						this.CurrentItem.CancelEdit();
						return true;
					});
			});

			RemoveLayerCommand = new ProxyCommand<LayerControlViewModel>((_) =>
				{
					if (this.CurrentItem != null)
					{
						ServiceLocator.Instance.Retrieve<ILayersService>().Delete(this.CurrentItem.LayerModel);

						// Notify other view models
						_messenger.NotifyColleagues(Events.LAYER_REMOVED, this.CurrentItem);

						this.Items.Remove(this.CurrentItem);

					}
				});

			ZIndexUpCommand = new ProxyCommand<LayerControlViewModel>((_) =>
			{
				if (this.CurrentItem != null)
				{
					this.MoveLayerUp(this.CurrentItem);

					// Notify other view models
					_messenger.NotifyColleagues(Events.LAYER_ZINDEX_CHANGED, this.Items);
				}
			});


			ZIndexDownCommand = new ProxyCommand<LayerControlViewModel>((_) =>
			{
				if (this.CurrentItem != null)
				{
					this.MoveLayerDown(this.CurrentItem);

					// Notify other view models
					_messenger.NotifyColleagues(Events.LAYER_ZINDEX_CHANGED, this.Items);
				}
			});
		}


		#endregion Init commands / services

		#region ZIndex
		/// <summary>
		/// Swap Zindex between layer and layer below
		/// </summary>
		/// <param name="layerBaseViewModel"></param>
		private void MoveLayerDown(LayerBaseViewModel layerBaseViewModel)
		{
			if (layerBaseViewModel == null)
				return;

			LayerBaseViewModel layerBelow = Items
												.Where(l => l.ZIndex < layerBaseViewModel.ZIndex)
												.OrderByDescending(l => l.ZIndex)
												.FirstOrDefault();

			if (layerBelow != null)
			{
				int zindexTmp = layerBelow.ZIndex;
				layerBelow.ZIndex = layerBaseViewModel.ZIndex;
				layerBaseViewModel.ZIndex = zindexTmp;

				Items.Move(Items.IndexOf(layerBaseViewModel), Items.IndexOf(layerBelow));
				RaisePropertyChanged<ICollectionView>(() => ZIndexView);
			}

		}

		/// <summary>
		/// Swap Zindex between layer and layer above
		/// </summary>
		/// <param name="layerBaseViewModel"></param>
		private void MoveLayerUp(LayerBaseViewModel layerBaseViewModel)
		{
			if (layerBaseViewModel == null)
				return;

			LayerBaseViewModel layerAbove = Items
												.Where(l => l.ZIndex > layerBaseViewModel.ZIndex)
												.OrderBy(l => l.ZIndex)
												.FirstOrDefault();

			if (layerAbove != null)
			{
				int zindexTmp = layerAbove.ZIndex;
				layerAbove.ZIndex = layerBaseViewModel.ZIndex;
				layerBaseViewModel.ZIndex = zindexTmp;

				Items.Move(Items.IndexOf(layerBaseViewModel), Items.IndexOf(layerAbove));
				RaisePropertyChanged<ICollectionView>(() => ZIndexView);
			}
		}

		private void EnsureNoZIndexOverlap(LayerBaseViewModel viewModel)
		{
			if (this.Items.Count == 0)
				return;

			int numItemsSameZIndex = this.Items
															.Where(l => l.ZIndex == viewModel.ZIndex)
															.Count();
			// Same ZIndex found. Set ZIndex to max+1
			if (numItemsSameZIndex != 0)
				viewModel.ZIndex = 1 + this.Items
														.Max(l => l.ZIndex);


		}
		#endregion

		protected override ObservableCollection<LayerBaseViewModel> LoadItems()
		{
			return new ObservableCollection<LayerBaseViewModel>();
		}

		public ProxyCommand<LayerControlViewModel> NewLayerCommand { get; private set; }
		public ProxyCommand<LayerControlViewModel> EditLayerCommand { get; private set; }
		public ProxyCommand<LayerControlViewModel> RemoveLayerCommand { get; private set; }
		public ProxyCommand<LayerControlViewModel> ZIndexUpCommand { get; private set; }
		public ProxyCommand<LayerControlViewModel> ZIndexDownCommand { get; private set; }
	}
}
