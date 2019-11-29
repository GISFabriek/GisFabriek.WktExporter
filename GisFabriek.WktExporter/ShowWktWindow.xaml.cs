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
