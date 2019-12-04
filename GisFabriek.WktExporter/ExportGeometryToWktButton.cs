/*
    MIT License

    Copyright (c) 2019 De GISFabriek

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all
    copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    SOFTWARE.
*/

using ArcGIS.Core.Data;
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

