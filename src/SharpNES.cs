using SharpNES.src.hardware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpNES.src
{
    internal class SharpNES
    {
        public static void Main()
        {
            Console.WriteLine("im alive!");
            ROM rom = new ROM("nestest.nes");

            new NES(ConsoleRegion.NTSC, rom).PowerOn();
            
        }

    }
}
