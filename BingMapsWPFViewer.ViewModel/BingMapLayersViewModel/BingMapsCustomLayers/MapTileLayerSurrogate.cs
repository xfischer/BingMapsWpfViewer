using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Maps.MapControl.WPF;
using BingMapsWPFViewer.Model;

namespace BingMapsWPFViewer.ViewModel
{
	/// <summary>
	/// Base class for all custom Tile Layers
	/// Ensures layers can be retrieved at runtime from BingMap
	/// </summary>
	public abstract class MapTileLayerSurrogate : MapTileLayer, IBingMapsWPFViewerLayer
	{
		private TileLayerBase _tileLayerBase;
		internal MapTileLayerSurrogate(TileLayerBase p_TileLayerBase)
		{
			_tileLayerBase = p_TileLayerBase;
		}

		#region IBingMapsWPFViewerLayer Membres

		public Guid Id
		{
			get { return _tileLayerBase.Id; }
		}

		#endregion
	}
}
