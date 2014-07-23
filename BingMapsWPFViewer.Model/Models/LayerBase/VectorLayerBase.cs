using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BingMapsWPFViewer.Model.Features;
using Microsoft.SqlServer.Types;
using System.Windows.Media;

namespace BingMapsWPFViewer.Model
{
	
	public abstract class VectorLayerBase : LayerBase
	{
		public override bool IsVectorLayer
		{
			get { return true; }
		}

		private int _srid;
		public int SRID
		{
			get { return _srid; }
			set { _srid = value; }
		}

		public List<FeatureField> Attributes {get;set;}

		public virtual FeatureQueryFilter QueryFilter {get;set;}

		public List<Feature> Features { get; set; }

		private Color _fillColor = Color.FromArgb(64, 0, 0, 255);
		public Color FillColor
		{
			get { return _fillColor; }
			set { _fillColor = value; }
		}
		
	}
}