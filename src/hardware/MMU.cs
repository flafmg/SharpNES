using System;

namespace SharpNES.src.hardware
{
    internal class MMU
    {
        private readonly byte[] memory = new byte[65536]; // Full 64KB addressable memory

        public byte Read(ushort address)
        {
            return memory[address];
        }
        public byte Read(int address)
        {
            return memory[(ushort)address];
        }

        public void Write(ushort address, byte value)
        {
            memory[address] = value;
        }

        public void Write(ushort address, int v)
        {
            memory[address] = (byte)v;
        }
        public void Write(int address, int v)
        {
            memory[(ushort)address] = (byte)v;
        }
    }
}
