using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace BingMapsWPFViewer.Model
{
	public class MapRepository 
	{
		private List<LayerBase> _allLayers;

		public MapRepository()
		{
			_allLayers = new List<LayerBase>();
		}

		public void AddLayer(LayerBase layer)
		{
			// Assign ZIndex
			if (_allLayers.Count > 0)
				layer.ZIndex = _allLayers.Max(l => l.ZIndex) + 1;
			else
				layer.ZIndex = 0;

			if (layer.DisplayName == null)
				layer.DisplayName = "Untitled " + layer.GetType().Name;

			_allLayers.Add(layer);			
		}

		public ObservableCollection<LayerBase> GetAllLayers(string fileName)
		{
			return new ObservableCollection<LayerBase>();
		}

	}
}
