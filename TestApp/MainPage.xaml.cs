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

using WonderWorkshop;
using System;
using System.Diagnostics;
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

        private DotDashBot theRobot;

        public MainPage()
        {
            thePage = this;

            this.InitializeComponent();
        }
        public void Update()
        {
            bool itemFound = false;
            if (ble != null)
            {
                if (ble.DevicesChanged)
                {
                    var newDeviceList = ble.BluetoothLeDevicesAdded;
                    foreach (var theNewDevice in newDeviceList)
                    {
                        ShowFeedback("added: " + theNewDevice.Name);
                        switch (whatToFind)
                        {
                            case WhatToFind.WonderWorkshopBot:
                                {
                                    string id = theNewDevice.DeviceInfo.Id;
                                    if (theNewDevice.Name.Contains("Dan"))
                                    {
                                        ShowFeedback("Dash found");
                                        theRobot = new DotDashBot(theNewDevice);

                                        // Make a persistance BTLE connection
                                        if (theRobot.Connect())
                                        {
                                            ShowFeedback("Robot BTLE connection made");
                                            itemFound = true;
                                            _OnConnectServicesBtn.IsEnabled = true;
                                        }

                                    }
                                }
                                break;

                            default:
                                {
                                    ShowFeedback("Enumeration when don't know what we are looking for");
                                }
                                break;
                        
                        }

                        if (itemFound)
                        {
                            break;
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

        enum WhatToFind
        {
            Undef=-1,
            WonderWorkshopBot
        }

        WhatToFind whatToFind = WhatToFind.Undef;


        private async void OnFindWW(object sender, RoutedEventArgs e)
        {
            ShowFeedback("Starting find for Wonder Workshop Bot");
            whatToFind = WhatToFind.WonderWorkshopBot;

            await Windows.System.Threading.ThreadPool.RunAsync(_ =>
            {
                ble = BluetoothLEHelper.Instance;
                ble.StartEnumeration();
            });
        }

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

        private void OnConnectServices(object sender, RoutedEventArgs e)
        {
            // This hooks up service connections and characteristic updates
            if (theRobot.ConnectService())
            {
                _DeviceContent.Children.Clear();
                _DeviceContent.Children.Add(new WonderWorkshop.WWUx(theRobot));
                ShowFeedback("Service / Charactersitic connection made");
            }
        }
    }
}