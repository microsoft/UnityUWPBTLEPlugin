using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityUWPBTLEPlugin
{

    public sealed class BTLEDevice
    {
        public String Name { get; set; }
    }
    public sealed class BTLEPlugin
    {
        public void StartEnumerate()
        {
            Debug.WriteLine("Start Enumerate");
        }

        public void StopEnumerate()
        {
            Debug.WriteLine("Stop Enumerate");
        }

        public IList<BTLEDevice> GetDevices()
        {
            List<BTLEDevice> devices = new List<BTLEDevice>();
            for(int i = 0; i < 5; i++)
            {
                BTLEDevice a = new BTLEDevice();
                a.Name = i.ToString();
                devices.Add(a);
            }

            return devices;
        }
    }
}
