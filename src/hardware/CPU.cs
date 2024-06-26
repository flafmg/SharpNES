﻿using System;
using System.Collections.Generic;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Runtime.Intrinsics.X86;
using System.Drawing;
using System.Numerics;

namespace SharpNES.src.hardware
{
    internal class CPU
    {
        private MMU mmu;

        private byte A;
        private byte X;
        private byte Y;
        private byte SP;
        private ushort PC = 5;

        private bool halt;

        private byte opcode;

        private byte status;

        public bool CarryFlag
        {
            get { return (status & 0x01) != 0; }
            set { status = (byte)((status & 0xFE) | (value ? 1 : 0)); }
        }

        public bool ZeroFlag
        {
            get { return (status & 0x02) != 0; }
            set { status = (byte)((status & 0xFD) | (value ? 2 : 0)); }
        }

        public bool InterruptDisableFlag
        {
            get { return (status & 0x04) != 0; }
            set { status = (byte)((status & 0xFB) | (value ? 4 : 0)); }
        }

        public bool DecimalModeFlag
        {
            get { return (status & 0x08) != 0; }
            set { status = (byte)((status & 0xF7) | (value ? 8 : 0)); }
        }

        public bool BreakFlag
        {
            get { return (status & 0x10) != 0; }
            set { status = (byte)((status & 0xEF) | (value ? 0x10 : 0)); }
        }

        public bool UnusedFlag
        {
            get { return (status & 0x20) != 0; }
            set { status = (byte)((status & 0xDF) | (value ? 0x20 : 0)); }
        }

        public bool OverflowFlag
        {
            get { return (status & 0x40) != 0; }
            set { status = (byte)((status & 0xBF) | (value ? 0x40 : 0)); }
        }

        public bool NegativeFlag
        {
            get { return (status & 0x80) != 0; }
            set { status = (byte)((status & 0x7F) | (value ? 0x80 : 0)); }
        }


        private byte fetchedData;
        private ushort absoluteAddress;
        private ushort relativeAddress;

        private int waitCycles; //wait cycles until next instruction
        private ulong clocksPassed;

        private Instruction[] ILT;

        public CPU(MMU mmu)
        {
            this.mmu = mmu;

            A = 0x0;
            X = 0x0;
            Y = 0x0;
            SP = 0xFD;
            PC = 0xFFFC;

            CarryFlag = false;
            ZeroFlag = false;
            InterruptDisableFlag = false;
            DecimalModeFlag = false;
            OverflowFlag = false;
            NegativeFlag = false;
            BreakFlag = false;

            fetchedData = 0x0;
            absoluteAddress = 0x0;
            relativeAddress = 0x0;
            waitCycles = 0;
            clocksPassed = 0;

            initializeILT();
        }

        public void Reset()
        {
            A = 0x00;
            X = 0x00;
            Y = 0x00;
            SP = 0xFD;
            absoluteAddress = 0xFFFC;

            byte lowByte = mmu.CPURead(absoluteAddress);
            byte highByte = mmu.CPURead(absoluteAddress + 1);
            

            PC = (ushort)((highByte << 8) | lowByte);
            

            absoluteAddress = 0x0000;
            relativeAddress = 0x0000;
            fetchedData = 0x0000;

            status = 0x0000;
            UnusedFlag = true;

            waitCycles = 8;
        }
        public void InterruptRequest()
        {
            if (!InterruptDisableFlag)
            {
                mmu.CPUWrite(0x100 + SP, (PC >> 8) & 0x00FF);
                SP--;
                mmu.CPUWrite(0x100 + SP, (PC) & 0x00FF);
                SP--;

                BreakFlag = false;
                UnusedFlag = true;
                InterruptDisableFlag = true;

                mmu.CPUWrite(0x100 + SP, status);
                SP--;

                absoluteAddress = 0xFFFE;

                byte lowByte = mmu.CPURead(absoluteAddress);
                byte highByte = mmu.CPURead(absoluteAddress + 1);

                waitCycles = 7;
            }
        }

        public void NonMaskableInterrupt()
        {
            mmu.CPUWrite(0x100 + SP, (PC >> 8) & 0x00FF);
            SP--;
            mmu.CPUWrite(0x100 + SP, (PC) & 0x00FF);
            SP--;

            BreakFlag = false;
            UnusedFlag = true;
            InterruptDisableFlag = true;

            mmu.CPUWrite(0x100 + SP, status);
            SP--;

            absoluteAddress = 0xFFFA;

            byte lowByte = mmu.CPURead(absoluteAddress);
            byte highByte = mmu.CPURead(absoluteAddress + 1);

            absoluteAddress = (ushort)((highByte << 8) | lowByte);
            PC = absoluteAddress;

            waitCycles = 8;
        }

        public void Cycle()
        {
            if (halt)
            {
                return;
            }
            if (waitCycles == 0)
            {
                UnusedFlag = true;
                opcode = mmu.CPURead(PC);

                PC++;
                waitCycles = ILT[opcode].Cycles;
               
                ILT[opcode].AddressingMode();
                ILT[opcode].InstructionAction();
                //Console.WriteLine($"0x{PC:X4} > 0x{opcode:X2} : {ILT[opcode].name}");

                UnusedFlag = true;
            }
            if (waitCycles > 0)
            {
                waitCycles--;
            }

            clocksPassed++;

        }

        private void initializeILT()
        {
            ILT = new Instruction[256];
            for (int i = 0; i < 256; i++)
            {
                ILT[i] = new Instruction(() => { }, () => { }, 0, "NOP");
            }


            ILT[0x00] = new Instruction(BRK, IMP, 7, "BRK");
            ILT[0x10] = new Instruction(BPL, REL, 2, "BPL");
            ILT[0x20] = new Instruction(JSR, ABS, 6, "JSR");
            ILT[0x30] = new Instruction(BMI, REL, 2, "BMI");
            ILT[0x40] = new Instruction(RTI, IMP, 6, "RTI");
            ILT[0x50] = new Instruction(BVC, REL, 2, "BVC");
            ILT[0x60] = new Instruction(RTS, IMP, 6, "RTS");
            ILT[0x70] = new Instruction(BVS, REL, 2, "BVS");
            ILT[0x80] = new Instruction(NOP, IMM, 2, "*NOP");
            ILT[0x90] = new Instruction(BCC, REL, 2, "BCC");
            ILT[0xA0] = new Instruction(LDY, IMM, 2, "LDY");
            ILT[0xB0] = new Instruction(BCS, REL, 2, "BCS");
            ILT[0xC0] = new Instruction(CPY, IMM, 2, "CPY");
            ILT[0xD0] = new Instruction(BNE, REL, 2, "BNE");
            ILT[0xE0] = new Instruction(CPX, IMM, 2, "CPX");
            ILT[0xF0] = new Instruction(BEQ, REL, 2, "BEQ");

            ILT[0x01] = new Instruction(ORA, IDX, 6, "ORA");
            ILT[0x11] = new Instruction(ORA, IDY, 5, "ORA");
            ILT[0x21] = new Instruction(AND, IDX, 6, "AND");
            ILT[0x31] = new Instruction(AND, IDY, 5, "AND");
            ILT[0x41] = new Instruction(EOR, IDX, 6, "EOR");
            ILT[0x51] = new Instruction(EOR, IDY, 5, "EOR");
            ILT[0x61] = new Instruction(ADC, IDX, 6, "ADC");
            ILT[0x71] = new Instruction(ADC, IDY, 5, "ADC");
            ILT[0x81] = new Instruction(STA, IDX, 6, "STA");
            ILT[0x91] = new Instruction(STA, IDY, 6, "STA");
            ILT[0xA1] = new Instruction(LDA, IDX, 6, "LDA");
            ILT[0xB1] = new Instruction(LDA, IDY, 5, "LDA");
            ILT[0xC1] = new Instruction(CMP, IDX, 6, "CMP");
            ILT[0xD1] = new Instruction(CMP, IDY, 5, "CMP");
            ILT[0xE1] = new Instruction(SBC, IDX, 6, "SBC");
            ILT[0xF1] = new Instruction(SBC, IDY, 5, "SBC");

            ILT[0xA2] = new Instruction(LDX, IMM, 2, "LDX");

            ILT[0x04] = new Instruction(NOP, ZP0, 3, "*NOP");
            ILT[0x14] = new Instruction(NOP, ZPX, 4, "*NOP");
            ILT[0x24] = new Instruction(BIT, ZP0, 3, "BIT");
            ILT[0x34] = new Instruction(NOP, ZPX, 4, "*NOP");
            ILT[0x44] = new Instruction(NOP, ZP0, 3, "*NOP");
            ILT[0x54] = new Instruction(NOP, ZPX, 4, "*NOP");
            ILT[0x64] = new Instruction(NOP, ZP0, 3, "*NOP");
            ILT[0x74] = new Instruction(NOP, ZPX, 4, "*NOP");
            ILT[0x84] = new Instruction(STY, ZP0, 3, "STY");
            ILT[0x94] = new Instruction(STY, ZPX, 4, "STY");
            ILT[0xA4] = new Instruction(LDY, ZP0, 3, "LDY");
            ILT[0xB4] = new Instruction(LDY, ZPX, 4, "LDY");
            ILT[0xC4] = new Instruction(CPY, ZP0, 3, "CPY");
            ILT[0xD4] = new Instruction(NOP, ZPX, 4, "*NOP");
            ILT[0xE4] = new Instruction(CPX, ZP0, 3, "CPX");
            ILT[0xF4] = new Instruction(NOP, ZPX, 4, "*NOP");

            ILT[0x05] = new Instruction(ORA, ZP0, 3, "ORA");
            ILT[0x15] = new Instruction(ORA, ZPX, 4, "ORA");
            ILT[0x25] = new Instruction(AND, ZP0, 3, "AND");
            ILT[0x35] = new Instruction(AND, ZPX, 4, "AND");
            ILT[0x45] = new Instruction(EOR, ZP0, 3, "EOR");
            ILT[0x55] = new Instruction(EOR, ZPX, 4, "EOR");
            ILT[0x65] = new Instruction(ADC, ZP0, 3, "ADC");
            ILT[0x75] = new Instruction(ADC, ZPX, 4, "ADC");
            ILT[0x85] = new Instruction(STA, ZP0, 3, "STA");
            ILT[0x95] = new Instruction(STA, ZPX, 4, "STA");
            ILT[0xA5] = new Instruction(LDA, ZP0, 3, "LDA");
            ILT[0xB5] = new Instruction(LDA, ZPX, 4, "LDA");
            ILT[0xC5] = new Instruction(CMP, ZP0, 3, "CMP");
            ILT[0xD5] = new Instruction(CMP, ZPX, 4, "CMP");
            ILT[0xE5] = new Instruction(SBC, ZP0, 3, "SBC");
            ILT[0xF5] = new Instruction(SBC, ZPX, 4, "SBC");

            ILT[0x06] = new Instruction(ASL, ZP0, 5, "ASL");
            ILT[0x16] = new Instruction(ASL, ZPX, 6, "ASL");
            ILT[0x26] = new Instruction(ROL, ZP0, 5, "ROL");
            ILT[0x36] = new Instruction(ROL, ZPX, 6, "ROL");
            ILT[0x46] = new Instruction(LSR, ZP0, 5, "LSR");
            ILT[0x56] = new Instruction(LSR, ZPX, 6, "LSR");
            ILT[0x66] = new Instruction(ROR, ZP0, 5, "ROR");
            ILT[0x76] = new Instruction(ROR, ZPX, 6, "ROR");
            ILT[0x86] = new Instruction(STX, ZP0, 3, "STX");
            ILT[0x96] = new Instruction(STX, ZPY, 4, "STX");
            ILT[0xA6] = new Instruction(LDX, ZP0, 3, "LDX");
            ILT[0xB6] = new Instruction(LDX, ZPY, 4, "LDX");
            ILT[0xC6] = new Instruction(DEC, ZP0, 5, "DEC");
            ILT[0xD6] = new Instruction(DEC, ZPX, 6, "DEC");
            ILT[0xE6] = new Instruction(INC, ZP0, 5, "INC");
            ILT[0xF6] = new Instruction(INC, ZPX, 6, "INC");

            ILT[0x08] = new Instruction(PHP, IMP, 3, "PHP");
            ILT[0x18] = new Instruction(CLC, IMP, 2, "CLC");
            ILT[0x28] = new Instruction(PLP, IMP, 4, "PLP");
            ILT[0x38] = new Instruction(SEC, IMP, 2, "SEC");
            ILT[0x48] = new Instruction(PHA, IMP, 3, "PHA");
            ILT[0x58] = new Instruction(CLI, IMP, 2, "CLI");
            ILT[0x68] = new Instruction(PLA, IMP, 4, "PLA");
            ILT[0x78] = new Instruction(SEI, IMP, 2, "SEI");
            ILT[0x88] = new Instruction(DEY, IMP, 2, "DEY");
            ILT[0x98] = new Instruction(TYA, IMP, 2, "TYA");
            ILT[0xA8] = new Instruction(TAY, IMP, 2, "TAY");
            ILT[0xB8] = new Instruction(CLV, IMP, 2, "CLV");
            ILT[0xC8] = new Instruction(INY, IMP, 2, "INY");
            ILT[0xD8] = new Instruction(CLD, IMP, 2, "CLD");
            ILT[0xE8] = new Instruction(INX, IMP, 2, "INX");
            ILT[0xF8] = new Instruction(SED, IMP, 2, "SED");

            ILT[0x09] = new Instruction(ORA, IMM, 2, "ORA");
            ILT[0x19] = new Instruction(ORA, ABY, 4, "ORA");
            ILT[0x29] = new Instruction(AND, IMM, 2, "AND");
            ILT[0x39] = new Instruction(AND, ABY, 4, "AND");
            ILT[0x49] = new Instruction(EOR, IMM, 2, "EOR");
            ILT[0x59] = new Instruction(EOR, ABY, 4, "EOR");
            ILT[0x69] = new Instruction(ADC, IMM, 2, "ADC");
            ILT[0x79] = new Instruction(ADC, ABY, 4, "ADC");
            ILT[0x99] = new Instruction(STA, ABY, 5, "STA");
            ILT[0xA9] = new Instruction(LDA, IMM, 2, "LDA");
            ILT[0xB9] = new Instruction(LDA, ABY, 4, "LDA");
            ILT[0xC9] = new Instruction(CMP, IMM, 2, "CMP");
            ILT[0xD9] = new Instruction(CMP, ABY, 4, "CMP");
            ILT[0xE9] = new Instruction(SBC, IMM, 2, "SBC");
            ILT[0xF9] = new Instruction(SBC, ABY, 4, "SBC");

            ILT[0x0A] = new Instruction(ASL, IMP, 2, "ASL");
            ILT[0x2A] = new Instruction(ROL, IMP, 2, "ROL");
            ILT[0x4A] = new Instruction(LSR, IMP, 2, "LSR");
            ILT[0x6A] = new Instruction(ROR, IMP, 2, "ROR");
            ILT[0x8A] = new Instruction(TXA, IMP, 2, "TXA");
            ILT[0x9A] = new Instruction(TXS, IMP, 2, "TXS");
            ILT[0xAA] = new Instruction(TAX, IMP, 2, "TAX");
            ILT[0xBA] = new Instruction(TSX, IMP, 2, "TSX");
            ILT[0xCA] = new Instruction(DEX, IMP, 2, "DEX");
            ILT[0xEA] = new Instruction(NOP, IMP, 2, "NOP");

            ILT[0x0C] = new Instruction(NOP, ABS, 4, "*NOP");
            ILT[0x1C] = new Instruction(NOP, ABX, 4, "*NOP");
            ILT[0x2C] = new Instruction(BIT, ABS, 4, "BIT");
            ILT[0x3C] = new Instruction(NOP, ABX, 4, "*NOP");
            ILT[0x4C] = new Instruction(JMP, ABS, 3, "JMP");
            ILT[0x5C] = new Instruction(NOP, ABX, 4, "*NOP");
            ILT[0x6C] = new Instruction(JMP, IND, 5, "JMP");
            ILT[0x7C] = new Instruction(NOP, ABX, 4, "*NOP");
            ILT[0x8C] = new Instruction(STY, ABS, 4, "STY");
            ILT[0x9C] = new Instruction(NOP, ABX, 4, "*NOP");
            ILT[0xAC] = new Instruction(LDY, ABS, 4, "LDY");
            ILT[0xBC] = new Instruction(LDY, ABX, 4, "LDY");
            ILT[0xCC] = new Instruction(CPY, ABS, 4, "CPY");
            ILT[0xDC] = new Instruction(NOP, ABX, 4, "*NOP");
            ILT[0xEC] = new Instruction(CPX, ABS, 4, "CPX");
            ILT[0xFC] = new Instruction(NOP, ABX, 4, "*NOP");

            ILT[0x0D] = new Instruction(ORA, ABS, 4, "ORA");
            ILT[0x1D] = new Instruction(ORA, ABX, 4, "ORA");
            ILT[0x2D] = new Instruction(AND, ABS, 4, "AND");
            ILT[0x3D] = new Instruction(AND, ABX, 4, "AND");
            ILT[0x4D] = new Instruction(EOR, ABS, 4, "EOR");
            ILT[0x5D] = new Instruction(EOR, ABX, 4, "EOR");
            ILT[0x6D] = new Instruction(ADC, ABS, 4, "ADC");
            ILT[0x7D] = new Instruction(ADC, ABX, 4, "ADC");
            ILT[0x8D] = new Instruction(STA, ABS, 4, "STA");
            ILT[0x9D] = new Instruction(STA, ABX, 5, "STA");
            ILT[0xAD] = new Instruction(LDA, ABS, 4, "LDA");
            ILT[0xBD] = new Instruction(LDA, ABX, 4, "LDA");
            ILT[0xCD] = new Instruction(CMP, ABS, 4, "CMP");
            ILT[0xDD] = new Instruction(CMP, ABX, 4, "CMP");
            ILT[0xED] = new Instruction(SBC, ABS, 4, "SBC");
            ILT[0xFD] = new Instruction(SBC, ABX, 4, "SBC");

            ILT[0x0E] = new Instruction(ASL, ABS, 6, "ASL");
            ILT[0x1E] = new Instruction(ASL, ABX, 7, "ASL");
            ILT[0x2E] = new Instruction(ROL, ABS, 6, "ROL");
            ILT[0x3E] = new Instruction(ROL, ABX, 7, "ROL");
            ILT[0x4E] = new Instruction(LSR, ABS, 6, "LSR");
            ILT[0x5E] = new Instruction(LSR, ABX, 7, "LSR");
            ILT[0x6E] = new Instruction(ROR, ABS, 6, "ROR");
            ILT[0x7E] = new Instruction(ROR, ABX, 7, "ROR");
            ILT[0x8E] = new Instruction(STX, ABS, 4, "STX");
            ILT[0xAE] = new Instruction(LDX, ABS, 4, "LDX");
            ILT[0xBE] = new Instruction(LDX, ABY, 4, "LDX");
            ILT[0xCE] = new Instruction(DEC, ABS, 6, "DEC");
            ILT[0xDE] = new Instruction(DEC, ABX, 7, "DEC");
            ILT[0xEE] = new Instruction(INC, ABS, 6, "INC");
            ILT[0xFE] = new Instruction(INC, ABX, 7, "INC");


        }

        private struct Instruction
        {
            public Action AddressingMode;
            public Action InstructionAction;
            public int Cycles;
            public string name;

            public Instruction(Action instructionAction, Action addressingMode, int cycles, string name)
            {
                AddressingMode = addressingMode;
                InstructionAction = instructionAction;
                Cycles = cycles;
                this.name = name;
            }
        }

        // adressing modes

        private void getData()
        {
            if ((ILT[opcode].AddressingMode != IMP))
            {
                fetchedData = mmu.CPURead(absoluteAddress);
            }
        }

        private void ABS()
        {
            byte lowByte = mmu.CPURead(PC);
            PC++;
            byte highByte = mmu.CPURead(PC);
            PC++;
            absoluteAddress = (ushort)((highByte << 8) | lowByte);


        }

        private void ABX()
        {
            byte lowByte = mmu.CPURead(PC);
            PC++;
            byte highByte = mmu.CPURead(PC);
            PC++;

            absoluteAddress = (ushort)((highByte << 8) | lowByte);
            absoluteAddress += X;

            if ((absoluteAddress & 0xFF00) != (highByte << 8))
                waitCycles++;
        }

        private void ABY()
        {
            byte lowByte = mmu.CPURead(PC);
            PC++;
            byte highByte = mmu.CPURead(PC);
            PC++;

            absoluteAddress = (ushort)((highByte << 8) | lowByte);
            absoluteAddress += Y;

            if ((absoluteAddress & 0xFF00) != (highByte << 8))
                waitCycles++;

        }

        private void IMM()
        {
            absoluteAddress = PC++;
        }

        private void IMP()
        {
            fetchedData = A;
        }

        private void IND()
        {

            byte lowByte = mmu.CPURead(PC);
            PC++;
            byte highByte = mmu.CPURead(PC);
            PC++;

            ushort pointer = (ushort)((highByte << 8) | lowByte);

            if (lowByte == 0xFF)
            {
                highByte = mmu.CPURead(pointer & 0xFF00);
            }
            else
            {
                highByte = mmu.CPURead(pointer + 1);
            }
            lowByte = mmu.CPURead(pointer);

            absoluteAddress = (ushort)(highByte << 8 | lowByte);

        }

        private void IDX()
        {
            ushort ptr = mmu.CPURead(PC);
            PC++;

            ushort lowByte = mmu.CPURead((ushort)((ptr + X) & 0x00FF));
            ushort highByte = mmu.CPURead((ushort)((ptr + X + 1) & 0x00FF));


            absoluteAddress = (ushort)((highByte << 8) | lowByte);
        }

        private void IDY()
        {
            ushort ptr = mmu.CPURead(PC);
            PC++;

            ushort lowByte = mmu.CPURead((ushort)((ptr) & 0x00FF));
            ushort highByte = mmu.CPURead((ushort)((ptr + 1) & 0x00FF));


            absoluteAddress = (ushort)((highByte << 8) | lowByte);
            absoluteAddress += Y;

            if ((absoluteAddress & 0xFF00) != (highByte << 8))
                waitCycles++;
        }


        private void REL()
        {
            relativeAddress = mmu.CPURead(PC);
            PC++;
            if ((relativeAddress & 0x80) != 0)
                relativeAddress |= 0xFF00;
        }

        private void ZP0()
        {
            absoluteAddress = mmu.CPURead(PC);
            PC++;
            absoluteAddress &= 0x00FF;
        }

        private void ZPX()
        {
            absoluteAddress = (ushort)(mmu.CPURead(PC) + X);
            PC++;
            absoluteAddress &= 0x00FF;
        }

        private void ZPY()
        {
            absoluteAddress = (ushort)(mmu.CPURead(PC) + Y);
            PC++;
            absoluteAddress &= 0x00FF;
        }


        // instructions

        private void ADC()
        {
            getData();

            ushort value = (ushort)(A + fetchedData + (CarryFlag ? 1 : 0));

            CarryFlag = value > 0x00FF;
            ZeroFlag = (byte)(value & 0x00FF) == 0x00;
            NegativeFlag = ((byte)(value & 0x00FF) > 127);
            OverflowFlag = (~(A ^ fetchedData) & (A ^ value) & 0x0080) != 0;

            A = (byte)(value & 0x00FF);
        }

        private void AND()
        {
            getData();

            A = ((byte)(A & fetchedData));
            ZeroFlag = A == 0x00;
            NegativeFlag = (A > 127);

        }

        private void ASL()
        {
            getData();

            ushort value = (ushort)(fetchedData << 1);
            CarryFlag = value > 0x00FF;
            ZeroFlag = ((byte)(value & 0x00FF) == 0x00);
            NegativeFlag = ((value & 0x00FF) > 127);

            if (ILT[opcode].AddressingMode == IMP)
            {
                A = (byte)(value & 0x00FF);
            }
            else
            {
                mmu.CPUWrite(absoluteAddress, (byte)(value & 0x00FF));
            }

        }

        private void BCC()
        {
            if (!CarryFlag)
            {
                waitCycles++;
                absoluteAddress = (ushort)(PC + relativeAddress);

                if ((absoluteAddress & 0xFF00) != (PC & 0xFF00))
                {
                    waitCycles++;
                }

                PC = absoluteAddress;
            }
        }

        private void BCS()
        {
            if (CarryFlag)
            {
                waitCycles++;
                absoluteAddress = (ushort)(PC + relativeAddress);
                if ((absoluteAddress & 0xFF00) != (PC & 0xFF00))
                {
                    waitCycles++;
                }

                PC = absoluteAddress;
            }
        }

        private void BEQ()
        {

            if (ZeroFlag)
            {
                waitCycles++;
                absoluteAddress = (ushort)(PC + relativeAddress);
                if ((absoluteAddress & 0xFF00) != (PC & 0xFF00))
                {
                    waitCycles++;
                }

                PC = absoluteAddress;
            }
        }

        private void BIT()
        {
            getData();

            byte value = (byte)(A & fetchedData);

            ZeroFlag = value == 0;
            NegativeFlag = ((fetchedData & 0x00FF) > 127);
            OverflowFlag = ((fetchedData & (1 << 6)) != 0);
        }

        private void BMI()
        {
            if (NegativeFlag)
            {
                waitCycles++;
                absoluteAddress = (ushort)(PC + relativeAddress);

                if ((absoluteAddress & 0xFF00) != (PC & 0xFF00))
                {
                    waitCycles++;
                }

                PC = absoluteAddress;
            }
        }

        private void BNE()
        {
            if (!ZeroFlag)
            {
                waitCycles++;
                absoluteAddress = (ushort)(PC + relativeAddress);

                if ((absoluteAddress & 0xFF00) != (PC & 0xFF00))
                {
                    waitCycles++;
                }

                PC = absoluteAddress;
            }
        }

        private void BPL()
        {
            if (!NegativeFlag)
            {
                waitCycles++;
                absoluteAddress = (ushort)(PC + relativeAddress);

                if ((absoluteAddress & 0xFF00) != (PC & 0xFF00))
                {
                    waitCycles++;
                }

                PC = absoluteAddress;
            }
        }

        private void BRK()
        {
            halt = true;

            PC++;

            InterruptDisableFlag = true;

            mmu.CPUWrite((ushort)(0x0100 + SP), (byte)((PC >> 8) & 0x00FF));
            SP--;
            mmu.CPUWrite((ushort)(0x0100 + SP), (byte)(PC & 0x00FF));
            SP--;

            BreakFlag = true;

            mmu.CPUWrite((ushort)(0x0100 + SP), status);

            BreakFlag = false;

            PC = (ushort)(mmu.CPURead(0xFFFE) | (mmu.CPURead(0xFFFF) << 8));

        }

        private void BVC()
        {
            if (!OverflowFlag)
            {
                waitCycles++;
                absoluteAddress = (ushort)(PC + relativeAddress);

                if ((absoluteAddress & 0xFF00) != (PC & 0xFF00))
                {
                    waitCycles++;
                }

                PC = absoluteAddress;
            }
        }

        private void BVS()
        {
            if (OverflowFlag)
            {
                waitCycles++;
                absoluteAddress = (ushort)(PC + relativeAddress);

                if ((absoluteAddress & 0xFF00) != (PC & 0xFF00))
                {
                    waitCycles++;
                }

                PC = absoluteAddress;
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
            getData();

            byte value = (byte)(A - fetchedData);

            CarryFlag = A >= fetchedData;
            ZeroFlag = value == 0x00;
            NegativeFlag = (value > 127);

        }

        private void CPX()
        {
            getData();

            ushort value = (ushort)(X - fetchedData);

            CarryFlag = X >= fetchedData;
            ZeroFlag = value == 0x00;
            NegativeFlag = (value > 127);
        }

        private void CPY()
        {
            getData();

            ushort value = (ushort)(Y - fetchedData);

            CarryFlag = Y >= fetchedData;
            ZeroFlag = value == 0x00;
            NegativeFlag = (value > 127);
        }

        private void DEC()
        {
            getData();

            byte value = (byte)(fetchedData - 1);

            mmu.CPUWrite(absoluteAddress, value);

            ZeroFlag = value == 0x00;
            NegativeFlag = value > 127;
        }

        private void DEX()
        {
            X--;
            ZeroFlag = X == 0x00;
            NegativeFlag = X > 127;
        }

        private void DEY()
        {
            Y--;
            ZeroFlag = Y == 0x00;
            NegativeFlag = Y > 127;
        }

        private void EOR()
        {
            getData();

            A = (byte)(A ^ fetchedData);
            ZeroFlag = A == 0x00;
            NegativeFlag = (A > 127);
        }

        private void INC()
        {
            getData();

            byte value = (byte)(fetchedData + 1);

            mmu.CPUWrite(absoluteAddress, value);


            ZeroFlag = value == 0x00;
            NegativeFlag = value > 127;
        }

        private void INX()
        {
            X++;

            ZeroFlag = X == 0x00;
            NegativeFlag = X > 127;

        }

        private void INY()
        {
            Y++;

            ZeroFlag = Y == 0x00;
            NegativeFlag = Y > 127;
        }

        private void JMP()
        {
            PC = absoluteAddress;
        }

        private void JSR()
        {
            PC--;

            mmu.CPUWrite((ushort)(0x0100 + SP), (byte)((PC >> 8) & 0x00FF));
            SP--;
            mmu.CPUWrite((ushort)(0x0100 + SP), (byte)(PC & 0x00FF));
            SP--;

            PC = absoluteAddress;

        }

        private void LDA()
        {
            getData();
            A = (byte)fetchedData;

            ZeroFlag = A == 0x00;
            NegativeFlag = (A > 127);

        }

        private void LDX()
        {
            getData();
            X = (byte)fetchedData;

            ZeroFlag = X == 0x00;
            NegativeFlag = (X > 127);
        }

        private void LDY()
        {
            getData();
            Y = (byte)fetchedData;

            ZeroFlag = Y == 0x00;
            NegativeFlag = (Y > 127);
        }

        private void LSR()
        {
            getData();

            CarryFlag = (fetchedData & 0x0001) == 1;
            byte value = (byte)(fetchedData >> 1);
            ZeroFlag = value == 0x00;
            NegativeFlag = (value > 127);

            if (ILT[opcode].AddressingMode == IMP)
            {
                A = (byte)(value & 0x00FF);
            }
            else
            {
                mmu.CPUWrite(absoluteAddress, value & 0x00FF);
            }

        }

        private void NOP()
        {
            //does nothing
        }

        private void ORA()
        {
            getData();
            A = (byte)(A | fetchedData);
            ZeroFlag = A == 0x00;
            NegativeFlag = (A > 127);
        }

        private void PHA()
        {
            mmu.CPUWrite(0x0100 + SP, A);
            SP--;

        }

        private void PHP()
        {
            mmu.CPUWrite(0x0100 + SP, status);
            BreakFlag = false;
            UnusedFlag = false;
            SP--;
        }

        private void PLA()
        {
            SP++;
            A = mmu.CPURead(0x0100 + SP);
            ZeroFlag = A == 0x00;
            NegativeFlag = (A > 127);

        }

        private void PLP()
        {
            SP++;
            status = mmu.CPURead(0x0100 + SP);

        }

        private void ROL()
        {
            getData();

            ushort value = (ushort)((fetchedData << 1) | (CarryFlag ? 1 : 0));
            CarryFlag = value > 0x00FF;
            ZeroFlag = (value & 0x00FF) == 0x00;
            NegativeFlag = (value & 0x00FF) > 127;

            if (ILT[opcode].AddressingMode == IMP)
            {
                A = (byte)(value & 0x00FF);
            }
            else
            {
                mmu.CPUWrite(absoluteAddress, value & 0x00FF);
            }
        }

        private void ROR()
        {
            getData();
            ushort value = (ushort)(((CarryFlag ? 1 : 0) << 7) | (fetchedData >> 1));
            CarryFlag = (fetchedData & 0x01) != 0;
            ZeroFlag = (value & 0x00FF) == 0x00;
            NegativeFlag = (value & 0x00FF) > 127;

            if (ILT[opcode].AddressingMode == IMP)
            {
                A = (byte)(value & 0x00FF);
            }
            else
            {
                mmu.CPUWrite(absoluteAddress, value & 0x00FF);
            }
        }

        private void RTI()
        {
            SP++;
            status = mmu.CPURead(0x0100 + SP);

            SP++;
            PC = mmu.CPURead(0x0100 + SP);
            SP++;
            PC |= (ushort)(mmu.CPURead(0x0100 + SP) << 8);

        }

        private void RTS()
        {
            SP++;
            PC = mmu.CPURead(0x0100 + SP);
            SP++;
            PC |= (ushort)(mmu.CPURead(0x0100 + SP) << 8);

            PC++;
        }

        private void SBC()
        {
            getData();

            ushort v = (ushort)(fetchedData ^ 0x00FF);

            ushort value = (ushort)(A + v + (CarryFlag ? 1 : 0));
            CarryFlag = (value & 0xFF00) != 0;
            ZeroFlag = (value & 0x00FF) == 0x00;
            NegativeFlag = ((value & 0x00FF) > 127);
            OverflowFlag = ((value ^ A) & (value ^ v) & 0x0080) != 0;
            A = (byte)(value & 0x00FF);
        }

        private void SEC()
        {
            CarryFlag = true;
        }

        private void SED()
        {
            DecimalModeFlag = true;
        }

        private void SEI()
        {
            InterruptDisableFlag = true;
        }

        private void STA()
        {
            mmu.CPUWrite(absoluteAddress, A);
        }

        private void STX()
        {
            mmu.CPUWrite(absoluteAddress, X);
        }

        private void STY()
        {
            mmu.CPUWrite(absoluteAddress, Y);
        }

        private void TAX()
        {
            X = A;
            ZeroFlag = A == 0x00;
            NegativeFlag = A > 127;
        }

        private void TAY()
        {
            Y = A;
            ZeroFlag = Y == 0x00;
            NegativeFlag = Y > 127;
        }

        private void TSX()
        {
            X = SP;
            ZeroFlag = X == 0x00;
            NegativeFlag = X > 127;
        }

        private void TXA()
        {
            A = X;
            ZeroFlag = A == 0x00;
            NegativeFlag = A > 127;
        }

        private void TXS()
        {
            SP = X;
        }

        private void TYA()
        {
            A = Y;
            ZeroFlag = A == 0x00;
            NegativeFlag = A > 127;
        }

        // illegal opcodes

        // todo

    }
}
