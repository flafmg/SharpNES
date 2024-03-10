using System;
using System.Collections.Generic;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Runtime.Intrinsics.X86;

namespace SharpNES.src.hardware
{
    internal class CPU
    {
        private MMU mmu;
        private ROM rom;

        private byte A;
        private byte X;
        private byte Y;
        private byte SP;
        private ushort PC;

        private ushort opcode;

        private bool CarryFlag;
        private bool ZeroFlag;
        private bool InterruptDisableFlag;
        private bool DecimalModeFlag;
        private bool OverflowFlag;
        private bool NegativeFlag;
        private bool BreakFlag;

        private byte FetchedData;
        private ushort AbsoluteAddress;
        private ushort RelativeAddress;

        private uint Clocks; //clocks until next instruction

        private Instruction[] ILT;

        public CPU(MMU mmu, ROM rom)
        {
            this.mmu = mmu;
            this.rom = rom;

            A = 0x0;
            X = 0x0;
            Y = 0x0;
            SP = 0xFD;
            PC = 0x0;

            CarryFlag = false;
            ZeroFlag = false;
            InterruptDisableFlag = false;
            DecimalModeFlag = false;
            OverflowFlag = false;
            NegativeFlag = false;
            BreakFlag = false;

            FetchedData = 0x0;
            AbsoluteAddress = 0x0;
            RelativeAddress = 0x0;

            initializeILT();
        }


        private void initializeILT()
        {
            ILT = new Instruction[256];
            for (int i = 0; i < 256; i++)
            {
                ILT[i] = new Instruction(() => { }, () => { }, 0);
            }

            ILT[0x00] = new Instruction(BRK, IMP, 7);
            ILT[0x10] = new Instruction(BPL, REL, 2);
            ILT[0x20] = new Instruction(JSR, ABS, 6);
            ILT[0x30] = new Instruction(BMI, REL, 2);
            ILT[0x40] = new Instruction(RTI, IMP, 6);
            ILT[0x50] = new Instruction(BVC, REL, 2);
            ILT[0x60] = new Instruction(RTS, IMP, 6);
            ILT[0x70] = new Instruction(BVS, REL, 2);
            ILT[0x90] = new Instruction(BCC, REL, 2);
            ILT[0xA0] = new Instruction(LDY, IMM, 2);
            ILT[0xB0] = new Instruction(BCS, REL, 2);
            ILT[0xC0] = new Instruction(CPY, IMM, 2);
            ILT[0xD0] = new Instruction(BNE, REL, 2);
            ILT[0xE0] = new Instruction(CPX, IMM, 2);
            ILT[0xF0] = new Instruction(BEQ, REL, 2);

            ILT[0x01] = new Instruction(ORA, IDX, 6);
            ILT[0x11] = new Instruction(ORA, IDY, 5);
            ILT[0x21] = new Instruction(AND, IDX, 6);
            ILT[0x31] = new Instruction(AND, IDY, 5);
            ILT[0x41] = new Instruction(EOR, IDX, 6);
            ILT[0x51] = new Instruction(EOR, IDY, 5);
            ILT[0x61] = new Instruction(ADC, IDX, 6);
            ILT[0x71] = new Instruction(ADC, IDY, 5);
            ILT[0x81] = new Instruction(STA, IDX, 6);
            ILT[0x91] = new Instruction(STA, IDY, 6);
            ILT[0xA1] = new Instruction(LDA, IDX, 6);
            ILT[0xB1] = new Instruction(LDA, IDY, 5);
            ILT[0xC1] = new Instruction(CMP, IDX, 6);
            ILT[0xD1] = new Instruction(CMP, IDY, 5);
            ILT[0xE1] = new Instruction(SBC, IDX, 6);
            ILT[0xF1] = new Instruction(SBC, IDY, 5);

            ILT[0xA2] = new Instruction(LDX, IMM, 2);

            ILT[0x24] = new Instruction(BIT, ZP0, 3);
            ILT[0x84] = new Instruction(STY, ZP0, 3);
            ILT[0x94] = new Instruction(STY, ZPX, 4);
            ILT[0xA4] = new Instruction(LDY, ZP0, 3);
            ILT[0xB4] = new Instruction(LDY, ZPX, 4);
            ILT[0xC4] = new Instruction(CPY, ZP0, 3);
            ILT[0xE4] = new Instruction(CPX, ZP0, 3);

            ILT[0x05] = new Instruction(ORA, ZP0, 3);
            ILT[0x15] = new Instruction(ORA, ZPX, 4);
            ILT[0x25] = new Instruction(AND, ZP0, 3);
            ILT[0x35] = new Instruction(AND, ZPX, 4);
            ILT[0x45] = new Instruction(EOR, ZP0, 3);
            ILT[0x55] = new Instruction(EOR, ZPX, 4);
            ILT[0x65] = new Instruction(ADC, ZP0, 3);
            ILT[0x75] = new Instruction(ADC, ZPX, 4);
            ILT[0x85] = new Instruction(STA, ZP0, 3);
            ILT[0x95] = new Instruction(STA, ZPX, 4);
            ILT[0xA5] = new Instruction(LDA, ZP0, 3);
            ILT[0xB5] = new Instruction(LDA, ZPX, 4);
            ILT[0xC5] = new Instruction(CMP, ZP0, 3);
            ILT[0xD5] = new Instruction(CMP, ZPX, 4);
            ILT[0xE5] = new Instruction(SBC, ZP0, 3);
            ILT[0xF5] = new Instruction(SBC, ZPX, 4);

            ILT[0x06] = new Instruction(ASL, ZP0, 5);
            ILT[0x16] = new Instruction(ASL, ZPX, 6);
            ILT[0x26] = new Instruction(ROL, ZP0, 5);
            ILT[0x36] = new Instruction(ROL, ZPX, 6);
            ILT[0x46] = new Instruction(LSR, ZP0, 5);
            ILT[0x56] = new Instruction(LSR, ZPX, 6);
            ILT[0x66] = new Instruction(ROR, ZP0, 5);
            ILT[0x76] = new Instruction(ROR, ZPX, 6);
            ILT[0x86] = new Instruction(STX, ZP0, 3);
            ILT[0x96] = new Instruction(STX, ZPY, 4);
            ILT[0xA6] = new Instruction(LDX, ZP0, 3);
            ILT[0xB6] = new Instruction(LDX, ZPY, 4);
            ILT[0xC6] = new Instruction(DEC, ZP0, 5);
            ILT[0xD6] = new Instruction(DEC, ZPX, 6);
            ILT[0xE6] = new Instruction(INC, ZP0, 5);
            ILT[0xF6] = new Instruction(INC, ZPX, 6);

            ILT[0x08] = new Instruction(PHP, IMP, 3);
            ILT[0x18] = new Instruction(CLC, IMP, 2);
            ILT[0x28] = new Instruction(PLP, IMP, 4);
            ILT[0x38] = new Instruction(SEC, IMP, 2);
            ILT[0x48] = new Instruction(PHA, IMP, 3);
            ILT[0x58] = new Instruction(CLI, IMP, 2);
            ILT[0x68] = new Instruction(PLA, IMP, 4);
            ILT[0x78] = new Instruction(SEI, IMP, 2);
            ILT[0x88] = new Instruction(DEY, IMP, 2);
            ILT[0x98] = new Instruction(TYA, IMP, 2);
            ILT[0xA8] = new Instruction(TAY, IMP, 2);
            ILT[0xB8] = new Instruction(CLV, IMP, 2);
            ILT[0xC8] = new Instruction(INY, IMP, 2);
            ILT[0xD8] = new Instruction(CLD, IMP, 2);
            ILT[0xE8] = new Instruction(INX, IMP, 2);
            ILT[0xF8] = new Instruction(SED, IMP, 2);

            ILT[0x09] = new Instruction(ORA, IMM, 2);
            ILT[0x19] = new Instruction(ORA, ABY, 4);
            ILT[0x29] = new Instruction(AND, IMM, 2);
            ILT[0x39] = new Instruction(AND, ABY, 4);
            ILT[0x49] = new Instruction(EOR, IMM, 2);
            ILT[0x59] = new Instruction(EOR, ABY, 4);
            ILT[0x69] = new Instruction(ADC, IMM, 2);
            ILT[0x79] = new Instruction(ADC, ABY, 4);
            ILT[0x99] = new Instruction(STA, ABY, 5);
            ILT[0xA9] = new Instruction(LDA, IMM, 2);
            ILT[0xB9] = new Instruction(LDA, ABY, 4);
            ILT[0xC9] = new Instruction(CMP, IMM, 2);
            ILT[0xD9] = new Instruction(CMP, ABY, 4);
            ILT[0xE9] = new Instruction(SBC, IMM, 2);
            ILT[0xF9] = new Instruction(SBC, ABY, 4);

            ILT[0x6C] = new Instruction(JMP, IND, 5);

            ILT[0x2C] = new Instruction(BIT, ABS, 4);
            ILT[0x4C] = new Instruction(JMP, ABS, 3);
            ILT[0x8C] = new Instruction(STY, ABS, 4);
            ILT[0xAC] = new Instruction(LDY, ABS, 4);
            ILT[0xBC] = new Instruction(LDY, ABX, 4);
            ILT[0xCC] = new Instruction(CPY, ABS, 4);
            ILT[0xEC] = new Instruction(CPX, ABS, 4);

            ILT[0x0E] = new Instruction(ASL, ABS, 6);
            ILT[0x1E] = new Instruction(ASL, ABX, 7);
            ILT[0x2E] = new Instruction(ROL, ABS, 6);
            ILT[0x3E] = new Instruction(ROL, ABX, 7);
            ILT[0x4E] = new Instruction(LSR, ABS, 6);
            ILT[0x5E] = new Instruction(LSR, ABX, 7);
            ILT[0x6E] = new Instruction(ROR, ABS, 6);
            ILT[0x7E] = new Instruction(ROR, ABX, 7);
            ILT[0x8E] = new Instruction(STX, ABS, 4);
            ILT[0xAE] = new Instruction(LDX, ABS, 4);
            ILT[0xBE] = new Instruction(LDX, ABX, 4);
            ILT[0xCE] = new Instruction(DEC, ABS, 6);
            ILT[0xDE] = new Instruction(DEC, ABX, 7);
            ILT[0xEE] = new Instruction(INC, ABS, 6);
            ILT[0xFE] = new Instruction(INC, ABX, 7);

            ILT[0x10] = new Instruction(BPL, REL, 2);
            ILT[0x30] = new Instruction(BMI, REL, 2);
            ILT[0x50] = new Instruction(BVC, REL, 2);
            ILT[0x70] = new Instruction(BVS, REL, 2);
            ILT[0x90] = new Instruction(BCC, REL, 2);
            ILT[0xB0] = new Instruction(BCS, REL, 2);
            ILT[0xD0] = new Instruction(BNE, REL, 2);
            ILT[0xF0] = new Instruction(BEQ, REL, 2);
        }

        private struct Instruction
        {
            public Action AddressingMode;
            public Action InstructionAction;
            public int Cycles;

            public Instruction(Action addressingMode, Action instructionAction, int cycles)
            {
                AddressingMode = addressingMode;
                InstructionAction = instructionAction;
                Cycles = cycles;
            }
        }

        // adressing modes

        private void ABS()
        {
            byte lowByte = mmu.Read(PC);
            PC++;
            byte highByte = mmu.Read(PC);
            PC++;

            AbsoluteAddress = (ushort)((highByte << 8) | lowByte);
         
        }

        private void ABX()
        {
            byte lowByte = mmu.Read(PC);
            PC++;
            byte highByte = mmu.Read(PC);
            PC++;

            AbsoluteAddress = (ushort)((highByte << 8) | lowByte);
            AbsoluteAddress += X;

            if ((AbsoluteAddress & 0xFF00) != (highByte << 8)) 
                Clocks ++;
        }

        private void ABY()
        {
            byte lowByte = mmu.Read(PC);
            PC++;
            byte highByte = mmu.Read(PC);
            PC++;

            AbsoluteAddress = (ushort)((highByte << 8) | lowByte);
            AbsoluteAddress += Y;

            if ((AbsoluteAddress & 0xFF00) != (highByte << 8))
                Clocks++;
        }

        private void IMM()
        {
            AbsoluteAddress = PC++;
        }

        private void IMP()
        {
            FetchedData = A;
        }

        private void IND()
        {
            ushort lowByte = mmu.Read(PC);
            PC++;
            ushort highByte = mmu.Read(PC);
            PC++;

            ushort pointer = (ushort)((highByte << 8) | lowByte);

            AbsoluteAddress = ((ushort)(mmu.Read((ushort)((pointer + 1) << 8)) | mmu.Read(pointer)));
        }

        private void IDX()
        {

            ushort lowByte = mmu.Read((ushort)((PC + X) & 0x00FF));
            PC++;
            ushort highByte = mmu.Read((ushort)((PC + X) & 0x00FF));
            PC++;

            AbsoluteAddress = (ushort)((highByte << 8) | lowByte);

        }

        private void IDY()
        {
            ushort lowByte = mmu.Read((ushort)(PC & 0x00FF));
            PC++;
            ushort highByte = mmu.Read((ushort)(PC & 0x00FF));
            PC++;

            AbsoluteAddress = (ushort)((highByte << 8) | lowByte);
            AbsoluteAddress += Y;

            if ((AbsoluteAddress & 0xFF00) != (highByte << 8))
                Clocks++;
        }


        private void REL()
        {
            RelativeAddress = mmu.Read(PC);
            PC++;
            if ((RelativeAddress & 0x80) == 1)
                RelativeAddress |= 0xFF00;
        }

        private void ZP0()
        {
            AbsoluteAddress = mmu.Read(PC);
            PC++;
            AbsoluteAddress &= 0x00FF;
        }

        private void ZPX()
        {
            AbsoluteAddress = (ushort)(mmu.Read(PC) + X);
            PC++;
            AbsoluteAddress &= 0x00FF;
        }

        private void ZPY()
        {
            AbsoluteAddress = (ushort)(mmu.Read(PC) + Y);
            PC++;
            AbsoluteAddress &= 0x00FF;
        }


        // instructions

        private void ADC()
        {
            ushort data = AbsoluteAddress;

            ushort value = (ushort)(A + data + (CarryFlag ? 1 : 0));

            CarryFlag = value > 255;
            ZeroFlag = (value & 0x00FF) == 0;
            NegativeFlag = (value & 0x80) == 1;
            OverflowFlag = ((A ^ data) & 0x80) == 0 && ((A ^ value) & 0x80) != 0;

            A = (byte)(value & 0x00FF);
        }

        private void AND()
        {
            ushort data = AbsoluteAddress;

            A = ((byte)(A & data));
            ZeroFlag = A == 0x00;
            NegativeFlag = (A & 0x80) == 1;

        }

        private void ASL()
        {
            ushort data = AbsoluteAddress;

            ushort value = (ushort)(data << 1);

            CarryFlag = (value & 0xFF00) > 0 ;
            ZeroFlag = (data & 0x00FF) == 0x00;
            NegativeFlag = (data & 0x80) == 1;

            if (ILT[opcode].AddressingMode == IMP)
            {
                A = (byte)(data & 0x00FF);
            }
            else
            {
                mmu.Write(AbsoluteAddress, (byte)(data & 0x00FF));
            }

        }

        private void BCC()
        {
            if (!CarryFlag)
            {
                Clocks++;
                AbsoluteAddress = (ushort)(PC + RelativeAddress);

                if((AbsoluteAddress & 0xFF00) != (PC & 0xFF00))
                {
                    Clocks++;
                }

                PC = AbsoluteAddress;
            }
        }

        private void BCS()
        {
            if (CarryFlag)
            {
                Clocks++;
                AbsoluteAddress = (ushort)(PC + RelativeAddress);

                if ((AbsoluteAddress & 0xFF00) != (PC & 0xFF00))
                {
                    Clocks++;
                }

                PC = AbsoluteAddress;
            }
        }

        private void BEQ()
        {
            if (ZeroFlag)
            {
                Clocks++;
                AbsoluteAddress = (ushort)(PC + RelativeAddress);

                if ((AbsoluteAddress & 0xFF00) != (PC & 0xFF00))
                {
                    Clocks++;
                }

                PC = AbsoluteAddress;
            }
        }

        private void BIT()
        {
            ushort data = AbsoluteAddress;

            ushort value = (ushort)(A & data);

            ZeroFlag = (value & 0x00FF) == 0;
            NegativeFlag = (value & 0x80) == 1;
            OverflowFlag = (value & (1<<6)) == 1;

        }

        private void BMI()
        {
            if (NegativeFlag)
            {
                Clocks++;
                AbsoluteAddress = (ushort)(PC + RelativeAddress);

                if ((AbsoluteAddress & 0xFF00) != (PC & 0xFF00))
                {
                    Clocks++;
                }

                PC = AbsoluteAddress;
            }
        }

        private void BNE()
        {
            if (!ZeroFlag)
            {
                Clocks++;
                AbsoluteAddress = (ushort)(PC + RelativeAddress);

                if ((AbsoluteAddress & 0xFF00) != (PC & 0xFF00))
                {
                    Clocks++;
                }

                PC = AbsoluteAddress;
            }
        }

        private void BPL()
        {
            if (!NegativeFlag)
            {
                Clocks++;
                AbsoluteAddress = (ushort)(PC + RelativeAddress);

                if ((AbsoluteAddress & 0xFF00) != (PC & 0xFF00))
                {
                    Clocks++;
                }

                PC = AbsoluteAddress;
            }
        }

        private void BRK()
        {
            PC++;

            InterruptDisableFlag = true;
            mmu.Write((ushort)(0x0100 + SP), (byte)((PC >> 8) & 0x00FF));
            SP++;
            mmu.Write((ushort)(0x0100 + SP), (byte)(PC & 0x00FF));
            SP++;

            BreakFlag = true;

            byte status = 0;
            status |= (byte)(CarryFlag ? 0x01 : 0x00);
            status |= (byte)(ZeroFlag ? 0x02 : 0x00);
            status |= (byte)(InterruptDisableFlag ? 0x04 : 0x00);
            status |= (byte)(DecimalModeFlag ? 0x08 : 0x00);
            status |= (byte)(BreakFlag ? 0x10 : 0x00);
            status |= (byte)(OverflowFlag ? 0x40 : 0x00);
            status |= (byte)(NegativeFlag ? 0x80 : 0x00);

            BreakFlag = false;
            mmu.Write((ushort)(0x0100 + SP), status);

            PC = (ushort)(mmu.Read(0xFFFE) | (mmu.Read(0xFFFF) << 8));

        }

        private void BVC()
        {
            if (!OverflowFlag)
            {
                Clocks++;
                AbsoluteAddress = (ushort)(PC + RelativeAddress);

                if ((AbsoluteAddress & 0xFF00) != (PC & 0xFF00))
                {
                    Clocks++;
                }

                PC = AbsoluteAddress;
            }
        }

        private void BVS()
        {
            if (OverflowFlag)
            {
                Clocks++;
                AbsoluteAddress = (ushort)(PC + RelativeAddress);

                if ((AbsoluteAddress & 0xFF00) != (PC & 0xFF00))
                {
                    Clocks++;
                }

                PC = AbsoluteAddress;
            }
        }

        private void CLC()
        {
            CarryFlag = false;
        }

        private void CLD()
        {
            DecimalModeFlag = false;
        }

        private void CLI()
        {
            InterruptDisableFlag = false;
        }

        private void CLV()
        {
            OverflowFlag = false;
        }

        private void CMP()
        {
            ushort data = AbsoluteAddress;

            ushort value = (ushort)(A - data);

            CarryFlag = A >= data;
            ZeroFlag = (value & 0x00FF) == 0;
            NegativeFlag = (value & 0x0000) == 1;

        }

        private void CPX()
        {
            ushort data = AbsoluteAddress;

            ushort value = (ushort)(X - data);

            CarryFlag = X >= data;
            ZeroFlag = (value & 0x00FF) == 0;
            NegativeFlag = (value & 0x0000) == 1;
        }

        private void CPY()
        {
            ushort data = AbsoluteAddress;

            ushort value = (ushort)(Y - data);

            CarryFlag = Y >= data;
            ZeroFlag = (value & 0x00FF) == 0;
            NegativeFlag = (value & 0x0000) == 1;
        }

        private void DEC()
        {
            ushort data = AbsoluteAddress;

            ushort value = (ushort)(data - 1);

            CarryFlag = X >= data;
            ZeroFlag = (value & 0x00FF) == 0;
            NegativeFlag = (value & 0x0000) == 1;
        }

        private void DEX()
        {
            X--;
            ZeroFlag = X == 0x00;
            NegativeFlag = X == 0x00;
        }

        private void DEY()
        {
            Y--;
            ZeroFlag = Y == 0x00;
            NegativeFlag = Y == 0x00;
        }

        private void EOR()
        {
            ushort data = AbsoluteAddress;

            A = (byte)(A ^ data);

        }

        private void INC()
        {
        }

        private void INX()
        {
        }

        private void INY()
        {
        }

        private void JMP()
        {
        }

        private void JSR()
        {
        }

        private void LDA()
        {
        }

        private void LDX()
        {
        }

        private void LDY()
        {
        }

        private void LSR()
        {
        }

        private void NOP()
        {
        }

        private void ORA()
        {
        }

        private void PHA()
        {
        }

        private void PHP()
        {
        }

        private void PLA()
        {
        }

        private void PLP()
        {
        }

        private void ROL()
        {
        }

        private void ROR()
        {
        }

        private void RTI()
        {
        }

        private void RTS()
        {
        }

        private void SBC()
        {
        }

        private void SEC()
        {
        }

        private void SED()
        {
        }

        private void SEI()
        {
        }

        private void STA()
        {
        }

        private void STX()
        {
        }

        private void STY()
        {
        }

        private void TAX()
        {
        }

        private void TAY()
        {
        }

        private void TSX()
        {
        }

        private void TXA()
        {
        }

        private void TXS()
        {
        }

        private void TYA()
        {
        }

    }
}
