using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BingMapsWPFViewer.Tools.Geometry.Reprojection;
using BingMapsWPFViewer.Tools.Geometry.SpatialReference;
using Microsoft.SqlServer.Types;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;
using ProjNet.Converters.WellKnownText;

namespace BingMapsWPFViewer.Tools.Geometry
{
	public sealed class GeometryReprojector
	{
		// Create the transformation instance:
		ICoordinateTransformation _coordTransform;
		int _toSrid;

		public double[] ConvertPoint(double[] point)
		{
			if (_coordTransform == null)
				return point;
			else
			{
				double[] fromPoint = point;
				double[] toPoint = _coordTransform.MathTransform.Transform(fromPoint);
				return toPoint;
			}
		}

		public GeometryReprojector(int fromSrid, int toSrid)
		{
			_toSrid = toSrid;
			if (fromSrid == toSrid)
				_coordTransform = null;
			else
			{
				// Create the source coordinate system from WKT
				ICoordinateSystem fromCS = SridReader.GetCSbyID(fromSrid);

				// Create the destination coordinate system from WKT
				ICoordinateSystem toCS = SridReader.GetCSbyID(toSrid);

				// Create a CoordinateTransformationFactory:
				ProjNet.CoordinateSystems.Transformations.CoordinateTransformationFactory ctfac = new ProjNet.CoordinateSystems.Transformations.CoordinateTransformationFactory();

				// Create the transformation instance:
				_coordTransform = ctfac.CreateFromCoordinateSystems(fromCS, toCS);
			}
		}

		public GeometryReprojector(string fromCsWKT, int toSrid)
		{
			_toSrid = toSrid;

			// Create the source coordinate system from WKT
			ICoordinateSystem fromCS = CoordinateSystemWktReader.Parse(fromCsWKT) as ICoordinateSystem;

			// Create the destination coordinate system from WKT
			ICoordinateSystem toCS = SridReader.GetCSbyID(toSrid);

			if (fromCS.EqualParams(toCS))
				_coordTransform = null;
			else
			{
				// Create a CoordinateTransformationFactory:
				ProjNet.CoordinateSystems.Transformations.CoordinateTransformationFactory ctfac = new ProjNet.CoordinateSystems.Transformations.CoordinateTransformationFactory();

				// Create the transformation instance:
				_coordTransform = ctfac.CreateFromCoordinateSystems(fromCS, toCS);
			}
		}

		public GeometryReprojector(int fromSrid, string toCsWKT)
		{
			_toSrid = 0;

			// Create the source coordinate system from WKT
			ICoordinateSystem fromCS = SridReader.GetCSbyID(fromSrid);

			// Create the destination coordinate system from WKT
			ICoordinateSystem toCS = CoordinateSystemWktReader.Parse(toCsWKT) as ICoordinateSystem;

			if (fromCS.EqualParams(toCS))
				_coordTransform = null;
			else
			{
				// Create a CoordinateTransformationFactory:
				ProjNet.CoordinateSystems.Transformations.CoordinateTransformationFactory ctfac = new ProjNet.CoordinateSystems.Transformations.CoordinateTransformationFactory();

				// Create the transformation instance:
				_coordTransform = ctfac.CreateFromCoordinateSystems(fromCS, toCS);
			}
		}

		public GeometryReprojector(string fromCsWKT, string toCsWKT)
		{
			_toSrid = 0;

			// Create the source coordinate system from WKT
			ICoordinateSystem fromCS = CoordinateSystemWktReader.Parse(fromCsWKT) as ICoordinateSystem;

			// Create the destination coordinate system from WKT
			ICoordinateSystem toCS = CoordinateSystemWktReader.Parse(toCsWKT) as ICoordinateSystem;

			if (fromCS.EqualParams(toCS))
				_coordTransform = null;
			else
			{
				// Create a CoordinateTransformationFactory:
				ProjNet.CoordinateSystems.Transformations.CoordinateTransformationFactory ctfac = new ProjNet.CoordinateSystems.Transformations.CoordinateTransformationFactory();

				// Create the transformation instance:
				_coordTransform = ctfac.CreateFromCoordinateSystems(fromCS, toCS);
			}
		}

		public SqlGeometry ConvertSqlGeometry2Geometry(SqlGeometry geom)
		{
			if (_coordTransform == null)
			{
				// srid are the same, geom is our result
				return geom;
			}
			else
			{
				// create a sink that will create a geometry instance
				SqlGeometryBuilder b = new SqlGeometryBuilder();
				b.SetSrid(_toSrid);

				// create a sink to do the shift and plug it in to the builder
				TransformGeometryToGeometrySink s = new TransformGeometryToGeometrySink(_coordTransform, b);

				// plug our sink into the geometry instance and run the pipeline
				geom.Populate(s);

				// the end of our pipeline is now populated with the shifted geometry instance
				return b.ConstructedGeometry;
			}
		}

		public SqlGeometry ConvertSqlGeography2Geometry(SqlGeography geom)
		{
			// since we want an SqlGeometry, we need to convert from SqlGeography in all cases
			// the sink will handle transform nullity

			// create a sink that will create a geometry instance
			SqlGeometryBuilder b = new SqlGeometryBuilder();
			b.SetSrid(_toSrid);

			// create a sink to do the shift and plug it in to the builder
			TransformGeographyToGeometrySink s = new TransformGeographyToGeometrySink(_coordTransform, b);

			// plug our sink into the geometry instance and run the pipeline
			geom.Populate(s);

			// the end of our pipeline is now populated with the shifted geometry instance
			return b.ConstructedGeometry;
		}

		public SqlGeography ConvertSqlGeometry2Geography(SqlGeometry geom)
		{
			// create a sink that will create a geometry instance
			SqlGeographyBuilder b = new SqlGeographyBuilder();
			b.SetSrid(_toSrid);

			// create a sink to do the shift and plug it in to the builder
			TransformGeometryToGeographySink s = new TransformGeometryToGeographySink(_coordTransform, b);

			// plug our sink into the geometry instance and run the pipeline
			geom.Populate(s);

			// the end of our pipeline is now populated with the shifted geometry instance
			return b.ConstructedGeography;
		}

		public SqlGeography ConvertSqlGeography2Geography(SqlGeography geom)
		{
			if (_coordTransform == null)
			{
				// srid are the same, geom is our result
				return geom;
			}
			else
			{
				// create a sink that will create a geometry instance
				SqlGeographyBuilder b = new SqlGeographyBuilder();
				b.SetSrid(_toSrid);

				// create a sink to do the shift and plug it in to the builder
				TransformGeographyToGeographySink s = new TransformGeographyToGeographySink(_coordTransform, b);

				// plug our sink into the geometry instance and run the pipeline
				geom.Populate(s);

				// the end of our pipeline is now populated with the shifted geometry instance
				return b.ConstructedGeography;
			}
		}

	}

}
