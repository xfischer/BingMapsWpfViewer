using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BingMapsWPFViewer.Framework
{
	public class ViewModelLocator
	{
		private readonly static Dictionary<string, ViewModelBase> _viewModelCache
				= new Dictionary<string, ViewModelBase>();

		public ViewModelBase this[string key]
		{
			get
			{
				ViewModelBase viewModel;
				_viewModelCache.TryGetValue(key, out viewModel);
				return viewModel;
			}
		}

		public static ViewModelBase Retrieve(string key)
		{
			ViewModelBase viewModel;
			_viewModelCache.TryGetValue(key, out viewModel);
			return viewModel;
		}

		public static ViewModelBase Retrieve<TViewModel>()
			where TViewModel : ViewModelBase
		{
			string key = typeof(TViewModel).Name;
			ViewModelBase viewModel;
			_viewModelCache.TryGetValue(key, out viewModel);
			return viewModel;
		}


		/// <summary>
		/// Register a viewModel providing its name
		/// </summary>
		/// <param name="key"></param>
		/// <param name="viewModel"></param>
		public static void RegisterViewModel(string key, ViewModelBase viewModel)
		{
			_viewModelCache.Add(key, viewModel);

		}

		/// <summary>
		/// Register a ViewModel providing a type as access key
		/// </summary>
		public static void RegisterViewModel<TViewModel>(ViewModelBase viewModel)
			where TViewModel : ViewModelBase
		{
			RegisterViewModel(typeof(TViewModel).Name, viewModel);
		}
		/// <summary>
		/// Register a ViewModel provider its type name 
		/// </summary>
		public static void RegisterViewModel(ViewModelBase viewModel)
		{
			RegisterViewModel(viewModel.GetType().Name, viewModel);
		}
	}
}
