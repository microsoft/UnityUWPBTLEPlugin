using UnityUWPBTLEPlugin;

using System;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace WonderWorkshop
{
    public class Command
    {
        public enum eCommandId
        {
            LeftEarLight = 0x0b,
            RightEarLight = 0x0c,
            ChestEyeLight = 0x03,
            PlaySound = 0x18,
            PlayAnimation = 0x26,
            StopAnimation = 0x2b,
            HeadPan = 0x06,
            HeadTilt = 0x07,
            Pose = 0x23,
            Power = 0x2E,
            Stop = 0x2F,
            WheelSpeed = 0x01,
            StopSound = 0x1a,
        }

        public void ShowFeedback(string msg)
        {
            Debug.WriteLine(msg);
            //LegoBTLE.MainPage.thePage.ShowFeedback(msg);
        }


        eCommandId commandId;
        // Derived commands will have payload of from 0 to 19 bytes in length depending on command.
        private byte commandLength = 1;


        public Command(eCommandId c, byte length)
        {
            CommandId = c;
            CommandLength = length;
        }

        public eCommandId CommandId { get => commandId; set => commandId = value; }
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

    public class LightRGB : Command
    {
        public LightRGB(byte r, byte g, byte b, eCommandId c) : base(c, 4)
        {

            Red = r;
            Green = g;
            Blue = b;
        }
        private byte red;
        private byte green;
        private byte blue;

        public byte Red { get => red; set => red = value; }
        public byte Green { get => green; set => green = value; }
        public byte Blue { get => blue; set => blue = value; }

        public override void PopulateBuffer(byte[] buffer)
        {
            base.PopulateBuffer(buffer);
            buffer[1] = Red;
            buffer[2] = Green;
            buffer[3] = Blue;
        }

    }

    public class LeftEarRGB : LightRGB
    {
        public LeftEarRGB(byte r, byte g, byte b) : base(r, g, b, eCommandId.LeftEarLight) { }
    }


    public class RightEarRGB : LightRGB
    {
        public RightEarRGB(byte r, byte g, byte b) : base(r, g, b, eCommandId.RightEarLight) { }
    }

    public class ChestEyeRGB : LightRGB
    {
        public ChestEyeRGB(byte r, byte g, byte b) : base(r, g, b, eCommandId.ChestEyeLight) { }
    }

    public class PlaySound : Command
    {
        // NOTE: Filename must be padded out to 10 bytes with trailing 0x0
        public static string[] BuiltInSounds =
        {
                "SONARPING",
                "RASPBERRY",
                "HOWDY",
                "HORSEWHIN2",
                "HORSEWHIN3"
            };

        const string folder = "SYST";

        string SoundToPlay;
        public PlaySound(string soundToPlay) : base(eCommandId.PlaySound, 15)
        {
            SoundToPlay = soundToPlay;
        }
        public override void PopulateBuffer(byte[] buffer)
        {
            try
            {
                base.PopulateBuffer(buffer);
                var tmp = System.Text.Encoding.ASCII.GetBytes(folder);
                System.Buffer.BlockCopy(tmp, 0, buffer, 1, tmp.Length);
                tmp = System.Text.Encoding.ASCII.GetBytes(SoundToPlay);
                System.Buffer.BlockCopy(tmp, 0, buffer, 5, tmp.Length);
            }catch(Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }

    }

    public class StopSound : Command
    {
        public StopSound() : base(eCommandId.StopSound, 1)
        { }

    }

    public class PlayAnimation : Command
    {
        const string folder = "SYST";

        // NOTE: Filename must be padded out to 10 bytes with trailing 0x0
        public static string[] BuiltIn =
        {
                "A10000_0", // 
                "A10001_0", // 
                "A10002_0", // 
                "A10003_0", // 
                "A10004_0", // 
                "A10005_0", // 
                "A10006_0", // laughing
                "A10007_0", // 
                "A10008_0", // 
                "A10009_0", // 
                "A10010_0", // 
                "A10011_0", // 
                "A10012_0", // 
                "A10013_0", // 
                "A10014animationName_0", // dizzy
            };

        string animationName;

        public PlayAnimation(string animName) : base(eCommandId.PlayAnimation, 15)
        {
            AnimationName = animName;
        }

        public string AnimationName { get => animationName; set => animationName = value; }

        public override void PopulateBuffer(byte[] buffer)
        {
            try
            {
                base.PopulateBuffer(buffer);
                var tmp = System.Text.Encoding.ASCII.GetBytes(folder);
                System.Buffer.BlockCopy(tmp, 0, buffer, 1, tmp.Length);
                tmp = System.Text.Encoding.ASCII.GetBytes(AnimationName);
                System.Buffer.BlockCopy(tmp, 0, buffer, 5, tmp.Length);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }

    }
    public class StopAnimation : Command
    {
        public StopAnimation() : base(eCommandId.StopAnimation, 1)
        {
            // Nothing additional
        }
    }

    // Only available on Dash, looking down onto dash from above
    // - = clockwise
    // + = counter clockwise
    public class HeadPan : Command
    {
        short panDegrees;
        public HeadPan(short Degrees) : base(eCommandId.HeadPan, 3)
        {
            PanDegrees = Degrees;
        }

        public short PanDegrees { get => panDegrees; set => panDegrees = value; }
        public override void PopulateBuffer(byte[] buffer)
        {
            // convert to centedegrees
            short cent = (short)(PanDegrees * 100);
            try
            {
                base.PopulateBuffer(buffer);
                buffer[1] = (byte)(cent % 0xFF);
                buffer[2] = (byte)(cent >> 8);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }

    }

    // Only available on Dash, looking at dash from front
    // - = up
    // + = down
    public class HeadTilt : Command
    {
        short tiltDegrees;
        public HeadTilt(short Degrees) : base(eCommandId.HeadTilt, 3)
        {
            TiltDegrees = Degrees;
        }

        public short TiltDegrees { get => tiltDegrees; set => tiltDegrees = value; }
        public override void PopulateBuffer(byte[] buffer)
        {
            // negate
            short temp = (short)(TiltDegrees * -1);
            try
            {
                base.PopulateBuffer(buffer);
                buffer[1] = (byte)(temp >> 8);
                buffer[2] = (byte)(temp & 0xFF); 
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }
    }

    public enum eWhichPose
    {
        CCW180,
        CCW90,
        CCW45,
        CW45,
        CW90,
        CW180,
        Forward30cm,
        Forward20cm,
        Forward10cm,
        Back10cm,
        Back20cm,
        Back30cm
    };

    // Notes on movement:
    // "Right-handed coordinate system"
    // Forward = + Y
    // Turning head / body counter-clockwise = + theta
    // Points to left of robot are + X, to the right - X

    // used to have robot achieve a given "pose" in an amount of time.  Pose = X, Y, Theta
    public class Pose : Command
    {
        eWhichPose whichPose;

        byte[][] poseStrings =
        {
            new byte[] {0x00, 0x00, 0x3a, 0x00, 0xfa, 0x40, 0x00, 0x80 }, // ccw 180
            new byte[] {0x00, 0x00, 0x9d, 0x00, 0xfa, 0x00, 0x00, 0x80 }, // ccw 90
            new byte[] {0x00, 0x00, 0x4e, 0x00, 0xfa, 0x00, 0x00, 0x80 }, // ccw 45

            new byte[] {0x00, 0x00, 0xB2, 0x00, 0xfa, 0xc0, 0xc0, 0x80 }, // cw 45
            new byte[] {0x00, 0x00, 0x63, 0x00, 0xfa, 0xc0, 0xc0, 0x80 }, // cw 90
            new byte[] {0x00, 0x00, 0xc6, 0x00, 0xfa, 0x80, 0xc0, 0x80 }, // cw 180

            new byte[] {0x2c, 0x00, 0x00, 0x03, 0xe8, 0x01, 0x00, 0x82 }, // FWD 30cm
            new byte[] {0xc8, 0x00, 0x00, 0x03, 0xe8, 0x00, 0x00, 0x82 }, // FWD 20cm
            new byte[] {0x64, 0x00, 0x00, 0x03, 0xe8, 0x00, 0x00, 0x82 }, // FWD 10cm

            new byte[] {0x9c, 0x00, 0x00, 0x03, 0xe8, 0x3f, 0x00, 0x82 }, // Back 10cm
            new byte[] {0x38, 0x00, 0x00, 0x03, 0xe8, 0x3f, 0x00, 0x82 }, // Back 10cm
            new byte[] {0xd4, 0x00, 0x00, 0x03, 0xe8, 0x3e, 0x00, 0x82 }  // Back 10cm
};

        public Pose(eWhichPose pose) : base(eCommandId.Pose, 9)
        {
            WhichPose = pose;
        }

        public eWhichPose WhichPose { get => whichPose; set => whichPose = value; }
        public override void PopulateBuffer(byte[] buffer)
        {
            try
            {
                base.PopulateBuffer(buffer);

                System.Buffer.BlockCopy(poseStrings[(int)WhichPose], 0, buffer, 1, poseStrings[(int)WhichPose].Length);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
        }
    }

    public enum ePowerFunctions
    {
        Reset = 0x04,
        // TurnOff = ??,
        // Reboot = ??
    };

    public class Power : Command
    {
        ePowerFunctions powerFunction;
        public Power(ePowerFunctions function) : base(eCommandId.Power, 2)
        {
            PowerFunction = function;
        }

        public ePowerFunctions PowerFunction { get => powerFunction; set => powerFunction = value; }
    }

    public class WheelSpeed : Command
    {
        short leftCmPerSec;
        short rightCmPerSec;
        public WheelSpeed(short leftCmPerSec, short rightCmPerSec) : base(eCommandId.WheelSpeed, 5)
        {
            LeftSpeed = leftCmPerSec;
            RightSpeed = rightCmPerSec;
        }

        // Robot wants this speed in 1/100 millimeter per 30 milliseconds
        short CommandWheelSpeed(short cmPerSec)
        {
            return (short)((cmPerSec / 100.0f) / (0.01f / 1000.0f / (0.03f)) * cmPerSec * 30);
        }

        public short LeftSpeed { get => leftCmPerSec; set => leftCmPerSec = value; }
        public short RightSpeed { get => rightCmPerSec; set => rightCmPerSec = value; }
    }


    public class Stop : WheelSpeed
    {
        public Stop() : base(0, 0)
        {
        }
    }

}