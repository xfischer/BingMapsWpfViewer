using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BingMapsWPFViewer.ViewModel
{
	/// <summary>
	/// Application events passed throught Messenger
	/// </summary>
	public static class Events
	{
		public const string GEOMETRY_LOADED = "GEOMETRY_LOADED_EVENT";

		// Layer control events
		public const string LAYER_ADDED = "LAYER_ADDED_EVENT";
		public const string LAYER_REMOVED = "LAYER_REMOVED_EVENT";
		public const string LAYER_ZINDEX_CHANGED = "LAYER_ZINDEX_CHANGED_EVENT";

		// Layer events
		public const string LAYER_LOADING = "LAYER_LOADING";
		public const string LAYER_LOADED = "LAYER_LOADED";



	}
}

