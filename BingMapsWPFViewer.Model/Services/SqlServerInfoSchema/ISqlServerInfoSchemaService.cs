using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using BingMapsWPFViewer.Model.SqlInfoSchema;
using BingMapsWPFViewer.Tools;

namespace BingMapsWPFViewer.Model.Services
{
	
	public interface ISqlServerInfoSchemaService
	{
		List<SqlTable> GetSqlTables(string connectionString);
		List<SqlTable> GetSqlTables(SqlConnection p_SqlOpenedConnection);

		List<SqlColumn> GetSqlTableColumns(SqlTable p_SqlTable);

		List<SqlColumn> GetSqlTablePrimaryKey(SqlTable p_SqlTable);
		List<SqlColumn> GePrimaryKeyColumns(List<SqlColumn> p_SqlColumns);

		List<SqlColumn> GetSpatialColumns(SqlTable sqlTable);

		/// <summary>
		/// Read SRID from first row
		/// </summary>
		/// <param name="p_Column"></param>
		/// <returns></returns>
		int GetSpatialColumnSrid(SqlColumn p_Column, SqlConnection sqlOpenedConnection);
		int GetSpatialColumnSrid(SqlColumn p_Column, string connectionString);

		/// <summary>
		/// Returns all relevant information on table indexes
		/// </summary>
		/// <param name="p_SchemaName"></param>
		/// <param name="p_TableName"></param>
		/// <returns></returns>
		List<SqlIndex> GetTableIndexes(SqlConnection p_SqlOpenedConnection, string p_SchemaName, string p_TableName);
		List<SqlIndex> GetTableIndexes(string connectionString, string p_SchemaName, string p_TableName);
		
		void DropIndex(SqlConnection p_SqlOpenedConnection, SqlIndex p_SpatialIndex);
		void DropIndex(string connectionString, SqlIndex p_SpatialIndex);

		SqlSpatialIndex CreateSpatialIndexGeometry_InDB(SqlConnection p_SqlOpenedConnection, string p_SchemaName, string p_TableName, string p_SpatialColumnName
																														, string p_IndexName, BoundingBox p_BoundingBox, enGridLevel p_GridSizeLevel1, enGridLevel p_GridSizeLevel2, enGridLevel p_GridSizeLevel3, enGridLevel p_GridSizeLevel4);

		SqlSpatialIndex CreateSpatialIndexGeometry_InDB(string connectionString, string p_SchemaName, string p_TableName, string p_SpatialColumnName
																														, string p_IndexName, BoundingBox p_BoundingBox, enGridLevel p_GridSizeLevel1, enGridLevel p_GridSizeLevel2, enGridLevel p_GridSizeLevel3, enGridLevel p_GridSizeLevel4);
		
		SqlSpatialIndex CreateSpatialIndexGeography_InDB(SqlConnection p_SqlOpenedConnection, string p_SchemaName, string p_TableName, string p_SpatialColumnName
																														, string p_IndexName, enGridLevel p_GridSizeLevel1, enGridLevel p_GridSizeLevel2, enGridLevel p_GridSizeLevel3, enGridLevel p_GridSizeLevel4);
		SqlSpatialIndex CreateSpatialIndexGeography_InDB(string connectionString, string p_SchemaName, string p_TableName, string p_SpatialColumnName
																														, string p_IndexName, enGridLevel p_GridSizeLevel1, enGridLevel p_GridSizeLevel2, enGridLevel p_GridSizeLevel3, enGridLevel p_GridSizeLevel4);

		void RebuildIndex(SqlConnection p_SqlOpenedConnection, SqlIndex p_SpatialIndex);
		void RebuildIndex(string connectionString, SqlIndex p_SpatialIndex);

		void ReorganizeIndex(SqlConnection p_SqlOpenedConnection, SqlIndex p_SpatialIndex);
		void ReorganizeIndex(string connectionString, SqlIndex p_SpatialIndex);

		/// <summary>
		/// Returns index fragmentation
		/// </summary>
		/// <param name="p_SpatialIndex"></param>
		/// <returns>Null if index is not found</returns>
		double? GetIndexFragmentation(SqlConnection p_SqlOpenedConnection, SqlIndex p_SpatialIndex);
		double? GetIndexFragmentation(string connectionString, SqlIndex p_SpatialIndex);

		/// <summary>
		/// Returns an sql compliant string
		/// </summary>
		/// <param name="p_Name"></param>
		/// <returns></returns>
		string CleanupSqlName(string p_Name);

	}
}
