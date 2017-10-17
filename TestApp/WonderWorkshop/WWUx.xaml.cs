using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using WonderWorkshop;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace TestApp.WonderWorkshop
{
    public sealed partial class WWUx : UserControl
    {
        DotDashBot theRobot;

        public DotDashBot TheRobot { get => theRobot; set => theRobot = value; }

        public WWUx(DotDashBot bot)
        {
            this.InitializeComponent();
            TheRobot = bot;
        }


        private void OnTestLeftEar(object sender, RoutedEventArgs e)
        {
            var leftEarCommand = new LeftEarRGB(128, 255, 14);
            TheRobot.SendCommand(leftEarCommand).GetAwaiter();
        }

        internal void UpdateCommandText(string v)
        {
            //_Command.Text = v;
        }

        internal void UpdateSensor1Text(string v)
        {
            //_Sensor1.Text = v;
        }

        internal void UpdateSensor2Text(string v)
        {
            //_Sensor2.Text = v;
        }

        private void OnTestRightEar(object sender, RoutedEventArgs e)
        {

            var rightEyeCommand = new RightEarRGB(83, 1, 255);
            TheRobot.SendCommand(rightEyeCommand).GetAwaiter();

        }

        private void OnTestChestEye(object sender, RoutedEventArgs e)
        {

            var chestEyeCommand = new ChestEyeRGB(255, 1, 255);
            TheRobot.SendCommand(chestEyeCommand).GetAwaiter();

        }

        private void OnTestPlaySound(object sender, RoutedEventArgs e)
        {

            var playSoundCommand = new PlaySound(PlaySound.BuiltInSounds[2]);
            TheRobot.SendCommand(playSoundCommand).GetAwaiter();
        }

        private async void OnParty(object sender, RoutedEventArgs e)
        {
            Random ran = new Random();

            for (int i = 0; i < 10; i++)
            {
                var leftEyeCommand = new LeftEarRGB((byte)ran.Next(0, 255), (byte)ran.Next(0, 255), (byte)ran.Next(0, 255));
                await TheRobot.SendCommand(leftEyeCommand);

                var rightEyeCommand = new RightEarRGB((byte)ran.Next(0, 255), (byte)ran.Next(0, 255), (byte)ran.Next(0, 255));
                await TheRobot.SendCommand(rightEyeCommand);

                var chestEyeCommand = new ChestEyeRGB((byte)ran.Next(0, 255), (byte)ran.Next(0, 255), (byte)ran.Next(0, 255));
                await TheRobot.SendCommand(chestEyeCommand);

                var playSoundCommand = new PlaySound(PlaySound.BuiltInSounds[ran.Next(0, PlaySound.BuiltInSounds.Length)]);
                await TheRobot.SendCommand(playSoundCommand);

                var headTiltCommand = new HeadTilt((byte)ran.Next(-2250, 700));
                await TheRobot.SendCommand(headTiltCommand);

                var headPanCommand = new HeadPan((byte)ran.Next(-9000, 9000));
                await TheRobot.SendCommand(headPanCommand);

                await Task.Delay(1000);
            }
            OnTestHeadCenter(null, null);
        }

        private void OnTestPlayAnimation(object sender, RoutedEventArgs e)
        {
            Random ran = new Random();
            var playAnimationCommand = new PlayAnimation(PlayAnimation.BuiltIn[ran.Next(0, PlayAnimation.BuiltIn.Length)]);// should be laughing
            TheRobot.SendCommand(playAnimationCommand).GetAwaiter();
        }

        private void OnTestHeadCenter(object sender, RoutedEventArgs e)
        {
            var TiltCommand = new HeadTilt(0);
            TheRobot.SendCommand(TiltCommand).GetAwaiter();
            var PanCommand = new HeadPan(0);
            TheRobot.SendCommand(PanCommand).GetAwaiter();

        }

        // Tilt and pan are absolute positions, bot will move to the entered value, NOT move from current by that much more.
        private void OnTestHeadTiltUp(object sender, RoutedEventArgs e)
        {
            var TiltCommand = new HeadTilt(-2250); // up 22.5 (max)
            TheRobot.SendCommand(TiltCommand).GetAwaiter();
        }

        private void OnTestHeadTiltDown(object sender, RoutedEventArgs e)
        {
            var TiltCommand = new HeadTilt(700); // down 7 (max)
            TheRobot.SendCommand(TiltCommand).GetAwaiter();
        }
        private void OnTestHeadPanLeft(object sender, RoutedEventArgs e)
        {
            var PanCommand = new HeadPan(-9000); // CCW 90, (max)
            TheRobot.SendCommand(PanCommand).GetAwaiter();
        }

        private void OnTestHeadPanRight(object sender, RoutedEventArgs e)
        {
            var PanCommand = new HeadPan(9000); // CW 90, max
            TheRobot.SendCommand(PanCommand).GetAwaiter();
        }

        private void OnTestPose(object sender, RoutedEventArgs e)
        {
            var b = sender as Button;
            var param = (eWhichPose)b.CommandParameter;

            var command = new Pose(param);
            TheRobot.SendCommand(command).GetAwaiter();

        }

    }
}
