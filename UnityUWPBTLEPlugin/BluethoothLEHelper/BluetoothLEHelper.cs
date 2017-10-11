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
using System.Linq;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Enumeration;

namespace UnityUWPBTLEPlugin
{
    /// <summary>
    /// Context for the entire app. This is where all app wide variables are stored
    /// </summary>
    public class BluetoothLEHelper
    {
        /// <summary>
        /// AQS search string used to find bluetooth devices
        /// </summary>
        private const string BTLEDeviceWatcherAQSString =
            "(System.Devices.Aep.ProtocolId:=\"{bb7bb05e-5972-42b5-94fc-76eaa7084d49}\")";

        /// <summary>
        /// Lock around the <see cref="BluetoothLeDevices"/>. Used in the Add/Removed/Updated callbacks
        /// </summary>
        private readonly object _bluetoothLeDevicesLock;

        /// <summary>
        /// Device watcher used to find bluetooth devices
        /// </summary>
        private DeviceWatcher _deviceWatcher;

        /// <summary>
        /// Source for <see cref="EnumerationFinished"/> property
        /// </summary>
        private bool _enumorationFinished;

        /// <summary>
        /// Source for <see cref="IsCentralRoleSupported"/>
        /// </summary>
        private bool _isCentralRoleSupported = true;

        /// <summary>
        /// Source for <see cref="IsEnumerating"/> property
        /// </summary>
        private bool _isEnumerating;

        /// <summary>
        /// Source for <see cref="IsPeripheralRoleSupported"/>
        /// </summary>
        private bool _isPeripheralRoleSupported = true;

        /// <summary>
        /// We need to cache all DeviceInformation objects we get as they may
        /// get updated in the future. The update may make them eligible to be put on
        /// the displayed list.
        /// </summary>
        private readonly List<DeviceInformation> _unusedDevices;

        /// <summary>
        /// Prevents a default instance of the <see cref="BluetoothLEHelper" /> class from being created.
        /// </summary>
        private BluetoothLEHelper()
        {
            Init();
            _bluetoothLeDevicesLock = new object();
            _unusedDevices = new List<DeviceInformation>();
            _BluetoothLeDevices = new List<BluetoothLEDeviceWrapper>();
            _BluetoothLeDevicesAdded = new List<BluetoothLEDeviceWrapper>();
            _BluetoothLeDevicesRemoved = new List<BluetoothLEDeviceWrapper>();
        }

        /// <summary>
        /// Gets the app context
        /// </summary>
        static BluetoothLEHelper Context = null;
        public static BluetoothLEHelper Instance
        {
            get
            {
                if (Context == null)
                    Context = new BluetoothLEHelper();

                return Context;
            }
        }

        /// <summary>
        /// Gets the list of available bluetooth devices
        /// </summary>
        /// 
        List<BluetoothLEDeviceWrapper> _BluetoothLeDevices;
        List<BluetoothLEDeviceWrapper> _BluetoothLeDevicesAdded;
        List<BluetoothLEDeviceWrapper> _BluetoothLeDevicesRemoved;

        public IEnumerable<BluetoothLEDeviceWrapper> BluetoothLeDevicesAdded
        {
            get
            {
                lock (_bluetoothLeDevicesLock)
                {
                    IEnumerable<BluetoothLEDeviceWrapper> added = _BluetoothLeDevicesAdded; 
                    _BluetoothLeDevicesAdded = new List<BluetoothLEDeviceWrapper>();
                    return added;
                }
            }
        }

        public IEnumerable<BluetoothLEDeviceWrapper> BluetoothLeDevicesRemoved
        {
            get
            {
                lock (_bluetoothLeDevicesLock)
                {
                    IEnumerable<BluetoothLEDeviceWrapper> added = _BluetoothLeDevicesRemoved;
                    _BluetoothLeDevicesRemoved = new List<BluetoothLEDeviceWrapper>();
                    return added;
                }
            }
        }

        public IEnumerable<BluetoothLEDeviceWrapper> BluetoothLeDevices
        {
            get
            {
                lock (_bluetoothLeDevicesLock)
                {
                    return _BluetoothLeDevices;
                }
            }
        }

        /// <summary>
        /// Gets or sets the selected bluetooth device
        /// </summary>
        public BluetoothLEDeviceWrapper SelectedBluetoothLEDevice { get; set; } = null;

        /// <summary>
        /// Gets or sets the selected characteristic
        /// </summary>
        public GattCharacteristicsWrapper SelectedCharacteristic { get; set; } = null;

        /// <summary>
        /// Gets a value indicating whether app is currently enumerating
        /// </summary>
        public bool IsEnumerating
        {
            get { return _isEnumerating; }

            private set
            {
                if (_isEnumerating != value)
                {
                    _isEnumerating = value;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the app is finished enumerating
        /// </summary>
        public bool EnumerationFinished
        {
            get { return _enumorationFinished; }

            private set
            {
                if (_enumorationFinished != value)
                {
                    _enumorationFinished = value;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether peripheral mode is supported by this device
        /// </summary>
        public bool IsPeripheralRoleSupported
        {
            get { return _isPeripheralRoleSupported; }

            private set
            {
                if (_isPeripheralRoleSupported != value)
                {
                    _isPeripheralRoleSupported = value;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether central role is supported by this device
        /// </summary>
        public bool IsCentralRoleSupported
        {
            get { return _isCentralRoleSupported; }

            private set
            {
                if (_isCentralRoleSupported != value)
                {
                    _isCentralRoleSupported = value;
                }
            }
        }

        public bool DevicesChanged { get; internal set; }

        /// <summary>
        /// Initializes the app context
        /// </summary>
        private async void Init()
        {
            BluetoothAdapter adapter = await BluetoothAdapter.GetDefaultAsync();

            IsPeripheralRoleSupported = adapter.IsPeripheralRoleSupported;
            IsCentralRoleSupported = adapter.IsCentralRoleSupported;
        }

        /// <summary>
        /// Starts enumeration of bluetooth device
        /// </summary>
        public void StartEnumeration()
        {
            // Additional properties we would like about the device.
            string[] requestedProperties =
            {
                "System.Devices.Aep.Category",
                "System.Devices.Aep.ContainerId",
                "System.Devices.Aep.DeviceAddress",
                "System.Devices.Aep.IsConnected",
                "System.Devices.Aep.IsPaired",
                "System.Devices.Aep.IsPresent",
                "System.Devices.Aep.ProtocolId",
                "System.Devices.Aep.Bluetooth.Le.IsConnectable"
                ////"System.Devices.Aep.SignalStrength" //remove Sig strength for now. Might bring it back for sorting/filtering
            };

            // BT_Code: Currently Bluetooth APIs don't provide a selector to get ALL devices that are both paired and non-paired.
            _deviceWatcher =
                DeviceInformation.CreateWatcher(
                    BTLEDeviceWatcherAQSString,
                    requestedProperties,
                    DeviceInformationKind.AssociationEndpoint);

            // Register event handlers before starting the watcher.
            _deviceWatcher.Added += DeviceWatcher_Added;
            _deviceWatcher.Updated += DeviceWatcher_Updated;
            _deviceWatcher.EnumerationCompleted += DeviceWatcher_EnumerationCompleted;
            _deviceWatcher.Removed += DeviceWatcher_Removed;
            _deviceWatcher.Stopped += DeviceWatcher_Stopped;

            _BluetoothLeDevices.Clear();
            DevicesChanged = false;

            _deviceWatcher.Start();
            IsEnumerating = true;
            EnumerationFinished = false;
        }

        /// <summary>
        /// Stops enumeration of bluetooth device
        /// </summary>
        public void StopEnumeration()
        {
            if (_deviceWatcher != null)
            {
                // Unregister the event handlers.
                _deviceWatcher.Added -= DeviceWatcher_Added;
                _deviceWatcher.Updated -= DeviceWatcher_Updated;
                _deviceWatcher.Removed -= DeviceWatcher_Removed;
                _deviceWatcher.EnumerationCompleted -= DeviceWatcher_EnumerationCompleted;
                _deviceWatcher.Stopped -= DeviceWatcher_Stopped;

                // Stop the watchers
                _deviceWatcher.Stop();
                _deviceWatcher = null;

                IsEnumerating = false;
                EnumerationFinished = false;
            }
        }

        /// <summary>
        /// Callback when a new device is found
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="deviceInfo"></param>
        private void DeviceWatcher_Added(DeviceWatcher sender, DeviceInformation deviceInfo)
        {
            try
            {
                // Protect against race condition if the task runs after the app stopped the deviceWatcher.
                if (sender == _deviceWatcher)
                {
                    AddDeviceToList(deviceInfo);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DeviceWatcher_Added: " + ex.Message);
            }
        }

        /// <summary>
        /// Executes when a device is updated
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="deviceInfoUpdate"></param>
        private void DeviceWatcher_Updated(DeviceWatcher sender, DeviceInformationUpdate deviceInfoUpdate)
        {
            DeviceInformation di = null;
            var addNewDI = false;

            try
            {
                // Protect against race condition if the task runs after the app stopped the deviceWatcher.
                if (sender == _deviceWatcher)
                {
                    BluetoothLEDeviceWrapper dev;

                    // Need to lock as another DeviceWatcher might be modifying BluetoothLEDevices 
                    lock (_bluetoothLeDevicesLock)
                    {
                        dev =
                            BluetoothLeDevices.FirstOrDefault(
                                device => device.DeviceInfo.Id == deviceInfoUpdate.Id);
                        if (dev != null)
                        {
                            // Found a device in the list, updating it
                            dev.Update(deviceInfoUpdate);
                        }
                        else
                        {
                            // Need to add this device. Can't do that here as we have the lock
                            addNewDI = true;
                        }
                    }

                    if (addNewDI)
                    {
                        lock (_bluetoothLeDevicesLock)
                        {
                            di = _unusedDevices.FirstOrDefault(device => device.Id == deviceInfoUpdate.Id);
                            if (di != null)
                            {
                                // We found this device before.
                                _unusedDevices.Remove(di);
                                di.Update(deviceInfoUpdate);
                            }
                        }

                        if (di != null)
                        {
                            AddDeviceToList(di);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DeviceWatcher_Updated exception: " + ex.Message);
            }
        }

        /// <summary>
        /// Executes when a device is removed from enumeration
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="deviceInfoUpdate"></param>
        private void DeviceWatcher_Removed(DeviceWatcher sender, DeviceInformationUpdate deviceInfoUpdate)
        {
            try
            {
                // Protect against race condition if the task runs after the app stopped the deviceWatcher.
                if (sender == _deviceWatcher)
                {
                    BluetoothLEDeviceWrapper dev;

                    // Need to lock as another DeviceWatcher might be modifying BluetoothLEDevices 
                    lock (_bluetoothLeDevicesLock)
                    {
                        // Find the corresponding DeviceInformation in the collection and remove it.
                        dev = BluetoothLeDevices.FirstOrDefault(device => device.DeviceInfo.Id == deviceInfoUpdate.Id);
                        if (dev != null)
                        {
                            // Found it in our displayed devices
                            var removed = _BluetoothLeDevices.Remove(dev);
                            _BluetoothLeDevicesRemoved.Add(dev);
                            DevicesChanged = true;
                            Debug.Assert(removed == true, "DeviceWatcher_Removed: Failed to remove device from list");
                        }
                        else
                        {
                            // Did not find in displayed list, let's check the unused list
                            var di = _unusedDevices.FirstOrDefault(device => device.Id == deviceInfoUpdate.Id);

                            if (di != null)
                            {
                                // Found in unused devices, remove it
                                var removed = _unusedDevices.Remove(di);
                                Debug.Assert(removed == true, "DeviceWatcher_Removed: Failed to remove device from unused");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("DeviceWatcher_Removed: " + ex.Message);
            }
        }

        /// <summary>
        /// Executes when Enumeration has finished
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeviceWatcher_EnumerationCompleted(DeviceWatcher sender, object e)
        {
            // Protect against race condition if the task runs after the app stopped the deviceWatcher.
            if (sender == _deviceWatcher)
            {
                Debug.WriteLine("DeviceWatcher_EnumerationCompleted: Enumeration Finished");
                StopEnumeration();
                EnumerationFinished = true;
            }
        }

        /// <summary>
        /// Adds the new or updated device to the displayed or unused list
        /// </summary>
        /// <param name="deviceInfo"></param>
        /// <returns></returns>
        private void AddDeviceToList(DeviceInformation deviceInfo)
        {
            // Make sure device name isn't blank or already present in the list.
            if (deviceInfo.Name != string.Empty)
            {
                BluetoothLEDeviceWrapper dev = new BluetoothLEDeviceWrapper(deviceInfo);

                // Let's make it connectible by default, we have error handles in case it doesn't work
                var connectable = true;

                // If the connectible key exists then let's read it
                if (dev.DeviceInfo.Properties.Keys.Contains("System.Devices.Aep.Bluetooth.Le.IsConnectable") == true)
                {
                    connectable = (bool)dev.DeviceInfo.Properties["System.Devices.Aep.Bluetooth.Le.IsConnectable"];
                }

                if (connectable)
                {
                    // Need to lock as another DeviceWatcher might be modifying BluetoothLEDevices 
                    lock (_bluetoothLeDevicesLock)
                    {
                        if (BluetoothLeDevices.Contains(dev) == false)
                        {
                            _BluetoothLeDevices.Add(dev);
                            _BluetoothLeDevicesAdded.Add(dev);
                            DevicesChanged = true;
                        }
                    }
                }
                else
                {
                    lock (_bluetoothLeDevicesLock)
                    {
                        // Found but not adding because it's not connectable
                        _unusedDevices.Add(deviceInfo);
                    }
                }
            }
            else
            {
                lock (_bluetoothLeDevicesLock)
                {
                    //Found device without a name
                    _unusedDevices.Add(deviceInfo);
                }
            }
        }

        /// <summary>
        /// Executes when device watcher has stopped
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeviceWatcher_Stopped(DeviceWatcher sender, object e)
        {
            // Implemented for completeness
        }
    }
}
