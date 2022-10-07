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

namespace ControlSystem.Components
{
    /// <summary>
    /// Interaction logic for ThermometerTile.xaml
    /// </summary>
    public partial class ThermometerTile : UserControl
    {
        public ThermometerTile()
        {
            InitializeComponent();
        }


        public static readonly DependencyProperty TemperatureProperty = DependencyProperty.Register("Temperature", typeof(string), typeof(DeviceTile));
        public string Temperature
        {
            get { return (string)GetValue(TemperatureProperty); }
            set { SetValue(TemperatureProperty, value); }
        }

        public static readonly DependencyProperty HumidityProperty = DependencyProperty.Register("Humidity", typeof(string), typeof(DeviceTile));
        public string Humidity
        {
            get { return (string)GetValue(HumidityProperty); }
            set { SetValue(HumidityProperty, value); }
        }
    }
}
