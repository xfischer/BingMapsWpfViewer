using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using BingMapsWPFViewer.Model.SqlInfoSchema;
using BingMapsWPFViewer.Tools;

namespace BingMapsWPFViewer.Model.Services
{
	public class SqlServerInfoSchemaService : ISqlServerInfoSchemaService
	{
		#region Info schema

		List<SqlTable> ISqlServerInfoSchemaService.GetSqlTables(string connectionString)
		{
			using (SqlConnection con = new SqlConnection(connectionString))
			{
				con.Open();

				return ((ISqlServerInfoSchemaService)this).GetSqlTables(con);
			}
		}

		List<SqlTable> ISqlServerInfoSchemaService.GetSqlTables(SqlConnection sqlOpenedConnection)
		{
			List<SqlTable> v_retList = null;

			#region Information schema query
			string v_SqlQuery = @"SELECT 
																C.TABLE_CATALOG
																,C.TABLE_SCHEMA
																,C.TABLE_NAME
																,T.TABLE_TYPE
																,C.COLUMN_NAME
																,TC.CONSTRAINT_TYPE
																,C.COLUMN_DEFAULT
																,C.IS_NULLABLE
																,C.DATA_TYPE
																,C.CHARACTER_MAXIMUM_LENGTH
																,C.NUMERIC_PRECISION
																,C.NUMERIC_PRECISION_RADIX
																,C.NUMERIC_SCALE
																,C.ORDINAL_POSITION
															FROM INFORMATION_SCHEMA.TABLES T
															INNER JOIN INFORMATION_SCHEMA.COLUMNS C
																ON C.TABLE_CATALOG = T.TABLE_CATALOG
																AND C.TABLE_SCHEMA = T.TABLE_SCHEMA
																AND C.TABLE_NAME = T.TABLE_NAME
															LEFT JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE KC
																ON KC.TABLE_CATALOG = C.TABLE_CATALOG
																AND KC.TABLE_SCHEMA = C.TABLE_SCHEMA
																AND KC.TABLE_NAME = C.TABLE_NAME
																AND KC.COLUMN_NAME = C.COLUMN_NAME
															LEFT JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS TC
																ON TC.TABLE_CATALOG = C.TABLE_CATALOG
																AND TC.TABLE_SCHEMA = C.TABLE_SCHEMA
																AND TC.TABLE_NAME = C.TABLE_NAME
																AND TC.CONSTRAINT_NAME = KC.CONSTRAINT_NAME
															ORDER BY C.TABLE_CATALOG
																	 ,C.TABLE_SCHEMA
																	 ,C.TABLE_NAME
																	 ,C.ORDINAL_POSITION";
			#endregion Information schema query

			Dictionary<string, SqlTable> v_workList = new Dictionary<string, SqlTable>();

			SqlCommand v_com = new SqlCommand(v_SqlQuery, sqlOpenedConnection);
			v_com.CommandTimeout = 3 * 60; // 3 min
			using (SqlDataReader v_reader = v_com.ExecuteReader())
			{

				while (v_reader.Read())
				{
					string v_catalog = v_reader["TABLE_CATALOG"].ToString();
					string v_schema = v_reader["TABLE_SCHEMA"].ToString();
					string v_tableName = v_reader["TABLE_NAME"].ToString();
					string v_tableType = v_reader["TABLE_TYPE"].ToString();
					string v_colName = v_reader["COLUMN_NAME"].ToString();
					enSqlConstraintType? v_constraintType = this.ParseenSqlConstraintType(v_reader["CONSTRAINT_TYPE"]);
					object v_colDefault = v_reader["COLUMN_DEFAULT"];
					string v_isNullable = v_reader["IS_NULLABLE"].ToString();
					string v_DataType = v_reader["DATA_TYPE"].ToString();
					object v_maxCarLength = v_reader["CHARACTER_MAXIMUM_LENGTH"];
					object v_numericPrecision = v_reader["NUMERIC_PRECISION"];
					object v_numPrecisionRadix = v_reader["NUMERIC_PRECISION_RADIX"];
					object v_numericScale = v_reader["NUMERIC_SCALE"];
					object v_ordinal = v_reader["ORDINAL_POSITION"];

					#region Create table on first pass
					string v_fullTableName = v_catalog + "." + v_schema + "." + v_tableName;
					SqlTable v_curTable = null;
					if (!v_workList.ContainsKey(v_fullTableName))
					{
						v_curTable = new SqlTable();
						v_curTable.CatalogName = v_catalog;
						v_curTable.IsView = v_tableType == "VIEW";
						v_curTable.TableName = v_tableName;
						v_curTable.SchemaName = v_schema;

						v_workList.Add(v_fullTableName, v_curTable);
					}
					#endregion

					v_curTable = v_workList[v_fullTableName];

					SqlColumn v_col = new SqlColumn();
					v_col.ColumnName = v_colName;
					v_col.CatalogName = v_catalog;
					v_col.TableName = v_tableName;
					v_col.SchemaName = v_schema;
					v_col.ConstraintType = v_constraintType ?? enSqlConstraintType.None;
					v_col.DataType = v_DataType;
					v_col.DefaultValue = v_colDefault;
					v_col.IsNullable = v_isNullable == "YES";
					v_col.MaximumCaraterLength = this.GetValueOrNull<int>(v_maxCarLength);
					v_col.NumericPrecision = (int?)this.GetValueOrNull<byte>(v_numericPrecision);
					v_col.NumericPrecisionRadix = (int?)this.GetValueOrNull<Int16>(v_numPrecisionRadix);
					v_col.NumericScale = this.GetValueOrNull<int>(v_numericScale);
					v_col.OrdinalPosition = (int)v_ordinal;

					v_curTable.Columns.Add(v_col);

				}


				v_retList = v_workList.Values.ToList();
			}



			return v_retList;
		}

		List<SqlColumn> ISqlServerInfoSchemaService.GetSqlTableColumns(SqlTable sqlTable)
		{
			List<SqlColumn> v_ret = new List<SqlColumn>();

			if (sqlTable != null && sqlTable.Columns != null)
			{
				foreach (SqlColumn v_col in sqlTable.Columns)
				{
					v_ret.Add(v_col.Clone());
				}
			}

			return v_ret;
		}

		List<SqlColumn> ISqlServerInfoSchemaService.GetSqlTablePrimaryKey(SqlTable sqlTable)
		{
			throw new NotImplementedException();
		}

		List<SqlColumn> ISqlServerInfoSchemaService.GePrimaryKeyColumns(List<SqlColumn> sqlColumns)
		{
			throw new NotImplementedException();
		}

		List<SqlColumn> ISqlServerInfoSchemaService.GetSpatialColumns(SqlTable sqlTable)
		{
			List<SqlColumn> v_ret = new List<SqlColumn>();

			if (sqlTable != null && sqlTable.Columns != null)
			{
				foreach (SqlColumn v_col in sqlTable.Columns)
				{
					if (v_col.DataType == "geometry" || v_col.DataType == "geography")
						v_ret.Add(v_col.Clone());
				}
			}

			return v_ret;
		}

		int ISqlServerInfoSchemaService.GetSpatialColumnSrid(SqlColumn p_Column, string connectionString)
		{
			using (SqlConnection con = new SqlConnection(connectionString))
			{
				con.Open();

				return ((ISqlServerInfoSchemaService)this).GetSpatialColumnSrid(p_Column, con);
			}
		}

		int ISqlServerInfoSchemaService.GetSpatialColumnSrid(SqlColumn p_Column, SqlConnection sqlOpenedConnection)
		{
			int srid = 0;
			try
			{
				if (p_Column == null)
					throw new ArgumentNullException("Column is null");
				else
				{
					string sridQuery = string.Format("SELECT TOP 1 {0}.STSrid FROM {1}.{2} WHERE {0} IS NOT NULL", p_Column.ColumnName, p_Column.SchemaName, p_Column.TableName);
					SqlCommand cmd = new SqlCommand(sridQuery, sqlOpenedConnection);
					object sridObj = cmd.ExecuteScalar();
					int.TryParse(sridObj.ToString(), out srid);
					
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}

			return srid;
		}

		#endregion

		#region Index


		/// <summary>
		/// Returns all relevant information on table indexes
		/// </summary>
		/// <param name="p_SchemaName"></param>
		/// <param name="p_TableName"></param>
		/// <returns></returns>
		List<SqlIndex> ISqlServerInfoSchemaService.GetTableIndexes(SqlConnection p_SqlOpenedConnection, string p_SchemaName, string p_TableName)
		{
			Dictionary<string, SqlIndex> v_workList = new Dictionary<string, SqlIndex>();

			try
			{

				#region Index info query
				string v_SqlQuery = @"SELECT
																T.name						AS TABLE_NAME
																, T.object_id			as TABLE_ID
																, C.name					AS COLUMN_NAME
																, SI.name					AS INDEX_NAME
																, SI.index_id
																, SI.type_desc		AS INDEX_TYPE
																, IC.index_column_id
																, IC.is_included_column
																, SIT.tessellation_scheme
																, SIT.bounding_box_xmin, SIT.bounding_box_ymin, SIT.bounding_box_xmax, SIT.bounding_box_ymax
																, SIT.level_1_grid_desc, SIT.level_2_grid_desc, SIT.level_3_grid_desc, SIT.level_4_grid_desc
																from sys.schemas S
																inner join sys.tables T
																	on T.schema_id = s.schema_id
																inner join sys.indexes SI
																	ON SI.object_id = T.object_id
																inner join sys.index_columns IC
																	on IC.index_id = SI.index_id
																	and IC.object_id = T.object_id 
																inner join sys.columns C
																	ON C.object_id = T.object_id
																	AND C.column_id = IC.column_id
																left join sys.spatial_index_tessellations SIT
																	on SIT.index_id = SI.index_id
																	and SIT.object_id = T.object_id
															WHERE	s.name = @SchemaName
																			and T.name = @TableName
															ORDER BY T.object_id, SI.index_id, IC.index_column_id";
				#endregion  Index info query

				SqlCommand v_com = new SqlCommand(v_SqlQuery, p_SqlOpenedConnection);
				v_com.Parameters.AddWithValue("@SchemaName", p_SchemaName);
				v_com.Parameters.AddWithValue("@TableName", p_TableName);
				using (SqlDataReader v_reader = v_com.ExecuteReader())
				{
					while (v_reader.Read())
					{
						string v_tableName = v_reader["TABLE_NAME"].ToString();
						int v_objectId = int.Parse(v_reader["TABLE_ID"].ToString());
						string v_colName = v_reader["COLUMN_NAME"].ToString();
						string v_indexName = v_reader["INDEX_NAME"].ToString();
						int v_indexId = int.Parse(v_reader["index_id"].ToString());
						string v_indexType = v_reader["INDEX_TYPE"].ToString();
						int v_indexColId = int.Parse(v_reader["index_column_id"].ToString());
						bool v_isIncludedColumn = (bool)v_reader["is_included_column"];
						object v_tesselation = v_reader["tessellation_scheme"];

						bool v_isSpatial = (v_tesselation != DBNull.Value);

						SqlIndex v_curIndex = null;
						if (!v_workList.ContainsKey(v_indexName))
						{
							if (v_isSpatial)
								v_workList[v_indexName] = new SqlSpatialIndex();
							else
								v_workList[v_indexName] = new SqlIndex();
						}
						v_curIndex = v_workList[v_indexName];

						v_curIndex.Internal_IndexId = v_indexId;
						v_curIndex.Internal_ObjectId = v_objectId;
						v_curIndex.IndexType = (enSqlIndexType)Enum.Parse(typeof(enSqlIndexType), v_indexType);
						v_curIndex.IsSpatialIndex = v_isSpatial;
						v_curIndex.Name = v_indexName;
						v_curIndex.SchemaName = p_SchemaName;
						v_curIndex.TableName = v_tableName;

						if (v_isIncludedColumn)
							v_curIndex.IncludedColumns.Add(v_colName);
						else
							v_curIndex.KeyColumns.Add(v_colName);

						#region Spatial index specific
						if (v_isSpatial)
						{
							SqlSpatialIndex v_spatialIndex = (SqlSpatialIndex)v_curIndex;

							string v_tesselationScheme = v_tesselation.ToString();
							enGridLevel level_1_grid_desc = (enGridLevel)Enum.Parse(typeof(enGridLevel), v_reader["level_1_grid_desc"].ToString());
							enGridLevel level_2_grid_desc = (enGridLevel)Enum.Parse(typeof(enGridLevel), v_reader["level_2_grid_desc"].ToString());
							enGridLevel level_3_grid_desc = (enGridLevel)Enum.Parse(typeof(enGridLevel), v_reader["level_3_grid_desc"].ToString());
							enGridLevel level_4_grid_desc = (enGridLevel)Enum.Parse(typeof(enGridLevel), v_reader["level_4_grid_desc"].ToString());

							v_spatialIndex.GridSizeLevel1 = level_1_grid_desc;
							v_spatialIndex.GridSizeLevel2 = level_2_grid_desc;
							v_spatialIndex.GridSizeLevel3 = level_3_grid_desc;
							v_spatialIndex.GridSizeLevel4 = level_4_grid_desc;

							if (v_tesselationScheme == "GEOMETRY_GRID")
							{
								double v_xmin = (double)v_reader["bounding_box_xmin"];
								double v_ymin = (double)v_reader["bounding_box_ymin"];
								double v_xmax = (double)v_reader["bounding_box_xmax"];
								double v_ymax = (double)v_reader["bounding_box_ymax"];

								v_spatialIndex.BoundingBox = new BoundingBox(v_xmin, v_ymin, v_xmax, v_ymax);
								v_spatialIndex.IsGeography = false;
							}
							else
							{
								v_spatialIndex.BoundingBox = null;
								v_spatialIndex.IsGeography = true;
							}
						}
						#endregion Spatial index specific

						v_workList[v_indexName] = v_curIndex;

					}

				}
			}
			catch (Exception)
			{
				throw;
			}

			return v_workList.Values.ToList();
		}
		List<SqlIndex> ISqlServerInfoSchemaService.GetTableIndexes(string connectionString, string p_SchemaName, string p_TableName)
		{
			using (SqlConnection con = new SqlConnection(connectionString))
			{
				con.Open();
				return ((ISqlServerInfoSchemaService)this).GetTableIndexes(con, p_SchemaName, p_TableName);
			}
		}

		/// <summary>
		/// Returns index fragmentation in percent
		/// </summary>
		/// <param name="p_Index"></param>
		/// <returns>Null si l'index n'est pas trouvé</returns>
		double? ISqlServerInfoSchemaService.GetIndexFragmentation(SqlConnection p_SqlOpenedConnection, SqlIndex p_Index)
		{
			double? v_frag = null;

			try
			{
				if (p_Index != null)
				{


					#region Spatial index info query
					string v_SqlQuery = "select top 1 avg_fragmentation_in_percent from sys.dm_db_index_physical_stats(DB_ID(), @object_id, @index_id, NULL, 'LIMITED')";
					#endregion Spatial index info query

					SqlCommand v_com = new SqlCommand(v_SqlQuery, p_SqlOpenedConnection);
					v_com.Parameters.AddWithValue("@object_id", p_Index.Internal_ObjectId);
					v_com.Parameters.AddWithValue("@index_id", p_Index.Internal_IndexId);
					object v_fragObj = v_com.ExecuteScalar();
					if (v_fragObj != null)
						v_frag = v_fragObj as double?;

				}
			}
			catch (Exception v_ex)
			{
				throw v_ex;
			}

			return v_frag;
		}
		double? ISqlServerInfoSchemaService.GetIndexFragmentation(string connectionString, SqlIndex p_Index)
		{
			using (SqlConnection con = new SqlConnection(connectionString))
			{
				con.Open();
				return ((ISqlServerInfoSchemaService)this).GetIndexFragmentation(con, p_Index);
			}
		}

		#region CreateSpatialIndex
		SqlSpatialIndex ISqlServerInfoSchemaService.CreateSpatialIndexGeometry_InDB(SqlConnection p_SqlOpenedConnection, string p_SchemaName, string p_TableName, string p_SpatialColumnName
																														, string p_IndexName, BoundingBox p_BoundingBox, enGridLevel p_GridSizeLevel1, enGridLevel p_GridSizeLevel2, enGridLevel p_GridSizeLevel3, enGridLevel p_GridSizeLevel4)
		{
			if (string.IsNullOrEmpty(p_SchemaName))
				throw new ArgumentNullException("p_SchemaName");
			if (string.IsNullOrEmpty(p_TableName))
				throw new ArgumentNullException("p_TableName");
			if (string.IsNullOrEmpty(p_SpatialColumnName))
				throw new ArgumentNullException("p_SpatialColumnName");
			if (string.IsNullOrEmpty(p_IndexName))
				throw new ArgumentNullException("p_IndexName");
			if (p_BoundingBox == null)
				throw new ArgumentNullException("p_BoundingBox");


			SqlSpatialIndex v_ret = null;

			try
			{



				// 0 : index name
				// 1 : schema
				// 2 : table name
				// 3 : column name
				// 4 - 7 : grid level
				string v_sqlQuery = @"CREATE SPATIAL INDEX [{0}] ON [{1}].[{2}] 
														(
															[{3}]
														) USING  GEOMETRY_GRID 
														WITH (
														BOUNDING_BOX =(@xmin, @ymin, @xmax, @ymax), GRIDS =(LEVEL_1 = {4},LEVEL_2 = {5},LEVEL_3 = {6},LEVEL_4 = {7}), 
														CELLS_PER_OBJECT = 16, PAD_INDEX  = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)";
				v_sqlQuery = string.Format(v_sqlQuery, p_IndexName, p_SchemaName, p_TableName, p_SpatialColumnName, p_GridSizeLevel1.ToString(), p_GridSizeLevel2.ToString(), p_GridSizeLevel3.ToString(), p_GridSizeLevel4.ToString());


				v_sqlQuery = v_sqlQuery.Replace("@xmin", p_BoundingBox.XMin.ToString().Replace(",", "."));
				v_sqlQuery = v_sqlQuery.Replace("@ymin", p_BoundingBox.YMin.ToString().Replace(",", "."));
				v_sqlQuery = v_sqlQuery.Replace("@xmax", p_BoundingBox.XMax.ToString().Replace(",", "."));
				v_sqlQuery = v_sqlQuery.Replace("@ymax", p_BoundingBox.YMax.ToString().Replace(",", "."));

				SqlCommand v_com = new SqlCommand(v_sqlQuery, p_SqlOpenedConnection);
				v_com.CommandTimeout = 60 * 60; // 60 min
				v_com.ExecuteNonQuery();

				v_ret = ((ISqlServerInfoSchemaService)this).GetTableIndexes(p_SqlOpenedConnection, p_SchemaName, p_TableName)
															.Where(i => i.Name == p_IndexName && i.TableName == p_TableName && i.SchemaName == p_SchemaName)
															.FirstOrDefault() as SqlSpatialIndex;


			}
			catch (Exception v_ex)
			{
				throw v_ex;
			}

			return v_ret;

		}
		SqlSpatialIndex ISqlServerInfoSchemaService.CreateSpatialIndexGeometry_InDB(string connectionString, string p_SchemaName, string p_TableName, string p_SpatialColumnName
																														, string p_IndexName, BoundingBox p_BoundingBox, enGridLevel p_GridSizeLevel1, enGridLevel p_GridSizeLevel2, enGridLevel p_GridSizeLevel3, enGridLevel p_GridSizeLevel4)
		{
			using (SqlConnection con = new SqlConnection(connectionString))
			{
				con.Open();
				return ((ISqlServerInfoSchemaService)this).CreateSpatialIndexGeometry_InDB(con, p_SchemaName, p_TableName, p_SpatialColumnName
																														, p_IndexName, p_BoundingBox, p_GridSizeLevel1, p_GridSizeLevel2, p_GridSizeLevel3, p_GridSizeLevel4);
			}
		}




		SqlSpatialIndex ISqlServerInfoSchemaService.CreateSpatialIndexGeography_InDB(SqlConnection p_SqlOpenedConnection, string p_SchemaName, string p_TableName, string p_SpatialColumnName
																													, string p_IndexName, enGridLevel p_GridSizeLevel1, enGridLevel p_GridSizeLevel2, enGridLevel p_GridSizeLevel3, enGridLevel p_GridSizeLevel4)
		{
			if (string.IsNullOrEmpty(p_SchemaName))
				throw new ArgumentNullException("p_SchemaName");
			if (string.IsNullOrEmpty(p_TableName))
				throw new ArgumentNullException("p_TableName");
			if (string.IsNullOrEmpty(p_SpatialColumnName))
				throw new ArgumentNullException("p_SpatialColumnName");
			if (string.IsNullOrEmpty(p_IndexName))
				throw new ArgumentNullException("p_IndexName");


			SqlSpatialIndex v_ret = null;

			try
			{

				// 0 : index name
				// 1 : schema
				// 2 : table name
				// 3 : column name
				// 4 - 7 : grid level
				string v_sqlQuery = @"CREATE SPATIAL INDEX [{0}] ON [{1}].[{2}] 
														(
															[{3}]
														) USING  GEOGRAPHY_GRID 
														WITH ( GRIDS =(LEVEL_1 = {4},LEVEL_2 = {5},LEVEL_3 = {6},LEVEL_4 = {7}), 
														CELLS_PER_OBJECT = 16, PAD_INDEX  = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)";
				v_sqlQuery = string.Format(v_sqlQuery, p_IndexName, p_SchemaName, p_TableName, p_SpatialColumnName, p_GridSizeLevel1.ToString(), p_GridSizeLevel2.ToString(), p_GridSizeLevel3.ToString(), p_GridSizeLevel4.ToString());

				SqlCommand v_com = new SqlCommand(v_sqlQuery, p_SqlOpenedConnection);
				v_com.CommandTimeout = 60 * 60; // 60 min
				v_com.ExecuteNonQuery();

				v_ret = ((ISqlServerInfoSchemaService)this).GetTableIndexes(p_SqlOpenedConnection, p_SchemaName, p_TableName)
												.Where(i => i.Name == p_IndexName && i.TableName == p_TableName && i.SchemaName == p_SchemaName)
												.FirstOrDefault() as SqlSpatialIndex;


			}
			catch (Exception v_ex)
			{
				throw v_ex;
			}

			return v_ret;

		}
		SqlSpatialIndex ISqlServerInfoSchemaService.CreateSpatialIndexGeography_InDB(string connectionString, string p_SchemaName, string p_TableName, string p_SpatialColumnName
																													, string p_IndexName, enGridLevel p_GridSizeLevel1, enGridLevel p_GridSizeLevel2, enGridLevel p_GridSizeLevel3, enGridLevel p_GridSizeLevel4)
		{
			using (SqlConnection con = new SqlConnection(connectionString))
			{
				con.Open();
				return ((ISqlServerInfoSchemaService)this).CreateSpatialIndexGeography_InDB(con, p_SchemaName, p_TableName, p_SpatialColumnName
																														, p_IndexName, p_GridSizeLevel1, p_GridSizeLevel2, p_GridSizeLevel3, p_GridSizeLevel4);
			}
		}

		#endregion

		#region Drop
		void ISqlServerInfoSchemaService.DropIndex(SqlConnection p_SqlOpenedConnection, SqlIndex p_Index)
		{
			if (p_Index == null)
				return;
			if (p_Index.IndexType == enSqlIndexType.CLUSTERED)
				throw new NotImplementedException("DropIndex is not implemented for clustered indexes"); // TODO for implementation : drop PK constraint

			try
			{


				// 0 : index name
				// 1 : schema
				// 2 : table name
				string v_sqlQuery = "IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = @object_id AND name = @index_name)"
														+ " DROP INDEX [{0}] ON [{1}].[{2}]";
				v_sqlQuery = string.Format(v_sqlQuery, p_Index.Name, p_Index.SchemaName, p_Index.TableName);

				SqlCommand v_com = new SqlCommand(v_sqlQuery, p_SqlOpenedConnection);
				v_com.Parameters.AddWithValue("@object_id", p_Index.Internal_ObjectId);
				v_com.Parameters.AddWithValue("@index_name", p_Index.Name);
				v_com.CommandTimeout = 2 * 60; // 2 min
				v_com.ExecuteNonQuery();

			}
			catch (Exception v_ex)
			{
				throw v_ex;
			}
		}

		void ISqlServerInfoSchemaService.DropIndex(string connectionString, SqlIndex p_Index)
		{
			using (SqlConnection con = new SqlConnection(connectionString))
			{
				con.Open();
				((ISqlServerInfoSchemaService)this).DropIndex(con, p_Index);
			}
		}

		#endregion

		#region Reorganize / Rebuild

		void ISqlServerInfoSchemaService.ReorganizeIndex(SqlConnection p_SqlOpenedConnection, SqlIndex p_Index)
		{
			if (p_Index == null)
				return;

			try
			{
				// 0 : index name
				// 1 : schema
				// 2 : table name
				string v_sqlQuery = "ALTER INDEX [{0}] ON [{1}].[{2}] REORGANIZE";
				v_sqlQuery = string.Format(v_sqlQuery, p_Index.Name, p_Index.SchemaName, p_Index.TableName);

				SqlCommand v_com = new SqlCommand(v_sqlQuery, p_SqlOpenedConnection);
				v_com.CommandTimeout = 10 * 60; // 10 min
				v_com.ExecuteNonQuery();

			}
			catch (Exception v_ex)
			{
				throw v_ex;
			}
		}
		void ISqlServerInfoSchemaService.ReorganizeIndex(string connectionString, SqlIndex p_Index)
		{
			using (SqlConnection con = new SqlConnection(connectionString))
			{
				con.Open();
				((ISqlServerInfoSchemaService)this).ReorganizeIndex(con, p_Index);
			}
		}

		void ISqlServerInfoSchemaService.RebuildIndex(SqlConnection p_SqlOpenedConnection, SqlIndex p_Index)
		{
			if (p_Index == null)
				return;

			try
			{
				// 0 : index name
				// 1 : schema
				// 2 : table name
				string v_sqlQuery = "ALTER INDEX [{0}] ON [{1}].[{2}] REBUILD PARTITION = ALL WITH ( PAD_INDEX  = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, ONLINE = OFF, SORT_IN_TEMPDB = OFF )";
				v_sqlQuery = string.Format(v_sqlQuery, p_Index.Name, p_Index.SchemaName, p_Index.TableName);

				SqlCommand v_com = new SqlCommand(v_sqlQuery, p_SqlOpenedConnection);
				v_com.CommandTimeout = 60 * 60; // 60 min
				v_com.ExecuteNonQuery();

			}
			catch (Exception v_ex)
			{
				throw v_ex;
			}

		}
		void ISqlServerInfoSchemaService.RebuildIndex(string connectionString, SqlIndex p_Index)
		{
			using (SqlConnection con = new SqlConnection(connectionString))
			{
				con.Open();
				((ISqlServerInfoSchemaService)this).RebuildIndex(con, p_Index);
			}
		}

		#endregion

		string ISqlServerInfoSchemaService.CleanupSqlName(string p_Name)
		{
			return System.Text.RegularExpressions.Regex.Replace(p_Name, @"[^\w\.-_@]", "");
		}

		#endregion

		#region Private

		private enSqlConstraintType? ParseenSqlConstraintType(object dbValue)
		{
			if (dbValue is DBNull)
				return null;
			else
				switch (dbValue.ToString())
				{
					case "PRIMARY KEY":
						return enSqlConstraintType.PrimaryKey;
					case "CHECK":
						return enSqlConstraintType.Check;
					case "FOREIGN KEY":
						return enSqlConstraintType.ForeignKey;
					case "UNIQUE":
						return enSqlConstraintType.Unique;

					default:
						return enSqlConstraintType.None;
				}

		}

		private Nullable<T> GetValueOrNull<T>(object dbValue) where T : struct
		{
			try
			{
				if (!(dbValue is DBNull))
				{
					if (typeof(T).IsEnum)
						return (T)Enum.Parse(typeof(T), dbValue.ToString());
					else
						return (T)dbValue;
				}

			}
			catch
			{
				return null;
			}

			return null;
		}

		#endregion Private

	}
}
