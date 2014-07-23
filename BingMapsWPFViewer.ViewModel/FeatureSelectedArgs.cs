using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BingMapsWPFViewer.Model.Features;

namespace BingMapsWPFViewer.ViewModel
{
	internal class FeatureSelectedArgs
	{
		public Guid LayerId { get; private set; }
		public Guid FeatureId { get; private set; }
		public Feature Feature { get; private set; }

		public FeatureSelectedArgs(Guid layerId, Guid featureId, Feature feature)
		{
			this.LayerId = layerId;
			this.FeatureId = featureId;
			this.Feature = feature;
		}
	}
}
