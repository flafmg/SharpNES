using System;
using System.IO;

namespace SharpNES.src.hardware
{
    public class ROM
    {
        public string romName { get; private set; }
        public ushort numProgramRomBanks { get; private set; }
        public ushort numCharacterRomBanks { get; private set; }
        public ushort mapperID { get; private set; }
        public byte mirroring { get; private set; }
        public byte batteryBackedRam { get; private set; }
        public byte hasTrainer { get; private set; }
        public byte region { get; private set; }
        public byte isVsUnisystem { get; private set; }
        public byte isPlayChoice10 { get; private set; }
        public byte inesVersion { get; private set; }

        public Mapper mapper;

        public ROM(string filePath)
        {
            LoadRom(filePath);
        }

        private void LoadRom(string filePath)
        {
            FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            byte[] RomRawData = new byte[fileStream.Length];
            fileStream.Read(RomRawData, 0, (int)fileStream.Length);

            if (RomRawData.Length < 16 || RomRawData[0] != 0x4E || RomRawData[1] != 0x45 || RomRawData[2] != 0x53 || RomRawData[3] != 0x1A)
            {
                throw new Exception("Invalid file format.");
            }

            romName = Path.GetFileName(filePath);
            numProgramRomBanks = RomRawData[4];
            numCharacterRomBanks = RomRawData[5];
            mapperID = (ushort)((RomRawData[6] >> 4) | (RomRawData[7] & 0xF0));
            mirroring = (byte)(RomRawData[6] & 0x01);
            batteryBackedRam = (byte)((RomRawData[6] >> 1) & 0x01);
            hasTrainer = (byte)((RomRawData[6] >> 2) & 0x01);
            region = (byte)(RomRawData[9] & 0x03);
            isVsUnisystem = (byte)((RomRawData[9] >> 4) & 0x01);
            isPlayChoice10 = (byte)((RomRawData[9] >> 5) & 0x01);
            inesVersion = (byte)((RomRawData[10] >> 4) & 0x0F);

            int programRomOffset = 16;
            if (hasTrainer == 1)
            {
                programRomOffset += 512;
            }

            byte[,] programRomBanks = new byte[numProgramRomBanks, 16384];
            for (int i = 0; i < numProgramRomBanks; i++)
            {
                Buffer.BlockCopy(RomRawData, programRomOffset + i * 16384, programRomBanks, i * 16384, 16384);
            }

            byte[,] characterRomBanks = new byte[numCharacterRomBanks, 8192];
            for (int i = 0; i < numCharacterRomBanks; i++)
            {
                Buffer.BlockCopy(RomRawData, programRomOffset + numProgramRomBanks * 16384 + i * 8192, characterRomBanks, i * 8192, 8192);
            }

            switch (mapperID)
            {
                case 0:
                    mapper = new Mapper0(programRomBanks, characterRomBanks);
                    break;
                default:
                    throw new NotSupportedException($"Mapper {mapperID} not supported.");
            }

            PrintRomInfo();
        }


        public byte CPURead(ushort addr)
        {
            return mapper.PGRRead(addr);
        }
        public void CPUWrite(ushort addr, byte data)
        {
            mapper.PGRWrite(addr, data);
        }
        public byte PPURead(ushort addr)
        {
            return mapper.CHRRead(addr);
        }
        public void PPUWrite(ushort addr, byte data)
        {
            mapper.CHRWrite(addr, data);
        }
        public void PrintRomInfo()
        {
            Console.WriteLine($"ROM Name: {romName}");
            Console.WriteLine($"Program ROM Banks: {numProgramRomBanks}");
            Console.WriteLine($"Character ROM Banks: {numCharacterRomBanks}");
            Console.WriteLine($"ROM Mapper: 0x{mapperID:X4}");
            string romRegion = region == 0 ? "NTSC" : "PAL";
            Console.WriteLine($"ROM Region: {romRegion}");
            Console.WriteLine($"Mirroring: {(mirroring == 0 ? "Horizontal" : "Vertical")}");
            Console.WriteLine($"Battery-Backed RAM: {(batteryBackedRam == 1 ? "Yes" : "No")}");
            Console.WriteLine($"Trainer: {(hasTrainer == 1 ? "Yes" : "No")}");
            Console.WriteLine($"VS Unisystem: {(isVsUnisystem == 1 ? "Yes" : "No")}");
            Console.WriteLine($"PlayChoice-10: {(isPlayChoice10 == 1 ? "Yes" : "No")}");
            Console.WriteLine($"iNES Format Version: 0x{inesVersion:X2}");
        }
    }
}
