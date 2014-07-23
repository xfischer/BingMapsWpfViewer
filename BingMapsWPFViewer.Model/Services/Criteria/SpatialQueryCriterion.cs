using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BingMapsWPFViewer.Model.Features;
using BingMapsWPFViewer.Tools;

namespace BingMapsWPFViewer.Model.Services
{
	public class SpatialQueryCriterion : CriterionBase<SpatialQueryCriterion>
	{
		public MapViewport MapViewport { get; private set; }
		public List<FeatureField> FieldsToRetrieve { get; private set; }
		public List<FeatureQueryFilter> QueryFilter { get; private set; }
		public BoundingBox BoundingBox
		{
			get
			{
				if (MapViewport == null)
					return null;

				return MapViewport.GeographicBounds;
			}
		}

		public SpatialQueryCriterion(MapViewport mapViewport, List<FeatureField> fieldsToRetrieve, List<FeatureQueryFilter> queryFilter = null, int sizeLimit = 5000)
		{
			this.MapViewport = mapViewport;
			this.FieldsToRetrieve = fieldsToRetrieve;
			this.QueryFilter = queryFilter;
			this.SizeLimit = sizeLimit;
		}

		public SpatialQueryCriterion()
			: this(null, null)
		{ }
	}
}
