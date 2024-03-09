using System;
using System.IO;

namespace SharpNES.src.hardware
{
    public class ROM
    {
        public byte[][] ProgramRomBanks { get; private set; }
        public byte[][] CharacterRomBanks { get; private set; }
        public ushort NumProgramRomBanks { get; private set; }
        public ushort NumCharacterRomBanks { get; private set; }
        public ushort Mapper { get; private set; }

        public ROM(string filePath)
        {
            LoadRom(filePath);
            DumpRom();
        }

        private void LoadRom(string filePath)
        {
            FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            byte[] romRawData = new byte[fileStream.Length];
            fileStream.Read(romRawData, 0, (int)fileStream.Length);
            fileStream.Close();

            if (romRawData[0] != 0x4E || romRawData[1] != 0x45 || romRawData[2] != 0x53 || romRawData[3] != 0x1A)
            {
                throw new Exception("Invalid INES file format.");
            }

            NumProgramRomBanks = romRawData[4];
            NumCharacterRomBanks = romRawData[5];
            Mapper = (ushort)((romRawData[6] >> 4) | (romRawData[7] & 0xF0));
            bool hasTrainer = (romRawData[6] & 0x04) == 0x04;
            int programRomOffset = 16;
            if (hasTrainer)
            {
                programRomOffset += 512;
            }

            ProgramRomBanks = new byte[NumProgramRomBanks][];
            for (int i = 0; i < NumProgramRomBanks; i++)
            {
                ProgramRomBanks[i] = new byte[16384];
                Buffer.BlockCopy(romRawData, programRomOffset + i * 16384, ProgramRomBanks[i], 0, 16384);
            }

            CharacterRomBanks = new byte[NumCharacterRomBanks][];
            for (int i = 0; i < NumCharacterRomBanks; i++)
            {
                CharacterRomBanks[i] = new byte[8192];
                Buffer.BlockCopy(romRawData, programRomOffset + NumProgramRomBanks * 16384 + i * 8192, CharacterRomBanks[i], 0, 8192);
            }
        }

        public void DumpRom()
        {
            Console.WriteLine("Program ROM Banks:");
            for (int i = 0; i < NumProgramRomBanks; i++)
            {
                Console.WriteLine($"Bank {i}: {BitConverter.ToString(ProgramRomBanks[i])}");
            }

            Console.WriteLine("\nCharacter ROM Banks:");
            for (int i = 0; i < NumCharacterRomBanks; i++)
            {
                Console.WriteLine($"Bank {i}: {BitConverter.ToString(CharacterRomBanks[i])}");
            }
            Console.WriteLine($"this is using Mapper: {Mapper}, and has {NumProgramRomBanks} program rom banks and {NumCharacterRomBanks} char rom banks");
        }
    }
}
