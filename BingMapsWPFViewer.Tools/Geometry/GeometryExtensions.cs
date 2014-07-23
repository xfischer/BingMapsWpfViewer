using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Types;

namespace BingMapsWPFViewer.Tools
{
	public static class GeometryExtensions
	{
		public static SqlGeometry ToSqlGeometry(this BoundingBox bbox)
		{
			SqlGeometry bboxGeom = null;

			try
			{
				var geob = new SqlGeometryBuilder();
				geob.SetSrid(4326);
				geob.BeginGeometry(OpenGisGeometryType.Polygon);
				geob.BeginFigure(bbox.XMin, bbox.YMax);
				geob.AddLine(bbox.XMin, bbox.YMin);
				geob.AddLine(bbox.XMax, bbox.YMin);
				geob.AddLine(bbox.XMax, bbox.YMax);
				geob.AddLine(bbox.XMin, bbox.YMax);
				geob.EndFigure();
				geob.EndGeometry();
				bboxGeom = geob.ConstructedGeometry;
			}
			catch (Exception)
			{
				bboxGeom = null;
			}

			return bboxGeom;
		}
		
		
	}
}
