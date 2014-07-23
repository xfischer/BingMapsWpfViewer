using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Maps.MapControl.WPF;
using BingMapsWPFViewer.Model;
using System.Windows.Controls;

namespace BingMapsWPFViewer.ViewModel
{
	/// <summary>
	/// Base class from XYZ tile layers
	/// </summary>
	internal class MapTileLayerXYZ : MapTileLayerSurrogate
	{

		public MapTileLayerXYZ(TileLayerBase p_TileLayer) // example http://myserver.com/COL={0}&ROW={1}&LEVEL={3}
			: base(p_TileLayer)
		{
			base.TileSource = new MapTileSourceXYZ(p_TileLayer.UrlPattern, p_TileLayer.TileUriMethod);
		}
	}

	internal class MapTileSourceXYZ : TileSource
	{
		private Func<int, int, int, Uri> _GetUriMethod;
		public MapTileSourceXYZ(string urlPattern, Func<int, int, int, Uri> getUriMethod)
		{
			_GetUriMethod = getUriMethod;
		}

		public override Uri GetUri(int x, int y, int zoomLevel)
		{
			Uri uri = _GetUriMethod(x, y, zoomLevel);
			System.Diagnostics.Debug.WriteLine(uri.ToString());
			return uri;
		}
	}
}
