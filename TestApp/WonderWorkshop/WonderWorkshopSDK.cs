using UnityUWPBTLEPlugin;

using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.UI.Xaml;

namespace WonderWorkshop
{
    public class DotDashBot
    {
        const string sService_UUID_DashDot = "AF237777-879D-6186-1F49-DECA0E85D9C1";    // Dash & Dot
        const string sCommandCharacteristic = "AF230002-879D-6186-1F49-DECA0E85D9C1";   // Dash & Dot
        const string sSensor1Characteristic = "AF230003-879D-6186-1F49-DECA0E85D9C1";   // Dash & Dot
        const string sSensor2Characteristic = "AF230006-879D-6186-1F49-DECA0E85D9C1";   // Dash only

        BluetoothLEDeviceWrapper BTLEDevice;

        GattCharacteristicsWrapper CommandCharacteristic;
        GattCharacteristicsWrapper Sensor1Characteristic;
        GattCharacteristicsWrapper Sensor2Characteristic;


        public DotDashBot(BluetoothLEDeviceWrapper device)
        {
            BTLEDevice = device;
        }

        public bool ConnectHub()
        {
            bool connected = false;
            Debug.WriteLine("ConnectHub");
            if (BTLEDevice != null)
            {
                Debug.WriteLine("Wonder Workshop DashDot found");
                // Found a hub, is it connected?
                if (BTLEDevice.IsConnected)
                {
                    Debug.WriteLine("DashDot Connected");
                    connected = true;
                }
                else
                {
                    Debug.WriteLine("DashDot NOT connected");
                }

                if (!connected)
                {
                    Debug.WriteLine("BTLE Device wasn't connected, trying to connect");

                    connected = BTLEDevice.Connect();
                }
            }
            else
            {
                Debug.WriteLine("DashDot NOT found");
            }

            return connected;
        }


        GattDeviceServiceWrapper GattDeviceService;

        public void OnConnectService(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Device service count: " + BTLEDevice.ServiceCount);
            foreach (var service in BTLEDevice.Services)
            {
                Debug.WriteLine(service.Name);
                if (service.Name.Contains("30583"))//sService_UUID_DashDot))
                {
                    GattDeviceService = service;
                }
            }

            if (GattDeviceService != null)
            {
                Debug.WriteLine("DashDot service found");
                Debug.WriteLine("Characteristics count: " + GattDeviceService.Characteristics.Count);

                foreach (var characteristic in GattDeviceService.Characteristics)
                {
                    Debug.WriteLine("C Name: " + characteristic.Name);
                    Debug.WriteLine("C UUID: " + characteristic.UUID);
                    //Debug.WriteLine("C Value: " + characteristic.Value);
                    string UUID = characteristic.UUID.ToUpper();
                    if (UUID.Contains(sCommandCharacteristic))
                    {
                        Debug.WriteLine("DashDot Command characteristic found");
                        CommandCharacteristic = characteristic;
                        Debug.WriteLine(CommandCharacteristic.Name);
                        ShowFeedback("Command characteristic found!");

                        // Write only characteristic
                        continue;
                    }

                    if (UUID.Contains(sSensor1Characteristic))
                    {
                        ShowFeedback("DashDot Sensor 1 characteristic found");
                        Sensor1Characteristic = characteristic;

                        //Sensor1Characteristic.PropertyChanged += Sensor1Characteristic_PropertyChanged;
                        Sensor1Characteristic.Characteristic.ValueChanged += Sensor1Characteristic_ValueChanged;

                        continue;
                    }

                    if (UUID.Contains(sSensor2Characteristic))
                    {
                        ShowFeedback("DashDot Sensor 2 characteristic found");
                        Sensor2Characteristic = characteristic;
                        Debug.WriteLine(Sensor2Characteristic.Name);

                        //Sensor2Characteristic.PropertyChanged += Sensor2Characteristic_PropertyChanged;
                        Sensor1Characteristic.Characteristic.ValueChanged += Sensor2Characteristic_ValueChanged;

                        continue;
                    }

                }


            }

        }

        private void Sensor1Characteristic_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {

            //MainPage.thePage.UpdateSensor1Text(String.Format("DashDot Sensor 1: {0}", e.);

            ShowFeedback("Sensor1Characteristic_PropertyChanged");
            Debug.WriteLine("Sensor1Characteristic_PropertyChanged");
        }

        private void Sensor1Characteristic_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            ShowFeedback("Sensor1Characteristic_ValueChanged");
            Debug.WriteLine("Sensor1Characteristic_ValueChanged");
        }

        private void Sensor2Characteristic_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ShowFeedback("Sensor2Characteristic_PropertyChanged");
            Debug.WriteLine("Sensor2Characteristic_PropertyChanged");
        }

        private void Sensor2Characteristic_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            ShowFeedback("Sensor2Characteristic_ValueChanged");
            Debug.WriteLine("Sensor2Characteristic_ValueChanged");
        }

        public async Task SendCommand(Command whatToSend)
        {
            await whatToSend.Send(CommandCharacteristic);
        }

        public void ShowFeedback(string msg)
        {
            Debug.WriteLine(msg);
            //LegoBTLE.MainPage.thePage.ShowFeedback(msg);
        }

    }

}