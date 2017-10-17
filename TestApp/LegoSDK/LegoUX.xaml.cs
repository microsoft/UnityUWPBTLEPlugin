using LegoSDK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace TestApp.LegoSDK
{
    public sealed partial class LegoUX : UserControl
    {
        private LegoHub theHub;

        public LegoHub TheHub { get => theHub; set => theHub = value; }

        public LegoUX(LegoHub hub)
        {
            this.InitializeComponent();
            TheHub = hub;
        }


        private async void OnColorIndexTest(object sender, RoutedEventArgs e)
        {
            await TheHub.ColorIndexTest();
        }

        private async void OnColorRGBTest(object sender, RoutedEventArgs e)
        {
            await TheHub.ColorRGBTest();
        }

        private async void OnBrickMotorTest(object sender, RoutedEventArgs e)
        {
            await TheHub.BrickMotorTest();
        }

        private async void OnSingleMotorTest(object sender, RoutedEventArgs e)
        {
            await TheHub.SingleMotorTest();
        }
        private async void OnTimedMotorTest(object sender, RoutedEventArgs e)
        {
            await TheHub.TimedMotorTest();
        }
        private async void OnPortDVisionTest(object sender, RoutedEventArgs e)
        {
            await TheHub.PortDVisionTest(0x02);

        }
        private async void OnTimedMotorTachTest(object sender, RoutedEventArgs e)
        {
            await TheHub.TimedMotorTachTest(0x01);
        }

    }
}
