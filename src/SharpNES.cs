using SDL2;
using SharpNES.src.hardware;
using SharpNES.src.window;

namespace SharpNES.src
{
    internal class SharpNES
    {
        public static Window window {  get; private set; }
        public static Input input { get; private set; }
        public static Renderer renderer {  get; private set; }

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

            window = new Window("SharpNes - waiting rom", 480, 840, true);
            input = new Input();
            renderer = new Renderer(window);

            ROM rom = new ROM("nestest.nes");
            nes = new NES( rom);
            nes.Resume();
            Run();
        }
        public void Run()
        {
            running = true;
            while (running)
            {
                while (window.PollEvent(out SDL.SDL_Event sdlEvent))
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
