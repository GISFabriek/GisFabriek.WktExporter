using System.Windows;

namespace GisFabriek.WktExporter
{
    /// <summary>
    /// Interaction logic for AddWktWindow.xaml
    /// </summary>
    public partial class AddWktWindow
    {
        public AddWktWindow()
        {
            InitializeComponent();
        }

        public string WktText => WktTextBox.Text;

        public string FeatureLayerName
        {
            set => FeatureLayerText.Content = value;
        }

        public string TypeInfo
        {
            set => TypeInfoText.Content = value;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            Close();
        }

        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            Close();
        }
    }
}
