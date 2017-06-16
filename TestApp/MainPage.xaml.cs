using LegoSDK;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityUWPBTLEPlugin;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TestApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public static MainPage thePage;

        private BluetoothLEHelper ble;

        private LegoHub theHub;

        public MainPage()
        {
            thePage = this;

            this.InitializeComponent();
        }
        public void Update()
        {
            if (ble != null)
            {
                if (ble.DevicesChanged)
                {
                    var newDeviceList = ble.BluetoothLeDevicesAdded;
                    foreach (var theNewDevice in newDeviceList)
                    {
                        ShowFeedback("added: " + theNewDevice.Name);
                        if (theNewDevice.Name.Contains("LEGO") && theHub is null)
                        {
                            ShowFeedback("Found lego hub id: " + theNewDevice.DeviceInfo.Id);
                            theHub = new LegoHub(theNewDevice);
                            //theHub.Feedback = feedbackMsgs;
                            if (theHub.ConnectHub())
                            {
                                ShowFeedback("Hub connected");
                            }
                            else
                            {
                                ShowFeedback("Hub connection failed");
                            }
                        }
                    }

                    var removedDeviceList = ble.BluetoothLeDevicesRemoved;
                    foreach (var i in removedDeviceList)
                    {
                        ShowFeedback("removed: " + i.Name);
                        if (theHub != null && theHub.DeviceId == i.DeviceInfo.Id)
                        {
                            // removing found hub
                            ShowFeedback("Removing found lego hub id: " + i.DeviceInfo.Id);
                            theHub = null;
                        }
                    }
                }
            }
        }
        private async void OnEnumerate(object sender, RoutedEventArgs e)
        {
            ShowFeedback("Starting enumerating");
            await Windows.System.Threading.ThreadPool.RunAsync(_ =>
            {
                ble = BluetoothLEHelper.Instance;
                ble.StartEnumeration();
            });
        }

        private async void OnConnectService(object sender, RoutedEventArgs e)
        {
            await theHub.ConnectService();
        }

        private async void OnColorIndexTest(object sender, RoutedEventArgs e)
        {
            await theHub.ColorIndexTest();
        }

        private async void OnColorRGBTest(object sender, RoutedEventArgs e)
        {
            await theHub.ColorRGBTest();
        }

        private async void OnBrickMotorTest(object sender, RoutedEventArgs e)
        {
            await theHub.BrickMotorTest();
        }

        private async void OnSingleMotorTest(object sender, RoutedEventArgs e)
        {
            await theHub.SingleMotorTest();
        }
        private async void OnTimedMotorTest(object sender, RoutedEventArgs e)
        {
            await theHub.TimedMotorTest();
        }
        private async void OnPortDVisionTest(object sender, RoutedEventArgs e)
        {
            await theHub.PortDVisionTest(0x02);

        }
        private async void OnTimedMotorTachTest(object sender, RoutedEventArgs e)
        {
            await theHub.TimedMotorTachTest(0x01);
        }



        public void ShowFeedback(string msg)
        {
            Debug.WriteLine(msg);
            Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                TextBlock newItem = new TextBlock();
                newItem.Text = msg;
                Feedback.Items.Add(newItem);
                Feedback.ScrollIntoView(newItem);
            });
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Feedback.Items.Clear();

            TimeSpan period = TimeSpan.FromSeconds(1);

            ThreadPoolTimer PeriodicTimer = ThreadPoolTimer.CreatePeriodicTimer((source) =>
            {
                Dispatcher.RunAsync(CoreDispatcherPriority.High,
                    () =>
                    {
                        Update();
                    });

            }, period);

        }

    }
}