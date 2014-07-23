using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace BingMapsWPFViewer.Framework
{

	public class ViewModelTypeLocator
	{
		private readonly static Dictionary<string, Type> _viewModelCache
				= new Dictionary<string, Type>();

		public Type this[string key]
		{
			get
			{
				Type vm;
				_viewModelCache.TryGetValue(key, out vm);
				return vm;
			}
		}

		public static Type Retrieve(string key)
		{
			Type vm;
			_viewModelCache.TryGetValue(key, out vm);
			return vm;
		}

		/// <summary>
		/// Register a view model providing its name
		/// </summary>
		/// <param name="key"></param>
		/// <param name="viewModel"></param>
		public static void Register(string key, Type viewModel)
		{
			if (!viewModel.IsSubclassOf(typeof(ViewModelBase)))
				throw new ArgumentException("Type must inherit from ViewModelBase and have a constructor without arguments");

			_viewModelCache.Add(key, viewModel);

		}

		/// <summary>
		/// Register a viewmodel provider type name as access key
		/// </summary>
		public static void Register<TViewModel>(Type viewModelType)
		{
			Register(typeof(TViewModel).Name, viewModelType);
		}
		/// <summary>
		/// Register a Window provider its type name 
		/// </summary>
		public static void Register(Type viewModelType)
		{
			Register(viewModelType.GetType().Name, viewModelType);
		}
	}
}
