using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace BingMapsWPFViewer.Framework
{

    public class FreezableRelayCommand
        : Freezable, ICommand
    {

        protected override Freezable CreateInstanceCore()
        {
            //On ne fait rien ici
            throw new NotImplementedException();
        }
        #region CommandConcrete

        /// <summary>
        /// CommandConcrete Dependency Property
        /// </summary>
        public static readonly DependencyProperty CommandeConcreteProperty =
            DependencyProperty.Register("CommandeConcrete", typeof(ICommand),
            typeof(FreezableRelayCommand),
                new PropertyMetadata(OnCommandConcreteChanged));

        /// <summary>
        /// Gets or sets the CommandConcrete property. This dependency property 
        /// indicates ....
        /// </summary>
        public ICommand CommandeConcrete
        {
            get { return (ICommand)GetValue(CommandeConcreteProperty); }
            set { SetValue(CommandeConcreteProperty, value); }
        }

        /// <summary>
        /// Handles changes to the CommandConcrete property.
        /// </summary>
        private static void OnCommandConcreteChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            FreezableRelayCommand target = (FreezableRelayCommand)d;
            ICommand oldCommandConcrete = (ICommand)e.OldValue;
            ICommand newCommandConcrete = target.CommandeConcrete;
            target.OnCommandConcreteChanged(target, oldCommandConcrete,
                newCommandConcrete);
        }

        protected virtual void OnCommandConcreteChanged(
            FreezableRelayCommand appelant,
            ICommand ancienneCommande, ICommand nouvelleCommande)
        {
            if (appelant == null) return;

            //On se désabonne à l'événement de la commande concrète            
            if (ancienneCommande != null)
            {
                ancienneCommande.CanExecuteChanged -= appelant.CanExecuteChanged;
            }
            //On s'abonne à l'événement de la commande concrète
            
            if (nouvelleCommande != null)
            {
                nouvelleCommande.CanExecuteChanged += appelant.CanExecuteChanged;
            }
        }
        #endregion

        
        public bool CanExecute(object parametre)
        {
            return (CommandeConcrete != null)
                && CommandeConcrete.CanExecute(parametre);
        }

        public void Execute(object parametre)
        {
            CommandeConcrete.Execute(parametre);
        }

        //Fait partie de l'interface ICommand
        public event EventHandler CanExecuteChanged;

    }
}
