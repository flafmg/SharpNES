using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpNES.NES
{
    public class Cartrige
    {
        byte mapperID = 0;
        byte PRGBanks = 0;
        byte CHRBanks = 0;
        public Cartrige(String path)
        {

        }


        struct header
        {
            char[] name = new char[4];
            byte prgChunks;
            byte chrChunks;
            byte mapper1;
            byte mapper2;
            byte prgRamSize;
            byte tvSys1;
            byte tvSys2;

            public header()
            {
            }
        }

    }
}
