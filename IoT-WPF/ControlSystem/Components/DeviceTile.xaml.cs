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
    /// Interaction logic for DeviceTile.xaml
    /// </summary>
    public partial class DeviceTile : UserControl
    {
        public DeviceTile()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty DeviceTitleProperty = DependencyProperty.Register("DeviceTitle", typeof(string), typeof(DeviceTile));
        public string DeviceTitle
        {
            get { return (string)GetValue(DeviceTitleProperty); }
            set { SetValue(DeviceTitleProperty, value); }
        }

        public static readonly DependencyProperty DeviceTypeProperty = DependencyProperty.Register("DeviceType", typeof(string), typeof(DeviceTile));
        public string DeviceType
        {
            get { return (string)GetValue(DeviceTypeProperty); }
            set { SetValue(DeviceTypeProperty, value); }
        }

        public static readonly DependencyProperty DeviceIconActiveProperty = DependencyProperty.Register("DeviceIconActive", typeof(string), typeof(DeviceTile));
        public string DeviceIconActive
        {
            get { return (string)GetValue(DeviceIconActiveProperty); }
            set { SetValue(DeviceIconActiveProperty, value); }
        }

        public static readonly DependencyProperty DeviceIconInactiveProperty = DependencyProperty.Register("DeviceIconInactive", typeof(string), typeof(DeviceTile));
        public string DeviceIconInactive
        {
            get { return (string)GetValue(DeviceIconInactiveProperty); }
            set { SetValue(DeviceIconInactiveProperty, value); }
        }

        public static readonly DependencyProperty DeviceFontInactiveProperty = DependencyProperty.Register("DeviceFontActive", typeof(string), typeof(DeviceTile));
        public string DeviceFontActive
        {
            get { return (string)GetValue(DeviceFontInactiveProperty); }
            set { SetValue(DeviceFontInactiveProperty, value); }
        }

        public static readonly DependencyProperty DeviceFontActiveProperty = DependencyProperty.Register("DeviceFontInactive", typeof(string), typeof(DeviceTile));
        public string DeviceFontInactive
        {
            get { return (string)GetValue(DeviceFontActiveProperty); }
            set { SetValue(DeviceFontActiveProperty, value); }
        }
    }
}
