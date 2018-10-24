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

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace TestApp.Sample
{
    // A sample of device specific UX content
    public sealed partial class SampleUX : UserControl
    {
        private SampleDevice theDevice;
        public SampleUX(SampleDevice device)
        {
            this.InitializeComponent();
            theDevice = device;
        }

        private void OnClick(object sender, RoutedEventArgs e)
        {
            TestApp.Sample.Command.CommandIds whatCommand = TestApp.Sample.Command.CommandIds.MoveLeft;
            byte CommandLength = 14; // example value

            Command newCommand = new Command(whatCommand, CommandLength);

            theDevice.Send(newCommand).GetAwaiter();

            _Feedback.Text = "Clicked";
        }
    }
}
