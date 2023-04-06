using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpNES.NES
{
    public class BUS
    {
        PPUBUS ppubus;
        CPU cpu;
        public BUS()
        {
            ppubus = new PPUBUS(this);
            cpu = new CPU(this);
            RAM ram = new RAM(this);
            addComponent(ram, 0, 0x1fff);

            Cartrige c = new Cartrige("a");
            c.load();
        }

        // 0 is readandwrite 1 is readonly
        IComp[,] Components = new IComp[2,0xffff];

        void addComponent(IComp comp, ushort start, ushort end)
        {
            for (int i = start; i <= end; i++)
            {
                if (comp.isReadOnly())
                    Components[1,i] = comp;
                else
                    Components[0,i] = comp;
            }
        }

        public void Write(ushort address, byte value)
        {
            Components[0, address].Write((ushort)(address & Components[0, address].size()), value);
        }

        public byte Read(ushort address)
        {
            if (Components[0, address] == null)
            {
                return Components[1, address].Read((ushort)(address & Components[0, address].size()));
            }
            return Components[0, address].Read((ushort)(address & Components[0, address].size()));
        }
    }
    class PPUBUS
    {
        BUS bus;
        public PPUBUS(BUS bus)
        {
            this.bus = bus;
        }

        IComp[] Components = new IComp[0xffff];

        void addComponent(IComp comp, ushort start, ushort end)
        {
            for (int i = start; i <= end; i++)
            {
                Components[i] = comp;
            }
        }

        public void Write(ushort address, byte value)
        {
            Components[address].Write((ushort)(address & Components[address].size()), value);
        }
        public byte Read(ushort address)
        {
            return Components[address].Read((ushort)(address & Components[address].size()));
        }
    }

}
