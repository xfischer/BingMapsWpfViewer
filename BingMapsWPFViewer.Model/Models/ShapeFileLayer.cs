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
using System.IO;

namespace BingMapsWPFViewer.Model
{
	public class ShapeFileLayer : VectorLayerBase
	{

		#region Properties

		public string ShapeFileName { get; set; }

		#endregion Properties

		#region VectorLayerBase Properties override

		public override bool IsVectorLayer
		{
			get { return true; }
		}

		public override enLayerType LayerType
		{
			get { return enLayerType.ShapeFileLayer; }
		}

		#endregion

		public ShapeFileLayer(string shapeFileName, int spatialReferenceId)
		{
			ShapeFileName = shapeFileName;
			base.SRID = spatialReferenceId;
		}

		#region VectorLayerBase override

		public override string GenerateDisplayNameProposal()
		{
			string name = "New " + this.GetType().Name;
			if (ShapeFileName != null)
				try
				{
					name = string.Format("{0}", Path.GetFileName(this.ShapeFileName));
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
