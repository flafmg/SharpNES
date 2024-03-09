using System;
using System.Collections.Generic;

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

        private bool CarryFlag;
        private bool ZeroFlag;
        private bool InterruptDisableFlag;
        private bool DecimalModeFlag;
        private bool OverflowFlag;
        private bool NegativeFlag;

        private byte FetchedData;
        private ushort AbsoluteAddress;
        private ushort RelativeAddress;

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

            FetchedData = 0x0;
            AbsoluteAddress = 0x0;
            RelativeAddress = 0x0;
        }

        public struct Instruction
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


        private void ABS()
        {
            AbsoluteAddress = mmu.Read(PC);
            PC++;
            AbsoluteAddress |= (ushort)(mmu.Read(PC) << 8);
            PC++;
        }

        private void ABX()
        {
            AbsoluteAddress = (ushort)(mmu.Read(PC) | (mmu.Read((ushort)(PC + 1)) << 8));
            AbsoluteAddress += (ushort)X;
            PC += 2;
        }

        private void ABY()
        {
            AbsoluteAddress = (ushort)(mmu.Read(PC) | (mmu.Read((ushort)(PC + 1)) << 8));
            AbsoluteAddress += (ushort)Y;
            PC += 2;
        }

        private void IMM()
        {
            AbsoluteAddress = PC++;
        }

        private void IMP()
        {
            // Não faz nada
        }

        private void IND()
        {
            ushort ptr = mmu.Read(PC);
            PC++;
            ushort lo = mmu.Read(ptr);
            ushort hi = mmu.Read((ushort)(ptr + 1));

            AbsoluteAddress = (ushort)((hi << 8) | lo);
        }

        private void IDX()
        {
            ushort ptr = (ushort)(mmu.Read(PC) + X);
            PC++;
            ushort lo = mmu.Read(ptr);
            ushort hi = mmu.Read((ushort)(ptr + 1));

            AbsoluteAddress = (ushort)((hi << 8) | lo);
        }

        private void IDY()
        {
            ushort ptr = mmu.Read(PC);
            PC++;
            ushort lo = mmu.Read(ptr);
            ushort hi = mmu.Read((ushort)(ptr + 1));

            AbsoluteAddress = (ushort)((hi << 8) | lo);
            AbsoluteAddress += (ushort)Y;
        }

        private void REL()
        {
            RelativeAddress = mmu.Read(PC);
            PC++;
        }

        private void ZP()
        {
            AbsoluteAddress = mmu.Read(PC);
            PC++;
        }

        private void ZPX()
        {
            AbsoluteAddress = (ushort)(mmu.Read(PC) + X);
            PC++;
        }

        private void ZPY()
        {
            AbsoluteAddress = (ushort)(mmu.Read(PC) + Y);
            PC++;
        }


        private void ADC()
        {
        }

        private void AND()
        {
        }

        private void ASL()
        {
        }

        private void BCC()
        {
        }

        private void BCS()
        {
        }

        private void BEQ()
        {
        }

        private void BIT()
        {
        }

        private void BMI()
        {
        }

        private void BNE()
        {
        }

        private void BPL()
        {
        }

        private void BRK()
        {
        }

        private void BVC()
        {
        }

        private void BVS()
        {
        }

        private void CLC()
        {
        }

        private void CLD()
        {
        }

        private void CLI()
        {
        }

        private void CLV()
        {
        }

        private void CMP()
        {
        }

        private void CPX()
        {
        }

        private void CPY()
        {
        }

        private void DEC()
        {
        }

        private void DEX()
        {
        }

        private void DEY()
        {
        }

        private void EOR()
        {
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
