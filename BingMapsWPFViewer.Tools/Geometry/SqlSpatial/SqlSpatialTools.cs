using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Types;
using System.Data.SqlTypes;

namespace BingMapsWPFViewer.Tools.Geometry.SqlSpatial
{
	public static class SqlSpatialTools
	{
		/// <summary>
		/// Simplify a geometry and ensure geom dimension remains the same
		/// If not, an empty geometry is returned
		/// </summary>
		/// <param name="geom"></param>
		/// <returns></returns>
		public static SqlGeometry SimplifyGeometry(SqlGeometry geom, double mapResolution)
		{
			if (geom.InstanceOf("GeometryCollection"))
			{
				// If geometry collection then ensure that every geometry simplified has the same dimension, otherwise emptied
				// Union all the geometries to return a geom collection

				SqlGeometry retGeom = SqlSpatialTools.GetEmptyPoint();
				for (int i = 1; i <= geom.STNumGeometries(); i++)
				{
					SqlGeometry v_simplifiedGeom = SqlSpatialTools.SimplifyGeometry(geom.STGeometryN(i), mapResolution);
					if (v_simplifiedGeom.STDimension() == geom.STDimension())
						retGeom = retGeom.STUnion(v_simplifiedGeom);
				}
				return retGeom;
			}
			else
			{
				// Ensure that simplified geometry has the same dimension, otherwise emptied
				SqlGeometry simplifiedGeom = geom.Reduce(mapResolution);

				// Reduced geom can be a geometry collection (ie: polygon => polygon + line strings)
				// Check every geom part to exclude other dimension geometries
				if (simplifiedGeom.InstanceOf("GeometryCollection"))
				{
					SqlGeometry retGeom = SqlSpatialTools.GetEmptyPoint();
					for (int i = 1; i <= simplifiedGeom.STNumGeometries(); i++)
					{
						SqlGeometry v_simplifiedGeom = simplifiedGeom.STGeometryN(i);
						if (v_simplifiedGeom.STDimension() == geom.STDimension())
							retGeom = retGeom.STUnion(v_simplifiedGeom);
					}
					return retGeom;
				}
				else
				{
					if (geom.STDimension() == simplifiedGeom.STDimension())
						return simplifiedGeom;
					else
						return SqlSpatialTools.GetEmptyPoint();
				}
			}
		}

		public static SqlGeometry GetEmptyPoint(int srid = 4326)
		{
			return SqlGeometry.STPointFromText(new SqlChars(new SqlString("POINT EMPTY")), srid);
		}

		#region Ring orientation helper
		public enum enRingOrientation : int
		{
			Unknown = 0,
			Clockwise = -1,
			CounterClockwise = 1
		}

		public static enRingOrientation GetRingOrientation(Coordinate[] coordinates)
		{
			// Inspired by http://www.engr.colostate.edu/~dga/dga/papers/point_in_polygon.pdf

			// This algorithm is to simply determine the Ring Orientation, so to do so, find the
			// extreme left and right points, and then check orientation

			if (coordinates.Length < 4)
			{
				return enRingOrientation.Unknown;
				//throw new ArgumentException("A polygon requires at least 4 points.");
			}

			if (coordinates[0].X != coordinates[coordinates.Length - 1].X || coordinates[0].Y != coordinates[coordinates.Length - 1].Y)
			{
				return enRingOrientation.Unknown;
			}

			int rightmostIndex = 0;
			int leftmostIndex = 0;

			for (int i = 1; i < coordinates.Length; i++)
			{
				if (coordinates[i].X < coordinates[leftmostIndex].X)
				{
					leftmostIndex = i;
				}
				if (coordinates[i].X > coordinates[rightmostIndex].X)
				{
					rightmostIndex = i;
				}
			}


			Coordinate p0; // Point before the extreme
			Coordinate p1; // The extreme point
			Coordinate p2; // Point after the extreme

			double m; // Holds line slope

			double lenP2x;  // Length of the P1-P2 line segment's delta X
			double newP0y;  // The Y value of the P1-P0 line segment adjusted for X=lenP2x

			enRingOrientation left_orientation;
			enRingOrientation right_orientation;

			// Determine the orientation at the Left Point
			if (leftmostIndex == 0)
				p0 = coordinates[coordinates.Length - 2];
			else
				p0 = coordinates[leftmostIndex - 1];

			p1 = coordinates[leftmostIndex];

			if (leftmostIndex == coordinates.Length - 1)
				p2 = coordinates[1];
			else
				p2 = coordinates[leftmostIndex + 1];

			m = (p1.Y - p0.Y) / (p1.X - p0.X);

			if (double.IsInfinity(m))
			{
				// This is a vertical line segment, so just calculate the dY to
				// determine orientation

				left_orientation = (enRingOrientation)Math.Sign(p0.Y - p1.Y);
			}
			else if (double.IsNaN(m))
			{
				lenP2x = p2.X - p1.X;
				newP0y = p1.Y;


				left_orientation = (enRingOrientation)Math.Sign(newP0y - p2.Y);
			}
			else
			{
				lenP2x = p2.X - p1.X;
				newP0y = p1.Y + (m * lenP2x);


				left_orientation = (enRingOrientation)Math.Sign(newP0y - p2.Y);
			}



			// Determine the orientation at the Right Point
			if (rightmostIndex == 0)
				p0 = coordinates[coordinates.Length - 2];
			else
				p0 = coordinates[rightmostIndex - 1];

			p1 = coordinates[rightmostIndex];

			if (rightmostIndex == coordinates.Length - 1)
				p2 = coordinates[1];
			else
				p2 = coordinates[rightmostIndex + 1];

			m = (p1.Y - p0.Y) / (p1.X - p0.X);

			if (double.IsInfinity(m))
			{
				// This is a vertical line segment, so just calculate the dY to
				// determine orientation

				right_orientation = (enRingOrientation)Math.Sign(p1.Y - p0.Y);
			}
			else if (double.IsNaN(m))
			{
				lenP2x = p2.X - p1.X;
				newP0y = p1.Y;

				right_orientation = (enRingOrientation)Math.Sign(p2.Y - newP0y);
			}
			else
			{
				lenP2x = p2.X - p1.X;
				newP0y = p1.Y + (m * lenP2x);

				right_orientation = (enRingOrientation)Math.Sign(p2.Y - newP0y);
			}


			if (left_orientation == enRingOrientation.Unknown)
			{
				return right_orientation;
			}
			else
			{
				return left_orientation;
			}
		}

		#endregion Ring orientation helper
	}

	
}
