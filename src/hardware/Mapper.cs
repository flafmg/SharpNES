using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpNES.src.hardware
{
    public abstract class Mapper
    {
        protected byte[,] programRomBanks;
        protected byte[,] characterRomBanks;

        public Mapper(byte[,] programRomBanks, byte[,] characterRomBanks)
        {
            this.programRomBanks = programRomBanks;
            this.characterRomBanks = characterRomBanks;
        }

        public abstract byte PGRRead(ushort address);
        public abstract void PGRWrite(ushort address, byte data);
        public abstract byte CHRRead(ushort address);
        public abstract void CHRWrite(ushort address, byte data);
    }
    public class Mapper0 : Mapper
    {
        public Mapper0(byte[,] programRomBanks, byte[,] characterRomBanks) : base(programRomBanks, characterRomBanks)
        {
        }

        public override byte PGRRead(ushort address)
        {
            if (address >= 0x8000 && address <= 0xFFFF)
            {
                int bankIndex = (programRomBanks.GetLength(0) > 1 && address >= 0xC000) ? 1 : 0;

                int wrapedAdress = (address - 0x8000) % (1024*16);
                //Console.WriteLine($"reading bank {bankIndex} at value {wrapedAdress:x4} ({address:x4}) ");
                return programRomBanks[bankIndex, wrapedAdress];
            }

            Console.WriteLine(nameof(address), "Address out of range for PGRMem.");
            return 0;
        }

        public override void PGRWrite(ushort address, byte data)
        {

            Console.WriteLine(nameof(address), "Tring to write on ReadOnly space.");

        }

        public override byte CHRRead(ushort address)
        {
            if (address >= 0x0000 && address <= 0x1FFF)
            {
                return characterRomBanks[0, address];
            }
            Console.WriteLine(nameof(address), "Address out of range for CHRMem.");
            return 0;
        }

        public override void CHRWrite(ushort address, byte data)
        {

            Console.WriteLine(nameof(address), "Tring to write on ReadOnly space.");
        }
    }
}
