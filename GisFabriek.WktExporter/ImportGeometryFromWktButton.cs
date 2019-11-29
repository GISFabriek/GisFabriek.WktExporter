using ArcGIS.Core.CIM;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using ArcGIS.Desktop.Framework;
using MessageBox = ArcGIS.Desktop.Framework.Dialogs.MessageBox;

namespace GisFabriek.WktExporter
{
    internal class ImportGeometryFromWktButton : Button
    {
        private AddWktWindow _addWktWindow;
        private FeatureLayer _selectedFeatureLayer;

        private const string EditOperationName = "Create Feature from WKT";
        private const string NotificationImageUrl =
            @"pack://application:,,,/GisFabriek.WktExporter;component/Images/GISFabriek.png";

        protected override void OnClick()
        {
            var mapView = MapView.Active;
            var featureLayers = mapView.Map.Layers.OfType<FeatureLayer>().Where(x => x.IsSelectable).ToList();

            var selectedLayers = mapView.GetSelectedLayers();
            if (selectedLayers.Count == 0)
            {
                MessageBox.Show(Localization.Resources.SelectFeatureLayerMessage);
                return;
            }

            var selectedFeatureLayers = (from items in selectedLayers
                where featureLayers.Contains(items)
                select items).ToList();
            if (selectedFeatureLayers.Count == 0)
            {
                MessageBox.Show(Localization.Resources.SelectFeatureLayerMessage);
                return;
            }

            if (selectedFeatureLayers.Count > 1)
            {
                MessageBox.Show(Localization.Resources.SelectOneSingleFeatureLayerWithCorrectGeometryType);
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
                Owner = Application.Current.MainWindow, FeatureLayerName = $"{Localization.Resources.LayerTextFragment}: {_selectedFeatureLayer.Name}",
                TypeInfo = $"{Localization.Resources.TypeTextFragment}: {shapeType}; {Localization.Resources.SpatialReferenceTextFragment}: {wkId}"
            };
            if (_addWktWindow.ShowDialog() == true)
            {
                var text = _addWktWindow.WktText;
                if (string.IsNullOrWhiteSpace(text))
                {
                    MessageBox.Show(Localization.Resources.EnterAValidWktStringMessage);
                    return;
                }

                var wktText = new WktText(text);
                if (!CompareType(wktText.Type, shapeType))
                {
                    var shapeName = shapeType.ToString();
                    shapeName = shapeName.Substring(12);
                    MessageBox.Show(string.Format(Localization.Resources.InvalidWktTypeMessageTemplate, shapeName));
                    return;
                }

                var geometry = await text.ToGeometry(wkId, true);
                var succeeded = await AddGeometry(_selectedFeatureLayer, geometry);
                if (!succeeded)
                {
                    MessageBox.Show(Localization.Resources.AddingFeatureDidNotSucceedErrorMessage);
                    return;
                }
                FrameworkApplication.AddNotification(new SucceededNotification()
                {
                    Title = Localization.Resources.ReadyTextFragment,
                    Message = Localization.Resources.WktGeometryAddedMessage,
                    ImageUrl = NotificationImageUrl
                });
            }
            _addWktWindow = null;

        }

        internal class SucceededNotification : Notification
        {
            protected override void OnClick()
            {
                FrameworkApplication.RemoveNotification(this);
            }
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
                var editOperation = new EditOperation {Name = EditOperationName };
                editOperation.Create(featureLayer, geometry);
                return editOperation.ExecuteAsync();
            });

        }
    }
}
