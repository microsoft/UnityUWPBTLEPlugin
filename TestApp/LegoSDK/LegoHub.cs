using System;
using System.Collections.Generic;


using System.Threading.Tasks;
using UnityUWPBTLEPlugin;
using System.Diagnostics;
using Windows.Devices.Enumeration;
using Windows.Devices.Bluetooth;

namespace LegoSDK
{
    public partial class LegoHub
    {
        ObservableBluetoothLEDevice theLegoHub;
        ObservableGattDeviceService LegoHubService;
        ObservableGattCharacteristics LegoHubCharacteristic;
        public string DeviceId { get { return theLegoHub.DeviceInfo.Id; } }

        public LegoHub(ObservableBluetoothLEDevice device)
        {
            theLegoHub = device;
        }


        public bool ConnectHub()
        {
            bool connected = false;
            Debug.WriteLine("ConnectHub");
            if (theLegoHub != null)
            {
                Debug.WriteLine("Lego Hub found");
                // Found a hub, is it connected?
                if (theLegoHub.IsConnected)
                {
                    Debug.WriteLine("Lego Hub connected");
                    connected = true;
                }
                else
                {
                    Debug.WriteLine("Lego Hub NOT connected");
                }

                if (theLegoHub.IsPaired)
                {
                    Debug.WriteLine("Lego Hub IsPaired");
                }
                else
                {
                    Debug.WriteLine("Lego Hub trying to pair");

                    theLegoHub.DoInAppPairing(DevicePairingProtectionLevel.None, null);
                    if (theLegoHub.IsPaired)
                    {
                        Debug.WriteLine("Lego Hub Paired now");
                    }
                    else
                    {
                        Debug.WriteLine("Lego Hub Paired failed");
                    }
                }

                if (!connected)
                {
                    connected = theLegoHub.Connect();
                }
            }
            else
            {
                Debug.WriteLine("Lego Hub NOT found");
            }

            return connected;
        }


        public async Task ConnectService()
        {
            ShowFeedback("Lego hub service count: " + theLegoHub.ServiceCount);
            foreach (var service in theLegoHub.Services)
            {
                Debug.WriteLine(service.Name);
                if (service.Name.Contains("5667"))
                {
                    LegoHubService = service;
                }
            }

            if (LegoHubService != null)
            {
                ShowFeedback("Lego hub service #1623 found");
                ShowFeedback("Characteristics count: " + LegoHubService.Characteristics.Count);

                foreach (var characteristic in LegoHubService.Characteristics)
                {
                    ShowFeedback("C Name: " + characteristic.Name);
                    ShowFeedback("C UUID: " + characteristic.UUID);
                    if (characteristic.Name.Contains("5668"))
                    {
                        ShowFeedback("Lego hub characteristic #1624 found");
                        LegoHubCharacteristic = characteristic;
                        Debug.WriteLine(LegoHubCharacteristic.Name);
                        bool notifyOk = LegoHubCharacteristic.SetNotify();

                        LegoHubCharacteristic.Characteristic.ValueChanged += Characteristic_ValueChanged;

                        await GetManufacturerName();
                        await GetBatteryLevel();

                        foreach (BrickPorts port in Enum.GetValues(typeof(BrickPorts)))
                        {
                            await GetPortInformation((byte)port, LEMessagePortInformationType.Value);
                        }

                        foreach (BrickPorts port in Enum.GetValues(typeof(BrickPorts)))
                        {
                            await GetPortModeInformation((byte)port, 0, LEMessagePortModeInformationRequestType.LEMessagePortModeInformationRequestTypeName);
                        }

                        //await DumpPortModeInformation(); // getting all names

                    }

                }


            }

        }

        public async Task PortDVisionTest(byte portId)
        {
            await GetPortInformation(portId, LEMessagePortInformationType.Value);
            // Should respond with a 0x45 
        }

        

        public async Task ColorIndexTest()
        {
            ShowFeedback("OnColorIndexTest start");
            await SetRGBMode(0x32, RGBLightMode.RGB_LIGHT_MODE_DISCRETE);

            for (byte color = 0; color < 11; color++)
            {
                await writeColorIndex(0x32, color);
                await Task.Delay(1000);
            }

            // set it back to default
            await writeColorIndex(0x32, GetDefaultColorIndex(0x32));
            ShowFeedback("OnColorIndexTest start");
        }



        private async Task SetRGBMode(byte portId, RGBLightMode newMode)
        {
            await PortInputFormatSetup(portId, (byte)newMode, 1000, true);
        }


        public async Task ColorRGBTest()
        {
            ShowFeedback("OnColorRGBTest start");
            await SetRGBMode(0x32, RGBLightMode.RGB_LIGHT_MODE_ABSOLUTE);


            Random ran = new Random();
            byte red, green, blue;
            for (byte i = 0; i < 10; i++)
            {
                red = (byte)ran.Next(256);
                green = (byte)ran.Next(256);
                blue = (byte)ran.Next(256);

                await writeColorRGB(0x32, red, green, blue);
                await Task.Delay(1000);
            }

            // set it back to default
            await SetRGBMode(0x32, RGBLightMode.RGB_LIGHT_MODE_DISCRETE);
            await writeColorIndex(0x32, GetDefaultColorIndex(0x32));
            ShowFeedback("OnColorRGBTest done");

        }

        public async Task BrickMotorTest()
        {
            await WriteDualMotorPower(57, 42, -42);
            await Task.Delay(2000);
            await WriteDualMotorPower(57, 0, 0);
        }

        public async Task SingleMotorTest()
        {
            // Expecting it to be plunged into port 1 "C"
            await WriteMotorPower(0x01, 50);
            await Task.Delay(2000);
            await WriteMotorPower(0x01, 0);

        }

        public async Task TimedMotorTest()
        {
            await WriteSingleMotorTimedRun(0x01, 1000, 0x64, 0x64, MotorWithTachoEndState.DRIFTING, MotorWithTachoAccDecProfileConfiguration.NONE);
        }

        internal async Task TimedMotorTachTest(byte portId)
        {
            await ActiveMotorCombinedMode(portId);
            await TimedMotorTest();
        }



        private async Task DumpPortModeInformation()
        {
            // External ports
            for (byte portId = 0; portId < 50; portId++)
            {
                await GetPortModeInformation(portId, 0, LEMessagePortModeInformationRequestType.LEMessagePortModeInformationRequestTypeName);
            }
            // internal ports
            for (byte portId = 50; portId <= 100; portId++)
            {
                await GetPortModeInformation(portId, 0, LEMessagePortModeInformationRequestType.LEMessagePortModeInformationRequestTypeName);
            }
        }

        List<Byte> _activePorts = new List<byte>();

        public List<byte> ActivePorts { get => _activePorts; set => _activePorts = value; }

        private async Task DumpPortValues()
        {
            for (byte portId = 0; portId < 50; portId++)
            {
                await GetPortInformation(portId, LEMessagePortInformationType.Value);

            }
            for (byte portId = 0; portId < 50; portId++)
            {
                await GetPortInformation(portId, LEMessagePortInformationType.Value);

            }
        }


        public void ShowFeedback(string msg)
        {
            Debug.WriteLine(msg);
            //LegoBTLE.MainPage.thePage.ShowFeedback(msg);
        }
    }
}
 