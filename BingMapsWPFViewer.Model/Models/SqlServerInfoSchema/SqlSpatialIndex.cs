using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BingMapsWPFViewer.Tools;

namespace BingMapsWPFViewer.Model.Services
{
	public class SqlSpatialIndex : SqlIndex
	{
		public enGridLevel GridSizeLevel1 { get; internal set; }
		public enGridLevel GridSizeLevel2 { get; internal set; }
		public enGridLevel GridSizeLevel3 { get; internal set; }
		public enGridLevel GridSizeLevel4 { get; internal set; }

		public BoundingBox BoundingBox { get; internal set; }
		public bool HasBoundingBox
		{
			get { return !this.IsGeography; }
		}

		public bool IsGeography { get; internal set; }

		internal SqlSpatialIndex()
		{
		}

		public override string ToString()
		{
			string v_grids = string.Format("{0}Grid Size : {1} {2} {3} {4}", base.ToString() + Environment.NewLine, GridSizeLevel1, GridSizeLevel2, GridSizeLevel3, GridSizeLevel4);
			if (!IsGeography)
				v_grids += Environment.NewLine+ string.Format("BBox: {0}", BoundingBox);
			return v_grids;
		}
	}
}
