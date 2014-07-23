﻿using BingMapsWPFViewer.Tools.Geometry.SpatialReference;
using Microsoft.SqlServer.Types;
using ProjNet.CoordinateSystems;

namespace BingMapsWPFViewer.Tools.Geometry.Reprojection
{
	public partial class ReprojectionUtils
	{

		public static SqlGeography GeographyToGeography(SqlGeography geog, int toSRID)
		{

			// Create the source coordinate system from WKT
			ICoordinateSystem fromCS = SridReader.GetCSbyID(geog.STSrid.Value);

			// Create the destination coordinate system from WKT
			ICoordinateSystem toCS = SridReader.GetCSbyID(toSRID);

			// Create a CoordinateTransformationFactory:
			ProjNet.CoordinateSystems.Transformations.CoordinateTransformationFactory ctfac = new ProjNet.CoordinateSystems.Transformations.CoordinateTransformationFactory();

			// Create the transformation instance:
			ProjNet.CoordinateSystems.Transformations.ICoordinateTransformation trans = ctfac.CreateFromCoordinateSystems(fromCS, toCS);

			// create a sink that will create a geometry instance
			SqlGeographyBuilder b = new SqlGeographyBuilder();
			b.SetSrid((int)toSRID);

			// create a sink to do the shift and plug it in to the builder
			TransformGeographyToGeographySink s = new TransformGeographyToGeographySink(trans, b);

			// plug our sink into the geometry instance and run the pipeline
			geog.Populate(s);

			// the end of our pipeline is now populated with the shifted geometry instance
			return b.ConstructedGeography;

		}
	}
}
