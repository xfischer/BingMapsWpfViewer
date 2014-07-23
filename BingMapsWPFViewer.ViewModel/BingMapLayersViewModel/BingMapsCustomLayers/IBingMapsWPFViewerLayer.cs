using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Maps.MapControl.WPF;

namespace BingMapsWPFViewer.ViewModel
{
	/// <summary>
	/// Interface ensuring the link between a layer in the map and the layer Model
	/// Every layer class added to the Map must implement this interface
	/// </summary>
	public interface IBingMapsWPFViewerLayer
	{
		Guid Id { get; }
	}
}
