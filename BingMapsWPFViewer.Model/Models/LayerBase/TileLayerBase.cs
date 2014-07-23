using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BingMapsWPFViewer.Model
{

	//public delegate Uri GetTileUriDelegate(int x, int y, int zoomLevel);

	public abstract class TileLayerBase : LayerBase
	{

		public string UrlPattern { get; set; }
		public Func<int, int, int, Uri> TileUriMethod { get; set; }

		public override bool IsVectorLayer
		{
			get { return false; }
		}		
	}
}
