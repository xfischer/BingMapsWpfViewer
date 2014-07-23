using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BingMapsWPFViewer.Model;
using BingMapsWPFViewer.Tools;
using BingMapsWPFViewer.Model.Features;
using BingMapsWPFViewer.Model.Services;
using Microsoft.SqlServer.Types;

namespace BingMapsWPFViewer.Services
{
	public sealed class TileGridFeatureService
		: FeatureServiceBase<TileGridLayer, SpatialQueryCriterion>
	{
		public override List<Feature> LoadConcrete(TileGridLayer layer, SpatialQueryCriterion criterion)
		{
			List<Feature> features = new List<Feature>();
			try
			{
				int zoomLevel = (int)Math.Round(criterion.MapViewport.ZoomLevel, 0);
				Tuple<int, int> northWestPixel = BingMapsTileSystem.LatLongToPixelXY(criterion.BoundingBox.YMax, criterion.BoundingBox.XMin, zoomLevel);
				Tuple<int, int> southEastPixel = BingMapsTileSystem.LatLongToPixelXY(criterion.BoundingBox.YMin, criterion.BoundingBox.XMax, zoomLevel);
				Tuple<int, int> northWestTile = BingMapsTileSystem.PixelXYToTileXY(northWestPixel.Item1, northWestPixel.Item2);
				Tuple<int, int> southEastTile = BingMapsTileSystem.PixelXYToTileXY(southEastPixel.Item1, southEastPixel.Item2);
				for (int i = northWestTile.Item1; i <= southEastTile.Item1; i++)
					for (int j = northWestTile.Item2; j <= southEastTile.Item2; j++)
					{
						Dictionary<FeatureField, object> properties = new Dictionary<FeatureField, object>();

						string v_tileInfo = BingMapsTileSystem.GetTileInfo(i, j, zoomLevel);
						properties.Add(new FeatureField("TileInfo", "varchar", 1, enFeatureFieldUsage.Label), v_tileInfo);
						properties.Add(new FeatureField("Bing", "varchar",2, enFeatureFieldUsage.Label), string.Format("{0},{1}",i,j));
						properties.Add(new FeatureField("QuadKey", "varchar", 3, enFeatureFieldUsage.Label), BingMapsTileSystem.TileXYToQuadKey(i, j, zoomLevel));
						properties.Add(new FeatureField("Zoom", "int", 4, enFeatureFieldUsage.Label), zoomLevel);

						/*
						 * 		StringBuilder tileInfo = new StringBuilder();
			tileInfo.AppendFormat("Bing: ({0},{1})", x, y);
			tileInfo.AppendLine();
			tileInfo.AppendFormat("Quad: {0}", BingMapsTileSystem.TileXYToQuadKey(x, y, zoom));
			tileInfo.AppendLine();
			tileInfo.AppendFormat("Zoom: {0}", zoom);
			return tileInfo.ToString();
						 * */

						SqlGeometry geom = Tile2SqlGeometry(i, j, zoomLevel);
						Feature feature = new Feature(geom, geom.STCentroid(), properties);
						features.Add(feature);
					}
			}
			catch (Exception)
			{
				throw;
			}
			return features;
		}

		public SqlGeometry Tile2SqlGeometry(int tileX, int tileY, int zoomLevel)
		{
			Tuple<int, int> tileNWPixels = BingMapsTileSystem.TileXYToPixelXY(tileX, tileY);
			Tuple<int, int> tileSEPixels = new Tuple<int, int>(tileNWPixels.Item1 + 255, tileNWPixels.Item2 + 255);
			
			Tuple<double, double> NWCoords = BingMapsTileSystem.PixelXYToLatLong(tileNWPixels.Item1, tileNWPixels.Item2, zoomLevel);
			Tuple<double, double> SECoords = BingMapsTileSystem.PixelXYToLatLong(tileSEPixels.Item1, tileSEPixels.Item2, zoomLevel);
			SqlGeometryBuilder geomBuilder = new SqlGeometryBuilder();
			geomBuilder.SetSrid(4326);
			geomBuilder.BeginGeometry(OpenGisGeometryType.Polygon);
			geomBuilder.BeginFigure(NWCoords.Item2, NWCoords.Item1);
			geomBuilder.AddLine(SECoords.Item2, NWCoords.Item1);
			geomBuilder.AddLine(SECoords.Item2, SECoords.Item1);
			geomBuilder.AddLine(NWCoords.Item2, SECoords.Item1);
			geomBuilder.AddLine(NWCoords.Item2, NWCoords.Item1);
			geomBuilder.EndFigure();
			geomBuilder.EndGeometry();
			return geomBuilder.ConstructedGeometry;

		}


		public override void Cancel()
		{
			
		}
	}
}
