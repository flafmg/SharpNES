using SDL2;
using SharpNES.src.window;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SharpNES.src.SharpNES;

namespace SharpNES.src.hardware
{
    public class PPU
    {
        private MMU mmu;

        private byte control;
        private byte mask;
        private byte status;
        public byte OAMAddress;
        public ushort Scroll;
        public ushort Address;
        public byte Data;

        #region flags
        // control
        public byte BaseNameTableAddress
        {
            get { return (byte)(control & 0b00000011); }
            set { control = (byte)((control & 0b11111100) | (value & 0b00000011)); }
        }

        public bool VRAMAddressIncrement
        {
            get { return (control & 0b00000100) != 0; }
            set { control = (byte)((control & 0b11111011) | (value ? 0b00000100 : 0)); }
        }

        public byte SpritePatternTableAddress
        {
            get { return (byte)((control & 0b00001000) >> 3); }
            set { control = (byte)((control & 0b11110111) | ((value << 3) & 0b00001000)); }
        }

        public byte BackgroundPatternTableAddress
        {
            get { return (byte)((control & 0b00010000) >> 4); }
            set { control = (byte)((control & 0b11101111) | ((value << 4) & 0b00010000)); }
        }

        public bool SpriteSize
        {
            get { return (control & 0b00100000) != 0; }
            set { control = (byte)((control & 0b11011111) | (value ? 0b00100000 : 0)); }
        }

        public bool NMIEnabled
        {
            get { return (control & 0b10000000) != 0; }
            set { control = (byte)((control & 0b01111111) | (value ? 0b10000000 : 0)); }
        }

        // Mask
        public bool Grayscale
        {
            get { return (mask & 0b00000001) != 0; }
            set { mask = (byte)((mask & 0b11111110) | (value ? 0b00000001 : 0)); }
        }

        public bool ShowBackgroundLeftmost
        {
            get { return (mask & 0b00000010) != 0; }
            set { mask = (byte)((mask & 0b11111101) | (value ? 0b00000010 : 0)); }
        }

        public bool ShowSpritesLeftmost
        {
            get { return (mask & 0b00000100) != 0; }
            set { mask = (byte)((mask & 0b11111011) | (value ? 0b00000100 : 0)); }
        }

        public bool ShowBackground
        {
            get { return (mask & 0b00001000) != 0; }
            set { mask = (byte)((mask & 0b11110111) | (value ? 0b00001000 : 0)); }
        }

        public bool ShowSprites
        {
            get { return (mask & 0b00010000) != 0; }
            set { mask = (byte)((mask & 0b11101111) | (value ? 0b00010000 : 0)); }
        }

        public bool EmphasizeRed
        {
            get { return (mask & 0b00100000) != 0; }
            set { mask = (byte)((mask & 0b11011111) | (value ? 0b00100000 : 0)); }
        }

        public bool EmphasizeGreen
        {
            get { return (mask & 0b01000000) != 0; }
            set { mask = (byte)((mask & 0b10111111) | (value ? 0b01000000 : 0)); }
        }

        public bool EmphasizeBlue
        {
            get { return (mask & 0b10000000) != 0; }
            set { mask = (byte)((mask & 0b01111111) | (value ? 0b10000000 : 0)); }
        }

        // Status
        public bool SpriteOverflow
        {
            get { return (status & 0b00100000) != 0; }
            set { status = (byte)((status & 0b11011111) | (value ? 0b00100000 : 0)); }
        }

        public bool SpriteZeroHit
        {
            get { return (status & 0b01000000) != 0; }
            set { status = (byte)((status & 0b10111111) | (value ? 0b01000000 : 0)); }
        }

        public bool VerticalBlank
        {
            get { return (status & 0b10000000) != 0; }
            set { status = (byte)((status & 0b01111111) | (value ? 0b10000000 : 0)); }
        }

        #endregion


        private int scanLine;
        private int cycle;

        private uint[] output = new uint[256 * 240];

        private uint[] internalColorPalette = new uint[0x40];

        public byte[] nameTable = new byte[2048];
        public byte[] palette = new byte[32];

        private int vblankSize = 20;
        private int screenWidth = 256;
        private int screenHeight = 240;
        private int hblankSize = 85;

        public byte addrLatch = 0x00;
        private byte dataBuffer = 0x00;
        private ushort ppuadress = 0x0000; 

        private Canvas ppuVideo;
        private Canvas ppuDebug;

        bool debug = true;

        public bool isFrameCompleted { get; private set; }

        public PPU(MMU mmu)
        {
            this.mmu = mmu;

            ppuVideo = new Canvas(screenWidth, screenHeight);
            ppuVideo.x -= 64;
            renderer.AddCanvas(ppuVideo);

            if (debug)
            {
                ppuDebug = new Canvas(128, 260);
                ppuDebug.x += 128+16+8;
                renderer.AddCanvas(ppuDebug);
            }

            internalColorPalette[0x00] = rgbToHex(84, 84, 84);
            internalColorPalette[0x01] = rgbToHex(0, 30, 116);
            internalColorPalette[0x02] = rgbToHex(8, 16, 144);
            internalColorPalette[0x03] = rgbToHex(48, 0, 136);
            internalColorPalette[0x04] = rgbToHex(68, 0, 100);
            internalColorPalette[0x05] = rgbToHex(92, 0, 48);
            internalColorPalette[0x06] = rgbToHex(84, 4, 0);
            internalColorPalette[0x07] = rgbToHex(60, 24, 0);
            internalColorPalette[0x08] = rgbToHex(32, 42, 0);
            internalColorPalette[0x09] = rgbToHex(8, 58, 0);
            internalColorPalette[0x0A] = rgbToHex(0, 64, 0);
            internalColorPalette[0x0B] = rgbToHex(0, 60, 0);
            internalColorPalette[0x0C] = rgbToHex(0, 50, 60);
            internalColorPalette[0x0D] = rgbToHex(0, 0, 0);
            internalColorPalette[0x0E] = rgbToHex(0, 0, 0);
            internalColorPalette[0x0F] = rgbToHex(0, 0, 0);

            internalColorPalette[0x10] = rgbToHex(152, 150, 152);
            internalColorPalette[0x11] = rgbToHex(8, 76, 196);
            internalColorPalette[0x12] = rgbToHex(48, 50, 236);
            internalColorPalette[0x13] = rgbToHex(92, 30, 228);
            internalColorPalette[0x14] = rgbToHex(136, 20, 176);
            internalColorPalette[0x15] = rgbToHex(160, 20, 100);
            internalColorPalette[0x16] = rgbToHex(152, 34, 32);
            internalColorPalette[0x17] = rgbToHex(120, 60, 0);
            internalColorPalette[0x18] = rgbToHex(84, 90, 0);
            internalColorPalette[0x19] = rgbToHex(40, 114, 0);
            internalColorPalette[0x1A] = rgbToHex(8, 124, 0);
            internalColorPalette[0x1B] = rgbToHex(0, 118, 40);
            internalColorPalette[0x1C] = rgbToHex(0, 102, 120);
            internalColorPalette[0x1D] = rgbToHex(0, 0, 0);
            internalColorPalette[0x1E] = rgbToHex(0, 0, 0);
            internalColorPalette[0x1F] = rgbToHex(0, 0, 0);

            internalColorPalette[0x20] = rgbToHex(236, 238, 236);
            internalColorPalette[0x21] = rgbToHex(76, 154, 236);
            internalColorPalette[0x22] = rgbToHex(120, 124, 236);
            internalColorPalette[0x23] = rgbToHex(176, 98, 236);
            internalColorPalette[0x24] = rgbToHex(228, 84, 236);
            internalColorPalette[0x25] = rgbToHex(236, 88, 180);
            internalColorPalette[0x26] = rgbToHex(236, 106, 100);
            internalColorPalette[0x27] = rgbToHex(212, 136, 32);
            internalColorPalette[0x28] = rgbToHex(160, 170, 0);
            internalColorPalette[0x29] = rgbToHex(116, 196, 0);
            internalColorPalette[0x2A] = rgbToHex(76, 208, 32);
            internalColorPalette[0x2B] = rgbToHex(56, 204, 108);
            internalColorPalette[0x2C] = rgbToHex(56, 180, 204);
            internalColorPalette[0x2D] = rgbToHex(60, 60, 60);
            internalColorPalette[0x2E] = rgbToHex(0, 0, 0);
            internalColorPalette[0x2F] = rgbToHex(0, 0, 0);

            internalColorPalette[0x30] = rgbToHex(236, 238, 236);
            internalColorPalette[0x31] = rgbToHex(168, 204, 236);
            internalColorPalette[0x32] = rgbToHex(188, 188, 236);
            internalColorPalette[0x33] = rgbToHex(212, 178, 236);
            internalColorPalette[0x34] = rgbToHex(236, 174, 236);
            internalColorPalette[0x35] = rgbToHex(236, 174, 212);
            internalColorPalette[0x36] = rgbToHex(236, 180, 176);
            internalColorPalette[0x37] = rgbToHex(228, 196, 144);
            internalColorPalette[0x38] = rgbToHex(204, 210, 120);
            internalColorPalette[0x39] = rgbToHex(180, 222, 120);
            internalColorPalette[0x3A] = rgbToHex(168, 226, 144);
            internalColorPalette[0x3B] = rgbToHex(152, 226, 180);
            internalColorPalette[0x3C] = rgbToHex(160, 214, 228);
            internalColorPalette[0x3D] = rgbToHex(160, 162, 160);
            internalColorPalette[0x3E] = rgbToHex(0, 0, 0);
            internalColorPalette[0x3F] = rgbToHex(0, 0, 0);

        }

        public void Cycle()
        {
            
            isFrameCompleted = false;
            if (scanLine < screenHeight && scanLine >= 0 && cycle < screenWidth)
            {
                uint color = internalColorPalette[(scanLine+cycle) % 0x40];
                output[scanLine * screenWidth + cycle] = color;
                
            }

            cycle++;
            if (cycle >= (screenWidth + hblankSize))
            {
                cycle = 0;
                scanLine++;

                if (scanLine >= (screenHeight + vblankSize))
                {
                    scanLine = -1;
                    isFrameCompleted = true;

                    if (debug)
                    {
                       
                        DrawCHRTable(0, 0, 0);
                        DrawCHRTable(1, 0, 132);
                        DrawPalette();
                    }
                    ppuVideo.Pixels = output;
                }
            }
            if(scanLine > screenWidth)
            {
                VerticalBlank = true;
              
            }

 
        }
        public byte Read(ushort addr)
        {
            byte data = 0x00;
            switch (addr)
            {
                case 0x0000:
                    break;
                case 0x0001:
                    break;
                case 0x0002:
                    data = (byte)((status & 0xE0) | (dataBuffer & 0x1F));
                    VerticalBlank = false;
                    addrLatch = 0;
                    break;
                case 0x0003:
                    break;
                case 0x0004:
                    break;
                case 0x0005:
                    break;
                case 0x0006:
                    break;
                case 0x0007:
                    data = dataBuffer;
                    dataBuffer = mmu.PPURead(ppuadress);

                    if (ppuadress > 0x3f00)
                    {
                        data = dataBuffer;
                    }
                    break;
            }
            return data;
        }
        public void Write(ushort addr, byte data)
        {
            switch (addr)
            {
                case 0x0000: //c
                    control = data;
                    break;
                case 0x0001: //m
                    mask = data;
                    break;
                case 0x0002: //s
                    //cant write
                    break;
                case 0x0003: //qam addr
                    break;
                case 0x0004: //qam dat
                    break;
                case 0x0005: //scr
                    break;
                case 0x0006: //ppu addr
                    if(addrLatch == 0)
                    {
                        ppuadress = (ushort)((ppuadress & 0x00FF) | (data << 8));
                        addrLatch = 1;
                    }
                    else
                    {
                        ppuadress = (ushort)((ppuadress & 0xFF00) | data);
                        addrLatch = 0;
                    }
                    break;
                case 0x0007: //ppu dat 
                    mmu.PPUWrite(ppuadress, data);
                    ppuadress++;
                    break;
            }
        }

        public void DrawCHRTable(byte i, byte pallete, ushort yOffset)
        {
            for (ushort nTileY = 0; nTileY < 16; nTileY++)
            {
                for (ushort nTileX = 0; nTileX < 16; nTileX++)
                {
                    ushort nOffset = (ushort)(nTileY * 256 + nTileX * 16);
                    for (ushort row = 0; row < 8; row++)
                    {
                        byte tilel = mmu.PPURead(i * 0x1000 + nOffset + row);
                        byte tileh = mmu.PPURead(i * 0x1000 + nOffset + row + 8);

                        for (ushort col = 0; col < 8; col++)
                        {
                            byte pixel = (byte)((tilel & 0x01) + (tileh & 0x01));
                            tileh >>= 1;
                            tilel >>= 1;
                            uint color = getColor(pallete, pixel);
                            ppuDebug.Pixels[(nTileX * 8 + (7 - col)) + ((nTileY * 8 + row + yOffset) * 128)] = color;
                        }
                    }
                }
            }
        }
        public void DrawPalette()
        {
            ushort xOffset = 0;
            ushort yOffset = 128;
            ushort colorBoxSize = 3;
            ushort paletteStride = 32;
            ushort chunkSeparation = 1;

            for (int i = 0; i < 32; i++)
            {
                byte colorIndex = palette[i];
                uint color = internalColorPalette[colorIndex];

                ushort chunkIndex = (ushort)(i / 4);
                ushort chunkXOffset = (ushort)(xOffset + (chunkIndex * (colorBoxSize * 4 + chunkSeparation)));
                ushort x = (ushort)(chunkXOffset + ((i % paletteStride) % 4) * (colorBoxSize )); 

                ushort y = (ushort)(yOffset); 

                for (ushort dy = 0; dy < colorBoxSize; dy++)
                {
                    for (ushort dx = 0; dx < colorBoxSize; dx++)
                    {
                        ppuDebug.Pixels[(x + dx) + (y + dy) * 128] = color;
                    }
                }
            }
        }



        private uint getColor(byte pallete, byte pixel)
        {
            return internalColorPalette[mmu.PPURead(0x3F00 + (pallete << 2) + pixel)];
        }
        private uint rgbToHex(byte red, byte green, byte blue)
        {
            return (uint)((red << 24) | (green << 16) | (blue << 8) | 0xFF);
        }
    }
}
