using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Diagnostics;
using BingMapsWPFViewer.Framework;
using BingMapsWPFViewer.Model;
using BingMapsWPFViewer.Services;
using BingMapsWPFViewer.Model.Features;
using BingMapsWPFViewer.Model.Services;
using BingMapsWPFViewer.Tools;
using BingMapsWPFViewer.Tools.Geometry;
using Microsoft.SqlServer.Types;
using System.IO;
using BingMapsWPFViewer.Tools.Geometry.SqlSpatial;

namespace BingMapsWPFViewer.Services
{
	public sealed class SqlServerFeatureService
		: FeatureServiceBase<SqlServerLayer, SpatialQueryCriterion>
	{

		private const int OUTPUT_SRID = 4326;
		private bool _cancel = false;
		private SqlCommand _command = null;

		public override List<Feature> LoadConcrete(SqlServerLayer layer, SpatialQueryCriterion criterion)
		{
			// return list
			List<Feature> featureList = new List<Feature>();

			// get attribute fields
			List<FeatureField> attributeFields = FeatureFieldHelper.GetQueryAttributeFields(criterion.FieldsToRetrieve);
			FeatureField geomField = FeatureFieldHelper.GetQuerySpatialField(criterion.FieldsToRetrieve);
			SqlGeometry bboxGeom = criterion.BoundingBox.ToSqlGeometry();

			// converters 
			GeometryReprojector db2ScreenConverter = new GeometryReprojector(layer.SRID, OUTPUT_SRID); // converts results fetched from db
			GeometryReprojector screen2DbConverter = new GeometryReprojector(OUTPUT_SRID, layer.SRID); // converts bbox to db table srid

			Stopwatch stopwatchQuery = new Stopwatch();
			Stopwatch stopwatchConvert = new Stopwatch();

			if (_cancel)
				return featureList;

			using (SqlConnection con = new SqlConnection(layer.ConnectionString))
			{
				long execReaderTime = 0;

				try
				{
					con.Open();

					#region build Sql Command
					string v_sqlQuery = "SELECT TOP {0} {1} --.Reduce(@mapResolution)" + Environment.NewLine
																+ " FROM {2} {3}" + Environment.NewLine
																+ " WHERE [{4}].STIntersects(@bbox) = 1" + Environment.NewLine
																+ " {5}";

					#region Force index
					string v_usingIndexClause = null;
					if (!string.IsNullOrEmpty(layer.SpatialIndexName))
						v_usingIndexClause = "WITH(INDEX(" + layer.SpatialIndexName + "))";
					#endregion Force index

					#region Build query text

					// Column names
					string v_colNames = FeatureFieldHelper.BuildQueryColumnNamesPart(criterion.FieldsToRetrieve);

					// filters
					string v_filters = FeatureFieldHelper.BuildQueryFilterPart(criterion.QueryFilter);

					// build query string
					v_sqlQuery = string.Format(v_sqlQuery, criterion.SizeLimit, v_colNames, layer.TableName, v_usingIndexClause, geomField.FieldName, v_filters);


					#endregion Build query text

					#region Init command and parameters
					_command = new SqlCommand(v_sqlQuery, con);
					SqlParameter bboxParam = _command.Parameters.Add("@bbox", SqlDbType.Udt);
					if (layer.IsSqlGeography)
					{
						bboxParam.UdtTypeName = "geography";
						bboxParam.Value = screen2DbConverter.ConvertSqlGeometry2Geography(bboxGeom);
					}
					else
					{
						bboxParam.UdtTypeName = "geometry";
						bboxParam.Value = screen2DbConverter.ConvertSqlGeometry2Geometry(bboxGeom);
					}
					_command.Parameters.AddWithValue("@mapResolution", criterion.MapViewport.MapResolution);
					#endregion Init command and parameters

					#endregion build Sql Command

					if (_cancel)
						return featureList;

					DebugHelper.WriteLine(this, string.Format("Querying {0}... (bbox: {1})", layer.TableName, bboxParam.Value));

					#region sync query
					stopwatchQuery.Start();
					using (SqlDataReader reader = _command.ExecuteReader())
					{
						execReaderTime = stopwatchQuery.ElapsedMilliseconds;

						while (!_cancel && reader.Read())
						{

							// Read feature, convert/reproject geometry and read attributes
							Feature feature = this.ReadFeature(reader, db2ScreenConverter, layer.IsSqlGeography, attributeFields, stopwatchQuery, stopwatchConvert, criterion.MapViewport.MapResolution);
							featureList.Add(feature);

						}

						if (!_cancel)
							_command.Cancel();
					}
					_command.Dispose();
					_command = null;

					#endregion sync query


				}
				catch (SqlException exSql)
				{
					throw exSql;
				}
				catch (Exception ex)
				{
					throw ex;
				}
				finally
				{
					DebugHelper.WriteLine(this, string.Format("ExecuteReader = {0}, Fetch = {1} ms, convert = {2} ms, {3} features", execReaderTime, stopwatchQuery.ElapsedMilliseconds, stopwatchConvert.ElapsedMilliseconds, featureList.Count));
				}
			}



			return featureList;
		}

		private Feature ReadFeature(SqlDataReader reader											// data reader from which results will be fetched
																			, GeometryReprojector db2ScreenConverter		// coordinate and geometry converter
																			, bool isGeography												// true if spatial data is SqlGeography, false if SqlGeometry
																			, List<FeatureField> attributeFields			// list of attributes columns to retrieve
																			, Stopwatch queryFetchStopWatch						// stop watch use to measure fetch performance
																			, Stopwatch queryConvertStopWatch					// stop watch use to measure convert/reproject performance
																			, double mapResolution										// tolerance for geometry reduction
																			)
		{
			Feature feature = null;
			try
			{
				SqlGeometry v_geom = null;

				using (BinaryReader binReader = new BinaryReader(reader.GetSqlBytes(0).Stream))
				{
					queryFetchStopWatch.Stop();
					queryConvertStopWatch.Start();


					if (isGeography)
					{
						SqlGeography geog = new SqlGeography();
						geog.Read(binReader);
						v_geom = db2ScreenConverter.ConvertSqlGeography2Geometry(geog);
					}
					else
					{
						SqlGeometry geom = new SqlGeometry();
						geom.Read(binReader);
						v_geom = db2ScreenConverter.ConvertSqlGeometry2Geometry(geom);
					}
				}

				// simplify geometry
				v_geom = SqlSpatialTools.SimplifyGeometry(v_geom, mapResolution);

				Dictionary<FeatureField, object> attributes = new Dictionary<FeatureField, object>();
				foreach (FeatureField attributeField in attributeFields)
					attributes.Add(attributeField, reader[attributeField.FieldName]);

				feature = new Feature(v_geom, null, attributes);

				queryConvertStopWatch.Stop();
				queryFetchStopWatch.Start();
			}
			catch (Exception ex)
			{
				throw new Exception("Error reading feature: " + ex.Message, ex);
			}
			return feature;
		}





		public override void Cancel()
		{
			if (!_cancel)
			{
				_cancel = true;
				if (_command != null)
				{
					DebugHelper.WriteLine(this, "Command cancelled");
					_command.Cancel();
				}
			}
		}
	}
}
