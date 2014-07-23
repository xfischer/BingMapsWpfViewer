using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Data;
using BingMapsWPFViewer.Framework;
using BingMapsWPFViewer.Model;

namespace BingMapsWPFViewer.ViewModel
{
	public class LayerTypesViewModel : ViewModelBase
	{
		private List<LayerType> _allLayerTypes;
		public List<LayerType> AllLayerTypes
		{
			get { return _allLayerTypes; }
		}

		public LayerTypesViewModel()
		{
			_allLayerTypes = new List<LayerType>();
			foreach (var en in Enum.GetValues(typeof(enLayerType)))
			{
				Type vmType = ViewModelTypeLocator.Retrieve(en.ToString());
				LayerBaseViewModel vmBase = Activator.CreateInstance(vmType) as LayerBaseViewModel;
				if (vmBase != null)
				{
					//_allLayerTypes.Add(new LayerType(en.ToString(), vmBase.IconName));
					///BingMapsWPFViewer.Main;component/Images/add.png
					_allLayerTypes.Add(new LayerType(en.ToString(), vmBase.IconName, (enLayerType)en));
				}				
			}

		}

		public ICollectionView IconView
		{
			get
			{
				ICollectionView view = CollectionViewSource.GetDefaultView(AllLayerTypes);
				view.SortDescriptions.Add(new SortDescription("IconName", ListSortDirection.Ascending));
				return view;
			}
		}


	}
}
