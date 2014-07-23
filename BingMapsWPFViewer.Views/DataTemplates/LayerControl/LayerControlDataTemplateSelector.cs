using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using BingMapsWPFViewer.Model;
using BingMapsWPFViewer.ViewModel;

namespace BingMapsWPFViewer.Views
{

	public class LayerControlDataTemplateSelector : DataTemplateSelector
	{
		public override DataTemplate
				SelectTemplate(object item, DependencyObject container)
		{

			if (item != null)
			{
				if (item is SQLServerLayerViewModel)
				return Application.Current
						.TryFindResource("SQLServerLayerControlDataTemplate") as DataTemplate;
				else if (item is TileLayerXYZViewModel)
					return Application.Current
						.TryFindResource("TileLayerXYZLayerControlDataTemplate") as DataTemplate;
			}

			return Application.Current
					.TryFindResource("DefaultLayerControlDataTemplate") as DataTemplate;
		}
	}
}
