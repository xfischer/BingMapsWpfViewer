using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using BingMapsWPFViewer.Model.Services;
using BingMapsWPFViewer.Model;

namespace BingMapsWPFViewer.Services.Mocks
{
	public class LayersServiceMock : ILayersService
	{

		internal List<LayerBase> _layers;

		public LayersServiceMock()
		{
			_layers = new List<LayerBase>();
			_layers.Add(new TileLayerXYZ("https://a.tile.openstreetmap.fr/osmfr/{2}/{0}/{1}.png", (x, y, z) => new Uri(string.Format("https://a.tile.openstreetmap.fr/osmfr/{2}/{0}/{1}.png", x, y, z)))
			{
				DisplayName = "Cloud Front",
				ZIndex = 0
			});
			//_layers.Add(new TileGridLayer()
			//{
			//	DisplayName = "Tile Grid Layer",
			//	ZIndex = 1
			//});


			//_layers.Add(new SqlServerLayer("Data Source=XAVIER-IMAC\\SQLEXPRESS2012;Initial Catalog=SpatialData_Sample;Integrated Security=True;Asynchronous Processing=True;", "IGN_CANTON_WGS84", "geom4326_geom", 4326, "IDX_geom4326_geom")
			//{
			//  DisplayName = "IGN_CANTON_WGS84",
			//  ZIndex = 1
			//});
			//_layers.Add(new SqlServerLayer("Data Source=XAVIER-IMAC\\SQLEXPRESS2012;Initial Catalog=SpatialData_Sample;Integrated Security=True", "IGN_CANTON_WGS84", "geom4326_geom", 4326, "IDX_geom4326_geom")
			//_layers.Add(new SqlServerLayer("Data Source='XAVIER-IMAC\\SQLEXPRESS2012';Integrated Security=True;Initial Catalog='SampleSpatialData';Pooling=False;", "IGN_DEPARTEMENT_4326", "geom4326_geom", 4326, "IDX_geom4326_geom")
			//{
			//  DisplayName = "IGN_CANTON_WGS84",
			//  ZIndex = 1
			//});
		}


		#region IServiceBase<LayerBase,CriterionLayer> Membres

		public bool Create(Model.LayerBase toCreate)
		{
			_layers.Add(toCreate);
			return true;
		}

		public bool Update(Model.LayerBase toUpdate)
		{
			LayerBase layer = _layers.Where(l => l.Id == toUpdate.Id).FirstOrDefault();
			if (layer == null)
				return false;

			layer = toUpdate;
			return true;

		}

		public bool Delete(Model.LayerBase toDelete)
		{
			_layers.Remove(toDelete);
			return true;
		}

		public Model.LayerBase Load(Guid idToGet)
		{
			throw new NotImplementedException();
		}

		public List<Model.LayerBase> Load(CriterionLayer criterion)
		{
			return _layers
							.Select(l => l.Clone() as LayerBase)
							.ToList();
		}

		public bool LoadAsync(CriterionLayer criterion, Action<Model.AsyncResponse, List<Model.LayerBase>> callback)
		{
			throw new NotImplementedException();
		}

		public void ApplyChanges()
		{
			throw new NotImplementedException();
		}

		public void DiscardChanges()
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}

