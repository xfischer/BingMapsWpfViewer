using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BingMapsWPFViewer.Model.Features
{
	public class FeatureQueryFilter 
	{
		public string QueryText { get; set; }

		public FeatureQueryFilter(string queryText)
		{
			this.QueryText = queryText;
		}

		public override string ToString()
		{
			return this.QueryText;
		}



	}
}
