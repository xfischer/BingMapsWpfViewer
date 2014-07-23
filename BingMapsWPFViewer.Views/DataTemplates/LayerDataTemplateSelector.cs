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

	public class LayerDataTemplateSelector : DataTemplateSelector
	{
		public override DataTemplate
				SelectTemplate(object item, DependencyObject container)
		{

			DataTemplate retDataTemplate = null;

			if (item != null)
			{
				if (item is LayerBaseViewModel)
				{
					if (item is SQLServerLayerViewModel)
						retDataTemplate = Application.Current
								.TryFindResource("SQLServerLayerDataTemplate") as DataTemplate;
					else if (item is ShapeFileLayerViewModel)
						retDataTemplate = Application.Current
							.TryFindResource("ShapeFileLayerDataTemplate") as DataTemplate;
					else if (item is TileGridLayerImageViewModel)
						retDataTemplate = Application.Current
							.TryFindResource("TileGridLayerDataTemplate") as DataTemplate;
					else if (item is TileLayerXYZViewModel)
						retDataTemplate=  Application.Current
							.TryFindResource("TileLayerXYZDataTemplate") as DataTemplate;
					else if (item is TileGridLayerViewModel)
						retDataTemplate= Application.Current
							.TryFindResource("TileGridLayerDataTemplate") as DataTemplate;
					else
						throw new Exception("View model not supported in LayerDataTemplateSelector");
				}
				else
					throw new Exception("View model not supported in LayerDataTemplateSelector");
			}

			if (retDataTemplate == null)
				retDataTemplate = Application.Current
					.TryFindResource("DefaultLayerDataTemplate") as DataTemplate;

			return retDataTemplate;
		}
	}
}
