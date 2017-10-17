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

using LegoSDK;
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

        enum WhatToFind
        {
            Undef=-1,
            LegoHub,
            WonderWorkshopBot
        }

        WhatToFind whatToFind = WhatToFind.Undef;

        private async void OnFindLego(object sender, RoutedEventArgs e)
        {
            ShowFeedback("Starting find for lego hub");
            whatToFind = WhatToFind.LegoHub;

            await Windows.System.Threading.ThreadPool.RunAsync(_ =>
            {
                ble = BluetoothLEHelper.Instance;
                ble.StartEnumeration();
            });
        }

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


        private async void OnConnectService(object sender, RoutedEventArgs e)
        {
            await theHub.ConnectService();
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

    }
}