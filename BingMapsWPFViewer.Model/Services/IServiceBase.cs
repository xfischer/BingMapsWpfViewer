using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BingMapsWPFViewer.Framework;

namespace BingMapsWPFViewer.Model.Services
{
	/// <summary>
	/// Base interface for services
	/// </summary>
	public interface IServiceBase<TModelBase, in TCriterion>
		where TModelBase : ModelBase
		where TCriterion : CriterionBase<TCriterion>, new()
	{
		bool Create(TModelBase toCreate);
		bool Update(TModelBase toUpdate);
		bool Delete(TModelBase toDelete);
		TModelBase Load(Guid idToGet);
		List<TModelBase> Load(TCriterion criterion);
		bool LoadAsync(TCriterion criterion, Action<AsyncResponse, List<TModelBase>> callback);
		void ApplyChanges();
		void DiscardChanges();
	}

	/// <summary>
	/// Base interface for services without search criterion
	/// </summary>
	public interface IServiceBase<TModelBase>
	: IServiceBase<TModelBase, CriterionBase>
	where TModelBase : ModelBase { }

}
