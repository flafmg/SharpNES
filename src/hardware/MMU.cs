using System;

namespace SharpNES.src.hardware
{
    public class MMU
    {
        private byte[] ram = new byte[2048];
        private ROM rom;

        public MMU(ROM rom)
        {
            this.rom = rom;
        }

        public byte CPURead(ushort address)
        {
            byte data;

            switch (address)
            {
                case ushort addr when addr < 0x2000:
                    return ram[addr % 0x0800];

                case ushort addr when addr >= 0x2000 && addr < 0x4000:
                    return 0;

                case ushort addr when addr >= 0x8000:
                    return rom.programRomBanks[0][((addr - 0x8000) % 0x4000)];

                default:
                    return 0;
            }
        }

        public byte CPURead(int address)
        {
            return CPURead((ushort)address);
        }

        public void CPUWrite(ushort address, byte value)
        {
            switch (address)
            {
                case ushort addr when addr < 0x2000:
                    ram[addr & 0x07FF] = value;
                    break;

                case ushort addr when addr >= 0x2000 && addr <= 0x3FFF:
                    Console.WriteLine($"PPU register write at address {address}, value: {value}");
                    break;

                default:
                    Console.WriteLine("Attempted to write on ROM space");
                    break;
            }
        }

        public void CPUWrite(ushort address, int v)
        {
            CPUWrite(address, (byte)v);
        }

        public void CPUWrite(int address, int v)
        {
            CPUWrite((ushort)address, (byte)v);
        }


        // PPU STUFF

        private byte[] nameTable = new byte[2048];
        private byte[] pallete = new byte[32];


    }
}
