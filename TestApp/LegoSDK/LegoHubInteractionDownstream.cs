using System;
using System.Collections.Generic;


using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityUWPBTLEPlugin;
using Windows.UI.Xaml;
using System.Diagnostics;
using Windows.Devices.Enumeration;


namespace LegoSDK
{
    public partial class LegoHub
    {



        public async Task writeColorIndex(byte portId, byte colorIndex, StartupInformation si = StartupInformation.ExecuteImmediately, CompletionInformation ci = CompletionInformation.CommandFeedback)
        {
            // Valid color indexes are 0-10 inclusive
            // The built in LED on the brick is on port 50 (0x32)

            /*
             * Set RGB color to index 1
            0 = 8 (0x8)     // length
            1 = 0 (0x0)     // hub id
            2 = -127 (0x81) // command
            3 = 50 (0x32)   // port id
            4 = 1 (0x1)     // startup and completion
            5 = 81 (0x51)   // subcommand (direct write)
            6 = 0 (0x0)     // color mode (index)
            7 = 1 (0x1)     // color index (1)
            */


            byte[] message = new byte[]
            {
                0x06,   // message length
                0x00,   // hub id
                (byte)LEMessageType.LEMessageTypePortOutputCommand, // message
                portId, // port id
                Helper.BuildStartupCompletionInformation(si, ci), // Startup & Completion information 
                0x51, // direct mode
                (byte)RGBLightMode.RGB_LIGHT_MODE_DISCRETE, // Color format 
                colorIndex};

            message[0] = (byte)message.Length;

            IBuffer buff = message.AsBuffer();
            var res = await LegoHubCharacteristic.Characteristic.WriteValueAsync(buff);

        }

        public async Task writeColorRGB(byte portId, byte red, byte green, byte blue, StartupInformation si = StartupInformation.ExecuteImmediately, CompletionInformation ci = CompletionInformation.CommandFeedback)
        {
            //0 = 10(0xA)
            //1 = 0(0x0)
            //2 = -127(0x81)
            //3 = 50(0x32)
            //4 = 1(0x1)
            //5 = 81(0x51)
            //6 = 1(0x1)
            //7 = 105(0x69)
            //8 = 58(0x3A)
            //9 = -95(0xA1)

            // The built in LED on the brick is on port 50 (0x32)

            byte[] message = new byte[]
            {
                0x00,   // message length
                0x00,   // hub id
                (byte)LEMessageType.LEMessageTypePortOutputCommand, // message
                portId,                                             // port id
                Helper.BuildStartupCompletionInformation(si, ci),   // Startup & Completion information 
                0x51,                                               // direct mode
                (byte)RGBLightMode.RGB_LIGHT_MODE_ABSOLUTE,         // Color format 
                red,
                green,
                blue
            };

            message[0] = (byte)message.Length;

            IBuffer buff = message.AsBuffer();
            var res = await LegoHubCharacteristic.Characteristic.WriteValueAsync(buff);

        }

        
        private async Task PortInputFormatSetup(byte portId, byte newMode, UInt32 interval, bool nofificationEnabled)
        {
            // Must be called with RGB before you can set the color using RGB
            //0 = 10(0xA)   // length
            //1 = 0(0x0)    // hub id
            //2 = 65(0x41)  // command (port input format setup)
            //3 = 50(0x32)  // Port id
            //4 = 1(0x1)    // new mode
            //5 = 1(0x1)    // Data interval byte0
            //6 = 0(0x0)    // Data interval byte1
            //7 = 0(0x0)    // Data interval byte2
            //8 = 0(0x0)    // Data interval byte3
            //9 = 1(0x1)    // Notification enabled (true)

            byte[] message = new byte[]
            {
                0x00,   // message length
                0x00,   // hub id
                (byte)LEMessageType.LEMessageTypePortInputFormatSetupSingle, // message
                portId,                                             // port id
                newMode,
                0x01,  // Data interval byte0
                0x00,  // Data interval byte1
                0x00,  // Data interval byte2
                0x00,  // Data interval byte3
                (byte)(nofificationEnabled ? 0x01 : 0x00),  // Notification enabled
            };

            message[0] = (byte)message.Length;

            IBuffer buff = message.AsBuffer();
            var res = await LegoHubCharacteristic.Characteristic.WriteValueAsync(buff);

        }
        

        public async Task WriteSingleMotorTimedRun(byte portId, short timeInMs, sbyte speed, byte maxPower, MotorWithTachoEndState endState, MotorWithTachoAccDecProfileConfiguration profile)
        {
            /*
                Turn on motor for time(1319ms?)
                0 = 12 (0xC)    // length
                1 = 0 (0x0)     // hub id
                2 = -127 (0x81) // command
                3 = 1 (0x1)     // port id
                4 = 17 (0x11)   // startup and completion
                5 = 9 (0x9)     // sub command (StartSpeedForTime)
                6 = 39 (0x27)   // time (int16)
                7 = 5 (0x5)     // time (int16)
                8 = 100 (0x64)  // speed
                9 = 100 (0x64)  // max power
                10 = 0 (0x0)    // end state
                11 = 0 (0x0)    // profile
              */

            byte[] message = new byte[]
            {
                0x00,   // message length (set after allocation)
                0x00,   // hub id
                (byte)LEMessageType.LEMessageTypePortOutputCommand, // message
                portId, // port id
                Helper.BuildStartupCompletionInformation(StartupInformation.ExecuteImmediately, CompletionInformation.CommandFeedback), // Startup & Completion information 
                (byte)PortOutputCommandSubCommandTypeEnum.SPEED_FOR_TIME,
                (byte)(timeInMs & 0xFF),  // LSB
                (byte)(timeInMs >> 8), // MSB
                (byte)speed,
                maxPower, // max power
                (byte)endState,
                (byte)profile
            };

            message[0] = (byte)message.Length;

            IBuffer buff = message.AsBuffer();
            var res = await LegoHubCharacteristic.Characteristic.WriteValueAsync(buff);

        }



        public byte GetDefaultColorIndex(byte portId)
        {
            // We have no reliable way of reading the default color of the Hub, so it is hardcoded here
            return 3;
        }




        public async Task WriteDualMotorPower(byte portId, short speed1, short speed2, StartupInformation si = StartupInformation.BufferIfNecessary, CompletionInformation ci = CompletionInformation.CommandFeedback)
        {
            byte[] message = new byte[]
            {
                0x00,   // message length (set after allocation)
                0x00,   // hub id
                (byte)LEMessageType.LEMessageTypePortOutputCommand, // message
                portId, // port id
                Helper.BuildStartupCompletionInformation(si, ci), // Startup & Completion information 
                (byte)PortOutputCommandSubCommandTypeEnum.START_POWER_MOTOR_ONE_AND_TWO,
                (byte)speed1,
                (byte)speed2
            };

            message[0] = (byte)message.Length;

            IBuffer buff = message.AsBuffer();
            var res = await LegoHubCharacteristic.Characteristic.WriteValueAsync(buff);

        }


        public async Task WriteMotorPower(byte portID, short power)
        {
            byte[] data = new byte[] { (byte)power };

            await WriteDirectForMode((byte)MotorWithTachoMode.POWER, data, portID, StartupInformation.ExecuteImmediately, CompletionInformation.NoAction, 0);
        }


        public async Task WriteDirectForMode(byte mode, byte[] data, byte portID, StartupInformation si, CompletionInformation ci, int commandID)
        {
            Byte[] dataBuffer = new Byte[data.Length + 1];
            dataBuffer[0] = mode;
            System.Buffer.BlockCopy(data, 0, dataBuffer, 1, data.Length);

            int messageLength = 6 + dataBuffer.Length;
            byte[] message = new byte[messageLength];
            message[0] = (byte)message.Length;
            message[1] = 0x00;   // hub id
            message[2] = (byte)LEMessageType.LEMessageTypePortOutputCommand; // message
            message[3] = portID;
            message[4] = Helper.BuildStartupCompletionInformation(si, ci); // Startup & Completion information 
            message[5] = (byte)PortOutputCommandSubCommandTypeEnum.DIRECT_MODE_WRITE;
            System.Buffer.BlockCopy(dataBuffer, 0, message, 6, dataBuffer.Length);

            IBuffer buff = message.AsBuffer();
            var res = await LegoHubCharacteristic.Characteristic.WriteValueAsync(buff, GattWriteOption.WriteWithResponse);
        }
        

        // Lets try to get the Lego "property" Manufacturer name from the gatt characteristics  Should return:
        // 3 byte header LLIITT
        //    LL = total length of message
        //    II = Hub id
        //    TT = Message type, in this case should be 01 (Hub property)
        // 1 byte property ID = 0x08 (Manufacturer name)
        // 1 byte Operation id = 0x06 (Update)
        // 15 bytes of Uint8 payload = "LEGO System A/S"
        private async Task GetManufacturerName()
        {
            try
            {
                IBuffer buff = GattConvert.ToIBufferFromHexString("0500010805");
                var res = await LegoHubCharacteristic.Characteristic.WriteValueAsync(buff);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetManufacturerName Exception {ex.Message}");
                Debug.Assert(false);
            }

        }


        // Lets try to get the battery level from the gatt characteristics  
        // "0500010605"
        private async Task GetBatteryLevel()
        {
            try
            {
                byte[] message = new byte[5]
                {
                    0x05, // length
                    0x00, // hubId
                    (byte)LEMessageType.LEMessageTypeHubProperties, // message
                    (byte)HubProperty.BATTERY_VOLTAGE, // property
                    (byte)HubPropertyOperation.REQUEST_UPDATE // Operation
                };

                IBuffer buff = message.AsBuffer();
                var res = await LegoHubCharacteristic.Characteristic.WriteValueAsync(buff);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetBatteryLevel Exception {ex.Message}");
                Debug.Assert(false);
            }

        }



        // Lets try to get what is on a port from the gatt characteristics  
        // Should return:
        // 3 byte header LLIITT
        //    LL = total length of message (0x0f) 20 bytes)
        //    II = Hub id
        //    TT = Message type, in this case should be 04 (IO Connection)
        // 1 byte Port ID = 0x01 (Port "C")
        // 1 byte Event = 0x01 (Attached)
        // 2 byte Sensor type (0x2500) vision sensor 
        // 4 byte Hardware rev
        // 4 byte firmware rev

        private async Task GetPortInformation(byte portId, LEMessagePortInformationType portInformationToGet) // 0x21
        {
            // 0500210100 (value) gets a 0x45 response for standard ports or a 0x46 for combined mode ports
            // 05 00 45 01 ff

            // 0500210101 (mode info) 0500210102 (possible mode combinations) gets a 0x43 response
            // 0b 00 43 01 01 07 0b 5f 06 a0 00

            byte[] message = new byte[5]
            {
                0x05, // length
                0x00, // hubId
                (byte)LEMessageType.LEMessageTypePortInformationRequest, // messageId
                portId, // portId
                (byte)portInformationToGet // what to get
            };

            IBuffer buff = message.AsBuffer();

            var res = await LegoHubCharacteristic.Characteristic.WriteValueAsync(buff);
        }





        // message 0x22 which will be responded by a 0x44
        private async Task GetPortModeInformation(byte portId, byte mode, LEMessagePortModeInformationRequestType portModeInformationRequestType)
        {
            // 6 bytes, downstream only
            // Standard Header
            // Port Id
            // Mode to get info of
            // What information

            // 060022010000

            byte[] message = new byte[6]
            {
                0x06,
                0x00,
                (byte)LEMessageType.LEMessageTypePortModeInformationRequest,
                portId,
                mode,
                (byte)portModeInformationRequestType
            };

            IBuffer buff = message.AsBuffer();

            var res = await LegoHubCharacteristic.Characteristic.WriteValueAsync(buff);
        }

        // message 0x42 which will be responded by a 0x48
        private async Task LockDeviceForSetup(byte portId)
        {
            // Starts a device configuration batch
            /*
                0 = 5 (0x5)     // length
                1 = 0 (0x0)     // hub
                2 = 66 (0x42)   // command (input format
                3 = 1 (0x1)     // port id
                4 = 2 (0x2)     // lock
            */

            byte[] message = new byte[]
            {
                0x00,   // length
                0x00,   // hub id
                (byte)LEMessageType.LEMessageTypePortInputFormatSetupCombined,
                portId,
                0x02   // lock
            };

            message[0] = (byte)message.Length;

            IBuffer buff = message.AsBuffer();
            var res = await LegoHubCharacteristic.Characteristic.WriteValueAsync(buff);
        }

        // Activate combined mode
        private async Task ActiveMotorCombinedMode(byte portId)
        {
            await LockDeviceForSetup(portId);
            await PortInputFormatSetup(portId, 0x01, 0x01000000, true);
            await PortInputFormatSetup(portId, 0x02, 0x01000000, true);
            await PortInputFormatSetupCombined(portId, 0x00, 0x10, 0x20);
            await UnlockDeviceWithMultiUpdate(portId, true);
        }

        private async Task UnlockDeviceWithMultiUpdate(byte portId, bool enableUpdate)
        {
            //0 = 5 (0x5)  // length
            //1 = 0 (0x0)  // hub
            //2 = 66 (0x42 // command (input format)
            //3 = 1 (0x1)  // port id
            //4 = 3 (0x3)  // Unlock with update enabled

            byte[] message = new byte[]
           {
                0x00,   // length
                0x00,   // hub id
                (byte)LEMessageType.LEMessageTypePortInputFormatSetupCombined,
                portId,
                (byte) (enableUpdate ? 0x03 : 0x04)
           };

            message[0] = (byte)message.Length;

            IBuffer buff = message.AsBuffer();
            var res = await LegoHubCharacteristic.Characteristic.WriteValueAsync(buff);
        }

        private async Task PortInputFormatSetupCombined(byte portId, byte combinationIndex, byte data0, byte data1)
        {

            //0 = 8(0x8)     // len
            //1 = 0(0x0)     // hub
            //2 = 66(0x42)   // command
            //3 = 1(0x1)     // port id
            //4 = 1(0x1)     // Sub command (set mode and data combinations)
            //5 = 0(0x0)     // combination index
            //6 = 16(0x10)   // Mode/DataSet[0]
            //7 = 32(0x20)   // Mode/DataSet[1]

            byte[] message = new byte[]
            {
                0x00,   // length
                0x00,   // hub id
                (byte)LEMessageType.LEMessageTypePortInputFormatSetupCombined,
                portId,
                0x01,   // lock
                combinationIndex,
                data0,
                data1
            };

            message[0] = (byte)message.Length;

            IBuffer buff = message.AsBuffer();
            var res = await LegoHubCharacteristic.Characteristic.WriteValueAsync(buff);
        }



        /*
            0 = 10 (0xA)    // len
            1 = 0 (0x0)     // hub
            2 = 65 (0x41)   // command
            3 = 1 (0x1)     // port id
            4 = 1 (0x1)     // new mode
            5 = 1 (0x1)     // interval msb
            6 = 0 (0x0)     // interval 
            7 = 0 (0x0)     // interval 
            8 = 0 (0x0)     // interval lsb
            9 = 1 (0x1)     // notification enabled
         */

        /*
           0 = 10 (0xA)    // len
           1 = 0 (0x0)     // hub
           2 = 65 (0x41)   // command
           3 = 1 (0x1)     // port id
           4 = 2 (0x2)     // new mode
           5 = 1 (0x1)     // interval msb
           6 = 0 (0x0)     // interval 
           7 = 0 (0x0)     // interval 
           8 = 0 (0x0)     // interval lsb
           9 = 1 (0x1)     // notification enabled
       */

        /*
            0 = 8 (0x8)     // len
            1 = 0 (0x0)     // hub
            2 = 66 (0x42)   // command
            3 = 1 (0x1)     // port id
            4 = 1 (0x1)     // Sub command (set mode and data combinations)
            5 = 0 (0x0)     // combination index
            6 = 16 (0x10)   // Mode/DataSet[0]
            7 = 32 (0x20)   // Mode/DataSet[1]
        */

        /*
            0 = 5 (0x5)  // length
            1 = 0 (0x0)  // hub
            2 = 66 (0x42 // command (input format)
            3 = 1 (0x1)  // port id
            4 = 3 (0x3)  // Unlock with update enabled
        */


        // Reset combined mode
        /*
0 = 5 (0x5)
1 = 0 (0x0)
2 = 66 (0x42)
3 = 1 (0x1)
4 = 6 (0x6)
            */

    }
}
 