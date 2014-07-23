using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BingMapsWPFViewer.Tools
{
	public class BoundingBox
	{
		public double XMin { get; private set; }
		public double XMax { get; private set; }
		public double YMin { get; private set; }
		public double YMax { get; private set; }

		public double Height
		{
			get { return YMax - YMin; }
		}
		public double Width
		{
			get { return XMax - XMin; }
		}

		public BoundingBox(double p_XMin, double p_YMin, double p_XMax, double p_YMax)
		{
			if (p_XMin > p_XMax)
				throw new ArgumentOutOfRangeException("Xmax must be greater than Xmin");
			if (p_YMin > p_YMax)
				throw new ArgumentOutOfRangeException("Ymax must be greater than Ymin");

			XMin = p_XMin;
			XMax = p_XMax;
			YMin = p_YMin;
			YMax = p_YMax;
		}

		public override string ToString()
		{
			return string.Format("xmin={0} ymin={1} xmax={2} ymax={3}", XMin, YMin, XMax, YMax);
		}

		public string WKT
		{
			get
			{
				return this.ToSqlGeometry().ToString();
			}
		}

	}
}
