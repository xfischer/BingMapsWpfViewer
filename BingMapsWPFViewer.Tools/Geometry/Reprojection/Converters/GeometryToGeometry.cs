using BingMapsWPFViewer.Tools.Geometry.SpatialReference;
using Microsoft.SqlServer.Types;
using ProjNet.CoordinateSystems;

namespace BingMapsWPFViewer.Tools.Geometry.Reprojection
{
	public partial class ReprojectionUtils
	{

		public static SqlGeometry GeometryToGeometry(SqlGeometry geom, int toSRID)
		{
			// Create the source coordinate system from WKT
			ICoordinateSystem fromCS = SridReader.GetCSbyID(geom.STSrid.Value);

			// Create the destination coordinate system from WKT
			ICoordinateSystem toCS = SridReader.GetCSbyID(toSRID);

			// Create a CoordinateTransformationFactory:
			ProjNet.CoordinateSystems.Transformations.CoordinateTransformationFactory ctfac = new ProjNet.CoordinateSystems.Transformations.CoordinateTransformationFactory();

			// Create the transformation instance:
			ProjNet.CoordinateSystems.Transformations.ICoordinateTransformation trans = ctfac.CreateFromCoordinateSystems(fromCS, toCS);

			// create a sink that will create a geometry instance
			SqlGeometryBuilder b = new SqlGeometryBuilder();
			b.SetSrid((int)toSRID);

			// create a sink to do the shift and plug it in to the builder
			TransformGeometryToGeometrySink s = new TransformGeometryToGeometrySink(trans, b);

			// plug our sink into the geometry instance and run the pipeline
			geom.Populate(s);

			// the end of our pipeline is now populated with the shifted geometry instance
			return b.ConstructedGeometry;

		}
	}
}
