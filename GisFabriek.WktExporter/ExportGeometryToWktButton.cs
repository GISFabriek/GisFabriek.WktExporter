﻿using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using MessageBox = ArcGIS.Desktop.Framework.Dialogs.MessageBox;

namespace GisFabriek.WktExporter
{
    internal class ExportGeometryToWktButton : Button
    {
        private ShowWktWindow _showWktWindow;

        protected override void OnClick()
        {
            ExportFirstSelectedFeatureAsync();
        }

        public Task ExportFirstSelectedFeatureAsync()
        {
            return QueuedTask.Run(async () =>
            {
                var mapView = MapView.Active;
                if (mapView == null)
                {
                    return;
                }

                var featureLayers = mapView.Map.Layers.OfType<FeatureLayer>();
                var wktWindowShown = false;
                foreach (var featureLayer in featureLayers)
                {
                    var selection = featureLayer.GetSelection();
                    if (selection.GetCount() > 0)
                    {
                        var cursor = selection.Search();
                        cursor.MoveNext();
                        if (cursor.Current is Feature feature)
                        {
                            var geometry = feature.GetShape();

                            wktWindowShown = true;
                            UiHelper.RunOnUiThread(Show);
                            var text = await geometry.ToWellKnownText();
                            UiHelper.RunOnUiThread(() =>
                            {
                                if (_showWktWindow != null)
                                {
                                    _showWktWindow.WktText = text;
                                }
                            });
                        }
                        break;
                    }
                }

                if (!wktWindowShown)
                {
                    UiHelper.RunOnUiThread(() => { MessageBox.Show(Localization.Resources.SelectAFeatureMessage, Localization.Resources.NoFeatureSelectedCaption); });
                }
            });
        }

        public void Show()
        {
            if (_showWktWindow != null)
            {
                return;
            }
            _showWktWindow = new ShowWktWindow { Owner = Application.Current.MainWindow, WktText = Localization.Resources.PleaseWaitMessage };
            _showWktWindow.Closed += (o, e) => { _showWktWindow = null; };
            _showWktWindow.Show();
        }
    }
}

