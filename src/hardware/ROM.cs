using System;
using System.IO;

namespace SharpNES.src.hardware
{
    public class ROM
    {
        public byte[] romRawData { get; private set; }
        public byte[][] ProgramRomBanks { get; private set; }
        public byte[][] CharacterRomBanks { get; private set; }
        public ushort NumProgramRomBanks { get; private set; }
        public ushort NumCharacterRomBanks { get; private set; }
        public ushort Mapper { get; private set; }

        public ROM(string filePath)
        {
            LoadRom(filePath);
        }

        private void LoadRom(string filePath)
        {
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                // Lendo todo o conteúdo do arquivo diretamente em romRawData
                romRawData = new byte[fileStream.Length];
                fileStream.Read(romRawData, 0, (int)fileStream.Length);
            }

            Console.WriteLine("rom raw data size: ");

            // Verificando o formato do arquivo
            if (romRawData.Length < 16 || romRawData[0] != 0x4E || romRawData[1] != 0x45 || romRawData[2] != 0x53 || romRawData[3] != 0x1A)
            {
                throw new Exception("Invalid file format.");
            }

            // Obtendo informações do cabeçalho
            NumProgramRomBanks = romRawData[4];
            NumCharacterRomBanks = romRawData[5];
            Mapper = (ushort)((romRawData[6] >> 4) | (romRawData[7] & 0xF0));
            bool hasTrainer = (romRawData[6] & 0x04) == 0x04;
            int programRomOffset = 16;
            if (hasTrainer)
            {
                programRomOffset += 512;
            }

            // Inicializando os bancos de ROM
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
            //DumpRom();
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
        }
    }
}
