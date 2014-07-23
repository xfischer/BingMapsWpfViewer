using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BingMapsWPFViewer.Model.Features;

namespace BingMapsWPFViewer.Model.Services
{
	public interface IFeatureServiceBase<TLayerModel, in TCriterion>
		where TLayerModel : LayerBase
		where TCriterion : CriterionBase<TCriterion>, new()
	{
		List<Feature> Load(TLayerModel layer, TCriterion criterion);
		bool LoadAsync(TLayerModel layer, TCriterion criterion, Action<AsyncResponse, List<Feature>> callback);
		void Cancel();
	}
}
