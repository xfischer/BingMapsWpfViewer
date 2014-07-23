using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BingMapsWPFViewer.Model.Services
{
	/// <summary>
	/// Defines a search criterion for services data access methods
	/// </summary>
	/// <typeparam name="T">Criterion real type</typeparam>
	public abstract class CriterionBase<T> where T : CriterionBase<T>, new()
	{
		public static T Empty { get { return new T(); } }


		/// <summary>
		/// Limit size of returned data set
		/// </summary>
		public int SizeLimit { get; set; }
	}

	public class CriterionBase : CriterionBase<CriterionBase> { }
}
