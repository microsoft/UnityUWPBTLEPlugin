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
        private void LegoHubCharacteristic_PropertyChanged(GattCharacteristicsWrapper sender)
        {
            GattCharacteristicsWrapper c = sender as GattCharacteristicsWrapper;
            Debug.Assert(c != null);

            if (c != null)
            {
                byte[] messageData = new byte[c.RawData[0]];
                c.RawData.CopyTo(messageData, 0);

                LEMessageType messageType;
                LEMessageType.TryParse(messageData[2].ToString(), out messageType);
                switch (messageType)
                {
                    case LEMessageType.LEMessageTypeHubProperties:
                        {
                            ProcessHubProperties(messageData);
                        }
                        break;

                    case LEMessageType.LEMessageTypeError:
                        {
                            ProcessMessageError(messageData);
                        }
                        break;

                    case LEMessageType.LEMessageTypeHubAttachedIO:
                        {
                            ProcessHubAttachedIo(messageData);
                        }
                        break;

                    case LEMessageType.LEMessageTypePortOutputCommandFeedback:
                        {
                            ProcessPortOutputComandFeedback(messageData);
                        }
                        break;

                    case LEMessageType.LEMessageTypePortInformation:
                        {
                            ProcessGetPortInformation(messageData);
                        }
                        break;

                    case LEMessageType.LEMessageTypePortValueSingle:
                        {
                            ProcessGetPortInformationValueSingle(messageData);
                        }
                        break;

                    case LEMessageType.LEMessageTypePortValueCombined:
                        {
                            ProcessGetPortInformationValueCombined(messageData);
                        }
                        break;

                    case LEMessageType.LEMessageTypePortModeInformation:
                        {
                            ProcessGetPortModeInformation(messageData);
                        }
                        break;

                    default:
                        {
                            Debug.WriteLine($"Unhandled characteristic update {messageType}");
                        }
                        break;
                }

            }

        }


        // The low level characteristic change notification.  We will use this to trigger the observable characteristic to read
        // the new value.
        private void Characteristic_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            if (sender != null)
            {
                LegoHubCharacteristic.ReadValue();
                LegoHubCharacteristic_PropertyChanged(LegoHubCharacteristic);
            }
        }

        private void ProcessMessageError(byte[] messageData)
        {
            // Message 0x05

            // Standard header
            byte length = messageData[0];
            byte hubId = messageData[1];
            byte messageType = messageData[2]; // will be 0x05

            LEMessageType respondingToMessageType;
            LEMessageType.TryParse(messageData[3].ToString(), out respondingToMessageType);


            LEMessageErrorType error;
            LEMessageErrorType.TryParse(messageData[4].ToString(), out error);

            Debug.WriteLine($"Response error: {error} to message {respondingToMessageType} (probably nothing connected to port)");

        }
        private void ProcessHubProperties(byte[] messageData)
        {
            // Standard header
            byte length = messageData[0];
            byte hubId = messageData[1];
            byte messageType = messageData[2];

            HubProperty propertyId;
            HubProperty.TryParse(messageData[3].ToString(), out propertyId);

            switch (propertyId)
            {
                case HubProperty.BATTERY_VOLTAGE:
                    {
                        ProcessGetBatteryLevel(messageData);
                    }
                    break;

                case HubProperty.MANUFACTURER_NAME:
                    {
                        ProcessGetManufacturerName(messageData);
                    }
                    break;

                default:
                    {
                        Debug.WriteLine($"Unhandled Hub Property Request {propertyId}");
                        Debug.Fail("Unhandled Hub Property Request {propertyId}");
                    }
                    break;
            }
        }

        private void ProcessPortOutputComandFeedback(byte[] messageData)
        {
            // Standard header
            byte length = messageData[0];
            byte hubId = messageData[1];
            byte messageType = messageData[2];

            int messageCount = (length - 3) / 2;
            byte portIdPosition = 3;
            byte feedbackPosition = 4;

            byte portId;
            PortOutputCommandFeedbackType feedback = new PortOutputCommandFeedbackType();

            for (int whichPort = 0; whichPort < messageCount; whichPort++)
            {
                portId = messageData[portIdPosition];
                feedback.Data = messageData[feedbackPosition];

                Debug.WriteLine($"PortOutput Feedback {portId} Event: {feedback.ToString()}");
            }
        }

        private void ProcessHubAttachedIo(byte[] data)
        {
            byte portId = data[4];
            AttachedIoEvent ioEvent;
            AttachedIoEvent.TryParse(data[4].ToString(), out ioEvent);
            Debug.WriteLine($"Hub attached io event Port: {portId} Event: {ioEvent.ToString()}");
        }

        private void ProcessGetPortModeInformation(byte[] messageData)
        {
            // Message 0x44

            // Standard header
            byte length = messageData[0];
            byte hubId = messageData[1];
            byte messageType = messageData[2];

            byte portId = messageData[3];
            byte mode = messageData[4];

            LEMessagePortModeInformationRequestType requestType;
            LEMessagePortModeInformationRequestType.TryParse(messageData[5].ToString(), out requestType);

            switch (requestType)
            {
                case LEMessagePortModeInformationRequestType.LEMessagePortModeInformationRequestTypeName:
                    {
                        string name = System.Text.Encoding.UTF8.GetString(messageData);

                        name = name.Substring(6);
                        name.TrimEnd();
                        Debug.WriteLine($"Port {portId} mode information MODE NAME: {name}");
                        Debug.WriteLine("");
                    }
                    break;

                default:
                    {
                        Debug.WriteLine($"GetPortModeInformation Unhandled PortModeInformationRequestType {requestType} TODO");
                        Debug.Fail("GetPortModeInformation Unhandled PortModeInformationRequestType {requestType} TODO");
                    }
                    break;
            }
        }


        string _ManufacturerName = "";
        private void ProcessGetManufacturerName(byte[] messageData)
        {
            string _Data = System.Text.Encoding.UTF8.GetString(messageData);

            // cut off the non string part of the message
            _ManufacturerName = _Data.Substring(5);

            ShowFeedback($"Manufacturer Name: {_ManufacturerName}");

        }


        byte _batteryLevel = 0xFF;
        // Should return:
        // 3 byte header LLIITT
        //    LL = total length of message
        //    II = Hub id
        //    TT = Message type, in this case should be 01 (Hub property)
        // 1 byte property ID = 0x06 (battery voltage)
        // 1 byte Operation id = 0x06 (Update)
        // 1 bytes of Uint8 payload = 0x00-0x64 (0-100 dec)
        private void ProcessGetBatteryLevel(byte[] messageData)
        {
            // Standard header
            byte length = messageData[0];
            byte hubId = messageData[1];
            byte messageType = messageData[2];

            byte propertyId = messageData[3];
            byte operationId = messageData[4];
            byte batteryLevel = LegoHubCharacteristic.RawData[5];

            Debug.Assert(length == 6);
            Debug.Assert(messageType == (byte)LEMessageType.LEMessageTypeHubProperties);
            Debug.Assert(propertyId == (byte)HubProperty.BATTERY_VOLTAGE);

            _batteryLevel = batteryLevel;
            ShowFeedback($"Battery level: {batteryLevel}%");
        }




        private void ProcessGetPortInformationValueSingle(byte[] messageData)
        {
            // 0x45
            // Standard header
            byte length = messageData[0];
            byte hubId = messageData[1];
            byte messageType = messageData[2];

            byte portId = LegoHubCharacteristic.RawData[3];

            // Resultant type of value with be determined by message length
            switch (length)
            {
                case 5: // Uint8
                    {
                        byte value = LegoHubCharacteristic.RawData[4];
                        ShowFeedback($"Port {portId} Uint8 Value: {value}");
                    }
                    break;
                case 6: // Uint16
                    {
                        UInt16 value = LegoHubCharacteristic.RawData[4];
                        ShowFeedback($"Port {portId} Uint16 Value: {value}");
                    }
                    break;
                case 8: // Uint32 or float
                    {
                        UInt32 value = LegoHubCharacteristic.RawData[4];
                        ShowFeedback($"Port {portId} Uint32 / float Value: {value}");
                    }
                    break;
            }
        }

        private void ProcessGetPortInformationValueCombined(byte[] messageData)
        {
            // 0x45
            // Standard header
            byte length = messageData[0];
            byte hubId = messageData[1];
            byte messageType = messageData[2];

            byte portId = LegoHubCharacteristic.RawData[3];

            byte inputValue = LegoHubCharacteristic.RawData[4];

            UInt16 a = (UInt16)(LegoHubCharacteristic.RawData[5] << 8);
            UInt16 b = (UInt16)LegoHubCharacteristic.RawData[6];

            UInt16 modeDatasetBits = (UInt16)(a + b);

            UInt16[] modesArray = new UInt16[16];

            int valuesCount = 0;
            for (int i = 0; i <= 15; ++i)
            {
                int bit = (1 << i);
                if ((modeDatasetBits & bit) != 0)
                {
                    valuesCount++;
                    modesArray[i] = 1;
                }
            }

            try
            {
                a = (UInt16)(LegoHubCharacteristic.RawData[8] << 8);
                b = (UInt16)LegoHubCharacteristic.RawData[7];
                UInt16 Value1 = (UInt16)(a + b);

                a = (UInt16)(LegoHubCharacteristic.RawData[9] << 8);
                b = (UInt16)LegoHubCharacteristic.RawData[10];
                UInt16 Value2 = (UInt16)(a + b);

                ShowFeedback($"Port: {portId} PortValueCombined Value1:{Value1} Value2:{Value2}");
                //Debug.Fail("Port: {portId} PortValueCombined TODO");
            }
            catch(Exception)
            {
                Debug.WriteLine($"Port: {portId} PortValueCombined Exception");

            }
        }


        private void ProcessGetPortInformation(byte[] messageData)
        {
            // 0x43
            // Standard header
            byte length = messageData[0];
            byte hubId = messageData[1];
            byte messageType = messageData[2];

            byte msgPortId = LegoHubCharacteristic.RawData[3];

            LEMessagePortInformationType informationType;
            LEMessagePortInformationType.TryParse(LegoHubCharacteristic.RawData[4].ToString(), out informationType);

            switch (informationType)
            {
                case LEMessagePortInformationType.Mode:
                    {
                        // Message should be 11 bytes
                        Debug.Assert(length == 11);

                        byte capabilities = LegoHubCharacteristic.RawData[5];
                        byte numberOfPortModes = LegoHubCharacteristic.RawData[6];
                        UInt16 inputPortModes = (UInt16)(LegoHubCharacteristic.RawData[7] << 8 + LegoHubCharacteristic.RawData[8]);
                        UInt16 outputPortModes = (UInt16)(LegoHubCharacteristic.RawData[9] << 8 + LegoHubCharacteristic.RawData[10]);

                        Debug.WriteLine($"Port {msgPortId} information, Capabilities: {capabilities}, inputPortModes: {inputPortModes}, outputPortModes: {outputPortModes}");
                    }
                    break;

                case LEMessagePortInformationType.ModeCombinations:
                    {
                        // message should be 21 bytes
                        Debug.Assert(length == 21);
                        Debug.WriteLine($"Port {msgPortId} LEMessageTypePortInformation Complex mode information TODO");
                    }
                    break;

                default:
                    {
                        // Invalid result
                        Debug.WriteLine($"Port {msgPortId} LEMessageTypePortInformation unexpected information type {informationType}");
                    }
                    break;
            }
        }

    }

        
}
 