using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Diagnostics;
using BingMapsWPFViewer.Model;
using BingMapsWPFViewer.Model.Features;
using BingMapsWPFViewer.Model.Services;
using BingMapsWPFViewer.Model.Services.FeatureServices;
using Microsoft.SqlServer.Types;


namespace BingMapsWPFViewer.Services
{
	public sealed class FeatureSpatialService
		: FeatureServiceBase<LayerBase, SpatialQueryCriterion>
	{

		private ICancelable _cancelableService = null;


		public override List<Feature> LoadConcrete(LayerBase layer, SpatialQueryCriterion criterion)
		{
			if (layer is SqlServerLayer)
			{
				#region SqlServerLayer
				
				SqlServerFeatureService sqlService = new SqlServerFeatureService();
				_cancelableService = sqlService;
				return sqlService.LoadConcrete((SqlServerLayer)layer, criterion);

				#endregion SqlServerLayer
			}
			else if (layer is ShapeFileLayer)
			{
				#region ShapeFileLayer

				ShapeFileFeatureService shapeService = new ShapeFileFeatureService();
				_cancelableService = shapeService;
				return shapeService.LoadConcrete((ShapeFileLayer)layer, criterion);

				#endregion ShapeFileLayer
			}
			else if (layer is TileGridLayer)
			{
				#region TileGridLayer

				return new TileGridFeatureService().LoadConcrete((TileGridLayer)layer, criterion);

				#endregion TileGridLayer
			}
			else
			{
				throw new NotImplementedException("Service for layer " + layer.GetType().Name + " is not implemented. Add implementation in class " + this.GetType().Name + ", BingMapsWPFViewer.Services assembly");
			}
		}

		public override void Cancel()
		{
			if (_cancelableService != null)
				_cancelableService.Cancel();
		}
	}
}

