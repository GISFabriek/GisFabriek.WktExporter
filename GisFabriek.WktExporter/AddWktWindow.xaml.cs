using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GisFabriek.WktExporter
{
    /// <summary>
    /// Interaction logic for AddWktWindow.xaml
    /// </summary>
    public partial class AddWktWindow : ArcGIS.Desktop.Framework.Controls.ProWindow
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
