using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BingMapsWPFViewer.Model;
using System.ComponentModel.DataAnnotations;
using System.IO;
using BingMapsWPFViewer.Tools.TinyHttpHost;

namespace BingMapsWPFViewer.ViewModel
{
	public class TileLayerXYZViewModel : LayerBaseViewModel
	{
		[Required(ErrorMessage = "URL is required")]
		public string UrlPattern
		{
			get { return this.Layer.UrlPattern; }
			set
			{
				if (this.Layer.UrlPattern != value)
				{
					if (Directory.Exists(value))
					{
						this.Layer.UrlPattern = value;
						string encodedPath = value.Replace(@"\", ";");
						string localUrl = @"http://localhost:" + TinyHttpHost.PortNumber.ToString() + "/disktile/" + encodedPath + "/{2}/{0}/{1}.png";
						this.Layer.UrlPattern = localUrl;
						RaisePropertyChanged<string>(() => UrlPattern);
					}
					else
					{
						this.Layer.UrlPattern = value;
						this.Layer.TileUriMethod = (x, y, z) => new Uri(string.Format(value, x, y, z));
						RaisePropertyChanged<string>(() => UrlPattern);
					}
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

		public TileLayerXYZViewModel()
			: base(new TileLayerXYZ("https://a.tile.openstreetmap.fr/osmfr/{2}/{0}/{1}.png"))
		{
		}

		public TileLayerXYZViewModel(TileLayerXYZ layer)
			: base(layer)
		{
		}

	}
}
