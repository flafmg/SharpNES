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

        private const double NTSC_FPS = 60.0;
        private const double PAL_FPS = 50.0;
        private const double NTSC_FRAME_TIME = 1000.0 / NTSC_FPS; 
        private const double PAL_FRAME_TIME = 1000.0 / PAL_FPS;

        private const double CPU_FREQUENCY = 1789773;

        private double frameTime;
        private double cpuInstructionsPerFrame;

        private Renderer renderer;

        public NES(Renderer renderer, ROM rom)
        {
            this.renderer = renderer;
 
            mmu = new MMU(rom);
            cpu = new CPU(mmu);
            ppu = new PPU();
            this.region = rom.region == 0 ? ConsoleRegion.NTSC : ConsoleRegion.PAL;

            frameTime = (region == ConsoleRegion.NTSC) ? NTSC_FRAME_TIME : PAL_FRAME_TIME;
            cpuInstructionsPerFrame = CPU_FREQUENCY / (region == ConsoleRegion.NTSC ? NTSC_FPS : PAL_FPS);

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

        }

        public void Run()
        {
           
            if (running)
            {
                double startTime = GetCurrentTime();

                for (int i = 0; i < cpuInstructionsPerFrame; i++)
                {
                    cpu.cycle();
                }

                renderer.CopyPixelsTexture();
                renderer.Present();

                double elapsedTime = GetCurrentTime() - startTime;
                double remainingTime = frameTime - elapsedTime;
                if (remainingTime > 0)
                {
                    int delayMilliseconds = (int)Math.Floor(remainingTime);
                    Thread.Sleep(delayMilliseconds);
                }
            }
        }

        private double GetCurrentTime()
        {
            return DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        }
    }
}
