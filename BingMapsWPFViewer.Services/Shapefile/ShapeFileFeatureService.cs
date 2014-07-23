using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BingMapsWPFViewer.Model.Services;
using BingMapsWPFViewer.Model;
using BingMapsWPFViewer.Tools;
using BingMapsWPFViewer.Model.Features;
using Catfood.Shapefile;
using System.IO;
using BingMapsWPFViewer.Tools.Geometry;
using System.Diagnostics;
using Microsoft.SqlServer.Types;
using BingMapsWPFViewer.Tools.Geometry.SqlSpatial;

namespace BingMapsWPFViewer.Services
{
	public sealed class ShapeFileFeatureService
		: FeatureServiceBase<ShapeFileLayer, SpatialQueryCriterion>
	{

		private const int OUTPUT_SRID = 4326;
		private bool _cancel = false;

		/// <summary>
		/// Load shapes from shape file
		/// Shapes bbox must intersect view bbox
		/// </summary>
		/// <param name="layer"></param>
		/// <param name="criterion"></param>
		/// <returns></returns>
		public override List<Feature> LoadConcrete(ShapeFileLayer layer, SpatialQueryCriterion criterion)
		{
			// return list
			List<Feature> featureList = new List<Feature>();
			List<FeatureField> featureFields = null;
			GeometryReprojector file2ScreenConverter = null;
			GeometryReprojector screen2fileConverter = null;
			Stopwatch stopwatchConvert = new Stopwatch();

			try
			{

				using (Shapefile shapefile = new Shapefile(layer.ShapeFileName))
				{
					#region Setup coord conversion
					string projCS = null;
					try
					{
						projCS = ShapefileHelper.GetProjection(layer.ShapeFileName);

						file2ScreenConverter = new GeometryReprojector(projCS, OUTPUT_SRID); // converts results fetched from shapefile
						screen2fileConverter = new GeometryReprojector(OUTPUT_SRID, projCS); // converts from screen coords to shapefile coords
					}
					catch (Exception exPrj)
					{
						throw new Exception("Unable to setup coordinate transform from shapefile .prj file (" + exPrj.Message + ")");
					}

					#endregion Setup coord conversion

					SqlGeometry bbox = criterion.BoundingBox.ToSqlGeometry();
					bbox = screen2fileConverter.ConvertSqlGeometry2Geometry(bbox);

					// enumerate all shapes
					foreach (Shape shape in shapefile)
					{
						if (_cancel)
							break;

						if (featureFields == null)
							featureFields = this.BuildFeatureFieldsFromShape(shape);


						Feature feature = this.ReadShape(shape, file2ScreenConverter, featureFields, stopwatchConvert, bbox, criterion.MapViewport.MapResolution);
						if (feature != null)
							featureList.Add(feature);

					}
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
			finally
			{
				DebugHelper.WriteLine(this, string.Format("Convert+Filter = {0} ms, {1} features", stopwatchConvert.ElapsedMilliseconds, featureList.Count));
			}

			return featureList;
		}

		private List<FeatureField> BuildFeatureFieldsFromShape(Shape shape)
		{
			List<FeatureField> fields = new List<FeatureField>();
			try
			{
				// each shape may have associated metadata
				string[] metadataNames = shape.GetMetadataNames();
				if (metadataNames != null)
				{
					foreach (string metadataName in metadataNames)
					{
						int ordinal = shape.DataRecord.GetOrdinal(metadataName);
						FeatureField field = new FeatureField(metadataName, shape.DataRecord.GetDataTypeName(ordinal), ordinal, enFeatureFieldUsage.None);
						fields.Add(field);
					}
				}
			}
			catch (Exception)
			{
				throw;
			}
			return fields;
		}

		private Feature ReadShape(Shape shape
															, GeometryReprojector db2ScreenConverter		// coordinate and geometry converter
															, List<FeatureField> attributeFields			// list of attributes columns to retrieve
															, Stopwatch queryConvertStopWatch					// stop watch use to measure convert/reproject performance
															, SqlGeometry queryBbox										// input bbox (spatial filter)
															, double mapResolution										// tolerance for geometry reduction
															)
		{
			Feature feature = null;
			try
			{
				queryConvertStopWatch.Start();

				SqlGeometry shapeBbox = this.ReadShapeBBox(shape);

				if (shapeBbox.STIsValid().Value &&  shapeBbox.STIntersects(queryBbox).Value)
				{
					SqlGeometry geom = this.ReadShapeGeometry(shape, db2ScreenConverter);

					// simplify geometry
					geom = SqlSpatialTools.SimplifyGeometry(geom, mapResolution);

					Dictionary<FeatureField, object> attributes = new Dictionary<FeatureField, object>();
					foreach (FeatureField attributeField in attributeFields)
						attributes.Add(attributeField, shape.GetMetadata(attributeField.FieldName));

					feature = new Feature(geom, null, attributes);
				}

				queryConvertStopWatch.Stop();
			}
			catch (Exception ex)
			{
				throw new Exception("Error reading feature: " + ex.Message, ex);
			}
			return feature;
		}

		#region Shape BBOX

		private SqlGeometry ReadShapeBBox(Shape shape)
		{
			SqlGeometry geom = null;

			try
			{

				switch (shape.Type)
				{
					#region Point
					case ShapeType.Point:
					case ShapeType.PointM:
					case ShapeType.PointZ:

						// a point is just a single x/y point
						ShapePoint shapePoint = shape as ShapePoint;
						geom = SqlGeometry.Point(shapePoint.Point.X, shapePoint.Point.Y, 0);


						break;
					#endregion Point

					#region MultiPoint
					case ShapeType.MultiPoint:
					case ShapeType.MultiPointM:
					case ShapeType.MultiPointZ:

						ShapeMultiPoint shapeMultiPoint = shape as ShapeMultiPoint;
						geom = this.FromRectangleDToSqlGeometry(shapeMultiPoint.BoundingBox);

						break;

					#endregion MultiPoint

					#region Null
					case ShapeType.Null:
						return SqlGeometry.Null;
					#endregion Null

					#region Polyline
					case ShapeType.PolyLine:
					case ShapeType.PolyLineM:
					case ShapeType.PolyLineZ:


						ShapePolyLine shapePolyline = shape as ShapePolyLine;
						geom = this.FromRectangleDToSqlGeometry(shapePolyline.BoundingBox);


						break;
					#endregion Polyline

					#region Polygon
					case ShapeType.Polygon:
					case ShapeType.PolygonM:
					case ShapeType.PolygonZ:

						ShapePolygon shapePolygon = shape as ShapePolygon;
						geom = this.FromRectangleDToSqlGeometry(shapePolygon.BoundingBox);
						break;

					#endregion Polygon

					default:
						throw new NotImplementedException("Shape type " + shape.Type.ToString() + " is not implemented in " + this.GetType().Name);
				}

			}
			catch (Exception ex)
			{
				throw new Exception("Error reading ReadShapeBBox: " + ex.Message, ex);
			}

			return geom;
		}

		private SqlGeometry FromRectangleDToSqlGeometry(RectangleD rect)
		{
			SqlGeometryBuilder geom = new SqlGeometryBuilder();
			geom.SetSrid(0);
			geom.BeginGeometry(OpenGisGeometryType.Polygon);
			geom.BeginFigure(rect.Left, rect.Top);
			geom.AddLine(rect.Left, rect.Bottom);
			geom.AddLine(rect.Right, rect.Bottom);
			geom.AddLine(rect.Right, rect.Top);
			geom.AddLine(rect.Left, rect.Top);
			geom.EndFigure();
			geom.EndGeometry();
			return geom.ConstructedGeometry;
		}

		#endregion Shape BBOX

		#region Shape geometry

		private SqlGeometry ReadShapeGeometry(Shape shape, GeometryReprojector db2ScreenConverter)
		{
			SqlGeometry geom = null;
			SqlGeometryBuilder gb = new SqlGeometryBuilder();
			gb.SetSrid(OUTPUT_SRID);

			try
			{
				// cast shape based on the type
				switch (shape.Type)
				{
					#region Point
					case ShapeType.Point:
					case ShapeType.PointM:
					case ShapeType.PointZ:
						// a point is just a single x/y point
						ShapePoint shapePoint = shape as ShapePoint;

						gb.BeginGeometry(OpenGisGeometryType.Point);
						double[] point = this.ConvertPoint(shapePoint.Point, db2ScreenConverter);
						gb.BeginFigure(point[0], point[1]);
						gb.EndFigure();
						gb.EndGeometry();

						break;
					#endregion Point

					#region MultiPoint
					case ShapeType.MultiPoint:
					case ShapeType.MultiPointM:
					case ShapeType.MultiPointZ:

						ShapeMultiPoint shapeMultiPoint = shape as ShapeMultiPoint;
						gb.BeginGeometry(OpenGisGeometryType.MultiPoint);
						foreach (PointD curPoint in shapeMultiPoint.Points)
						{
							gb.BeginGeometry(OpenGisGeometryType.Point);
							double[] outPoint = this.ConvertPoint(curPoint, db2ScreenConverter);
							gb.BeginFigure(outPoint[0], outPoint[1]);
							gb.EndFigure();
							gb.EndGeometry();
						}
						gb.EndGeometry();

						break;

					#endregion MultiPoint

					#region Null
					case ShapeType.Null:
						return null;
					#endregion Null

					#region Polyline
					case ShapeType.PolyLine:
					case ShapeType.PolyLineM:
					case ShapeType.PolyLineZ:


						ShapePolyLine shapePolyline = shape as ShapePolyLine;
						bool isMultiPolyline = shapePolyline.Parts.Count > 1;
						if (isMultiPolyline)
							gb.BeginGeometry(OpenGisGeometryType.MultiLineString);

						foreach (PointD[] curPart in shapePolyline.Parts)
						{
							gb.BeginGeometry(OpenGisGeometryType.LineString);
							bool isFirstPoint = true;
							foreach (PointD curPoint in curPart)
							{
								double[] outPoint = this.ConvertPoint(curPoint, db2ScreenConverter);
								if (isFirstPoint)
								{
									gb.BeginFigure(outPoint[0], outPoint[1]);
									isFirstPoint = false;
								}
								else
									gb.AddLine(outPoint[0], outPoint[1]);
							}
							gb.EndFigure();
							gb.EndGeometry();

						}

						if (isMultiPolyline)
							gb.EndGeometry();

						break;
					#endregion Polyline

					#region Polygon
					case ShapeType.Polygon:
					case ShapeType.PolygonM:
					case ShapeType.PolygonZ:

						// a polygon contains one or more parts - each part is a list of points which
						// are clockwise for boundaries and anti-clockwise for holes 
						// see http://www.esri.com/library/whitepapers/pdfs/shapefile.pdf
						ShapePolygon shapePolygon = shape as ShapePolygon;
						gb.BeginGeometry(OpenGisGeometryType.Polygon);
						foreach (PointD[] part in shapePolygon.Parts)
						{
							SqlSpatialTools.enRingOrientation orientation = SqlSpatialTools.GetRingOrientation(part.Select(p => new Coordinate(p.X, p.Y)).ToArray());

							bool isFirstPoint = true;
							foreach (PointD curPoint in part)
							{
								double[] outPoint = this.ConvertPoint(curPoint, db2ScreenConverter);
								if (isFirstPoint)
								{
									gb.BeginFigure(outPoint[0], outPoint[1]);
									isFirstPoint = false;
								}
								else
									gb.AddLine(outPoint[0], outPoint[1]);
							}
							gb.EndFigure();
						}

						gb.EndGeometry();
						break;

					#endregion Polygon

					default:
						throw new NotImplementedException("Shape type " + shape.Type.ToString() + " is not implemented in " + this.GetType().Name);
				}

				geom = gb.ConstructedGeometry;
				if (!geom.STIsValid().Value)
					geom = geom.MakeValid();
			}
			catch (Exception)
			{
				throw;
			}

			return geom;
		}

		private double[] ConvertPoint(PointD shapePoint, GeometryReprojector db2ScreenConverter)
		{
			return db2ScreenConverter.ConvertPoint(new double[] { shapePoint.X, shapePoint.Y });
		}

		#endregion Shape geometry

		public override void Cancel()
		{
			if (!_cancel)
			{
				_cancel = true;
				DebugHelper.WriteLine(this, "ShapeFileFeatureService cancelled");
			}
		}



	}
}

