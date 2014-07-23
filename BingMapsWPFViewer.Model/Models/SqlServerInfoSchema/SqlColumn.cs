using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BingMapsWPFViewer.Model.SqlInfoSchema
{
	public class SqlColumn
	{
		public string CatalogName { get; set; }
		public string SchemaName { get; set; }
		public string TableName { get; set; }
		public string ColumnName { get; set; }
		public enSqlConstraintType ConstraintType { get; set; }
		public object DefaultValue { get; set; }
		public bool IsNullable { get; set; }
		public string DataType { get; set; }

		public int? MaximumCaraterLength { get; set; }

		public int? NumericPrecision { get; set; }
		public int? NumericPrecisionRadix { get; set; }
		public int? NumericScale { get; set; }
		public int OrdinalPosition { get; set; }

		public override string ToString()
		{
			return string.Format("{0} ({1})", this.ColumnName, this.DataType);
		}

		public bool IsSpatial
		{
			get { return DataType == "geometry" || DataType == "geography"; }
		}

		public string Type
		{
			get
			{
				if (MaximumCaraterLength.HasValue)
					return this.DataType + "(" + MaximumCaraterLength.Value.ToString() + ")";
				else if (NumericPrecision.HasValue)
				{
					string v_ret = this.DataType + "(" + MaximumCaraterLength.Value.ToString();
					if (NumericPrecisionRadix.HasValue)
						v_ret += ", " + NumericPrecisionRadix.Value.ToString();
					if (NumericScale.HasValue)
						v_ret += ", " + NumericScale.Value.ToString();

					return v_ret + ")";
				}
				else
					return this.DataType;
			}
		}

		#region ICloneable Membres

		public SqlColumn Clone()
		{
			return (SqlColumn)this.MemberwiseClone();
		}

		#endregion
	}
}
