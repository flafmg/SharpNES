using System;

namespace SharpNES.src.hardware.mappers
{
    public class Mapper0 : Mapper
    {
        public Mapper0(byte[][] programRomBanks, byte[][] characterRomBanks) : base(programRomBanks, characterRomBanks)
        {
        }

        public override byte PGRRead(ushort address)
        {
            if(address >= 0x8000 && address <= 0xFFFF)
            {

            }
            return 0;
        }

        public override void PGRWrite(ushort address, byte data)
        {
            if (address >= 0x8000 && address <= 0xFFFF)
            {

            }
        }

        public override byte CHRRead(ushort address)
        {
            if (address >= 0x0000 && address <= 0x1FFF)
            {

            }
            return 0;
        }

        public override void CHRWrite(ushort address, byte data)
        {
            if (address >= 0x0000 && address <= 0x1FFF)
            {

            }
        }
    }
}
