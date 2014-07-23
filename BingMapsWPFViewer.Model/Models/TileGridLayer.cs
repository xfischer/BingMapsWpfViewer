using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BingMapsWPFViewer.Model
{
	public class TileGridLayer : VectorLayerBase
	{

		public TileGridLayer()
			: base()
		{
			this.Attributes = new List<Features.FeatureField>();
			this.Attributes.Add(new Features.FeatureField("TileInfo", "varchar", 1, Model.Features.enFeatureFieldUsage.Label));
		}

		public override enLayerType LayerType
		{
			get { return enLayerType.TileGridLayerVector; }
		}

		public override string GenerateDisplayNameProposal()
		{
			return this.GetType().Name;
		}
	}
}
