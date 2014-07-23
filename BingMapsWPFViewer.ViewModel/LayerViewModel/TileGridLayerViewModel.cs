using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BingMapsWPFViewer.Model;

namespace BingMapsWPFViewer.ViewModel
{
	public class TileGridLayerViewModel : LayerBaseViewModel
	{
		private TileGridLayer Layer
		{
			get { return (TileGridLayer)base.LayerModel; }
			set { base.LayerModel = value; }
		}

		public TileGridLayerViewModel()
			: base(new TileGridLayer())
		{
		}

		public TileGridLayerViewModel(TileGridLayer layer)
			: base(layer)
		{
		}

		public override string IconName
		{
			get
			{
				return "grid.png";
			}
		}
	}
}
