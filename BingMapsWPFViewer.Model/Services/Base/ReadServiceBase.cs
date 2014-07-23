using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using BingMapsWPFViewer.Model;

namespace BingMapsWPFViewer.Model.Services
{
	public abstract class ReadServiceBase<TModelBase, TEntity, TCriterion>
		: IReadServiceBase<TModelBase, TCriterion>
		where TModelBase : class
		where TEntity : class
		where TCriterion : CriterionBase<TCriterion>, new()
	{

		#region abstract methods
		protected abstract TEntity MapToEntity(TModelBase model);
		protected abstract TModelBase MapToModel(TEntity entity);
		protected abstract IQueryable<TEntity> LoadConcrete(TCriterion criterion);
		#endregion

		#region IReadServiceBase<TModelBase,TCriterion> Membres

		public TModelBase Load(Guid idToGet)
		{
			throw new NotImplementedException();
		}

		public List<TModelBase> Load(TCriterion criterion)
		{
			IQueryable<TEntity> loadConcrete = this.LoadConcrete(criterion);
			if (criterion.SizeLimit > 0)
			{
				loadConcrete = loadConcrete.Take(criterion.SizeLimit);
			}
			var list = loadConcrete.ToList().Select(MapToModel);

			return list.ToList();
		}

		public bool LoadAsync(TCriterion criterion, Action<AsyncResponse, List<TModelBase>> callback)
		{
			// UI thread
			var UISyncContext = SynchronizationContext.Current;

			// define call 
			WaitCallback waitCallBack =
					param =>
					{
						AsyncResponse response = new AsyncResponse();
						List<TModelBase> returnValue = default(List<TModelBase>);
						try
						{
							// Load element list
							returnValue = Load((TCriterion)param);
						}
						catch (Exception e)
						{
							response.HasError = true;
							response.ErrorMessage = e.Message;
						}
						finally
						{
							// Execute callback on UI thread
							UISyncContext
									.Post(cParam => callback(response, (List<TModelBase>)cParam)
									, returnValue);
						}
					};

			// Launch async thread
			return ThreadPool.QueueUserWorkItem(waitCallBack, criterion);
		}

		#endregion
	}
}
