using System;

namespace SharpNES.src.hardware
{
    public class MMU
    {
        private readonly byte[] ram = new byte[2048]; 
        private ROM rom;

        public MMU(ROM rom)
        {
            this.rom = rom;
        }

        public byte Read(ushort address)
        {
            //Console.WriteLine($"reading 0x{address:X4}");

            if (address < 0x2000)
            {
                CPU.debug += ($"{(ram[address % 0x0800]):X2} ");
                return ram[address % 0x0800];
              
            }
            else if (address >= 0x2000 && address <= 0x3FFF)
            {
                Console.WriteLine($"PPU register read at address {address}");
                return 0;
            }
            else if(address >= 0x8000)
            {
                CPU.debug += ($"{(rom.ProgramRomBanks[0][(address - 0x8000) % 0x4000]):X2} ");
                return rom.ProgramRomBanks[0][(address - 0x8000) % 0x4000];
            }

            return 0;
        }
        public byte Read(int address)
        {
            return Read((ushort)address);
        }

        public void Write(ushort address, byte value)
        {
            if (address < 0x2000)
            {
                ram[address & 0x07FF] = value;
            }
            else if (address >= 0x2000 && address <= 0x3FFF)
            {
                Console.WriteLine($"PPU register write at address {address}, value: {value}");
            }
            else 
            {
                Console.WriteLine("attempedt to write on Rom space");
            }
        }

        public void Write(ushort address, int v)
        {
            Write(address, (byte)v);
        }

        public void Write(int address, int v)
        {
            Write((ushort)address, (byte)v);
        }
    }
}
