using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BingMapsWPFViewer.Model.SqlInfoSchema
{
	public enum enSqlConstraintType
	{
		None,
		Check,
		Unique,
		PrimaryKey, // Implies Unique
		ForeignKey,
	}
}
