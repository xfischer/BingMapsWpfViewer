using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Linq.Expressions;

namespace BingMapsWPFViewer.Framework
{

    /// <summary>
    /// Base class for all ViewModels
    /// </summary>
    public class ViewModelBase : ObservedBase
    {

        public ViewModelBase()
        {
            InitCommands();
            InitServices();
        }

        protected virtual void InitServices() { }
        protected virtual void InitCommands() { }

        public event EventHandler LocalCanExecuteChanged;

        /// <summary>
        /// Raise event indicating that all command execution
        /// conditions must be reevaluated
        /// </summary>
        protected void RaiseLocalCanExecuteChanged()
        {
            EventHandler handler = LocalCanExecuteChanged;
            if (handler != null)
                handler.Invoke(this, EventArgs.Empty);
        }
    }
}
