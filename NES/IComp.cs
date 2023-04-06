using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpNES.NES
{
    public interface IComp
    {
        public ushort size();
        public bool isReadOnly();
        public void Write(ushort address, byte value);
        public byte Read(ushort address);
    }
}
