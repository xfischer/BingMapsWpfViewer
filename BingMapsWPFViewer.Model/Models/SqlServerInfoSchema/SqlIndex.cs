using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BingMapsWPFViewer.Model.Services
{
	public class SqlIndex
	{
		public int Internal_IndexId { get; internal set; } // Index unique id
		public int Internal_ObjectId { get; internal set; } // Table unique id

		public enSqlIndexType IndexType { get; internal set; }

		public virtual bool IsSpatialIndex { get; internal set; }

		public string Name { get; internal set; }

		public string TableName { get; internal set; }

		public string SchemaName { get; internal set; }

		public List<string> KeyColumns { get; internal set; }

		public List<string> IncludedColumns { get; internal set; }

		internal SqlIndex()
		{
			this.KeyColumns = new List<string>();
			this.IncludedColumns = new List<string>();
		}

		public override string ToString()
		{
			string v_ret = string.Format("Index {0} {1} on table {2}.{3} keys=({4})", IndexType, Name, SchemaName, TableName, string.Join(", ", KeyColumns.ToArray()));
			if (IncludedColumns.Count > 0)
				v_ret += string.Format(" Included columns: {0}", string.Join(", ", IncludedColumns.ToArray()));
			return v_ret;
		}
	}
}
