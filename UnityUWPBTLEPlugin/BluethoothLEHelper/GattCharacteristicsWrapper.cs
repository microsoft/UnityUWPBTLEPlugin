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
using System.Diagnostics;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;

namespace UnityUWPBTLEPlugin
{
    /// <summary>
    /// Wrapper around <see cref="GattCharacteristic"/>  to make it easier to use
    /// </summary>
    public sealed class GattCharacteristicsWrapper
    {
        /// <summary>
        /// Source for <see cref="Characteristic"/>
        /// </summary>
        private GattCharacteristic _characteristic;

        /// <summary>
        /// byte array representation of the characteristic value
        /// </summary>
        private byte[] _data;

        /// <summary>
        /// Source for <see cref="IsIndicateSet"/>
        /// </summary>
        private bool _isIndicateSet;

        /// <summary>
        /// Source for <see cref="IsNotifySet"/>
        /// </summary>
        private bool _isNotifySet;

        /// <summary>
        /// Raw buffer of this value of this characteristic
        /// </summary>
        private IBuffer _rawData;

        /// <summary>
        /// Source for <see cref="Name"/>
        /// </summary>
        private string _name;

        /// <summary>
        /// Source for <see cref="Parent"/>
        /// </summary>
        private GattDeviceServiceWrapper _parent;

        /// <summary>
        /// Source for <see cref="UUID"/>
        /// </summary>
        private string _uuid;

        /// <summary>
        /// Source for <see cref="Value"/>
        /// </summary>
        private string _value;

        /// <summary>
        /// Initializes a new instance of the<see cref="GattCharacteristicsWrapper" /> class.
        /// </summary>
        /// <param name="characteristic">Characteristic this class wraps</param>
        /// <param name="parent">The parent service that wraps this characteristic</param>
        public GattCharacteristicsWrapper(GattCharacteristic characteristic, GattDeviceServiceWrapper parent)
        {
            Characteristic = characteristic;
            Parent = parent;
            Name = GattUuidsService.ConvertUuidToName(Characteristic.Uuid);
            UUID = Characteristic.Uuid.ToString();

            // get the current value
            ReadValue();

            characteristic.ValueChanged += Characteristic_ValueChanged;
        }

        /// <summary>
        /// Gets or sets the characteristic this class wraps
        /// </summary>
        public GattCharacteristic Characteristic
        {
            get { return _characteristic; }

            set
            {
                if (_characteristic != value)
                {
                    _characteristic = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether indicate is set
        /// </summary>
        public bool IsIndicateSet
        {
            get { return _isIndicateSet; }

            set
            {
                if (_isIndicateSet != value)
                {
                    _isIndicateSet = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether notify is set
        /// </summary>
        public bool IsNotifySet
        {
            get { return _isNotifySet; }

            set
            {
                if (_isNotifySet != value)
                {
                    _isNotifySet = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the parent service of this characteristic
        /// </summary>
        public GattDeviceServiceWrapper Parent
        {
            get { return _parent; }

            set
            {
                if (_parent != value)
                {
                    _parent = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the name of this characteristic
        /// </summary>
        public string Name
        {
            get { return _name; }

            set
            {
                if (_name != value)
                {
                    _name = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the UUID of this characteristic
        /// </summary>
        public string UUID
        {
            get { return _uuid; }

            set
            {
                if (_uuid != value)
                {
                    _uuid = value;
                }
            }
        }

        /// <summary>
        /// Gets the value of this characteristic
        /// </summary>
        public string Value
        {
            get { return _value; }

            private set
            {
                if (_value != value)
                {
                    _value = value;
                }
            }
        }

        /// <summary>
        /// The raw data of the characteristic
        /// </summary>
        public byte[] RawData
        {
            get { return _data; }
        }


        /// <summary>
        /// Reads the value of the Characteristic
        /// </summary>
        public void ReadValue()
        {
            try
            {
                var result = Characteristic.ReadValueAsync(BluetoothCacheMode.Uncached).GetAwaiter().GetResult();

                if (result.Status == GattCommunicationStatus.Success)
                {
                    SetValue(result.Value);
                }
                else if (result.Status == GattCommunicationStatus.ProtocolError)
                {
                    Value = GattProtocolErrorParser.GetErrorString(result.ProtocolError);
                }
                else
                {
                    Value = "Unreachable";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception: " + ex.Message);
                Value = "Exception!";
            }
        }

     
        /// <summary>
        /// Sets the notify characteristic
        /// </summary>
        /// <returns>Set notify task</returns>
        public bool SetNotify()
        {
            if (IsNotifySet)
            {
                // already set
                return true;
            }

            try
            {
                // BT_Code: Must write the CCCD in order for server to send indications.
                // We receive them in the ValueChanged event handler.
                // Note that this sample configures either Indicate or Notify, but not both.
                var result = Characteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify).GetAwaiter().GetResult();
                if (result == GattCommunicationStatus.Success)
                {
                    Debug.WriteLine("Successfully registered for notifications");
                    IsNotifySet = true;
                    return true;
                }
                if (result == GattCommunicationStatus.ProtocolError)
                {
                    Debug.WriteLine("Error registering for notifications: Protocol Error");
                    IsNotifySet = false;
                    return false;
                }
                if (result == GattCommunicationStatus.Unreachable)
                {
                    Debug.WriteLine("Error registering for notifications: Unreachable");
                    IsNotifySet = false;
                    return false;
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                // This usually happens when a device reports that it support indicate, but it actually doesn't.
                Debug.WriteLine("Unauthorized Exception: " + ex.Message);
                IsNotifySet = false;
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Generic Exception: " + ex.Message);
                IsNotifySet = false;
                return false;
            }

            IsNotifySet = false;
            return false;
        }

        /// <summary>
        /// Unsets the notify descriptor
        /// </summary>
        /// <returns>Unset notify task</returns>
        public bool StopNotify()
        {
            if (IsNotifySet == false)
            {
                // indicate is not set, can skip this
                return true;
            }

            try
            {
                // BT_Code: Must write the CCCD in order for server to send indications.
                // We receive them in the ValueChanged event handler.
                // Note that this sample configures either Indicate or Notify, but not both.
                var result = Characteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.None).GetAwaiter().GetResult();
                if (result == GattCommunicationStatus.Success)
                {
                    Debug.WriteLine("Successfully un-registered for notifications");
                    IsNotifySet = false;
                    return true;
                }
                if (result == GattCommunicationStatus.ProtocolError)
                {
                    Debug.WriteLine("Error un-registering for notifications: Protocol Error");
                    IsNotifySet = true;
                    return false;
                }
                if (result == GattCommunicationStatus.Unreachable)
                {
                    Debug.WriteLine("Error un-registering for notifications: Unreachable");
                    IsNotifySet = true;
                    return false;
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                // This usually happens when a device reports that it support indicate, but it actually doesn't.
                Debug.WriteLine("Exception: " + ex.Message);
                IsNotifySet = true;
                return false;
            }

            return false;
        }

        /// <summary>
        /// Executes when value changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private async void Characteristic_ValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                Windows.UI.Core.CoreDispatcherPriority.Normal,
                () => { SetValue(args.CharacteristicValue); });
        }

        /// <summary>
        /// helper function that copies the raw data into byte array
        /// </summary>
        /// <param name="buffer">The raw input buffer</param>
        public void SetValue(IBuffer buffer)
        {
            _rawData = buffer;
            CryptographicBuffer.CopyToByteArray(_rawData, out _data);
        }

    }
}
