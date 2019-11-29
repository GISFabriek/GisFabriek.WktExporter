using ArcGIS.Core.CIM;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using MessageBox = ArcGIS.Desktop.Framework.Dialogs.MessageBox;

namespace GisFabriek.WktExporter
{
    internal class ImportGeometryFromWktButton : Button
    {
        private AddWktWindow _addWktWindow;
        private FeatureLayer _selectedFeatureLayer;

        protected override void OnClick()
        {
            var mapView = MapView.Active;
            var featureLayers = mapView.Map.Layers.OfType<FeatureLayer>().Where(x => x.IsSelectable).ToList();

            var selectedLayers = mapView.GetSelectedLayers();
            if (selectedLayers.Count == 0)
            {
                MessageBox.Show("Please select a FeatureLayer");
                return;
            }

            var selectedFeatureLayers = (from items in selectedLayers
                where featureLayers.Contains(items)
                select items).ToList();
            if (selectedFeatureLayers.Count == 0)
            {
                MessageBox.Show("Please select a FeatureLayer");
                return;
            }

            if (selectedFeatureLayers.Count > 1)
            {
                MessageBox.Show("Please select one single FeatureLayer having the geometry type of the WKT");
                return;
            }

            _selectedFeatureLayer = (FeatureLayer) selectedFeatureLayers.First();
#pragma warning disable 4014
            Show();
#pragma warning restore 4014

        }

        private async Task Show()
        {
            if (_addWktWindow != null)
            {
                return;
            }

            var wkId = await QueuedTask.Run(() => _selectedFeatureLayer.GetSpatialReference().Wkid);
            var shapeType = await QueuedTask.Run(() => _selectedFeatureLayer.ShapeType);
            _addWktWindow = new AddWktWindow
            {
                Owner = Application.Current.MainWindow, FeatureLayerName = $"Layer: {_selectedFeatureLayer.Name}",
                TypeInfo = $"Type: {shapeType}; Spatial Reference: {wkId}"
            };
            if (_addWktWindow.ShowDialog() == true)
            {
                var text = _addWktWindow.WktText;
                if (string.IsNullOrWhiteSpace(text))
                {
                    MessageBox.Show("Please enter a valid WKT string");
                    return;
                }

                var wktText = new WktText(text);
                if (!CompareType(wktText.Type, shapeType))
                {
                    var shapeName = shapeType.ToString();
                    shapeName = shapeName.Substring(12);
                    MessageBox.Show($"WKT is not a {shapeName}. Please enter a valid {shapeName} WKT string");
                    return;
                }

                var geometry = await text.ToGeometry(wkId, true);
                var succeeded = await AddGeometry(_selectedFeatureLayer, geometry);
                if (!succeeded)
                {
                    MessageBox.Show("Adding feature not successful");
                    return;
                }

            }

            _addWktWindow = null;

        }

        private bool CompareType(WktType wktType, esriGeometryType geometryType)
        {
            switch (wktType)
            {
                case WktType.LineString:
                case WktType.MultiLineString:
                    if (geometryType == esriGeometryType.esriGeometryPolyline)
                    {
                        return true;
                    }

                    return false;
                case WktType.Point:
                    if (geometryType == esriGeometryType.esriGeometryPoint)
                    {
                        return true;
                    }

                    return false;
                case WktType.MultiPoint:
                    if (geometryType == esriGeometryType.esriGeometryMultipoint)
                    {
                        return true;
                    }

                    return false;
                case WktType.Polygon:
                case WktType.MultiPolygon:
                    if (geometryType == esriGeometryType.esriGeometryPolygon)
                    {
                        return true;
                    }

                    return false;
                default:
                    return false;
            }
        }

        private async Task<bool> AddGeometry(FeatureLayer featureLayer, Geometry geometry)
        {
           return await QueuedTask.Run(() =>
            {
                var editOperation = new EditOperation {Name = "Create Feature from WKT"};
                editOperation.Create(featureLayer, geometry);
                return editOperation.ExecuteAsync();
            });

        }
    }
}
