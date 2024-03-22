using SDL2;
using SharpNES.src.hardware;
using SharpNES.src.window;

namespace SharpNES.src
{
    internal class SharpNES
    {
        private Window window;
        private Input input;
        private Renderer renderer;

        private NES nes;

        private bool running;

        public SharpNES()
        {


            if (SDL.SDL_Init(SDL.SDL_INIT_VIDEO | SDL.SDL_INIT_AUDIO) < 0)
            {
                throw new Exception($"Failed to initialize SDL: {SDL.SDL_GetError()}");
            }
            if (SDL_image.IMG_Init(SDL_image.IMG_InitFlags.IMG_INIT_PNG) < 0)
            {
                throw new Exception($"Failed to initialize SDL_Image: {SDL.SDL_GetError()}");
            }
            if (SDL_ttf.TTF_Init() < 0)
            {
                throw new Exception($"Failed to initialize SDL_ttf: {SDL.SDL_GetError()}");
            }
            if (SDL_mixer.Mix_OpenAudio(44100, SDL_mixer.MIX_DEFAULT_FORMAT, 2, 2048) < 0)
            {
                throw new Exception($"Failed to initialize SDL_mixer: {SDL.SDL_GetError()}");
            }

            window = new Window("SharpNes - waiting rom", 480, 640, true);
            input = new Input();
            renderer = new Renderer(window, 240, 256);

            ROM rom = new ROM("nestest.nes");
            nes = new NES(renderer, rom);
            nes.Resume();
            Run();
        }
        public void Run()
        {
            running = true;
            while (running)
            {
                while (SDL.SDL_PollEvent(out SDL.SDL_Event sdlEvent) != 0)
                {
                    if (sdlEvent.type == SDL.SDL_EventType.SDL_QUIT)
                    {
                        Exit();
                        if (!running) return; 
                    }

                    input.HandleEvent(sdlEvent);
                }

                nes.Run();
            }

            Exit();
        }
        public void Exit()
        {
            running = false;

            renderer.Destroy();
            window.Destroy();

            SDL_mixer.Mix_CloseAudio();
            SDL_mixer.Mix_Quit();
            SDL_ttf.TTF_Quit();
            SDL_image.IMG_Quit();
            SDL.SDL_Quit();
        }
        public static void Main(String[] args)
        {
            new SharpNES();
        }

    }
}
