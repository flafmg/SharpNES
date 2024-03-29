using System;

namespace SharpNES.src.hardware
{
    public class MMU
    {
        private byte[] ram = new byte[2048];
        private ROM rom;
        private PPU ppu;

        public MMU(ROM rom)
        {
            this.rom = rom;
  
        }
        public void addPPU(PPU ppu)
        {
            this.ppu = ppu;
        }

        public byte CPURead(ushort address)
        {
            switch (address)
            {
                case ushort addr when addr < 0x2000:
                    return ram[addr % 0x0800];

                case ushort addr when addr >= 0x2000 && addr < 0x4000:
                    //Console.WriteLine("ppu read");
                    return ppu.Read((ushort)(addr % 0x0008));

                case ushort addr when addr >= 0x8000:
                    return rom.CPURead(addr);

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
                    //Console.WriteLine($"ppu write {(addr % 0x0008):x4}");
                    ppu.Write((ushort)(addr % 0x0008), value);
                    break;

                default:
                    rom.CPUWrite(address, value);
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

       

        public byte PPURead(ushort address)
        {
            if(address < 0x2000)
            {
                return rom.PPURead(address);
            }
            else if(address >= 0x2000 && address <= 0x3EFF)
            {
                address -= 0x2000;
                if(rom.mirroring == 1)
                {
                    if(address >= 0x0000 && address <= 0x03ff)
                    {
                        return ppu.nameTable[0,address & 0x03ff];
                    }
                    else if(address >= 0x0400 && address <= 0x07ff)
                    {
                        return ppu.nameTable[1,address & 0x03ff];
                    }
                    else if(address >= 0x0800 && address <= 0x0Bff)
                    {
                        return ppu.nameTable[0,address & 0x03ff];
                    }
                    else if(address >= 0x0C00 && address <= 0x0fff)
                    {
                        return ppu.nameTable[1,address & 0x03ff];
                    }
                }
                else
                {
                    if(address >= 0x0000 && address <= 0x03ff)
                    {
                        return ppu.nameTable[0,address & 0x03ff];
                    }
                    else if(address >= 0x0400 && address <= 0x07ff)
                    {
                        return ppu.nameTable[0,address & 0x03ff];
                    }
                    else if(address >= 0x0800 && address <= 0x0Bff)
                    {
                        return ppu.nameTable[1,address & 0x03ff];
                    }
                    else if(address >= 0x0C00 && address <= 0x0fff)
                    {
                        return ppu.nameTable[1,address & 0x03ff];
                    }
                }
            }
            else if (address >= 0x3F00 && address <= 0x3FFF)
            {
                address &= 0x001F;
                if (address == 0x0010) address = 0x0000;
                if (address == 0x0014) address = 0x0004;
                if (address == 0x0018) address = 0x0008;
                if (address == 0x001C) address = 0x000C;
                return ppu.palette[address];
            }
            return 0;
        }
        public void PPUWrite(ushort address, byte value)
        {
            if (address < 0x2000)
            {
                 rom.PPUWrite(address, value);
            }
            else if (address >= 0x2000 && address <= 0x3EFF)
            {
                              address -= 0x2000;
                if(rom.mirroring == 1)
                {
                    if(address >= 0x0000 && address <= 0x03ff)
                    {
                        ppu.nameTable[0,address & 0x03ff] = value;
                    }
                    else if(address >= 0x0400 && address <= 0x07ff)
                    {
                        ppu.nameTable[1,address & 0x03ff] = value;
                    }
                    else if(address >= 0x0800 && address <= 0x0Bff)
                    {
                        ppu.nameTable[0,address & 0x03ff] = value;
                    }
                    else if(address >= 0x0C00 && address <= 0x0fff)
                    {
                        ppu.nameTable[1,address & 0x03ff] = value;
                    }
                }
                else
                {
                    if(address >= 0x0000 && address <= 0x03ff)
                    {
                        ppu.nameTable[0,address & 0x03ff] = value;
                    }
                    else if(address >= 0x0400 && address <= 0x07ff)
                    {
                        ppu.nameTable[0,address & 0x03ff] = value;
                    }
                    else if(address >= 0x0800 && address <= 0x0Bff)
                    {
                        ppu.nameTable[1,address & 0x03ff] = value;
                    }
                    else if(address >= 0x0C00 && address <= 0x0fff)
                    {
                       ppu.nameTable[1,address & 0x03ff] = value;
                    }
                }
            }
            else if (address >= 0x3F00 && address <= 0x3FFF)
            {
                address &= 0x001F;
                if (address == 0x0010) address = 0x0000;
                if (address == 0x0014) address = 0x0004;
                if (address == 0x0018) address = 0x0008;
                if (address == 0x001C) address = 0x000C;
                ppu.palette[address] = value;
            }
         
        }

        internal byte PPURead(int addr)
        {
            return PPURead((ushort)addr);
        }
    }
}
