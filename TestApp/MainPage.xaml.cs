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
using System.Collections.Generic;
using System.Linq;

namespace TestApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public static MainPage thePage;

        // The BTLE helper class
        private BluetoothLEHelper ble;

        // The selected device we are connecting with.
        private SampleDevice theSelectedDevice;

        // The cached list of BTLE devices we have seen
        private List<SampleDevice> theCachedDevices = new List<SampleDevice>();

        public MainPage()
        {
            thePage = this;

            this.InitializeComponent();
        }

        /// <summary>
        /// Called on a periodic timer to look for changes in the set of bluetooth devices detected.
        ///  Two things to check:
        ///  BluetoothLeDevicesAdded, new devices seen since last call
        ///  BluetoothLeDevicesRemoved, the devices no longer seen 
        /// </summary>
        public async void Update()
        {
            if (ble != null)
            {
                if (ble.DevicesChanged)
                {
                    var newDeviceList = ble.BluetoothLeDevicesAdded;
                    foreach (var theNewDevice in newDeviceList)
                    {
                        // First see if we already have it in the cache
                        var item = theCachedDevices.SingleOrDefault(r => r.DeviceInfo.Id == theNewDevice.DeviceInfo.Id);
                        if (item == null)
                        {
                            // new item

                            // Create the wrapper for the BTLE object
                            SampleDevice newSampleDevice = new SampleDevice(theNewDevice);

                            // Add it to our cache of devices
                            theCachedDevices.Add(newSampleDevice);

                            bool addToList = false;

                            ShowFeedback("BTLE Device added: " + theNewDevice.Name);
                            string id = theNewDevice.DeviceInfo.Id;
                            if (_Filter.Text.Length > 0)
                            {
                                // Filter defined so only take things that contain the filter name
                                if (theNewDevice.Name.Contains(_Filter.Text, StringComparison.OrdinalIgnoreCase))
                                {
                                    ShowFeedback("Filtered BTLE Device found");
                                    addToList = true;
                                }
                            }
                            else
                            {
                                // No filter so just list everything found
                                ShowFeedback("BTLE Device found: " + theNewDevice.Name);
                                addToList = true;
                            }

                            if (addToList)
                            {
                                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                                {
                                // Add it to the list
                                _Devices.Items.Add(newSampleDevice);
                                    _Devices.ScrollIntoView(newSampleDevice);
                                });
                            }
                        }
                        else
                        {
                            ShowFeedback("BTLE Duplicate device seen: " + theNewDevice.Name);
                        }
                    }



                    // For all the removed devices we want to remove them from the display list and cache
                    var removedDeviceList = ble.BluetoothLeDevicesRemoved;
                    foreach (var removed in removedDeviceList)
                    {
                        var itemToRemove = theCachedDevices.SingleOrDefault(r => r.DeviceInfo.Id == removed.DeviceInfo.Id);
                        if (itemToRemove != null)
                        {
                            // Found it so remove it from the listbox and the cache.
                            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                            {
                                // Remove it from the list
                                _Devices.Items.Remove(itemToRemove);
                            });

                            // And the cache
                            theCachedDevices.Remove(itemToRemove);

                            ShowFeedback("removed: " + removed.Name);

                        }

                        //// Look through the cache for this device
                        //foreach (var device in theSeenDevices)
                        //{
                        //    if (device.DeviceInfo.Id == removed.DeviceInfo.Id)
                        //    {
                        //        // Found it so remove it from the listbox and the cache.
                        //        await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                        //        {
                        //            // Remove it from the list
                        //            _Devices.Items.Remove(device);
                        //        });

                        //        // And the cache
                        //        theSeenDevices.Remove(device);

                        //        ShowFeedback("removed: " + removed.Name);

                        //        // Done now
                        //        break;
                        //    }

                        //}
                    }
                }
            }
        }

        /// <summary>
        /// Called when the users starts the device enumeration
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnFindDevices(object sender, RoutedEventArgs e)
        {
            ShowFeedback("Starting find for BTLE Devices");

            await Windows.System.Threading.ThreadPool.RunAsync(_ =>
            {
                ble = BluetoothLEHelper.Instance;
                ble.StartEnumeration();
            });
        }

        /// <summary>
        /// Helper to show user feedback of what is going on.
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
        ///  Called when the page is loaded.  Used to setup our pooling of the BTLE device list during enumeration
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
                        Update();
                    });

            }, period);

        }

        /// <summary>
        /// Called when the user trys to connect to the devices services.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnConnectServices(object sender, RoutedEventArgs e)
        {
            // This hooks up service connections and characteristic updates
            if (theSelectedDevice.ConnectService())
            {
                _ServiceCount.Text = theSelectedDevice.ServiceCount.ToString();

                _DeviceContent.Children.Clear();
                _DeviceContent.Children.Add(new SampleUX(theSelectedDevice));
                ShowFeedback("Service / Characteristic connection made");
            }
        }

        /// <summary>
        // Called when the user picks a devices to connect to.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDeviceSelectedChanged(object sender, SelectionChangedEventArgs e)
        {
            theSelectedDevice = _Devices.SelectedItem as SampleDevice;

            if (theSelectedDevice != null)
            {
                // Make a persistent BTLE connection
                if (theSelectedDevice.Connect())
                {
                    ShowFeedback("BTLE connection made");
                    _ConnectedDevice.Text = theSelectedDevice.Name;
                    _OnConnectServicesBtn.IsEnabled = true;

                    // Connection made so we are done
                }
            }
        }
    }
}