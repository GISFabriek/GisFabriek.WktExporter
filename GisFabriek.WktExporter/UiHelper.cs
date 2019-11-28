using System;
using System.Windows;
using MessageBox = ArcGIS.Desktop.Framework.Dialogs.MessageBox;

namespace GisFabriek.WktExporter
{
    internal static class UiHelper
    {
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