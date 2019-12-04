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