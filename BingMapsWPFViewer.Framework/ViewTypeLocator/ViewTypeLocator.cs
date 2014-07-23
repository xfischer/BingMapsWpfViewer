using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace BingMapsWPFViewer.Framework
{

	public class ViewTypeLocator
	{
		private readonly static Dictionary<string, Type> _windowCache
				= new Dictionary<string, Type>();

		public Type this[string key]
		{
			get
			{
				Type win;
				_windowCache.TryGetValue(key, out win);
				return win;
			}
		}

		/// <summary>
		/// Register a window providing its name
		/// </summary>
		/// <param name="key"></param>
		/// <param name="window"></param>
		public static void RegisterWindow(string key, Type window)
		{
			if (!window.IsSubclassOf(typeof(Window)))
				throw new ArgumentException("Type must inherit from Window and have a constructor without arguments");

				_windowCache.Add(key, window);
			
		}

		/// <summary>
		/// Register a Window provider type name as access key
		/// </summary>
		public static void RegisterWindow<TWindow>(Type window)
		{
			RegisterWindow(typeof(TWindow).Name, window);
		}
		/// <summary>
		/// Register a Window provider its type name 
		/// </summary>
		public static void RegisterWindow(Type window)
		{
			RegisterWindow(window.GetType().Name, window);
		}
	}
}
