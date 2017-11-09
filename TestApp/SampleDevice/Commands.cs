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
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using UnityUWPBTLEPlugin;
using Windows.Storage.Streams;

namespace TestApp.Sample
{

    public class Command
    {
        public enum CommandIds
        {
            MoveLeft = 0x01,
            MoveRight = 0x02,
            GetBattery = 0xA,
            // etc
        }

        public void ShowFeedback(string msg)
        {
            Debug.WriteLine(msg);
        }


        CommandIds commandId;
        // Derived commands will have payload of from 0 to 19 bytes in length depending on command.
        private byte commandLength = 1;

        // How a command is packaged and sent will be determined by the BTLE device manufacturer


        public Command(CommandIds c, byte length)
        {
            CommandId = c;
            CommandLength = length;
        }

        public CommandIds CommandId { get => commandId; set => commandId = value; }
        public byte CommandLength { get => commandLength; set => commandLength = value; }

        public virtual void PopulateBuffer(byte[] buffer)
        {
            buffer[0] = (byte)commandId;
        }

        internal async Task Send(GattCharacteristicsWrapper commandCharacteristic)
        {
            byte[] message = new byte[CommandLength];
            PopulateBuffer(message);

            IBuffer buff = message.AsBuffer();

            await commandCharacteristic.Characteristic.WriteValueAsync(buff);
        }
    }

}
