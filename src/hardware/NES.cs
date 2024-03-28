using SharpNES.src.window;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SharpNES.src.hardware
{
    internal enum ConsoleRegion
    {
        NTSC,
        PAL
    }

    internal class NES
    {
        private bool running;

        private CPU cpu;
        private PPU ppu;
        private MMU mmu;
        private ConsoleRegion region;


        public NES(ROM rom)
        {
            mmu = new MMU(rom);
            cpu = new CPU(mmu);
            ppu = new PPU(mmu);
            mmu.addPPU(ppu);

            this.region = rom.region == 0 ? ConsoleRegion.NTSC : ConsoleRegion.PAL;

            Reset();

            SharpNES.window.SetWindowTitle($"SharpNES - {rom.romName}");
        }

        public void Resume()
        {
            running = true;
        }
        public void Pause()
        {
            running = false;
        }
        public void Reset()
        {
            cpu.Reset();
        }
        int globalCycleCount = 0;
        public void Run()
        {

            if (running)
            {
                Cycle();
                

                if (SharpNES.input.IsKeyClicked(SDL2.SDL.SDL_Scancode.SDL_SCANCODE_SPACE))
                {
                    running = false;
                }
            }
            else
            {
                if (SharpNES.input.IsKeyClicked(SDL2.SDL.SDL_Scancode.SDL_SCANCODE_SPACE))
                {
                    running = true;
                }
                else if (SharpNES.input.IsKeyClicked(SDL2.SDL.SDL_Scancode.SDL_SCANCODE_C))
                {
                    Cycle();
                }
                else if (SharpNES.input.IsKeyClicked(SDL2.SDL.SDL_Scancode.SDL_SCANCODE_F))
                {
                    Cycle();
                    while (!ppu.isFrameCompleted)
                    {
                        Cycle();
                    }
                    
                }
            }

            if (ppu.isFrameCompleted)
            {
              SharpNES.renderer.Render();
               
            }
        }
        
        private void Cycle()
        {
            ppu.Cycle();
            if (globalCycleCount % 3 == 1)
                cpu.Cycle();

            globalCycleCount++;
        }
    }
}
