using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using BingMapsWPFViewer.Model;

namespace BingMapsWPFViewer.ViewModel
{
	public class LayerType
	{
		public string Name { get; set; }

		public enLayerType UnderlyingType { get; set; }

		private string _icon;
		public string IconName
		{
			get { return _icon; }
		}

		private BitmapImage _image;
		public BitmapImage Icon
		{
			get
			{
				if (string.IsNullOrWhiteSpace(_icon))
					return null;

				return _image;
			}
		}

		public LayerType(string name, string icon, enLayerType type)
		{
			this.Name = name;
			this._icon = icon;
			this.UnderlyingType = type;
			_image = new BitmapImage();
			_image.BeginInit();
			_image.UriSource = new Uri(@"pack://application:,,,/BingMapsWPFViewer.ViewModel;component/Icons/" + _icon, UriKind.Absolute);
			_image.CacheOption = BitmapCacheOption.OnLoad;
			_image.EndInit();
		}
	}
}
