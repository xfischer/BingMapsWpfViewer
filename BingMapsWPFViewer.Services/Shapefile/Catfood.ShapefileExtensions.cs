using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Catfood.Shapefile;
using BingMapsWPFViewer.Tools.Geometry.SqlSpatial;
using BingMapsWPFViewer.Tools.Geometry;

namespace BingMapsWPFViewer.Services
{
	public static class CatfoodShapefileExtensions
	{

		/// <summary>
		/// Returns clockwise ordered shape part list
		/// Not used
		/// </summary>
		/// <param name="shapePolygon"></param>
		/// <param name="clockwiseFirst"></param>
		/// <returns></returns>
		public static List<PointD[]> PartsClockwiseOrdered(this ShapePolygon shapePolygon, bool clockwiseFirst = true)
		{
			if (shapePolygon.Parts.Count <= 1)
				return shapePolygon.Parts;

			List<Tuple<SqlSpatialTools.enRingOrientation, PointD[]>> listParts = new List<Tuple<SqlSpatialTools.enRingOrientation, PointD[]>>();
			foreach (PointD[] part in shapePolygon.Parts)
			{
				SqlSpatialTools.enRingOrientation orientation = SqlSpatialTools.GetRingOrientation(part.Select(p => new Coordinate(p.X, p.Y)).ToArray());
				listParts.Add(new Tuple<SqlSpatialTools.enRingOrientation, PointD[]>(orientation, part));
			}

			if (clockwiseFirst)
				return listParts
									.OrderBy(t => t.Item1)
									.Select(t => t.Item2)
									.ToList();
			else
				return listParts
										.OrderByDescending(t => t.Item1)
										.Select(t => t.Item2)
										.ToList();
		}
	}
}
