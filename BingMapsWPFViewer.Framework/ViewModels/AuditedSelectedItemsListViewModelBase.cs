using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace BingMapsWPFViewer.Framework.ViewModels
{
    public abstract class AuditedSelectedItemsListViewModelBase<T> 
        : ListViewModelBase<T> where T:class
       
    {

        /// <summary>
        /// Surcharge de la propriété correspondant aux éléments sélectionnés.
        /// </summary>
        public new ObservableCollection<T> SelectedItems
        {
            get { return base.SelectedItems; }
            set
            {
                //Action avant le changement de valeur
                base.SelectedItems = value;
                //Action après le changement de valeur
            }
        }

    }
}
