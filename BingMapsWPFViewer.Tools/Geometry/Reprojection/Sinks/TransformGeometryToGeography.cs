
using Microsoft.SqlServer.Types;
using ProjNet.CoordinateSystems.Transformations;

namespace BingMapsWPFViewer.Tools.Geometry.Reprojection
{
	class TransformGeometryToGeographySink : IGeometrySink110
	{
		private readonly ICoordinateTransformation _trans;
		private readonly IGeographySink110 _sink;

		public TransformGeometryToGeographySink(ICoordinateTransformation trans, IGeographySink110 sink)
		{
			_trans = trans;
			_sink = sink;
		}

		public void BeginGeometry(OpenGisGeometryType type)
		{
			_sink.BeginGeography((OpenGisGeographyType)type);
		}

		public void EndGeometry()
		{
			_sink.EndGeography();
		}

		public void BeginFigure(double x, double y, double? z, double? m)
		{
			if (_trans == null)
			{
				_sink.BeginFigure(y, x, z, m);
			}
			else
			{
				double[] fromPoint = { x, y };
				double[] toPoint = _trans.MathTransform.Transform(fromPoint);
				double longitude = toPoint[0];
				double latitude = toPoint[1];
				_sink.BeginFigure(latitude, longitude, z, m);
			}
		}

		public void AddLine(double x, double y, double? z, double? m)
		{
			if (_trans == null)
			{
				_sink.AddLine(y, x, z, m);
			}
			else
			{
				double[] fromPoint = { x, y };
				double[] toPoint = _trans.MathTransform.Transform(fromPoint);
				double longitude = toPoint[0];
				double latitude = toPoint[1];
				_sink.AddLine(latitude, longitude, z, m);
			}
		}

		public void EndFigure()
		{
			_sink.EndFigure();
		}

		public void SetSrid(int srid)
		{
			// Input argument not used since a new srid is defined in the constructor.
			//_sink.SetSrid(srid);
		}

		#region IGeometrySink110 Membres

		public void AddCircularArc(double x1, double y1, double? z1, double? m1, double x2, double y2, double? z2, double? m2)
		{
			if (_trans == null)
			{
				_sink.AddCircularArc(x1, y1, z1, m1, x2, y2, z2, m2);
			}
			else
			{
				double[] fromPoint1 = { x1, y1 };
				double[] toPoint1 = _trans.MathTransform.Transform(fromPoint1);
				double[] fromPoint2 = { x2, y2 };
				double[] toPoint2 = _trans.MathTransform.Transform(fromPoint2);
				double longitude1 = toPoint1[0];
				double latitude1 = toPoint1[1];
				double longitude2 = toPoint2[0];
				double latitude2 = toPoint2[1];
				_sink.AddCircularArc(longitude1, latitude1, z1, m1, longitude2, latitude2, z2, m2);
			}
		}

		#endregion
	}
}
