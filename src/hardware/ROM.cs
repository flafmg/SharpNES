using SharpNES.src.hardware.mappers;
using System;
using System.IO;

namespace SharpNES.src.hardware
{
    public class ROM
    {
        public string romName { get; private set; }
        public byte[] header { get; private set; }
        public byte[] romRawData { get; private set; }
        public byte[][] programRomBanks { get; private set; }
        public byte[][] characterRomBanks { get; private set; }
        public ushort numProgramRomBanks { get; private set; }
        public ushort numCharacterRomBanks { get; private set; }
        public ushort mapperID { get; private set; }
        public byte mirroring { get; private set; }
        public byte batteryBackedRam { get; private set; }
        public byte trainer { get; private set; }
        public byte region { get; private set; }
        public byte vsUnisystem { get; private set; }
        public byte playChoice10 { get; private set; }
        public byte inesFormatVersion { get; private set; }

        public Mapper mapper;
        public ROM(string filePath)
        {
            LoadRom(filePath);
        }

        private void LoadRom(string filePath)
        {
            FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            romRawData = new byte[fileStream.Length];
            fileStream.Read(romRawData, 0, (int)fileStream.Length);

            if (romRawData.Length < 16 || romRawData[0] != 0x4E || romRawData[1] != 0x45 || romRawData[2] != 0x53 || romRawData[3] != 0x1A)
            {
                throw new Exception("Invalid file format.");
            }

            numProgramRomBanks = romRawData[4];
            numCharacterRomBanks = romRawData[5];
            mapperID = (ushort)((romRawData[6] >> 4) | (romRawData[7] & 0xF0));
            mirroring = (byte)(romRawData[6] & 0x01);
            batteryBackedRam = (byte)((romRawData[6] >> 1) & 0x01);
            trainer = (byte)((romRawData[6] >> 2) & 0x01);
            region = (byte)(romRawData[9] & 0x03);
            vsUnisystem = (byte)((romRawData[9] >> 4) & 0x01);
            playChoice10 = (byte)((romRawData[9] >> 5) & 0x01);
            inesFormatVersion = (byte)((romRawData[10] >> 4) & 0x0F);

            int programRomOffset = 16;
            if (trainer == 1)
            {
                programRomOffset += 512;
            }

            programRomBanks = new byte[numProgramRomBanks][];
            for (int i = 0; i < numProgramRomBanks; i++)
            {
                programRomBanks[i] = new byte[16384];
                Buffer.BlockCopy(romRawData, programRomOffset + i * 16384, programRomBanks[i], 0, 16384);
            }

            characterRomBanks = new byte[numCharacterRomBanks][];
            for (int i = 0; i < numCharacterRomBanks; i++)
            {
                characterRomBanks[i] = new byte[8192];
                Buffer.BlockCopy(romRawData, programRomOffset + numProgramRomBanks * 16384 + i * 8192, characterRomBanks[i], 0, 8192);
            }
            //PrintRomContents();
        }
        public void PrintRomContents()
        {
            for (int i = 0; i < numProgramRomBanks; i++)
            {
                Console.WriteLine($"\n\nPRG BANK: {i}");
                Console.Write("ADDR | ");
                for (int addrTxt = 0; addrTxt < 16; addrTxt++)
                {
                    Console.Write($"{addrTxt:X2} | ");
                }
                for (int b = 0; b < programRomBanks[i].Length; b++)
                {
                    if (b % 0x10 == 0)
                    {
                        Console.Write($"\n{b:X4} | ");
                        Console.Write($"{programRomBanks[i][b]:X2} | ");
                    }
                    else
                    {
                        Console.Write($"{programRomBanks[i][b]:X2} | ");
                    }
                }
            }

            for (int i = 0; i < numCharacterRomBanks; i++)
            {
                Console.WriteLine($"\n\nCHR BANK: {i}");
                Console.Write("ADDR | ");
                for (int addrTxt = 0; addrTxt < 16; addrTxt++)
                {
                    Console.Write($"{addrTxt:X2} | ");
                }
                for (int b = 0; b < characterRomBanks[i].Length; b++)
                {
                    if (b % 0x10 == 0)
                    {
                        Console.Write($"\n{b:X4} | ");
                        Console.Write($"{characterRomBanks[i][b]:X2} | ");
                    }
                    else
                    {
                        Console.Write($"{characterRomBanks[i][b]:X2} | ");
                    }
                }
            }
            Console.WriteLine($"\nProgram ROM Banks: {numProgramRomBanks}");
            Console.WriteLine($"Character ROM Banks: {numCharacterRomBanks}");
            Console.WriteLine($"ROM Mapper: 0x{mapperID:X4}");
            string romrg = region == 0 ? "NTSC" : "PAL";
            Console.WriteLine($"ROM Region: {romrg}");

        }
    }
}
