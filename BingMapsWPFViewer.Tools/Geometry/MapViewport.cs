using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BingMapsWPFViewer.Tools
{
	public class MapViewport
	{
		/// <summary>
		/// Map geographic bounds (lat / lon)
		/// </summary>
		public BoundingBox GeographicBounds { get; private set; }

		/// <summary>
		/// Viewport width in screen pixels
		/// </summary>
		public double Width { get; private set; }

		/// <summary>
		/// Viewport height in screen pixels
		/// </summary>
		public double Height { get; private set; }

		/// <summary>
		/// Map zoom level (in Bing tile system)
		/// </summary>
		public double ZoomLevel { get; private set; }

		/// <summary>
		/// Map resolution (in degrees / pixel)
		/// Useful for simplification algorithms
		/// </summary>
		public double MapResolution  { get; private set; }
		
		public MapViewport(BoundingBox geoBounds, double viewportPixelWidth, double viewportPixelHeight, double zoomLevel)
		{
			this.GeographicBounds = geoBounds;
			this.Width = viewportPixelWidth;
			this.Height = viewportPixelHeight;
			this.MapResolution = geoBounds.Height / viewportPixelHeight;
			this.ZoomLevel = zoomLevel;
		}

		public override string ToString()
		{
			StringBuilder builder = new StringBuilder();
			builder.AppendFormat("Viewport bounding box : {0}", GeographicBounds.ToString());
			builder.AppendLine();
			builder.AppendFormat("Viewport size : {0} * {1} px", Width, Height);
			builder.AppendLine();
			builder.AppendFormat("Zoom level : {0}", ZoomLevel);
			builder.AppendLine();
			builder.AppendFormat("Resolution (degress/pixel): {0} lat, {1} lon", MapResolution, GeographicBounds.Width/ Width);
			return builder.ToString();
		}
	}
}
