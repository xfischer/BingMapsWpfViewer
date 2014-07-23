using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace BingMapsWPFViewer.Framework
{
    public class ProxyCommand<T> : ICommand where T : ViewModelBase
    {
        #region Attributs
        private readonly Action<object> executeAction;
        private readonly Predicate<Object> canExecutePredicat;
        private readonly T parentViewModel;
				private Action<object> action;
        #endregion

        #region Constructeurs
        public ProxyCommand(Action<Object> actionAExecuter)
            : this(actionAExecuter, null, null) { }

        public ProxyCommand(Action<Object> actionAExecuter, T viewModel)
            : this(actionAExecuter, null, viewModel) { }

        public ProxyCommand(Action<Object> actionAExecuter,
            Predicate<Object> canExecute,
            T viewModel)
        {
            if (actionAExecuter == null)
                throw new ArgumentNullException(
                    "Il faut fournir une action à executer.");
            this.parentViewModel = viewModel;
            this.executeAction = actionAExecuter;
            this.canExecutePredicat = canExecute;
        }

				
        #endregion

        #region Méthodes d'ICommand
        /// <summary>
        /// Est-ce que l'on peut executer la commande ?
        /// Délégue la logique aux délégué fournit. 
        /// Si aucun n'est fourni, alors on renvoit true.
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public bool CanExecute(object parameter)
        {
            return (canExecutePredicat == null)
                ? true : canExecutePredicat(parameter);
        }

        /// <summary>
        /// Méthode appelée lorsque la commande est exécutée.
        /// Délégue le travail à l'action fournie.
        /// </summary>
        /// <param name="parameter">Le paramètre de la commande.</param>
        public void Execute(object parameter)
        {
            executeAction(parameter);
        }

        #endregion

        #region Evénements d'ICommand
        /// <summary>
        /// Evénement déclenché lorsque la méthode
        /// CanExecute renvoit potentiellement une valeur
        /// différente.
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add
            {
                if (parentViewModel == null) return;
                parentViewModel.LocalCanExecuteChanged += value;
            }
            remove
            {
                if (parentViewModel == null) return;
                parentViewModel.LocalCanExecuteChanged -= value;
            }
        }
        #endregion
    }
}
