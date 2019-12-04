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

using Microsoft.Win32;
using System.IO;
using System.Windows;

namespace GisFabriek.WktExporter
{
    /// <summary>
    /// Interaction logic for ShowWktWindow.xaml
    /// </summary>
    public partial class ShowWktWindow
    {
        public ShowWktWindow()
        {
            InitializeComponent();
            ExportButton.IsEnabled = false;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public string WktText
        {
            get => WktTextBlock.Text;
            set
            {
                WktTextBlock.Text = value;
                ExportButton.IsEnabled = !string.IsNullOrWhiteSpace(value);
            }
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog {Filter = Localization.Resources.SaveFileDialogFilterText};
            if (saveFileDialog.ShowDialog() == true)
            {
                File.WriteAllText(saveFileDialog.FileName, WktText);
            }
        }

        private void ClipBoardButton_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(WktText);
        }
    }
}
