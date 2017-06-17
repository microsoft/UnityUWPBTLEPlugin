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

using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace UnityUWPBTLEPlugin
{
    /// <summary>
    /// Helper function when working with <see cref="GattProtocolError"/>
    /// </summary>
    public static class GattProtocolErrorParser
    {
        /// <summary>
        /// Helper to convert an gatt error value into a string
        /// </summary>
        /// <param name="errorValue"></param>
        /// <returns>String representation of the error</returns>
        public static string GetErrorString(byte? errorValue)
        {
            string ret = "Protocol Error";
            
            if (errorValue.HasValue == false)
            {
                return ret;
            }

            if (errorValue == GattProtocolError.AttributeNotFound)
            {
                return "Attribute Not Found";
            }
            else if (errorValue == GattProtocolError.AttributeNotLong)
            {
                return "Attribute Not Long";
            }
            else if (errorValue == GattProtocolError.InsufficientAuthentication)
            {
                return "Insufficient Authentication";
            }
            else if (errorValue == GattProtocolError.InsufficientAuthorization)
            {
                return "Insufficient Authorization";
            }
            else if (errorValue == GattProtocolError.InsufficientEncryption)
            {
                return "Insufficient Encryption";
            }
            else if (errorValue == GattProtocolError.InsufficientEncryptionKeySize)
            {
                return "Insufficient Encryption Key Size";
            }
            else if (errorValue == GattProtocolError.InsufficientResources)
            {
                return "Insufficient Resources";
            }
            else if (errorValue == GattProtocolError.InvalidAttributeValueLength)
            {
                return "Invalid Attribute Value Length";
            }
            else if (errorValue == GattProtocolError.InvalidHandle)
            {
                return "Invalid Handle";
            }
            else if (errorValue == GattProtocolError.InvalidOffset)
            {
                return "Invalid Offset";
            }
            else if (errorValue == GattProtocolError.InvalidPdu)
            {
                return "Invalid Pdu";
            }
            else if (errorValue == GattProtocolError.PrepareQueueFull)
            {
                return "Prepare Queue Full";
            }
            else if (errorValue == GattProtocolError.ReadNotPermitted)
            {
                return "Read Not Permitted";
            }
            else if (errorValue == GattProtocolError.RequestNotSupported)
            {
                return "Request Not Supported";
            }
            else if (errorValue == GattProtocolError.UnlikelyError)
            {
                return "UnlikelyError";
            }
            else if (errorValue == GattProtocolError.UnsupportedGroupType)
            {
                return "Unsupported Group Type";
            }
            else if (errorValue == GattProtocolError.WriteNotPermitted)
            {
                return "Write Not Permitted";
            }

            return ret;
        }
    }
}
