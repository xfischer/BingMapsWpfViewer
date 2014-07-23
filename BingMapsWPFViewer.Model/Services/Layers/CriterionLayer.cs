using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BingMapsWPFViewer.Model.Services
{
	public class CriterionLayer : CriterionBase<CriterionLayer>
	{
		public string SearchString { get; set; }

		public enLayerType LayerType { get; set; }
	}
}
