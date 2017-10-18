using UnityUWPBTLEPlugin;

using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using System.Runtime.InteropServices.WindowsRuntime;
//using System.Runtime.InteropServices.WindowsRuntime;

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

        public bool Connect()
        {
            bool connected = false;
            Debug.WriteLine("Connect");
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

        public void ConnectService()
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
                        bool notifyOk = Sensor1Characteristic.SetNotify();

                        continue;
                    }

                    if (UUID.Contains(sSensor2Characteristic))
                    {
                        ShowFeedback("DashDot Sensor 2 characteristic found");
                        Sensor2Characteristic = characteristic;
                        Debug.WriteLine(Sensor2Characteristic.Name);

                        //Sensor2Characteristic.PropertyChanged += Sensor2Characteristic_PropertyChanged;
                        Sensor2Characteristic.Characteristic.ValueChanged += Sensor2Characteristic_ValueChanged;
                        bool notifyOk = Sensor2Characteristic.SetNotify();

                        continue;
                    }

                }


            }

        }

        enum sensor1BytePosition
        {
            Button = 8,
            Flags = 11,

        }

        enum sensor2BytePositions
        {
            LeftDistanceSensor = 6,
            RightDistanceSnesor = 7,
            RearDistanceSensor = 8,
            LeftWheelEncoder = 14, // and 15
            RightWheelEncoder = 16 // and 17

        }

        private double ConvertToCM(int ticks)
        {
            // approx 0.0205 cm per tick
            // 1200 ticks per rotation
            // wheel diameter of 7.85cm
            return ((double)ticks) * 7.85f * System.Math.PI / 1200.0f;
        }

        bool button1Pressed = false;
        bool button2Pressed = false;
        bool button3Pressed = false;
        bool buttonMainPressed = false;

        public bool Button1Pressed
        {
            internal set
            {
                if (value != button1Pressed)
                {
                    ShowFeedback("Button 1 changed");
                    button1Pressed = value;
                }
            }

            get { return button1Pressed; }
        }
        public bool Button2Pressed
        {
            internal set
            {
                if (value != button2Pressed)
                {
                    ShowFeedback("Button 2 changed");
                    button2Pressed = value;
                }
            }

            get { return button2Pressed; }
        }
        public bool Button3Pressed
        {
            internal set
            {
                if (value != button3Pressed)
                {
                    ShowFeedback("Button 3 changed");
                    button3Pressed = value;
                }
            }

            get { return button3Pressed; }
        }

        public bool ButtonMainPressed
        {
            internal set
            {
                if (value != buttonMainPressed)
                {
                    ShowFeedback("Button Main changed");
                    buttonMainPressed = value;
                }
            }

            get { return buttonMainPressed; }
        }


        private void Sensor1Characteristic_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            //ShowFeedback("Sensor1Characteristic_ValueChanged");

            byte[] newValue = args.CharacteristicValue.ToArray();

            int buttonFlags = newValue[(int)sensor1BytePosition.Button];

            ButtonMainPressed = (buttonFlags & (0x10 << 0)) > 0 ? true : false;
            Button1Pressed = (buttonFlags & (0x10 << 1)) > 0 ? true : false;
            Button2Pressed = (buttonFlags & (0x10 << 2)) > 0 ? true : false;
            Button3Pressed = (buttonFlags & (0x10 << 3)) > 0 ? true : false;
        }

        private void Sensor2Characteristic_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            //ShowFeedback("Sensor2Characteristic_ValueChanged");
        }

        public async Task SendCommand(Command whatToSend)
        {
            await whatToSend.Send(CommandCharacteristic);
        }

        public void ShowFeedback(string msg)
        {
            Debug.WriteLine(msg);
            TestApp.MainPage.thePage.ShowFeedback(msg);
        }

    }

}