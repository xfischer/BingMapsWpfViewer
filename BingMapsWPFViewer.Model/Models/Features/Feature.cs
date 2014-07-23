using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Types;

namespace BingMapsWPFViewer.Model.Features
{
	public class Feature 
	{
		private Guid _featureID;
		public Guid FeatureID
		{
			get { return _featureID; }
		}


		private SqlGeometry _geometry;
		public SqlGeometry Geometry
		{
			get { return _geometry; }
			set { _geometry = value; }
		}

		private SqlGeometry _centroid;
		public SqlGeometry Centroid
		{
			get
			{
				return _centroid;
			}
		}

		private Dictionary<FeatureField, object> _attributes;
		public Dictionary<FeatureField, object> Attributes
		{
			get { return _attributes; }
		}

		public Feature(SqlGeometry geom, SqlGeometry geomCentroid, Dictionary<FeatureField, object> attributes)
		{
			this._featureID = Guid.NewGuid();
			this._geometry = geom;
			this._attributes = attributes;
			this._centroid = geomCentroid;
		}



	}
}
