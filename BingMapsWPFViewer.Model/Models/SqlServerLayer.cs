using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Microsoft.SqlServer.Types;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.ComponentModel.DataAnnotations;
using BingMapsWPFViewer.Tools;
using BingMapsWPFViewer.Model.Services;
using BingMapsWPFViewer.Model.Features;

namespace BingMapsWPFViewer.Model
{
	public class SqlServerLayer : VectorLayerBase
	{

		#region Properties

		public string ConnectionString { get; set; }

		public string TableName { get; set; }

		public string SpatialIndexName { get; set; }

		public bool IsSqlGeography { get; set; }

		#endregion Properties

		#region VectorLayerBase Properties override

		public override bool IsVectorLayer
		{
			get { return true; }
		}

		public override enLayerType LayerType
		{
			get { return enLayerType.SQLServerLayer; }
		}

		#endregion

		public SqlServerLayer(string connectionString, string tableName, int spatialReferenceId, string spatialIndexName = null)
		{
			ConnectionString = connectionString;
			TableName = tableName;
			SpatialIndexName = spatialIndexName;
			base.SRID = spatialReferenceId;
		}

		#region VectorLayerBase override

		public override string GenerateDisplayNameProposal()
		{
			string name = "New " + this.GetType().Name;
			if (ConnectionString != null)
				try
				{
					name = string.Format("{1}.{2} ({0})", ConnectionStringUtils.GetSqlServerName(ConnectionString), ConnectionStringUtils.GetSqlDatabaseName(ConnectionString), TableName);
				}
				finally
				{

				}
			return name;
		}

		#endregion


		private FeatureQueryFilter _queryFilter;
		public override FeatureQueryFilter QueryFilter
		{
			get
			{
				return _queryFilter;
			}
			set
			{
				_queryFilter = value;
			}
		}
	}
}
