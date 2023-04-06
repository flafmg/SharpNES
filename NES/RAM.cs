using System;

namespace SharpNES.NES
{
    public class RAM : IComp
    {
        BUS bus;
        public RAM(BUS bus)
        {
            this.bus = bus;
        }

        byte[] Memory = new byte[64 * 1024];

        public ushort size()
        {
            return 0x07FF;
        }
        public bool isReadOnly()
        {
            return false;
        }
        public void Write(ushort address, byte value)
        {
            Console.WriteLine("Adress:"+ address + " value: "+ value);
            Memory[address] = value;
        }
        public byte Read(ushort address)
        {
            return Memory[address];
        }
    }
}
