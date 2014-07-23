using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace BingMapsWPFViewer.Framework.Messages
{
    /// <summary>
    /// Interaction logic for SaveOrCancelWindow.xaml
    /// </summary>
    public partial class SaveOrCancelWindow : Window
    {

        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        public SaveOrCancelWindow()
            : base()
        {
            InitializeComponent();
            Loaded += SaveOrCancelWindow_Loaded;
        }

        #region CloseReason

        /// <summary>
        /// CloseReason Dependency Property
        /// </summary>
        public static readonly DependencyProperty RaisonDeFermetureProperty =
            DependencyProperty.Register("RaisonDeFermeture", typeof(MessageBoxResult), typeof(SaveOrCancelWindow),
                new FrameworkPropertyMetadata(MessageBoxResult.OK));

        /// <summary>
        /// Gets or sets the CloseReason property. This dependency property 
        /// indicates ....
        /// </summary>
        public MessageBoxResult RaisonDeFermeture
        {
            get { return (MessageBoxResult)GetValue(RaisonDeFermetureProperty); }
            set { SetValue(RaisonDeFermetureProperty, value); }
        }

        #endregion


        void SaveOrCancelWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);
        }

        private void _cancelButton_Click(object sender, RoutedEventArgs e)
        {
            RaisonDeFermeture = MessageBoxResult.Cancel;
            Close();
        }

        private void _saveButton_Click(object sender, RoutedEventArgs e)
        {
            RaisonDeFermeture = MessageBoxResult.OK;
            Close();
            VisualStateManager.GoToState(this, "ADesErreurs", true);
        }
    }
}
