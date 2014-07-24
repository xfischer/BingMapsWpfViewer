using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BingMapsWPFViewer.Model.Services;
using BingMapsWPFViewer.Model;
using BingMapsWPFViewer.Tools;
using BingMapsWPFViewer.Model.Features;
using System.IO;
using BingMapsWPFViewer.Tools.Geometry;
using System.Diagnostics;
using Microsoft.SqlServer.Types;
using BingMapsWPFViewer.Tools.Geometry.SqlSpatial;
using NetTopologySuite.IO;
using NetTopologySuite.Geometries;
using GeoAPI.Geometries;
using System.Data.SqlTypes;

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


				using (ShapefileDataReader reader = new ShapefileDataReader(layer.ShapeFileName, new GeometryFactory()))
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

					featureFields = this.BuildFeatureFieldsFromDbaseHeader(reader.DbaseHeader);

					SqlGeometry bbox = criterion.BoundingBox.ToSqlGeometry();
					bbox = screen2fileConverter.ConvertSqlGeometry2Geometry(bbox);
					List<Coordinate> coords = new List<Coordinate>();
					for (int i = 1; i <= bbox.STNumPoints().Value; i++)
					{
						coords.Add(new Coordinate(bbox.STPointN(i).STX.Value, bbox.STPointN(i).STY.Value));
					}

					double maxX = coords.Max(c=> c.X);
					double maxY = coords.Max(c=> c.Y);
					double minX = coords.Min(c=> c.X);
					double minY = coords.Min(c=> c.Y);

					Envelope env = new Envelope(minX, maxX, minY, maxY);				


					// enumerate all shapes
					while (reader.Read())
					{
						if (_cancel)
							break;

						Feature feature = this.ReadShape(reader, file2ScreenConverter, featureFields, stopwatchConvert, bbox,env, criterion.MapViewport.MapResolution);
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

		private List<FeatureField> BuildFeatureFieldsFromDbaseHeader(DbaseFileHeader dbaseHeader)
		{
			List<FeatureField> fields = new List<FeatureField>();
			try
			{

				int ordinal = 1;
				foreach (var fld in dbaseHeader.Fields)
				{
					FeatureField field = new FeatureField(fld.Name, fld.Type.Name, ordinal, enFeatureFieldUsage.None);
					fields.Add(field);
				}
			}
			catch (Exception)
			{
				throw;
			}
			return fields;
		}

		private SqlGeometry ConvertGeoAPIToSql(IGeometry geometry)
		{
			try
			{
				SqlBytes bytes = new SqlBytes(geometry.AsBinary());
				SqlGeometry sg = SqlGeometry.STGeomFromWKB(bytes, 0);
				if (sg.STIsValid().IsFalse)
				{
					sg = sg.MakeValid();
				}
				return sg;
			}
			catch (Exception)
			{
				
				throw;
			}		

		}

		private Feature ReadShape(ShapefileDataReader reader
															, GeometryReprojector db2ScreenConverter		// coordinate and geometry converter
															, List<FeatureField> attributeFields			// list of attributes columns to retrieve
															, Stopwatch queryConvertStopWatch					// stop watch use to measure convert/reproject performance
															, SqlGeometry queryBbox										// input bbox (spatial filter)
															, Envelope queryBboxEnvelope
															, double mapResolution										// tolerance for geometry reduction
															)
		{
			Feature feature = null;
			try
			{
				queryConvertStopWatch.Start();

				Envelope shapeBbox = reader.ShapeHeader.Bounds;
				

				queryConvertStopWatch.Stop();
				

				if (shapeBbox.Intersects(queryBboxEnvelope))
				{
					SqlGeometry nativeGeom = this.ConvertGeoAPIToSql(reader.Geometry);
					SqlGeometry geom = db2ScreenConverter.ConvertSqlGeometry2Geometry(nativeGeom);					

					// simplify geometry
					geom = SqlSpatialTools.SimplifyGeometry(geom, mapResolution);

					Dictionary<FeatureField, object> attributes = new Dictionary<FeatureField, object>();
					foreach (FeatureField attributeField in attributeFields)
						attributes.Add(attributeField, reader[attributeField.FieldName]);

					feature = new Feature(geom, null, attributes);
				}

				//queryConvertStopWatch.Stop();
			}
			catch (Exception ex)
			{
				throw new Exception("Error reading feature: " + ex.Message, ex);
			}
			return feature;
		}


		private SqlGeometry ConvertEnvelopeToSqlGeometry(Envelope envelope)
		{
			SqlGeometryBuilder b = new SqlGeometryBuilder();
			b.SetSrid(0);
			b.BeginGeometry(OpenGisGeometryType.Polygon);
			b.BeginFigure(envelope.MinX, envelope.MinY);
			b.AddLine(envelope.MaxX, envelope.MinY);
			b.AddLine(envelope.MaxX, envelope.MaxY);
			b.AddLine(envelope.MinX, envelope.MaxY);
			b.AddLine(envelope.MinX, envelope.MinY);
			b.EndFigure();
			b.EndGeometry();

			SqlGeometry geom = b.ConstructedGeometry;
			if (geom.STIsValid().IsTrue)
				return geom;
			else
				return geom.MakeValid();


		}

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

