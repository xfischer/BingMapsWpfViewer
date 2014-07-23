using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using BingMapsWPFViewer.Framework;
using BingMapsWPFViewer.Model;
using BingMapsWPFViewer.Model.Features;
using BingMapsWPFViewer.Tools;
using Microsoft.Maps.MapControl.WPF;
using Microsoft.SqlServer.Types;
using System.Text;

namespace BingMapsWPFViewer.ViewModel
{
	/// <summary>
	/// BingMap MapLayer surrogate
	/// Performs data retrieval / conversion / drawing
	/// </summary>
	public class MapVectorLayer : MapLayer, IBingMapsWPFViewerLayer
	{

		#region Members

		bool _cancelPending = false;
		Dictionary<Guid, Feature> _featuresOnScreen;
		DataTemplate _tooltipDataTemplate;

		public VectorLayerBase VectorLayerBase { get; private set; }

		#region IBingMapsWPFViewerLayer Membres

		public Guid Id
		{
			get { return VectorLayerBase.Id; }
		}

		#endregion

		#endregion Members

		public MapVectorLayer(VectorLayerBase p_VectorLayerBase)
			: base()
		{
			VectorLayerBase = p_VectorLayerBase;
			_tooltipDataTemplate = Application.Current.TryFindResource("FeatureTooltipDataTemplate") as DataTemplate;
			
		}

		#region Refresh view and feature drawing

		public void RefreshView()
		{
			_cancelPending = false;

			this.RemoveAllShapes();

			// Add features on map
			this.DrawFeatures(VectorLayerBase.Features);
		}

		public void CancelRefresh()
		{
			_cancelPending = true;
		}

		public void DrawFeatures(List<Feature> featureList)
		{
			Stopwatch watchDraw = new Stopwatch();

			if (featureList != null)
			{
				_featuresOnScreen = new Dictionary<Guid, Feature>();
				foreach (Feature feature in featureList)
				{
					if (_cancelPending)
					{
						DebugHelper.WriteLine(this, "DrawFeatures cancelled");
						break;
					}
					watchDraw.Start();
					DrawFeature(feature);
					watchDraw.Stop();

					_featuresOnScreen.Add(feature.FeatureID, feature);
				}
			}

			DebugHelper.WriteLine(this, string.Format("Draw time : {0} ms", watchDraw.ElapsedMilliseconds));
		}

		private void DrawFeature(Feature feature)
		{
			SqlGeometry geom = feature.Geometry;
			if (geom.IsNull || geom.STIsEmpty().Value)
				return;

			// Default styles
			SolidColorBrush stroke = new SolidColorBrush(Colors.Blue);
			SolidColorBrush fill = new SolidColorBrush(VectorLayerBase.FillColor); //     new SolidColorBrush(Color.FromArgb(64, 0, 0, 255));

			List<FrameworkElement> shapes = new List<FrameworkElement>();

			// Draw each feature if geom is multi geom
			if (geom.InstanceOf("GeometryCollection"))
			{
				for (int i = 1; i <= geom.STNumGeometries().Value; i++)
					shapes.AddRange(this.FromSqlGeometryToMapUIElement(geom.STGeometryN(i), stroke, fill));
			}
			else
				shapes.AddRange(this.FromSqlGeometryToMapUIElement(geom, stroke, fill));

			foreach (FrameworkElement shape in shapes)
			{
				AddShape(shape, feature);
			}


			// Add labels
			List<string> labels = feature.Attributes
												 .Where(kv => kv.Key.Usage.HasFlag(enFeatureFieldUsage.Label))
												 .Select(kv => kv.Value.ToString())
												 .ToList();
			if (labels.Count > 0)
			{
				string label = string.Join(Environment.NewLine, labels.ToArray());
				TextBlock txtBlock = new TextBlock();
				//txtBlock.Width = 400;
				txtBlock.HorizontalAlignment = HorizontalAlignment.Stretch;
				txtBlock.TextWrapping = TextWrapping.Wrap;
				txtBlock.Text = label;
				txtBlock.TextAlignment = TextAlignment.Center;
				txtBlock.Foreground = new SolidColorBrush(Colors.Black);
				txtBlock.Background = new SolidColorBrush(Colors.White);

				this.AddChild(txtBlock, new Location(feature.Centroid.STY.Value, feature.Centroid.STX.Value), PositionOrigin.Center);
			}

		}

		#region Add / Remove shapes

		private void AddShape(FrameworkElement shape, Feature feature)
		{
			RegistrerShapeEvents(shape);						// Subscribe to events (mouse, ...)

			shape.ToolTip = BuildTooltip(feature);	// set tooltip

			shape.Tag = feature.FeatureID;					// set shape tag (works as ID)

			this.Children.Add(shape);								// add shape to layer
		}

		private void RemoveAllShapes()
		{
			// Event deregistration
			foreach (FrameworkElement shape in this.Children)
			{
				UnregistrerShapeEvents(shape);
			}
			this.Children.Clear();
		}

		#endregion

		#region Shape event handlers

		#region Register / unregister events


		void RegistrerShapeEvents(FrameworkElement shape)
		{
			shape.MouseEnter += shape_MouseEnter;
			shape.MouseLeave += shape_MouseLeave;
		}



		void UnregistrerShapeEvents(FrameworkElement shape)
		{
			shape.MouseEnter -= shape_MouseEnter;
			shape.MouseLeave -= shape_MouseLeave;
		}

		#endregion Register / unregister events
				
		#region Highlight shape on mouse hover

		void shape_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
		{
			Guid _curGuid = (Guid)(sender as FrameworkElement).Tag;

			foreach (var element in this.Children)
			{
				FrameworkElement el = element as FrameworkElement;
				if (el != null)
				{
					if (el.Tag != null && el.Tag.Equals(_curGuid))
					{
						if (el is MapPolygon)
						{
							((MapPolygon)el).StrokeThickness /= 2d;
						}
						else if (el is MapPolyline)
						{
							((MapPolyline)el).StrokeThickness /= 2d;
						}
						else if (el is Pushpin)
						{
							// TODO : find hover behavior
						}
					}
				}
			}
		}

		void shape_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
		{
			Guid _curGuid = (Guid)(sender as FrameworkElement).Tag;

			foreach (var element in this.Children)
			{
				FrameworkElement el = element as FrameworkElement;
				if (el != null)
				{
					if (el.Tag != null && el.Tag.Equals(_curGuid))
					{
						if (el is MapPolygon)
						{
							((MapPolygon)el).StrokeThickness *= 2d;
						}
						else if (el is MapPolyline)
						{
							((MapPolyline)el).StrokeThickness *= 2d;
						}
						else if (el is Pushpin)
						{
							// TODO : find hover behavior
						}
					}
				}
			}

		}

		#endregion Highlight shape on mouse hover

		#endregion Shape event handlers

		#endregion Refresh view and feature drawing

		#region Utility

		private ToolTip BuildTooltip(Feature feature)
		{
			ToolTip tooltip = new ToolTip();
			tooltip.Content = feature;
			tooltip.ContentTemplate = _tooltipDataTemplate;


			return tooltip;
		}

		private string ShortenString(string str, int maxLen)
		{
			if (str.Length > maxLen)
				return str.Substring(0, maxLen);
			else
				return str;
		}

		#endregion

		#region SqlGeometry <-> Bing shape Conversion

		private List<FrameworkElement> FromSqlGeometryToMapUIElement(SqlGeometry geom, Brush stroke, Brush fill)
		{
			List<FrameworkElement> listShape = new List<FrameworkElement>();

			try
			{
				if (!geom.STIsValid().Value)
					geom = geom.MakeValid();

				//if (geom.STDimension().Value != 2)
				//  return listPoly;

				int numGeom = geom.STNumGeometries().Value;
				for (int i = 1; i <= numGeom; i++)
				{
					SqlGeometry curGeom = geom.STGeometryN(i);
					switch (curGeom.STGeometryType().ToString())
					{
						case "Polygon":

							#region Polygon

							// Must check if interior rings because of Bing Maps bug with holes
							// See http://rbrundritt.wordpress.com/2009/02/18/advance-polygon-shapes-in-virtual-earth/
							bool hasInteriorRings = curGeom.STNumInteriorRing().Value > 0;
							if (hasInteriorRings)
								listShape.AddRange(this.ExtractRings(geom, stroke));

							MapPolygon poly = new MapPolygon();
							poly.Stroke = hasInteriorRings ? new SolidColorBrush(Colors.Transparent) : stroke;
							poly.Fill = fill;
							poly.StrokeLineJoin = PenLineJoin.Bevel;

							//poly.MouseLeftButtonUp += new MouseButtonEventHandler(poly_MouseLeftButtonUp);
							//poly.MouseLeftButtonDown += new MouseButtonEventHandler(poly_MouseLeftButtonDown);
							//poly.MouseLeave += (ol, el) => { txtDept.Text = "Dept: -"; };
							//poly.MouseEnter += (o, e) => { txtDept.Text = "Dept:" + p_ShapeTag.ShapeLabel; };

							poly.Locations = new LocationCollection();


							int numPoints = curGeom.STNumPoints().Value;
							for (int j = 1; j <= numPoints; j++)
							{
								SqlGeometry point = curGeom.STPointN(j);
								poly.Locations.Add(new Location(point.STY.Value, point.STX.Value));
							}

							listShape.Add(poly);

							#endregion Polygon

							break;
						case "LineString":

							#region Polyline

							MapPolyline polyline = new MapPolyline();
							polyline.Stroke = stroke;
							polyline.StrokeLineJoin = PenLineJoin.Bevel;
							//poly.MouseLeftButtonUp += new MouseButtonEventHandler(poly_MouseLeftButtonUp);
							//poly.MouseLeftButtonDown += new MouseButtonEventHandler(poly_MouseLeftButtonDown);
							//poly.MouseLeave += (ol, el) => { txtDept.Text = "Dept: -"; };
							//poly.MouseEnter += (o, e) => { txtDept.Text = "Dept:" + p_ShapeTag.ShapeLabel; };

							polyline.Locations = new LocationCollection();

							int numLinePoints = curGeom.STNumPoints().Value;
							for (int j = 1; j <= numLinePoints; j++)
							{
								SqlGeometry point = curGeom.STPointN(j);
								polyline.Locations.Add(new Location(point.STY.Value, point.STX.Value));
							}

							listShape.Add(polyline);

							#endregion

							break;

						case "Point":

							#region Point

							Pushpin pin = new Pushpin();
							pin.Location = new Location(curGeom.STY.Value, curGeom.STX.Value);
							listShape.Add(pin);

							#endregion

							break;

						default:
							// Other shape types
							DebugHelper.WriteLine(this, "Geometry type '" + curGeom.STGeometryType().ToString() + "' not implemented");
							//throw new NotImplementedException("Geom type not implemented yet");
							break;
					}

				}

			}
			catch (Exception v_ex)
			{
				throw v_ex;
			}

			return listShape;
		}

		private List<FrameworkElement> ExtractRings(SqlGeometry geom, Brush stroke)
		{
			List<FrameworkElement> listRings = new List<FrameworkElement>();

			if (geom.STNumInteriorRing().Value > 0)
			{
				for (int i = 1; i <= geom.STNumInteriorRing().Value; i++)
				{
					LocationCollection inner = new LocationCollection();
					for (int j = 1; j <= geom.STInteriorRingN(i).STNumPoints().Value; j++)
						inner.Add(new Location(geom.STInteriorRingN(i).STPointN(j).STY.Value, geom.STInteriorRingN(i).STPointN(j).STX.Value));

					listRings.Add(new MapPolyline() { Locations = inner, Stroke = stroke });
				}

				LocationCollection outer = new LocationCollection();
				for (int i = 1; i <= geom.STExteriorRing().STNumPoints().Value; i++)
					outer.Add(new Location(geom.STExteriorRing().STPointN(i).STY.Value, geom.STExteriorRing().STPointN(i).STX.Value));

				listRings.Add(new MapPolyline() { Locations = outer, Stroke = stroke });

			}

			return listRings;
		}

		#endregion SqlGeometry -> Bing shape Conversion




	}
}
