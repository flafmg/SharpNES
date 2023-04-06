using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpNES.NES
{
    public class Cartrige
    {
        public Cartrige(String path)
        {

        }

        byte[][] prgChunks;
        byte[][] chrChunks;
        ushort numPrgBanks;
        ushort numChrBanks;
        ushort mapper;

        public void load()
        {
            FileStream fileStream = new FileStream("C:\\po.nes", FileMode.Open, FileAccess.Read);
            byte[] romRawData = new byte[fileStream.Length];
            fileStream.Read(romRawData, 0, (int)fileStream.Length);
            fileStream.Close();

            if (romRawData[0] == 0x4E && romRawData[1] == 0x45 && romRawData[2] == 0x53 && romRawData[3] == 0x1A)
            {
                Console.WriteLine("valid INES file: NES ");
            }

            numPrgBanks = romRawData[4];
            numChrBanks = romRawData[5];
            mapper = (ushort)((romRawData[6] >> 4) | (romRawData[7] & 0xF0));
            bool hasTrainer = (romRawData[6] & 0x04) == 0x04;
            int prgOffset = 16;
            if (hasTrainer)
            {
                prgOffset += 512;
            }


            Console.WriteLine($"  number of PRG-ROM banks: {numPrgBanks}");
            Console.WriteLine($"  number of CHR-ROM banks: {numChrBanks}");


            Console.WriteLine("$ mapper: " + mapper);

            byte[][] prgBanks = new byte[numPrgBanks][];
            for (int i = 0; i < numPrgBanks; i++)
            {
                prgBanks[i] = new byte[16384];
                Buffer.BlockCopy(romRawData, prgOffset + i * 16384, prgBanks[i], 0, 16384);
                Console.WriteLine($"\nDados do PRG-ROM Bank {i}: {BitConverter.ToString(prgBanks[i])}");
            }
            Console.WriteLine("\nCHR-ROM\n");
            byte[][] chrBanks = new byte[numChrBanks][];
            for (int i = 0; i < numChrBanks; i++)
            {
                chrBanks[i] = new byte[8192];
                Buffer.BlockCopy(romRawData, prgOffset + numPrgBanks * 16384 + i * 8192, chrBanks[i], 0, 8192);
                Console.WriteLine($"\nDados do CHR-ROM Bank {i}: {BitConverter.ToString(chrBanks[i])}");
            }



        }
    }
}