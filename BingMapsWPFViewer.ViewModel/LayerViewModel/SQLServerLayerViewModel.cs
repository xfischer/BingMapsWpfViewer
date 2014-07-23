using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BingMapsWPFViewer.Model;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using BingMapsWPFViewer.Framework;
using BingMapsWPFViewer.Framework.IOC;
using BingMapsWPFViewer.Model.Services;
using System.Windows.Controls;
using BingMapsWPFViewer.Model.SqlInfoSchema;
using BingMapsWPFViewer.Model.Features;
using BingMapsWPFViewer.Tools;
using System.Windows.Media;

namespace BingMapsWPFViewer.ViewModel
{
	public class SQLServerLayerViewModel : LayerBaseViewModel
	{

		#region Commands / Services
		private IDialogService _windowsService;
		private ISqlServerInfoSchemaService _sqlServerInfoSchemaService;

		public ProxyCommand<SQLServerLayerViewModel> SetupConnectionCommand { get; set; }

		protected override void InitCommands()
		{
			SetupConnectionCommand = new ProxyCommand<SQLServerLayerViewModel>((_) =>
			{
				this.ConnectionString = _windowsService.SetupSqlConnectionDialog();
			});
		}

		protected override void InitServices()
		{
			_windowsService = ServiceLocator.Instance.Retrieve<IDialogService>();
			_sqlServerInfoSchemaService = ServiceLocator.Instance.Retrieve<ISqlServerInfoSchemaService>();
		}

		#endregion

		[CustomValidation(typeof(ConnectionStringUtils), "ValidateConnectionString")]
		public string ConnectionString
		{
			get { return this.Layer.ConnectionString; }
			set
			{
				if (this.Layer.ConnectionString != value)
				{
					this.Layer.ConnectionString = value;

					this.TableName = null;
					this.SpatialColumnName = null;
					this.IndexName = null;
					this.Columns = null;
					this.Layer.Attributes = null;

					RaisePropertyChanged<string>(() => ConnectionString);

					if (ConnectionStringUtils.IsValid(value))
					{
						this.LoadSchema(ConnectionString);
					}
				}
			}
		}

		[Required(ErrorMessage = "Table is required")]
		public string TableName
		{
			get { return this.Layer.TableName; }
			set
			{
				if (this.Layer.TableName != value)
				{
					this.Layer.TableName = value;
					this.Columns = null;
					this.SpatialColumnName = null;
					this.IndexName = null;
					this.Layer.Attributes = null;
					RaisePropertyChanged<string>(() => TableName);

					if (value != null)
					{
						// Get table columns
						// Retrieve table
						SqlTable table = _tables
														.Where(t => t.FullTableName == value)
														.FirstOrDefault();
						if (table != null)
						{
							// get spatial columns info
							List<SqlColumn> tableColumns = _sqlServerInfoSchemaService
								.GetSqlTableColumns(table)
								.ToList();

							// Assign columns list 
							this.Columns = tableColumns.DistinctBy(c => c.OrdinalPosition).ToList();
							this.AssignFeatureFields();

						}

						RaisePropertyChanged<bool>(() => IsValid);
					}
				}
			}
		}

		private string _spatialColName;
		[Required(ErrorMessage = "Spatial column is required")]
		public string SpatialColumnName
		{
			get { return _spatialColName; }
			set
			{
				if (_spatialColName != value)
				{
					_spatialColName = value;

					if (value != null)
					{
						// Check if is geography
						if (this.SpatialColumns != null)
						{
							SqlColumn col = this.SpatialColumns.Where(c => c.ColumnName == _spatialColName).FirstOrDefault();
							this.Layer.IsSqlGeography = (col != null && col.DataType == "geography");
						}


						this.GetSpatialIndexes();
						this.GetSRID();
						this.AssignFeatureFields();

						RaisePropertyChanged<string>(() => SpatialColumnName);
						RaisePropertyChanged<bool>(() => IsValid);
					}
				}
			}
		}

		public string IndexName
		{
			get { return this.Layer.SpatialIndexName; }
			set
			{
				if (this.Layer.SpatialIndexName != value)
				{
					this.Layer.SpatialIndexName = value;

					RaisePropertyChanged<string>(() => IndexName);
				}
			}
		}

		public int SRID
		{
			get { return this.Layer.SRID; }
			set
			{
				if (this.Layer.SRID != value)
				{
					this.Layer.SRID = value;

					RaisePropertyChanged<int>(() => SRID);
				}
			}
		}

		public string QueryFilter
		{
			get
			{
				if (this.Layer.QueryFilter == null)
					return null;

				return this.Layer.QueryFilter.QueryText;

			}
			set
			{
				if (string.IsNullOrWhiteSpace(value))
					this.Layer.QueryFilter = null;
				else
					this.Layer.QueryFilter = new FeatureQueryFilter(value);
			}
		}

		public string FillColorText
		{
			get { return this.Layer.FillColor.ToString(); }
			set { this.Layer.FillColor = (Color)ColorConverter.ConvertFromString(value); }
		}

		public override string IconName
		{
			get
			{
				return "database.png";
			}
		}

		#region Combo boxes binding

		private List<SqlTable> _tables;
		public List<SqlTable> Tables
		{
			get { return _tables; }
			set
			{
				_tables = value;
				RaisePropertyChanged<List<SqlTable>>(() => Tables);
			}
		}

		private List<SqlColumn> _columns;
		public List<SqlColumn> Columns
		{
			get { return _columns; }
			set
			{
				_columns = value;
				RaisePropertyChanged<List<SqlColumn>>(() => Columns);
				RaisePropertyChanged<List<SqlColumn>>(() => SpatialColumns);
			}
		}

		public List<SqlColumn> SpatialColumns
		{
			get
			{
				if (_columns == null)
					return null;

				return _columns
						.Where(c => c.IsSpatial)
						.ToList();
			}
		}

		#endregion

		#region Private

		private void LoadSchema(string connectionString)
		{
			this.Tables = _sqlServerInfoSchemaService.GetSqlTables(connectionString);
		}

		private void GetSpatialIndexes()
		{
			try
			{
				SqlIndex index = _sqlServerInfoSchemaService.GetTableIndexes(this.ConnectionString, this.TableName.Split('.')[0], this.TableName.Split('.')[1])
													.Where(i => i.KeyColumns.Contains(this.SpatialColumnName))
													.FirstOrDefault();
				if (index != null)
					this.IndexName = index.Name;
				else
					this.IndexName = null;

			}
			finally
			{

			}
		}

		private void GetSRID()
		{
			try
			{
				if (this.SpatialColumnName == null)
					this.SRID = 0;
				else
				{
					SqlColumn col = this.SpatialColumns.Where(c => c.ColumnName == this.SpatialColumnName).FirstOrDefault();
					if (col == null)
						this.SRID = 0;
					else
					{
						int srid = _sqlServerInfoSchemaService.GetSpatialColumnSrid(col, this.ConnectionString);
						this.SRID = srid;
					}
				}
			}
			finally
			{

			}
		}

		private void AssignFeatureFields()
		{
			if (this.Columns == null)
				this.Layer.Attributes = null;
			else
			{
				this.Layer.Attributes = new List<FeatureField>();
				foreach (SqlColumn column in this.Columns)
				{
					FeatureField field = new FeatureField(column.ColumnName, column.DataType, column.OrdinalPosition, enFeatureFieldUsage.DataOnly | enFeatureFieldUsage.Tooltip);

					if (this.SpatialColumnName != null
							&& column.ColumnName == this.SpatialColumnName)
						field.Usage |= enFeatureFieldUsage.Spatial;

					this.Layer.Attributes.Add(field);
				}
			}
		}

		#endregion

		public SqlServerLayer Layer
		{
			get { return (SqlServerLayer)base.LayerModel; }
			set { base.LayerModel = value; }
		}

		public SQLServerLayerViewModel()
			: base(new SqlServerLayer(null, null, 0))
		{
		}

		public SQLServerLayerViewModel(SqlServerLayer layer)
			: base(layer)
		{
		}

		public override void BeginEdit()
		{
			base.BeginEdit();
			try
			{
				// Superficial copy
				SqlServerLayer layerCopy = this.Layer.Clone() as SqlServerLayer;
				
				// Reset properties for UI to reflect model state
				this.ConnectionString = null;
				this.ConnectionString = layerCopy.ConnectionString;
				this.TableName = layerCopy.TableName;
				//this.SpatialColumnName = layerCopy.Attributes.Where(a => a.IsSpatial).FirstOrDefault().FieldName; ;
				this.SpatialColumnName = FeatureFieldHelper.GetQuerySpatialField(layerCopy.Attributes).FieldName;

			}
			catch (Exception ex)
			{
				DebugHelper.WriteLine(this,ex.Message);
			}

		}
	}
}
