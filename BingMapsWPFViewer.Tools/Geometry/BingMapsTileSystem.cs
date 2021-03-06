﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BingMapsWPFViewer.Tools
{
	//------------------------------------------------------------------------------
	// <copyright company="Microsoft">
	//     Copyright (c) 2006-2009 Microsoft Corporation.  All rights reserved.
	// </copyright>
	//------------------------------------------------------------------------------

	public static class BingMapsTileSystem
	{

		public const double EarthRadius = 6378137;
		public const double EarthCircumference = 40075016.69;
		public const double MinLatitude = -85.05112878;
		private const double MaxLatitude = 85.05112878;
		private const double MinLongitude = -180;
		private const double MaxLongitude = 180;


		public static int UsefulDigitsAtResolution(double resolution)
		{
			return 1 + (int)Math.Floor(Math.Abs(Math.Log10(resolution * 360d / EarthCircumference)));
		}

		/// <summary>
		/// Clips a number to the specified minimum and maximum values.
		/// </summary>
		/// <param name="n">The number to clip.</param>
		/// <param name="minValue">Minimum allowable value.</param>
		/// <param name="maxValue">Maximum allowable value.</param>
		/// <returns>The clipped value.</returns>
		private static double Clip(double n, double minValue, double maxValue)
		{
			return Math.Min(Math.Max(n, minValue), maxValue);
		}



		/// <summary>
		/// Determines the map width and height (in pixels) at a specified level
		/// of detail.
		/// </summary>
		/// <param name="levelOfDetail">Level of detail, from 1 (lowest detail)
		/// to 23 (highest detail).</param>
		/// <returns>The map width and height in pixels.</returns>
		public static uint MapSize(int levelOfDetail)
		{
			return (uint)256 << levelOfDetail;
		}



		/// <summary>
		/// Determines the ground resolution (in meters per pixel) at a specified
		/// latitude and level of detail.
		/// </summary>
		/// <param name="latitude">Latitude (in degrees) at which to measure the
		/// ground resolution.</param>
		/// <param name="levelOfDetail">Level of detail, from 1 (lowest detail)
		/// to 23 (highest detail).</param>
		/// <returns>The ground resolution, in meters per pixel.</returns>
		public static double GroundResolution(double latitude, int levelOfDetail)
		{
			latitude = Clip(latitude, MinLatitude, MaxLatitude);
			return Math.Cos(latitude * Math.PI / 180) * 2 * Math.PI * EarthRadius / MapSize(levelOfDetail);
		}



		/// <summary>
		/// Determines the map scale at a specified latitude, level of detail,
		/// and screen resolution.
		/// </summary>
		/// <param name="latitude">Latitude (in degrees) at which to measure the
		/// map scale.</param>
		/// <param name="levelOfDetail">Level of detail, from 1 (lowest detail)
		/// to 23 (highest detail).</param>
		/// <param name="screenDpi">Resolution of the screen, in dots per inch.</param>
		/// <returns>The map scale, expressed as the denominator N of the ratio 1 : N.</returns>
		public static double MapScale(double latitude, int levelOfDetail, int screenDpi)
		{
			return GroundResolution(latitude, levelOfDetail) * screenDpi / 0.0254;
		}



		/// <summary>
		/// Converts a point from latitude/longitude WGS-84 coordinates (in degrees)
		/// into pixel XY coordinates at a specified level of detail.
		/// </summary>
		/// <param name="latitude">Latitude of the point, in degrees.</param>
		/// <param name="longitude">Longitude of the point, in degrees.</param>
		/// <param name="levelOfDetail">Level of detail, from 1 (lowest detail)
		/// to 23 (highest detail).</param>
		public static Tuple<int, int> LatLongToPixelXY(double latitude, double longitude, int levelOfDetail)
		{
			latitude = Clip(latitude, MinLatitude, MaxLatitude);
			longitude = Clip(longitude, MinLongitude, MaxLongitude);

			double x = (longitude + 180) / 360;
			double sinLatitude = Math.Sin(latitude * Math.PI / 180);
			double y = 0.5 - Math.Log((1 + sinLatitude) / (1 - sinLatitude)) / (4 * Math.PI);

			uint mapSize = MapSize(levelOfDetail);

			int pixelX = (int)Clip(x * mapSize + 0.5, 0, mapSize - 1);
			int pixelY = (int)Clip(y * mapSize + 0.5, 0, mapSize - 1);

			return new Tuple<int, int>(pixelX, pixelY);
		}



		/// <summary>
		/// Converts a pixel from pixel XY coordinates at a specified level of detail
		/// into latitude/longitude WGS-84 coordinates (in degrees).
		/// </summary>
		/// <param name="pixelX">X coordinate of the point, in pixels.</param>
		/// <param name="pixelY">Y coordinates of the point, in pixels.</param>
		/// <param name="levelOfDetail">Level of detail, from 1 (lowest detail)
		/// to 23 (highest detail).</param>
		public static Tuple<double, double> PixelXYToLatLong(int pixelX, int pixelY, int levelOfDetail)
		{
			double mapSize = MapSize(levelOfDetail);
			double x = (Clip(pixelX, 0, mapSize - 1) / mapSize) - 0.5;
			double y = 0.5 - (Clip(pixelY, 0, mapSize - 1) / mapSize);


			double latitude = 90 - 360 * Math.Atan(Math.Exp(-y * 2 * Math.PI)) / Math.PI;
			double longitude = 360 * x;
			return new Tuple<double, double>(latitude, longitude);
		}



		/// <summary>
		/// Converts pixel XY coordinates into tile XY coordinates of the tile containing
		/// the specified pixel.
		/// </summary>
		/// <param name="pixelX">Pixel X coordinate.</param>
		/// <param name="pixelY">Pixel Y coordinate.</param>
		public static Tuple<int, int> PixelXYToTileXY(int pixelX, int pixelY)
		{
			int tileX = pixelX / 256;
			int tileY = pixelY / 256;
			return new Tuple<int, int>(tileX, tileY);
		}



		/// <summary>
		/// Converts tile XY coordinates into pixel XY coordinates of the upper-left pixel
		/// of the specified tile.
		/// </summary>
		/// <param name="tileX">Tile X coordinate.</param>
		/// <param name="tileY">Tile Y coordinate.</param>
		public static Tuple<int, int> TileXYToPixelXY(int tileX, int tileY)
		{
			int pixelX = tileX * 256;
			int pixelY = tileY * 256;
			return new Tuple<int, int>(pixelX, pixelY);
		}



		/// <summary>
		/// Converts tile XY coordinates into a QuadKey at a specified level of detail.
		/// </summary>
		/// <param name="tileX">Tile X coordinate.</param>
		/// <param name="tileY">Tile Y coordinate.</param>
		/// <param name="levelOfDetail">Level of detail, from 1 (lowest detail)
		/// to 23 (highest detail).</param>
		/// <returns>A string containing the QuadKey.</returns>
		public static string TileXYToQuadKey(int tileX, int tileY, int levelOfDetail)
		{
			StringBuilder quadKey = new StringBuilder();
			for (int i = levelOfDetail; i > 0; i--)
			{
				char digit = '0';
				int mask = 1 << (i - 1);
				if ((tileX & mask) != 0)
				{
					digit++;
				}
				if ((tileY & mask) != 0)
				{
					digit++;
					digit++;
				}
				quadKey.Append(digit);
			}
			return quadKey.ToString();
		}



		/// <summary>
		/// Converts a QuadKey into tile XY coordinates.
		/// </summary>
		/// <param name="quadKey">QuadKey of the tile.</param>
		/// <returns>A tuple with following values
		/// Item1: parameter receiving the tile X coordinate.</param>
		/// Item2: parameter receiving the tile Y coordinate.</param>
		/// Item3: parameter receiving the level of detail.</param>
		public static Tuple<int, int, int> QuadKeyToTileXY(string quadKey)
		{
			int tileX = 0, tileY = 0;
			int levelOfDetail = quadKey.Length;
			for (int i = levelOfDetail; i > 0; i--)
			{
				int mask = 1 << (i - 1);
				switch (quadKey[levelOfDetail - i])
				{
					case '0':
						break;

					case '1':
						tileX |= mask;
						break;

					case '2':
						tileY |= mask;
						break;

					case '3':
						tileX |= mask;
						tileY |= mask;
						break;

					default:
						throw new ArgumentException("Invalid QuadKey digit sequence.");
				}
			}

			return new Tuple<int, int, int>(tileX, tileY, levelOfDetail);
		}

		public static string GetTileInfo(int x, int y, int zoom)
		{
			StringBuilder tileInfo = new StringBuilder();
			tileInfo.AppendFormat("Bing: ({0},{1})", x, y);
			tileInfo.AppendLine();
			tileInfo.AppendFormat("Quad: {0}", BingMapsTileSystem.TileXYToQuadKey(x, y, zoom));
			tileInfo.AppendLine();
			tileInfo.AppendFormat("Zoom: {0}", zoom);
			return tileInfo.ToString();
		}
	}
}
