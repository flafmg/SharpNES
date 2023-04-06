using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpNES.NES
{
    public class BUS
    {
        PPUBUS ppubus = new PPUBUS();
        CPU cpu;
        public BUS()
        {
            cpu = new CPU(this);
            RAM ram = new RAM(this);
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

    }
    
}
