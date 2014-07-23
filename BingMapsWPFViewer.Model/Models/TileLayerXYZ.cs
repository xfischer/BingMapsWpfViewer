using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BingMapsWPFViewer.Tools;

namespace BingMapsWPFViewer.Model
{
	public class TileLayerXYZ : TileLayerBase
	{
		public TileLayerXYZ(string p_UrlPattern)
		{
			base.UrlPattern = p_UrlPattern;
			base.TileUriMethod = (x, y, z) =>
			{
				string url = string.Format(p_UrlPattern, x, y, z);
				DebugHelper.WriteLine(this, url);
				return new Uri(url);
			};
		}

		public TileLayerXYZ(string urlPattern, Func<int, int, int, Uri> tileUriMethod)
		{
			base.UrlPattern = urlPattern;
			base.TileUriMethod = tileUriMethod;
		}

		public override enLayerType LayerType
		{
			get { return enLayerType.TileLayerXYZ; }
		}

		public override string GenerateDisplayNameProposal()
		{
			if (base.UrlPattern == null)
				return "New " + this.GetType().Name;
			else if (base.TileUriMethod == null)
				return new Uri(base.UrlPattern).DnsSafeHost;
			else
				return base.TileUriMethod(1, 1, 1).DnsSafeHost;
		}
	}
}
