using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using BingMapsWPFViewer.Model.Features;

namespace BingMapsWPFViewer.Model.Services
{
	public abstract class FeatureServiceBase<TLayerModel, TCriterion> : IFeatureServiceBase<TLayerModel, TCriterion>, ICancelable
		where TLayerModel : LayerBase
		where TCriterion : CriterionBase<TCriterion>, new()
	{

		public abstract List<Feature> LoadConcrete(TLayerModel layer, TCriterion criterion);
		
		#region IFeatureServiceBase<TLayerModel,TCriterion> Membres

		public List<Feature> Load(TLayerModel layer, TCriterion criterion)
		{
			return this.LoadConcrete(layer, criterion);
		}

		public bool LoadAsync(TLayerModel layer, TCriterion criterion, Action<AsyncResponse, List<Feature>> callback)
		{
			// get UI thread
			var UISyncContext = SynchronizationContext.Current;

			// call definition
			WaitCallback waitCallBack =
					param =>
					{
						AsyncResponse response = new AsyncResponse();
						List<Feature> returnList = default(List<Feature>);

						List<object> paramList = param as List<object>;

						try
						{
							// perform load
							returnList = Load((TLayerModel)paramList[0], (TCriterion)paramList[1]);
						}
						catch (Exception e)
						{
							response.HasError = true;
							response.ErrorMessage = e.Message;
						}
						finally
						{
							// execute callback on UI thread
							UISyncContext
									.Post(cParam => callback(response, (List<Feature>)cParam)
									, returnList);
						}
					};

			// Effective launch
			return ThreadPool.QueueUserWorkItem(waitCallBack, new List<object>() { layer, criterion });
		}

		public abstract void Cancel();
		
		#endregion

	}
}
