using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BingMapsWPFViewer.Model
{
	public class GoogleTileLayer : TileLayerBase
	{
		private GoogleLayerType _layerType;
		public enum GoogleLayerType
		{
			Road,
			Aerial,
			Labels
		}

		public GoogleTileLayer(GoogleLayerType layerType)
		{
			_layerType = layerType;
			switch (layerType)
			{
				case GoogleLayerType.Aerial:

					base.TileUriMethod = (x, y, zoom) => new Uri(string.Format("https://khms{0}.google.com/kh/v=121&src=app&x={1}&y={2}&z={3}&s=", new Random().Next() % 4, x, y, zoom));
					break;
				case GoogleLayerType.Labels:
					base.TileUriMethod = (x, y, zoom) => new Uri(string.Format("https://mts{0}.google.com/vt/lyrs=h@199000000&hl=fr&src=app&x={1}&y={2}&z={3}&s=G", new Random().Next() % 4, x, y, zoom));
					break;
				case GoogleLayerType.Road:
					base.TileUriMethod = (x, y, zoom) => new Uri(string.Format("http://mt{0}.google.com/vt/lyrs=m&z={3}&x={1}&y={2}", new Random().Next() % 4, x, y, zoom));
					break;
			}

		}

		public override enLayerType LayerType
		{
			get { return enLayerType.TileLayerXYZ; }
		}

		public override string GenerateDisplayNameProposal()
		{
			switch (_layerType)
			{
				case GoogleLayerType.Aerial:

					return "Google Aerial";

				case GoogleLayerType.Labels:
					return "Google Labels";

				case GoogleLayerType.Road:
					return "Google Road";

				default:
					return "Google tile layer";

			}
		}
	}
}
