using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using ProjNet.Converters.WellKnownText;
using ProjNet.CoordinateSystems;

namespace BingMapsWPFViewer.Tools.Geometry.SpatialReference
{

	public class SridReader
	{
		private static string SRID_FILE_RESOURCE_PATH = @"BingMapsWPFViewer.Tools.Geometry.SpatialReference.SRID.csv"; //Change this to point to the SRID.CSV file.

		public struct WKTstring
		{
			/// <summary>Well-known ID</summary>
			public int WKID;
			/// <summary>Well-known Text</summary>
			public string WKT;
		}

		/// <summary>Enumerates all SRID's in the SRID.csv file.</summary>
		/// <returns>Enumerator</returns>
		public static IEnumerable<WKTstring> GetSRIDs()
		{
			Assembly _assembly = Assembly.GetExecutingAssembly();
			using (Stream sridStream = _assembly.GetManifestResourceStream(SRID_FILE_RESOURCE_PATH))
			{
				if (sridStream == null)
					throw new Exception("SRID file not found. Expected resource location is at: " + SRID_FILE_RESOURCE_PATH);

				using (System.IO.StreamReader sr = new StreamReader(sridStream))
				{
					while (!sr.EndOfStream)
					{
						string line = sr.ReadLine();
						int split = line.IndexOf(';');
						if (split > -1)
						{
							WKTstring wkt = new WKTstring();
							wkt.WKID = int.Parse(line.Substring(0, split));
							wkt.WKT = line.Substring(split + 1);
							yield return wkt;
						}
					}
					sr.Close();
				}
			}
		}
		/// <summary>Gets a coordinate system from the SRID.csv file</summary>
		/// <param name="id">EPSG ID</param>
		/// <returns>Coordinate system, or null if SRID was not found.</returns>
		public static ICoordinateSystem GetCSbyID(int id)
		{
			foreach (SridReader.WKTstring wkt in SridReader.GetSRIDs())
			{
				if (wkt.WKID == id) //We found it!
				{
					return CoordinateSystemWktReader.Parse(wkt.WKT) as ICoordinateSystem;
				}
			}
			return null;
		}

		

	}
}
