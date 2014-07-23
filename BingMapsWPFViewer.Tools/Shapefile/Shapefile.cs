using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using ProjNet.Converters.WellKnownText;

namespace BingMapsWPFViewer.Tools
{
	public class ShapefileHelper
	{
		public static string GetProjection(string shapefileName)
		{
			string projCS = null;
			if (!File.Exists(shapefileName))
				throw new FileNotFoundException("Shapefile does not exists");

			string prjFile = Path.ChangeExtension(shapefileName, "prj");
			if (!File.Exists(prjFile))
				throw new FileNotFoundException("PRJ file does not exists");

			try
			{
				using (StreamReader reader = new StreamReader(prjFile))
					projCS = reader.ReadToEnd();
			}
			catch (Exception exPrj)
			{
				throw new Exception("Unable to read from shapefile .prj file (" + exPrj.Message + ")");
			}

			return projCS;
		}

		public static bool IsValidCoordSys(string coordSysWKT)
		{
			try
			{
				CoordinateSystemWktReader.Parse(coordSysWKT);
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

	}
}
