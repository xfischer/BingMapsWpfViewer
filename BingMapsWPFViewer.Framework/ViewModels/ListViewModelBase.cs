using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;

namespace BingMapsWPFViewer.Framework
{
    public abstract class ListViewModelBase<T> : ViewModelBase
        where T : class
    {
        #region Abstract methods
        /// <summary>
        /// Load items
        /// </summary>
        /// <returns>Loaded items</returns>
        protected abstract ObservableCollection<T> LoadItems();
        /// <summary>
				/// Method call when current item changes
				/// Override when special process needed
        /// </summary>
        protected virtual void CurrentItemChanged() { }
        protected bool _reloadNecessary = true;
				#endregion // Abstract methods

				private bool _isProcessing;
        public bool IsProcessing
        {
            get { return _isProcessing; }
            set
            {
                if (_isProcessing == value)
                    return;
                _isProcessing = value;
                RaisePropertyChanged<bool>(() => IsProcessing);
            }
        }
        
        #region Items
        private ObservableCollection<T> _items = null;

        /// <summary>
        /// ViewModel item list
        /// </summary>
        public ObservableCollection<T> Items
        {
            get
            {
                //If items are not loaded, child class wil load it
							if (_items == null || _reloadNecessary)
                {
                    _items = LoadItems();
                    _reloadNecessary = false;
                }
                return _items;
            }
            set
            {
                if (_items != value)
                {
                    _items = value;
										_reloadNecessary = value == null;
                    RaisePropertyChanged<ObservableCollection<T>>(() => Items);
                }
            }
        }
        #endregion //Items property

        /// <summary>
        /// Returns default view on item list
        /// </summary>
        protected ICollectionView DefaultView
        {
            get { return CollectionViewSource.GetDefaultView(Items); }
        }

        #region CurrentItem
        /// <summary>
        /// Current ViewModel in collection
        /// </summary>
        public T CurrentItem
        {
            get { return (Items != null) ? DefaultView.CurrentItem as T : null; }
            set
            {
                DefaultView.MoveCurrentTo(value);
                RaisePropertyChanged<T>(() => CurrentItem);
                CurrentItemChanged();

            }
        }
				#endregion //CurrentItem


				#region SelectedItems
				private ObservableCollection<T> _selectedItems = new ObservableCollection<T>();

        public ObservableCollection<T> SelectedItems
        {
            get { return _selectedItems; }
            set
            {
                if (_selectedItems != value)
                {
                    _selectedItems = value;
                    RaisePropertyChanged<ObservableCollection<T>>(() => SelectedItems);
                }
            }
        }
        #endregion //SelectedItems property

        public void Refresh()
        {
            _reloadNecessary = true;
            RaisePropertyChanged<ObservableCollection<T>>(() => Items);
        }
    }
}
