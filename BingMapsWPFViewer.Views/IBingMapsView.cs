using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Maps.MapControl.WPF;

namespace BingMapsWPFViewer.ViewModel
{
	/// <summary>
	/// Every view containing a Bing Maps control must implement this interface
	/// This ensures ViewModel can access a Map instance a act as if they were
	/// DataContext for the Map
	/// </summary>
	public interface IBingMapsView
	{
		Map MapInstance { get; }
	}
}
