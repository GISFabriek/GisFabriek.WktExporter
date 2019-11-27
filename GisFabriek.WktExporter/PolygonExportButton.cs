using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using MessageBox = ArcGIS.Desktop.Framework.Dialogs.MessageBox;

namespace GisFabriek.WktExporter
{
    internal class PolygonExportButton : Button
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
                            var text = await geometry.ToWellKnownText();
                            wktWindowShown = true;
                            RunOnUiThread(() => Show(text));
                        }
                        break;
                    }
                }

                if (!wktWindowShown)
                {
                    RunOnUiThread(() => { MessageBox.Show("Please select a Feature","No Feature selected");});
                }
            });
        }

        public void Show(string wktString)
        {
            if (_showWktWindow != null)
                return;
            _showWktWindow = new ShowWktWindow {Owner = Application.Current.MainWindow, WktText = wktString};
            _showWktWindow.Closed += (o, e) => { _showWktWindow = null; };
            _showWktWindow.Show();
        }

        /// <summary>
        /// utility function to enable an action to run on the UI thread (if not already)
        /// </summary>
        /// <param name="action">the action to execute</param>
        /// <returns></returns>
        internal static void RunOnUiThread(Action action)
        {
            try
            {
                if (IsOnUiThread)
                {
                    action();
                }
                else
                {
                    if (Application.Current.Dispatcher != null) Application.Current.Dispatcher.Invoke(action);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($@"Error in OpenAndActivateMap: {ex.Message}");
            }
        }


        /// <summary>
        /// Determines whether the calling thread is the thread associated with this 
        /// System.Windows.Threading.Dispatcher, the UI thread.
        /// 
        /// If called from a View model test it always returns true.
        /// </summary>
        public static bool IsOnUiThread => Application.Current.Dispatcher != null && (ArcGIS.Desktop.Framework.FrameworkApplication.TestMode || Application.Current.Dispatcher.CheckAccess());
    }
}

