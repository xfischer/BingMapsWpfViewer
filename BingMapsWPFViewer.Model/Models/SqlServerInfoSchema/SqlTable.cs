using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BingMapsWPFViewer.Model.SqlInfoSchema
{
	public class SqlTable
	{
		public bool IsView { get; set; }
		public string CatalogName { get; set; }
		public string SchemaName { get; set; }
		public string TableName { get; set; }

		public string FullTableName
		{
			get { return string.Format("{0}.{1}", SchemaName, TableName); }
		}

		internal List<SqlColumn> Columns { get; set; }

		internal SqlTable()
		{
			this.Columns = new List<SqlColumn>();
		}

	}
}
