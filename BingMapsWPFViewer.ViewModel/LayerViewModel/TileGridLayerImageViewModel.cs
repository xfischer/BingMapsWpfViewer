using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using BingMapsWPFViewer.Model;
using BingMapsWPFViewer.Tools;
using BingMapsWPFViewer.Tools.TinyHttpHost;

namespace BingMapsWPFViewer.ViewModel
{
	public class TileGridLayerImageViewModel : LayerBaseViewModel
	{
		
		[Required(ErrorMessage = "URL is required")]
		public string UrlPattern
		{
			get { return this.Layer.UrlPattern; }
			set
			{
				if (this.Layer.UrlPattern != value)
				{
					this.Layer.UrlPattern = value;
					this.Layer.TileUriMethod = (x, y, z) => new Uri(string.Format(value, x, y, z));
					RaisePropertyChanged<string>(() => UrlPattern);
				}

				RaisePropertyChanged<bool>(() => IsValid);
			}
		}

		public override string IconName
		{
			get
			{
				return "web.png";
			}
		}

		private TileLayerXYZ Layer
		{
			get { return (TileLayerXYZ)base.LayerModel; }
			set { base.LayerModel = value; }
		}

		public TileGridLayerImageViewModel() : base(null)
		{
			string localUrl = "http://localhost:" + TinyHttpHost.PortNumber.ToString() + "/tile/{2}/{0}/{1}.png";
			//string localUrl = @"http://localhost:" + TinyHttpHost.PortNumber.ToString() + "/disktile/" + "D:;Temp;TileSets;OSM_HILLSHADE" + "/{2}/{0}/{1}.png";
			base.LayerModel = new TileLayerXYZ(localUrl);
		}

		
	}
}
