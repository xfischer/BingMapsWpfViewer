using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BingMapsWPFViewer.Model.Features
{
	[Flags]
	public enum enFeatureFieldUsage
	{
		None = 0x0,									// field will be ignored
		Label = 0x1,								// field will be used as label
		Tooltip = 0x2,							// field will be shown on tooltip
		PrimaryKey = 0x4,						// field will be used as primary key
		Spatial = 0x8,							// field will be used as geometry for display
		SpatialCentroid = 0x10,		  // field will be used as geometry centroid for label positioning
		DataOnly = 0x20,						// field will be included as an attribute only
		Filter = 0x40								// field will be used as filter (WHERE clause)
	}	
}
