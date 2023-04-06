using System.Xml.Serialization;

namespace SharpNES.NES
{
    public class CPU
    {
        void Write(int address, int value)
        {
            bus.Write((ushort)address, (byte)value);
        }
        byte Read(int address)
        {
            return bus.Read((ushort)address);
        }
        BUS bus;
        public CPU(BUS bus)
        {
            this.bus = bus;

            Array.Fill(Inst, new INS(256, UNK, IMM));

            Inst[0x00] = new INS(7, BRK, IMP);
            Inst[0x10] = new INS(2, BPL, REL);
            Inst[0x20] = new INS(6, JSR, ABS);
            Inst[0x30] = new INS(2, BMI, REL);
            Inst[0x40] = new INS(6, RTI, IMP);
            Inst[0x50] = new INS(2, BVC, REL);
            Inst[0x60] = new INS(6, RTS, IMP);
            Inst[0x70] = new INS(2, BVS, REL);
            Inst[0x90] = new INS(2, BCC, REL);
            Inst[0xA0] = new INS(2, LDY, IMM);
            Inst[0xB0] = new INS(2, BCS, REL);
            Inst[0xC0] = new INS(2, CPY, IMM);
            Inst[0xD0] = new INS(2, BNE, REL);
            Inst[0xE0] = new INS(2, CPX, IMM);
            Inst[0xF0] = new INS(2, BEQ, REL);

            Inst[0x01] = new INS(6, ORA, IZX);
            Inst[0x11] = new INS(5, ORA, IZY);
            Inst[0x21] = new INS(6, AND, IZX);
            Inst[0x31] = new INS(5, AND, IZY);
            Inst[0x41] = new INS(6, EOR, IZX);
            Inst[0x51] = new INS(5, EOR, IZY);
            Inst[0x61] = new INS(6, ADC, IZX);
            Inst[0x71] = new INS(5, ADC, IZY);
            Inst[0x81] = new INS(6, STA, IZX);
            Inst[0x91] = new INS(6, STA, IZY);
            Inst[0xA1] = new INS(6, LDA, IZX);
            Inst[0xB1] = new INS(5, LDA, IZY);
            Inst[0xC1] = new INS(6, CMP, IZX);
            Inst[0xD1] = new INS(5, CMP, IZY);
            Inst[0xE1] = new INS(6, SBC, IZX);
            Inst[0xF1] = new INS(5, SBC, IZY);

            Inst[0xA2] = new INS(2, LDX, IMM);

            Inst[0x24] = new INS(3, BIT, ZP0);
            Inst[0x84] = new INS(3, STY, ZP0);
            Inst[0x94] = new INS(4, STY, ZPX);
            Inst[0xA4] = new INS(3, LDY, ZP0);
            Inst[0xB4] = new INS(4, LDY, ZPX);
            Inst[0xC4] = new INS(3, CPY, ZP0);
            Inst[0xE4] = new INS(3, CPX, ZP0);

            Inst[0x05] = new INS(3, ORA, ZP0);
            Inst[0x15] = new INS(4, ORA, ZPX);
            Inst[0x25] = new INS(3, AND, ZP0);
            Inst[0x35] = new INS(4, AND, ZPX);
            Inst[0x45] = new INS(3, EOR, ZP0);
            Inst[0x55] = new INS(4, EOR, ZPX);
            Inst[0x65] = new INS(3, ADC, ZP0);
            Inst[0x75] = new INS(4, ADC, ZPX);
            Inst[0x85] = new INS(3, STA, ZP0);
            Inst[0x95] = new INS(4, STA, ZPX);
            Inst[0xA5] = new INS(3, LDA, ZP0);
            Inst[0xB5] = new INS(4, LDA, ZPX);
            Inst[0xC5] = new INS(3, CMP, ZP0);
            Inst[0xD5] = new INS(4, CMP, ZPX);
            Inst[0xE5] = new INS(3, SBC, ZP0);
            Inst[0xF5] = new INS(4, SBC, ZPX);

            Inst[0x06] = new INS(5, ASL, ZP0);
            Inst[0x16] = new INS(6, ASL, ZPX);
            Inst[0x26] = new INS(5, ROL, ZP0);
            Inst[0x36] = new INS(6, ROL, ZPX);
            Inst[0x46] = new INS(5, LSR, ZP0);
            Inst[0x56] = new INS(6, LSR, ZPX);
            Inst[0x66] = new INS(5, ROR, ZP0);
            Inst[0x76] = new INS(6, ROR, ZPX);
            Inst[0x86] = new INS(3, STX, ZP0);
            Inst[0x96] = new INS(4, STX, ZPX);
            Inst[0xA6] = new INS(3, LDX, ZP0);
            Inst[0xB6] = new INS(4, LDX, ZPX);
            Inst[0xC6] = new INS(5, DEC, ZP0);
            Inst[0xD6] = new INS(6, DEC, ZPX);
            Inst[0xE6] = new INS(5, INC, ZP0);
            Inst[0xF6] = new INS(6, INC, ZPX);

            Inst[0x08] = new INS(3, PHP, IMP);
            Inst[0x18] = new INS(2, CLC, IMP);
            Inst[0x28] = new INS(4, PLP, IMP);
            Inst[0x38] = new INS(2, SEC, IMP);
            Inst[0x48] = new INS(3, PHA, IMP);
            Inst[0x58] = new INS(2, CLI, IMP);
            Inst[0x68] = new INS(4, PLA, IMP);
            Inst[0x78] = new INS(2, SEI, IMP);
            Inst[0x88] = new INS(2, DEY, IMP);
            Inst[0x98] = new INS(2, TYA, IMP);
            Inst[0xA8] = new INS(2, TAY, IMP);
            Inst[0xB8] = new INS(2, CLV, IMP);
            Inst[0xC8] = new INS(2, INY, IMP);
            Inst[0xD8] = new INS(2, CLD, IMP);
            Inst[0xE8] = new INS(2, INX, IMP);
            Inst[0xF8] = new INS(2, SED, IMP);

            Inst[0x09] = new INS(2, ORA, IMM);
            Inst[0x19] = new INS(4, ORA, ABY);
            Inst[0x29] = new INS(2, AND, IMM);
            Inst[0x39] = new INS(4, AND, ABY);
            Inst[0x49] = new INS(2, EOR, IMM);
            Inst[0x59] = new INS(4, EOR, ABY);
            Inst[0x69] = new INS(2, ADC, IMM);
            Inst[0x79] = new INS(4, ADC, ABY);
            Inst[0x99] = new INS(5, STA, ABY);
            Inst[0xA9] = new INS(2, LDA, IMM);
            Inst[0xB9] = new INS(4, LDA, ABY);
            Inst[0xC9] = new INS(2, CMP, IMM);
            Inst[0xD9] = new INS(4, CMP, ABY);
            Inst[0xE9] = new INS(2, SBC, IMM);
            Inst[0xF9] = new INS(4, SBC, ABY);

            Inst[0x0A] = new INS(2, ASL, IMP);
            Inst[0x2A] = new INS(2, ROL, IMP);
            Inst[0x4A] = new INS(2, LSR, IMP);
            Inst[0x6A] = new INS(2, ROR, IMP);
            Inst[0x88] = new INS(2, TXA, IMP);
            Inst[0x9A] = new INS(2, TXS, IMP);
            Inst[0xAA] = new INS(2, TAX, IMP);
            Inst[0xBA] = new INS(2, TSX, IMP);
            Inst[0xCA] = new INS(2, DEX, IMP);
            Inst[0xEA] = new INS(2, NOP, IMP);

            Inst[0x2C] = new INS(4, BIT, ABS);
            Inst[0x4C] = new INS(3, JMP, ABS);
            Inst[0x6C] = new INS(5, JMP, IND);
            Inst[0x8C] = new INS(4, STY, ABS);
            Inst[0xAC] = new INS(4, LDY, ABS);
            Inst[0xBC] = new INS(4, LDY, ABX);
            Inst[0xCC] = new INS(4, CPY, ABS);
            Inst[0xEC] = new INS(4, CPX, ABS);

            Inst[0x0E] = new INS(6, ASL, ABS);
            Inst[0x1E] = new INS(7, ASL, ABX);
            Inst[0x2E] = new INS(6, ROL, ABS);
            Inst[0x3E] = new INS(7, ROL, ABX);
            Inst[0x4E] = new INS(6, LSR, ABS);
            Inst[0x5E] = new INS(7, LSR, ABX);
            Inst[0x6E] = new INS(6, ROR, ABS);
            Inst[0x7E] = new INS(7, ROR, ABX);
            Inst[0x8E] = new INS(4, STX, ABS);
            Inst[0xAE] = new INS(4, LDX, ABS);
            Inst[0xBE] = new INS(4, LDX, ABX);
            Inst[0xCE] = new INS(6, DEC, ABS);
            Inst[0xDE] = new INS(7, DEC, ABX);
            Inst[0xEE] = new INS(6, INC, ABS);
            Inst[0xFE] = new INS(7, INC, ABX);
        }
        Action currentInst, currentAddrMode;
        INS[] Inst = new INS[256];
        UInt16 PC, opcode;
        byte SP, A, X, Y;
        bool C, Z, I, D, B, V, N;

        int WC; //WaitCycles


        public void Clock()
        {
            if (WC != 0)
            {
                WC--;
                return;
            }
            execute();
        }
        public void Reset()
        {
            A = 0; X = 0; Y = 0;
            SP = 0xFD;
            C = Z = I = D = B = V = N = false;

            absoluteAddr = 0xFFFC;
            PC = (ushort)((bus.Read((ushort)(absoluteAddr + 1)) << 8) | bus.Read(absoluteAddr));

            absoluteAddr = 0x0000;
            relativeAddr = 0x0000;
            data = 0x00;

            WC = 8;
        }
        public void IRQ()
        {
            if (!I)
            {
                bus.Write((ushort)(0x0100 + SP), (byte)(PC & 0xFF00));
                SP--;
                bus.Write((ushort)(0x0100 + SP), (byte)(PC & 0x00FF));
                SP--;

                I = true; B = false;
                byte status = 0;
                status |= (byte)(C ? 0x01 : 0x00);
                status |= (byte)(Z ? 0x02 : 0x00);
                status |= (byte)(I ? 0x04 : 0x00);
                status |= (byte)(D ? 0x08 : 0x00);
                status |= (byte)(B ? 0x10 : 0x00);
                status |= (byte)(V ? 0x40 : 0x00);
                status |= (byte)(N ? 0x80 : 0x00);
                Write(0x100 + SP, status);

                absoluteAddr = 0xFFFE;
                PC = (ushort)((Read(absoluteAddr + 1) << 8) | Read(absoluteAddr));

                WC = 7;
            }
        }
        public void NMI()
        {
            Write(0x0100 + SP, PC & 0xFF00);
            SP--;
            bus.Write((ushort)(0x0100 + SP), (byte)(PC & 0x00FF));
            SP--;

            I = true; B = false;
            byte status = 0;
            status |= (byte)(C ? 0x01 : 0x00);
            status |= (byte)(Z ? 0x02 : 0x00);
            status |= (byte)(I ? 0x04 : 0x00);
            status |= (byte)(D ? 0x08 : 0x00);
            status |= (byte)(B ? 0x10 : 0x00);
            status |= (byte)(V ? 0x40 : 0x00);
            status |= (byte)(N ? 0x80 : 0x00);
            Write(0x100 + SP, status);

            absoluteAddr = 0xFFFA;
            PC = (ushort)(Read((absoluteAddr + 1) << 8) | Read(absoluteAddr));

            WC = 8;
        }

        void execute()
        {
            ++PC;
            opcode = bus.Read(PC);

            WC = Inst[opcode].cycles;

            Inst[opcode].addrMode();
            Inst[opcode].operation();
        }
        void getData()
        {
            if (Inst[opcode].addrMode != IMP)
            {
                data = Read(absoluteAddr);
            }
        }

        //addr mode 

        byte data;
        UInt16 absoluteAddr, relativeAddr;

        void IMP()
        {
            data = A;
        }
        void IMM()
        {
            absoluteAddr = PC++;
        }
        void ZP0()
        {
            absoluteAddr = Read(PC);
            ++PC;
            absoluteAddr &= 0x00FF;
        }
        void ZPX()
        {
            absoluteAddr = (UInt16)(Read(PC) + X);
            ++PC;
            absoluteAddr &= 0x00FF;
        }
        void ZPY()
        {
            absoluteAddr = (UInt16)(Read(PC) + Y);
            ++PC;
            absoluteAddr &= 0x00FF;
        }
        void ABS()
        {
            UInt16 lowByte = Read(PC);
            ++PC;
            UInt16 hightByte = Read(PC);
            ++PC;

            absoluteAddr = (UInt16)((hightByte << 8) | lowByte);
        }
        void ABX()
        {
            UInt16 lowByte = Read(PC);
            ++PC;
            UInt16 hightByte = Read(PC);
            ++PC;

            absoluteAddr = (UInt16)((hightByte << 8) | lowByte);
            absoluteAddr += X;

            if ((absoluteAddr & 0xFF00) != hightByte << 8) //check if crossed one page
            {
                ++WC;
            }
        }
        void ABY()
        {
            UInt16 lowByte = Read(PC);
            ++PC;
            UInt16 hightByte = Read(PC);
            ++PC;

            absoluteAddr = (UInt16)((hightByte << 8) | lowByte);
            absoluteAddr += Y;

            if ((absoluteAddr & 0xFF00) != hightByte << 8) //check if crossed one page
            {
                ++WC;
            }
        }
        void IND()
        {
            UInt16 lowByte = Read(PC);
            ++PC;
            UInt16 hightByte = Read(PC);
            ++PC;

            UInt16 pointer = (UInt16)((hightByte << 8) | lowByte);

            if (lowByte == 0x00ff) // some bug in real hardware
            {
                absoluteAddr = (UInt16)(Read((pointer & 0xFF00) << 8) | bus.Read(pointer));
            }
            else
            {
                absoluteAddr = (UInt16)(Read((pointer + 1) << 8) | bus.Read(pointer));
            }

        }
        void IZX()
        {
            UInt16 t = bus.Read(PC);
            ++PC;
            UInt16 lowByte = Read((t + X) & 0x00FF);
            UInt16 highByte = Read((t + X + 1) & 0x00FF);

            absoluteAddr = (UInt16)((highByte << 8) | lowByte);
        }
        void IZY()
        {
            UInt16 t = Read(PC);
            ++PC;
            UInt16 lowByte = Read(t & 0x00FF);
            UInt16 highByte = Read((t + 1) & 0x00FF);

            absoluteAddr = (UInt16)((highByte << 8) | lowByte);
            absoluteAddr += Y;

            if ((absoluteAddr & 0xFF00) != highByte << 8)
            {
                ++WC;
            }
        }
        void REL()
        {
            relativeAddr = bus.Read(PC);
            ++PC;
            if ((relativeAddr & 0x80) == 1)
            {
                relativeAddr |= 0xFF00;
            }
        }



        //instructions

        struct INS
        {
            public int cycles;
            public Action operation;
            public Action addrMode;

            public INS(int cycles, Action operation, Action addrMode) : this()
            {
                this.cycles = cycles;
                this.operation = operation;
                this.addrMode = addrMode;
            }
        }


        // .------------------------------------------------.
        // |             see for more details               |
        // | www.masswerk.at/6502/6502_instruction_set.html |
        // `------------------------------------------------´

        void UNK() //error handling
        {
            Console.WriteLine("Unrecognized or illegal operation, skipping");
        }

        //Transfer Instructions

        void LDA() // load acumulator
        {
            getData();
            A = data;
            Z = A == 0x00;
            N = (A & 0x80) == 1;
        }
        void LDX() // load X
        {
            getData();
            X = data;
            Z = X == 0x00;
            N = (X & 0x80) == 1;
        }
        void LDY() // load Y
        {
            getData();
            Y = data;
            Z = Y == 0x00;
            N = (Y & 0x80) == 1;
        }
        void STA() // store acumulator
        {
            Write(absoluteAddr, A);
        }
        void STX() // store X
        {
            Write(absoluteAddr, X);
        }
        void STY() // store Y
        {
            Write(absoluteAddr, Y);
        }
        void TAX() // transfer acumulator to X
        {
            X = A;
            Z = X == 0x00;
            N = (X & 0x80) == 1;
        }
        void TAY() // transfer acumulator to Y
        {
            Y = A;
            Z = Y == 0x00;
            N = (Y & 0x80) == 1;
        }
        void TSX() // transfer stack pointer to X
        {
            X = SP;
            Z = X == 0x00;
            N = (X & 0x80) == 1;
        }
        void TXA() // transfer X to acomulator
        {
            A = X;
            Z = A == 0x00;
            N = (A & 0x80) == 1;
        }
        void TXS() // transfer X to stack pointer
        {
            SP = X;
        }
        void TYA() // transfer Y to acomulator
        {
            A = Y;
            Z = A == 0x00;
            N = (A & 0x80) == 1;
        }

        //Stack Instructions

        void PHA() // push acumulator
        {
            Write(0x0100 + SP, A);
            SP--;
        }
        void PHP() // push processor status register (with break flag set)
        {
            B = true;  
            byte status = 0;
            status |= (byte)(C ? 0x01 : 0x00);
            status |= (byte)(Z ? 0x02 : 0x00);
            status |= (byte)(I ? 0x04 : 0x00);
            status |= (byte)(D ? 0x08 : 0x00);
            status |= (byte)(B ? 0x10 : 0x00);
            status |= (byte)(V ? 0x40 : 0x00);
            status |= (byte)(N ? 0x80 : 0x00);
            Write(0x100 + SP, status);
            B = false;
            SP--;
        }
        void PLA() // pull acumulator
        {
            SP++;
            A = Read(0x0100 + SP);
            Z = A == 0x00;
            N = (A & 0x80) == 1;
        }
        void PLP() // pull processor status register
        {
            SP++;
            byte status = Read(0x0100 + SP);
            C = (status & 0x01) == 1;
            Z = (status & 0x02) == 1;
            I = (status & 0x04) == 1;
            D = (status & 0x08) == 1;
            B = (status & 0x10) == 1;
            V = (status & 0x40) == 1;
            N = (status & 0x80) == 1;

        }

        //Decrements & Increments

        void DEC()
        {
            getData();
            ushort value = (ushort)(data - 1);
            Write(absoluteAddr, value & 0x00FF);
            Z = (value & 0x00FF) == 0x00;
            N = (value & 0x0080) == 1; 
        }
        void DEX()
        {
            X--;
            Z = X == 0x00;
            N = (X & 0x80) == 1;
        }
        void DEY()
        {
            Y--;
            Z = Y == 0x00;
            N = (Y & 0x80) == 1;
        }
        void INC()
        {
            getData();
            ushort value = (ushort)(data + 1);
            Write(absoluteAddr, value & 0x00FF);
            Z = (value & 0x00FF) == 0x00;
            N = (value & 0x0080) == 1;
        }
        void INX()
        {
            X++;
            Z = X == 0x00;
            N = (X & 0x80) == 1;
        }
        void INY()
        {
            Y++;
            Z = Y == 0x00;
            N = (Y & 0x80) == 1;
        }

        //Arithmetic Operations

        void ADC()
        {
            getData();
            UInt16 value = (UInt16)(A + data + (C ? 1 : 0));
            C = value > 255;
            Z = (value & 0x00FF) == 0;
            N = (value & 0x80) == 1;
            V = ((A ^ data) & 0x80) == 0 && ((A ^ value) & 0x80) != 0;
            A = (byte)(value & 0x00FF);
        }
        void SBC()
        {
            getData();
            UInt16 v = (UInt16)(data ^ 0x00FF);
            UInt16 value = (UInt16)(A + v + (C ? 1 : 0));
            C = value > 255;
            Z = (value & 0x00FF) == 0;
            N = (value & 0x80) == 1;
            V = (value ^ (ushort)A) == 0 && ((value ^ v) & 0x0080) != 0;
            A = (byte)(value & 0x00FF);
        }

        //Logical Operations

        void AND()
        {
            getData();
            A = (byte)(A & data);
            Z = A == 0x00;
            N = (A & 0x80) == 1;
        }
        void EOR()
        {
            getData();
            A = (byte)(A ^ data);
            Z = A == 0x00;
            N = (A & 0x80) == 1;
        }
        void ORA()
        {
            getData();
            A = (byte)(A | data);
            Z = A == 0x00;
            N = (A & 0x80) == 1;
        }

        //Shift & Rotate Instructions

        void ASL()
        {
            getData();
            ushort value = (ushort)(data << 1);
            C = (value & 0xFF00) > 0;
            Z = value == 0x00;
            N = (value & 0x80) == 1;
            if (currentAddrMode == IMP)
            {
                A = (byte)(value & 0x00FF);
                return;
            }
            Write(absoluteAddr, value & 0x00FF);

        }
        void LSR()
        {
            getData();
            C = (data & 0x0001) == 1;
            ushort value = (ushort)(data >> 1);
            Z = (value & 0x00FF) == 0x00;
            N = (value & 0x0080) == 1;
            if (currentAddrMode == IMP)
            {
                A = (byte)(value & 0x00FF);
                return;
            }
            Write(absoluteAddr, value & 0x00FF);
        }
        void ROL()
        {
            getData();
            ushort value = (ushort)((data << 1) | (C ? 1 : 0));
            C = (value & 0xFF00) > 0;
            Z = (value & 0x00FF) == 0x00;
            N = (value & 0x0080) == 1;
            if (currentAddrMode == IMP)
            {
                A = (byte)(value & 0x00FF);
                return;
            }
            Write(absoluteAddr, value & 0x00FF);
        }
        void ROR()
        {
            getData();
            ushort value = (ushort)(((C?1:0) >> 7) | (data >> 1));
            C = (value & 0x01) > 0;
            Z = value == 0x00;
            N = (value & 0x0080) == 1;
            if (currentAddrMode == IMP)
            {
                A = (byte)(value & 0x00FF);
                return;
            }
            Write(absoluteAddr, value & 0x00FF);
        }

        //Flag Instructions

        void CLC()
        {
            C = false;
        }
        void CLD()
        {
            D = false;
        }
        void CLI()
        {
            I = false;
        }
        void CLV()
        {
            V = false;
        }
        void SEC()
        {
            C = true;
        }
        void SED()
        {
            D = true;
        }
        void SEI()
        {
            I = true;
        }

        //Comparisons

        void CMP()
        {
            getData();
            ushort value = (ushort)(A - data);
            C = A >= data;
            Z = (value & 0x00FF) == 0x00;
            N = (value & 0x0080) == 1;
        }
        void CPX()
        {
            getData();
            ushort value = (ushort)(X - data);
            C = X >= data;
            Z = (value & 0x00FF) == 0x00;
            N = (value & 0x0080) == 1;
        }
        void CPY()
        {
            getData();
            ushort value = (ushort)(Y - data);
            C = Y >= data;
            Z = (value & 0x00FF) == 0x00;
            N = (value & 0x0080) == 1;
        }

        //Conditional Branch Instructions

        void BCC()
        {
            if (!C)
            {
                WC++;
                absoluteAddr = (ushort)(PC + relativeAddr);
                if ((absoluteAddr & 0xFF00) != (PC & 0xFF00))
                {
                    WC++;
                }
                PC = absoluteAddr;
            }
        }
        void BCS()
        {
            if (C)
            {
                WC++;
                absoluteAddr = (ushort)(PC + relativeAddr);
                if ((absoluteAddr & 0xFF00) != (PC & 0xFF00))
                {
                    WC++;
                }
                PC = absoluteAddr;
            }
        }
        void BEQ()
        {
            if (Z)
            {
                WC++;
                absoluteAddr = (ushort)(PC + relativeAddr);
                if ((absoluteAddr & 0xFF00) != (PC & 0xFF00))
                {
                    WC++;
                }
                PC = absoluteAddr;
            }
        }
        void BMI()
        {
            if (N)
            {
                WC++;
                absoluteAddr = (ushort)(PC + relativeAddr);
                if ((absoluteAddr & 0xFF00) != (PC & 0xFF00))
                {
                    WC++;
                }
                PC = absoluteAddr;
            }
        }
        void BNE()
        {
            if (!Z)
            {
                WC++;
                absoluteAddr = (ushort)(PC + relativeAddr);
                if ((absoluteAddr & 0xFF00) != (PC & 0xFF00))
                {
                    WC++;
                }
                PC = absoluteAddr;
            }
        }
        void BPL()
        {
            if (!N)
            {
                WC++;
                absoluteAddr = (ushort)(PC + relativeAddr);
                if ((absoluteAddr & 0xFF00) != (PC & 0xFF00))
                {
                    WC++;
                }
                PC = absoluteAddr;
            }
        }

        void BVC()
        {
            if (!V)
            {
                WC++;
                absoluteAddr = (ushort)(PC + relativeAddr);
                if ((absoluteAddr & 0xFF00) != (PC & 0xFF00))
                {
                    WC++;
                }
                PC = absoluteAddr;
            }
        }
        void BVS()
        {
            if (V)
            {
                WC++;
                absoluteAddr = (ushort)(PC + relativeAddr);
                if ((absoluteAddr & 0xFF00) != (PC & 0xFF00))
                {
                    WC++;
                }
                PC = absoluteAddr;
            }
        }

        //Jumps & Subroutines

        void JMP()
        {
            PC = absoluteAddr;
        }
        void JSR()
        {
            PC--;
            Write(0x100 + SP, PC & 0xFF00); SP--;
            Write(0x100 + SP, PC & 0x00FF); SP--;

            PC = absoluteAddr;
        }
        void RTS()
        {
            SP++; PC = Read(0x100 + SP);
            SP++; PC |= (ushort)(Read(0x100 + SP) << 8);

            PC++;
        }

        //Interrupts

        void BRK()
        {
            PC++;

            I = true;
            Write(0x0100 + SP, PC & 0xFF00); SP--;
            Write(0x0100 + SP, PC & 0x00FF); SP--;

            B = true;
            byte status = 0;
            status |= (byte)(C ? 0x01 : 0x00);
            status |= (byte)(Z ? 0x02 : 0x00);
            status |= (byte)(I ? 0x04 : 0x00);
            status |= (byte)(D ? 0x08 : 0x00);
            status |= (byte)(B ? 0x10 : 0x00);
            status |= (byte)(V ? 0x40 : 0x00);
            status |= (byte)(N ? 0x80 : 0x00);
            Write(0x0100 + SP, status); SP--;
            B = false;

            PC = (ushort)(Read(0xFFFE) | Read(0xFFFF) << 8);

        }
        void RTI()
        {
            SP++;
            byte status = Read(0x0100 + SP);

            C = (status & 0x01) == 1;
            Z = (status & 0x02) == 1;
            I = (status & 0x04) == 1;
            D = (status & 0x08) == 1;
            B = false;
            V = (status & 0x40) == 1;
            N = (status & 0x80) == 1;

            SP++;
            PC = Read(0x100 + SP);
            SP++;
            PC |= (ushort)(Read(0x10 + SP) << 8);
        }

        //Other

        void BIT()
        {
            getData();
            ushort value = (ushort)(A & data);
            Z = (value & 0x00FF) == 0x00;
            N = (value & 0x0080) == 1;
            V = (value & (1 << 6)) == 1;

        }
        void NOP()
        {
            //=/ nothing here, sorry
        }

        void JAM() // KILL CPU
        {

        }

    }
}
