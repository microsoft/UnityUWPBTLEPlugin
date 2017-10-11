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
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.UI.Xaml.Media.Imaging;

namespace UnityUWPBTLEPlugin
{
    /// <summary>
    /// Wrapper around <see cref="BluetoothLEDevice"/> to make it easier to use
    /// </summary>
    public sealed class BluetoothLEDeviceWrapper
    {
        /// <summary>
        /// Source for <see cref="BluetoothLEDevice"/>
        /// </summary>
        private BluetoothLEDevice _bluetoothLeDevice;

        /// <summary>
        /// Source for <see cref="DeviceInfo"/>
        /// </summary>
        private DeviceInformation _deviceInfo;

        /// <summary>
        /// Source for <see cref="ErrorText"/>
        /// </summary>
        private string _errorText;

        /// <summary>
        /// Source for <see cref="Glyph"/>
        /// </summary>
        private BitmapImage _glyph;

        /// <summary>
        /// Source for <see cref="IsConnected"/>
        /// </summary>
        private bool _isConnected;

        /// <summary>
        /// Source for <see cref="IsPaired"/>
        /// </summary>
        private bool _isPaired;

        /// <summary>
        /// Source for <see cref="Name"/>
        /// </summary>
        private string _name;

        /// <summary>
        /// result of finding all the services
        /// </summary>
        private GattDeviceServicesResult _result;

        /// <summary>
        /// Source for <see cref="Services"/>
        /// </summary>
        private List<GattDeviceServiceWrapper> _services;

        /// <summary>
        /// Initializes a new instance of the<see cref="BluetoothLEDeviceWrapper" /> class.
        /// </summary>
        /// <param name="deviceInfo">The device info that describes this bluetooth device"/></param>
        public BluetoothLEDeviceWrapper(DeviceInformation deviceInfo)
        {
            DeviceInfo = deviceInfo;
            Name = DeviceInfo.Name;

            IsPaired = DeviceInfo.Pairing.IsPaired;
            _services = new List<GattDeviceServiceWrapper>();

            //LoadGlyph();
        }

        /// <summary>
        /// Gets the bluetooth device this class wraps
        /// </summary>
        public BluetoothLEDevice BluetoothLEDevice
        {
            get { return _bluetoothLeDevice; }

            private set
            {
                _bluetoothLeDevice = value;
            }
        }

        /// <summary>
        /// Gets or sets the glyph of this bluetooth device
        /// </summary>
        public BitmapImage Glyph
        {
            get { return _glyph; }

            set
            {
                _glyph = value;
            }
        }

        /// <summary>
        /// Gets the device information for the device this class wraps
        /// </summary>
        public DeviceInformation DeviceInfo
        {
            get { return _deviceInfo; }

            private set
            {
                _deviceInfo = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this device is connected
        /// </summary>
        public bool IsConnected
        {
            get { return _isConnected; }

            set
            {
                if (_isConnected != value)
                {
                    _isConnected = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this device is paired
        /// </summary>
        public bool IsPaired
        {
            get { return _isPaired; }

            set
            {
                if (_isPaired != value)
                {
                    _isPaired = value;
                }
            }
        }

        /// <summary>
        /// Gets the services this device supports
        /// </summary>
        public IEnumerable<GattDeviceServiceWrapper> Services
        {
            get { return _services; }
        }

        /// <summary>
        /// Gets or sets the number of services this device has
        /// </summary>
        public int ServiceCount
        {
            get { return _services.Count; }
        }

        /// <summary>
        /// Gets the name of this device
        /// </summary>
        public string Name
        {
            get { return _name; }

            private set
            {
                if (_name != value)
                {
                    _name = value;
                }
            }
        }

        /// <summary>
        /// Gets the error text when connecting to this device fails
        /// </summary>
        public string ErrorText
        {
            get { return _errorText; }

            private set
            {
                _errorText = value;
            }
        }

        /// <summary>
        /// Gets the bluetooth address of this device as a string
        /// </summary>
        public string BluetoothAddressAsString
        {
            get
            {
                var ret = String.Empty;

                try
                {
                    ret = DeviceInfo.Properties["System.Devices.Aep.DeviceAddress"].ToString();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }

                return ret;
            }
        }

        /// <summary>
        /// Gets the bluetooth address of this device
        /// </summary>
        public ulong BluetoothAddressAsUlong
        {
            get
            {
                try
                {
                    var ret = Convert.ToUInt64(BluetoothAddressAsString.Replace(":", String.Empty), 16);
                    return ret;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("BluetoothAddressAsUlong Getter: " + ex.Message);
                }

                return 0;
            }
        }

        /// <summary>
        /// Connect to this bluetooth device
        /// </summary>
        /// <returns>Connection task</returns>
        public bool Connect()
        {
            var ret = false;
            var debugMsg = "Connect: ";

            try
            {
                if (BluetoothLEDevice == null)
                {
                    // Try to get the cached device (if any) for this id
                    BluetoothLEDevice = BluetoothLEDevice.FromIdAsync(DeviceInfo.Id).GetAwaiter().GetResult();
                }
                else
                {
                    Debug.WriteLine(debugMsg + "Previously connected, not calling BluetoothLEDevice.FromIdAsync");
                }

                if (BluetoothLEDevice == null)
                {
                    ret = false;
                    Debug.WriteLine(debugMsg + "BluetoothLEDevice is null");
                }
                else
                {
                    Debug.WriteLine(debugMsg + "BluetoothLEDevice is " + BluetoothLEDevice.Name);

                    // Setup our event handlers
                    BluetoothLEDevice.ConnectionStatusChanged += BluetoothLEDevice_ConnectionStatusChanged;
                    BluetoothLEDevice.NameChanged += BluetoothLEDevice_NameChanged;

                    IsPaired = DeviceInfo.Pairing.IsPaired;
                    IsConnected = BluetoothLEDevice.ConnectionStatus == BluetoothConnectionStatus.Connected;
                    Name = BluetoothLEDevice.Name;

                    // Get all the services for this device
                    _result = BluetoothLEDevice.GetGattServicesAsync(BluetoothCacheMode.Uncached).GetAwaiter().GetResult();

                    if (_result.Status == GattCommunicationStatus.Success)
                    {
                        Debug.WriteLine(debugMsg + "GetGattServiceAsync SUCCESS");
                        foreach (var serv in _result.Services)
                        {
                            _services.Add(new GattDeviceServiceWrapper(serv));
                        }

                        ret = true;
                    }
                    else if (_result.Status == GattCommunicationStatus.ProtocolError)
                    {
                        ErrorText = debugMsg + "GetGattServiceAsync Error: Protocol Error - " +
                                    _result.ProtocolError.Value;
                        Debug.WriteLine(ErrorText);
                    }
                    else if (_result.Status == GattCommunicationStatus.Unreachable)
                    {
                        ErrorText = debugMsg + "GetGattServiceAsync Error: Unreachable";
                        Debug.WriteLine(ErrorText);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(debugMsg + "Exception - " + ex.Message);
                var msg = String.Format("Message:\n{0}\n\nInnerException:\n{1}\n\nStack:\n{2}", ex.Message,
                    ex.InnerException, ex.StackTrace);

                // Debugger break here so we can catch unknown exceptions
                Debugger.Break();
            }

            if (ret)
            {
                Debug.Write(debugMsg + "Now connected");
            }
            else
            {
                Debug.Write(debugMsg + "Not connected");
            }
            return ret;
        }

        public bool DoInAppPairing(DevicePairingProtectionLevel minProtectionLevel, IDevicePairingSettings devicePairingSettings)
        {
            Debug.WriteLine("Trying in app pairing");

            // BT_Code: Pair the currently selected device.
            DevicePairingResult result = DeviceInfo.Pairing.PairAsync(minProtectionLevel, devicePairingSettings).GetAwaiter().GetResult();

            Debug.WriteLine($"Pairing result: {result.Status}");

            if (result.Status != DevicePairingResultStatus.Paired ||
                result.Status != DevicePairingResultStatus.AlreadyPaired)
            {
                Debug.WriteLine("Pairing error: " + result.Status.ToString());
                return false;
            }
            return true;
        }

        public bool DoInAppPairing()
        {
            Debug.WriteLine("Trying in app pairing");

            // BT_Code: Pair the currently selected device.
            DevicePairingResult result = DeviceInfo.Pairing.PairAsync().GetAwaiter().GetResult();
            bool returnResult = true;

            Debug.WriteLine($"Pairing result: {result.Status}");

            if (result.Status != DevicePairingResultStatus.Paired ||
                result.Status != DevicePairingResultStatus.AlreadyPaired)
            {
                Debug.WriteLine("Pairing error " + result.Status.ToString());
                returnResult = false;
            }
            return returnResult;
        }

        /// <summary>
        /// Executes when the name of this devices changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private  void BluetoothLEDevice_NameChanged(BluetoothLEDevice sender, object args)
        {
            Name = BluetoothLEDevice.Name;
        }

        /// <summary>
        /// Executes when the connection state changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private  void BluetoothLEDevice_ConnectionStatusChanged(BluetoothLEDevice sender, object args)
        {
            IsPaired = DeviceInfo.Pairing.IsPaired;
            IsConnected = BluetoothLEDevice.ConnectionStatus == BluetoothConnectionStatus.Connected;
        }


        /// <summary>
        /// Overrides the ToString function to return the name of the device
        /// </summary>
        /// <returns>Name of this characteristic</returns>
        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Updates this device's deviceInformation
        /// </summary>
        /// <param name="deviceUpdate"></param>
        public void Update(DeviceInformationUpdate deviceUpdate)
        {
            DeviceInfo.Update(deviceUpdate);
        }
    }
}
