using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Microsoft.Maps.MapControl.WPF;
using System.Configuration;
using BingMapsWPFViewer.ViewModel;

namespace BingMapsWPFViewer.Main
{
	/// <summary>
	/// Logique d'interaction pour MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, IBingMapsView
	{
		public MainWindow()
		{
			InitializeComponent();

			// Main view model cannot be set from XAML because it needs the Bing Maps control instance
			MainViewModel mainViewModel = new MainViewModel(this.MapInstance);
			this.DataContext = mainViewModel;

		}		

		#region IBingMapsContainer Membres

		public Map MapInstance
		{
			get { return this.Map; }
		}

		#endregion
		

		
	}
}
