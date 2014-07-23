using System;
using Microsoft.SqlServer.Types;
using ProjNet.CoordinateSystems.Transformations;

namespace BingMapsWPFViewer.Tools.Geometry.Reprojection
{
  class TransformGeometryToGeometrySink : IGeometrySink110
  {
    private readonly ICoordinateTransformation _trans;
    private readonly IGeometrySink110 _sink;

    public TransformGeometryToGeometrySink(ICoordinateTransformation trans, IGeometrySink110 sink)
    {
      _trans = trans;
      _sink = sink;
    }

    public void BeginGeometry(OpenGisGeometryType type)
    {
      _sink.BeginGeometry(type);
    }

    public void EndGeometry()
    {
      _sink.EndGeometry();
    }

    public void BeginFigure(double x, double y, Nullable<double> z, Nullable<double> m)
    {
			if (_trans == null)
			{
				_sink.BeginFigure(x, y, z, m);
			}
			else
			{
				double[] fromPoint = { x, y };
				double[] toPoint = _trans.MathTransform.Transform(fromPoint);
				double tox = toPoint[0];
				double toy = toPoint[1];
				_sink.BeginFigure(tox, toy, z, m);
			}
    }

    public void AddLine(double x, double y, Nullable<double> z, Nullable<double> m)
    {
			if (_trans == null)
			{
				_sink.AddLine(x, y, z, m);
			}
			else
			{
				double[] fromPoint = { x, y };
				double[] toPoint = _trans.MathTransform.Transform(fromPoint);
				double tox = toPoint[0];
				double toy = toPoint[1];
				_sink.AddLine(tox, toy, z, m);
			}
    }

    public void EndFigure()
    {
      _sink.EndFigure();
    }

    public void SetSrid(int srid)
    {
      // _sink.SetSrid(srid);
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
