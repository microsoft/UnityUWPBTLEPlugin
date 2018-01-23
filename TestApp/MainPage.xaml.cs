// ******************************************************************
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THE CODE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH
// THE CODE OR THE USE OR OTHER DEALINGS IN THE CODE.
// ******************************************************************

using TestApp.Sample;
using System;
using System.Diagnostics;
using UnityUWPBTLEPlugin;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace TestApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public static MainPage thePage;

        private BluetoothLEHelper ble;

        private SampleDevice theDevice;

        public MainPage()
        {
            thePage = this;

            this.InitializeComponent();
        }

        /// <summary>
        /// Called on a periodic timer to look for changes in the set of bluetooth devices detected.
        /// </summary>
        public void UpdateBTLEList()
        {
            if (ble != null)
            {
                if (ble.DevicesChanged)
                {
                    var newDeviceList = ble.BluetoothLeDevicesAdded;
                    foreach (var theNewDevice in newDeviceList)
                    {
                        ShowFeedback("added: " + theNewDevice.Name);
                        string id = theNewDevice.DeviceInfo.Id;
                        if (_Filter.Text.Length > 0)
                        {
                            // Filter defined so only take things that contain the filter name
                            if (theNewDevice.Name.IndexOf(_Filter.Text, 0, StringComparison.CurrentCultureIgnoreCase) != -1)
                            {
                                ShowFeedback("Filtered BTLE Device found");
                                theDevice = new SampleDevice(theNewDevice);

                                // Make a persistent BTLE connection
                                if (theDevice.Connect())
                                {
                                    ShowFeedback("BTLE connection made");
                                    _DeviceInfo.Text = theNewDevice.Name;
                                    _OnConnectServicesBtn.IsEnabled = true;

                                    // Connection made so we are done
                                    break;
                                }
                            }
                        }
                        else
                        {
                            // No filter so just list everything found
                            ShowFeedback("Unfiltered BTLE Device found: " + theNewDevice.Name);
                        }
                    }

                    var removedDeviceList = ble.BluetoothLeDevicesRemoved;
                    foreach (var i in removedDeviceList)
                    {
                        ShowFeedback("removed: " + i.Name);
                    }
                }
            }
        }

        /// <summary>
        /// Notify the BTLE Helper to start the enumeration of BTLE Devices
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnFind(object sender, RoutedEventArgs e)
        {
            ShowFeedback("Starting find for BTLE Devices");

            await Windows.System.Threading.ThreadPool.RunAsync(_ =>
            {
                ble = BluetoothLEHelper.Instance;
                ble.StartEnumeration();
            });
        }

        /// <summary>
        /// Used to display to the user some feedback.
        /// </summary>
        /// <param name="msg"></param>
        public async void ShowFeedback(string msg)
        {
            Debug.WriteLine(msg);
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                TextBlock newItem = new TextBlock();
                newItem.Text = msg;
                Feedback.Items.Add(newItem);
                Feedback.ScrollIntoView(newItem);
            });
        }

        /// <summary>
        /// Called when the page is first loaded.
        /// Setup a background process to update the list of devices in an async way without blocking the UI thread
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Feedback.Items.Clear();

            TimeSpan period = TimeSpan.FromSeconds(1);

            ThreadPoolTimer PeriodicTimer = ThreadPoolTimer.CreatePeriodicTimer(async (source) =>
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.High,
                    () =>
                    {
                        UpdateBTLEList();
                    });

            }, period);
        }

        /// <summary>
        /// This hooks up service connections and characteristic updates
        ///
        /// NOTE: This will fail unless the device UUID's have been updated for your specific devices.
        /// NOTE: Update the SampleDevices.CS file or replace that with your device specific implementation.
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnConnectServices(object sender, RoutedEventArgs e)
        {
            int numberServices = 0;
            if (theDevice.ConnectService(out numberServices))
            {
                _DeviceContent.Children.Clear();
                _DeviceContent.Children.Add(new SampleUX(theDevice));
                ShowFeedback("Service / Characteristic connection made");
            }
            _ServiceCount.Text = numberServices + " Services Found";
        }
}
}