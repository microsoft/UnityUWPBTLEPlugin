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

using UnityUWPBTLEPlugin;

using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using System.Runtime.InteropServices.WindowsRuntime;

namespace TestApp.Sample
{
    public class SampleDevice
    {
        // These will be from the device manufacturer.  
        // They are just place holders and MUST be replaced with proper values for the connection code to work.
        const string sService_UUID = "FA1B5744-771E-44FC-B696-9D49990885DD";
        const string sCommandCharacteristic = "C341A438-039B-481E-9A8D-1E4595152497";
        const string sSensorCharacteristic = "8D94678D-3820-4E96-B2B8-A4D9AF776888";

        BluetoothLEDeviceWrapper BTLEDevice;

        // The number and type of characteristics is defined by the device manufacturer
        GattCharacteristicsWrapper CommandCharacteristic;
        GattCharacteristicsWrapper SensorCharacteristic;

        GattDeviceServiceWrapper GattDeviceService;




        public SampleDevice(BluetoothLEDeviceWrapper device)
        {
            BTLEDevice = device;
        }

        public bool Connect()
        {
            bool connected = false;
            Debug.WriteLine("Connect");
            if (BTLEDevice != null)
            {
                // Found a device, is it connected?
                if (BTLEDevice.IsConnected)
                {
                    Debug.WriteLine("BTLE device Connected");
                    connected = true;
                }
                else
                {
                    Debug.WriteLine("BTLE device not connected");
                }

                if (!connected)
                {
                    Debug.WriteLine("BTLE device wasn't connected, trying to connect");

                    connected = BTLEDevice.Connect();
                }
            }
            else
            {
                Debug.WriteLine("Connect called with no device.");
            }

            return connected;
        }

        // Connect to the service and then list out the characteristics
        public bool ConnectService(out int ServiceCount)
        {
            bool connectOk = false;
            ServiceCount = BTLEDevice.ServiceCount;
            Debug.WriteLine("Device service count: " + BTLEDevice.ServiceCount);
            foreach (var service in BTLEDevice.Services)
            {
                Debug.WriteLine(service.Name);
                if (service.Name.Contains(sService_UUID))
                {
                    GattDeviceService = service;
                }
            }

            if (GattDeviceService != null)
            {
                Debug.WriteLine("Characteristics count: " + GattDeviceService.Characteristics.Count);

                int retry = 5;
                while (GattDeviceService.Characteristics.Count == 0 && retry > 0)
                {
                    Task.Delay(500);
                    retry--;
                }

                if (GattDeviceService.Characteristics.Count == 0)
                {
                    Debug.WriteLine("Characteristics count: " + GattDeviceService.Characteristics.Count);
                }

                Debug.WriteLine("Characteristics count: " + GattDeviceService.Characteristics.Count);
                foreach (var characteristic in GattDeviceService.Characteristics)
                {
                    Debug.WriteLine("C Name: " + characteristic.Name);
                    Debug.WriteLine("C UUID: " + characteristic.UUID);
                    string UUID = characteristic.UUID.ToUpper();
                    if (UUID.Contains(sCommandCharacteristic))
                    {
                        Debug.WriteLine("Command characteristic found");
                        CommandCharacteristic = characteristic;
                        Debug.WriteLine(CommandCharacteristic.Name);

                        connectOk = true;
                        continue;
                    }

                    if (UUID.Contains(sSensorCharacteristic))
                    {
                        ShowFeedback("DashDot Sensor characteristic found");
                        SensorCharacteristic = characteristic;
                        SensorCharacteristic.Characteristic.ValueChanged += SensorCharacteristic_ValueChanged;

                        // If you don't call SetNotify you won't get the ValueChanged notifications.  
                        bool notifyOk = SensorCharacteristic.SetNotify();

                        continue;
                    }
                }


            }

            if (connectOk != true)
            {
                Debug.WriteLine("/// ");
                Debug.WriteLine("/// Connection FAILED: Did you update the UUID's?");
                Debug.WriteLine("/// ");
            }

            return connectOk;
        }

        // Again depending on the BTLE device manufacturer
        public async Task Send(Command whatToSend)
        {
            await whatToSend.Send(CommandCharacteristic);
        }


        private void SensorCharacteristic_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            ShowFeedback("SensorCharacteristic_ValueChanged");

            // How to read characterisicts values will be determined by the device manufacturer
            byte[] newValue = args.CharacteristicValue.ToArray();
        }

        public void ShowFeedback(string msg)
        {
            Debug.WriteLine(msg);
            TestApp.MainPage.thePage.ShowFeedback(msg);
        }

    }
}
