using SDL2;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace SharpNES.src.window
{
    public class Renderer
    {
        private static nint renderer;
        private List<Canvas> canvases = new List<Canvas>();
        private Window window;

        public Renderer(Window window)
        {
            this.window = window;
            renderer = SDL.SDL_CreateRenderer(window.GetWindowPtr(), -1, SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED);
            if (renderer == nint.Zero)
            {
                window.ShowMessageBox("SDL Renderer Creation Error", $"Failed to create SDL renderer: {SDL.SDL_GetError()}", SDL.SDL_MessageBoxFlags.SDL_MESSAGEBOX_ERROR);
                return;
            }
        }

        public static nint GetRendererPtr()
        {
            return renderer;
        }

        public void AddCanvas(Canvas canvas)
        {
            canvases.Add(canvas);
        }

        public void RemoveCanvas(Canvas canvas)
        {
            canvases.Remove(canvas);
        }

        public void Render()
        {
            SDL.SDL_SetRenderDrawColor(renderer, 0, 0, 0, 0);
            SDL.SDL_RenderClear(renderer);

            foreach (var canvas in canvases)
            {
                canvas.Render();

                float scaleX = (float)window.Width / canvas.Width;
                float scaleY = (float)window.Height / canvas.Height;
                float translatedScale = Math.Min(scaleX, scaleY);

                int translatedX = (int)((window.Width - canvas.Width * translatedScale) / 2);
                int translatedY = (int)((window.Height - canvas.Height * translatedScale) / 2);

                SDL.SDL_Rect destRect = new SDL.SDL_Rect()
                {
                    x = (int)(translatedX + canvas.x * translatedScale),
                    y = (int)(translatedY + canvas.y * translatedScale),
                    w = (int)(canvas.Width * translatedScale),
                    h = (int)(canvas.Height * translatedScale)
                };
                SDL.SDL_RenderCopy(Renderer.GetRendererPtr(), canvas.GetTexture(), IntPtr.Zero, ref destRect);
            }

            SDL.SDL_RenderPresent(renderer);
        }

        public (int x, int y) GetAutoSizedPos(Canvas canvas)
        {
            if (!canvas.AutoSize)
            {
                return (canvas.x, canvas.y);
            }

            float scaleX = (float)window.Width / canvas.Width;
            float scaleY = (float)window.Height / canvas.Height;
            float scale = Math.Min(scaleX, scaleY);

            int translatedX = (int)((window.Width - canvas.Width * scale) / 2);
            int translatedY = (int)((window.Height - canvas.Height * scale) / 2);

            return (translatedX, translatedY);
        }

        public float GetAutoSizedScale(Canvas canvas)
        {
            if (!canvas.AutoSize)
            {
                return canvas.Scale;
            }

            float scaleX = (float)window.Width / canvas.Width;
            float scaleY = (float)window.Height / canvas.Height;
            float scale = Math.Min(scaleX, scaleY);

            return scale * canvas.Scale;
        }

        public void Destroy()
        {
            foreach (var canvas in canvases)
            {
                canvas.Destroy();
            }

            SDL.SDL_DestroyRenderer(renderer);
        }
    }
}
