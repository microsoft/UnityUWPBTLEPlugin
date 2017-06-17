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
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace UnityUWPBTLEPlugin
{
    /// <summary>
    /// Wrapper around <see cref="GattDeviceService"/> to make it easier to use
    /// </summary>
    public sealed class GattDeviceServiceWrapper
    {
        /// <summary>
        /// Source for <see cref="Characteristics"/>
        /// </summary>
        private ObservableCollection<GattCharacteristicsWrapper> _characteristics =
            new ObservableCollection<GattCharacteristicsWrapper>();

        /// <summary>
        /// Source for <see cref="Name"/>
        /// </summary>
        private string _name;

        /// <summary>
        /// Source for <see cref="Service"/>
        /// </summary>
        private GattDeviceService _service;

        /// <summary>
        /// Source for <see cref="UUID"/>
        /// </summary>
        private string _uuid;

        /// <summary>
        /// Initializes a new instance of the <see cref="GattDeviceServiceWrapper" /> class.
        /// </summary>
        /// <param name="service">The service this class wraps</param>
        public GattDeviceServiceWrapper(GattDeviceService service)
        {
            Service = service;
            Name = GattUuidsService.ConvertUuidToName(service.Uuid);
            UUID = Service.Uuid.ToString();
            GetAllCharacteristics();
        }

        /// <summary>
        /// Gets or sets the service this class wraps
        /// </summary>
        public GattDeviceService Service
        {
            get { return _service; }

            set
            {
                if (_service != value)
                {
                    _service = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets all the characteristics of this service
        /// </summary>
        public IList<GattCharacteristicsWrapper> Characteristics
        {
            get { return _characteristics; }
        }

        /// <summary>
        /// Gets or sets the name of this service
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
        /// Gets or sets the UUID of this service
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
        /// Gets all the characteristics of this service
        /// </summary>
        private async void GetAllCharacteristics()
        {
            var sb = new StringBuilder();
            sb.Append("GattDeviceServiceWrapper::getAllCharacteristics: ");
            sb.Append(Name);

            try
            {
                var tokenSource = new CancellationTokenSource(5000);
                var t =
                    Task.Run(
                        () => Service.GetCharacteristicsAsync(Windows.Devices.Bluetooth.BluetoothCacheMode.Uncached),
                        tokenSource.Token);

                GattCharacteristicsResult result = null;
                result = await t.Result;

                if (result.Status == GattCommunicationStatus.Success)
                {
                    sb.Append(" - getAllCharacteristics found ");
                    sb.Append(result.Characteristics.Count);
                    sb.Append(" characteristics");
                    //Debug.WriteLine(sb);
                    foreach (GattCharacteristic gattchar in result.Characteristics)
                    {
                        _characteristics.Add(new GattCharacteristicsWrapper(gattchar, this));
                    }
                }
                else if (result.Status == GattCommunicationStatus.Unreachable)
                {
                    sb.Append(" - getAllCharacteristics failed with Unreachable");
                    //Debug.WriteLine(sb.ToString());
                }
                else if (result.Status == GattCommunicationStatus.ProtocolError)
                {
                    sb.Append(" - getAllCharacteristics failed with Unreachable");
                    //Debug.WriteLine(sb.ToString());
                }
            }
            catch (AggregateException ae)
            {
                foreach (var ex in ae.InnerExceptions)
                {
                    if (ex is TaskCanceledException)
                    {
                        Debug.WriteLine("Getting characteristics took too long.");
                        Name += " - Timed out getting some characteristics";
                        return;
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Bug 9145823:GetCharacteristicsAsync throw System.UnauthorizedAccessException when querying GenericAccess Service Characteristics
                Name += " - Unauthorized Access";
            }
            catch (Exception ex)
            {
                Debug.WriteLine("getAllCharacteristics: Exception - {0}" + ex.Message);
                throw;
            }
        }
    }
}
