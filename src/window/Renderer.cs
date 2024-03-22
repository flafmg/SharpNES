using SDL2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpNES.src.window
{
    public class Renderer
    {
        private static nint renderer;
        private nint texture;
        public uint[] Pixels;

        public static int Height, Width;
        public Renderer(Window window, int rendererHeight, int rendererWidth)
        {
            Height = rendererHeight;
            Width = rendererWidth;

            renderer = SDL.SDL_CreateRenderer(window.GetWindowPtr(), -1, SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED);

            if (renderer == nint.Zero)
            {
                window.ShowMessageBox("SDL Renderer Creation Error", $"Failed to create SDL renderer: {SDL.SDL_GetError()}", SDL.SDL_MessageBoxFlags.SDL_MESSAGEBOX_ERROR);
                return;
            }

            Pixels = new uint[rendererHeight * rendererWidth];
            texture = SDL.SDL_CreateTexture(renderer, SDL.SDL_PIXELFORMAT_RGBA8888, (int)SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_STREAMING, rendererWidth, rendererHeight);
            if (texture == nint.Zero)
            {
                window.ShowMessageBox("SDL Texture Creation Error", $"Failed to create texture: {SDL.SDL_GetError()}", SDL.SDL_MessageBoxFlags.SDL_MESSAGEBOX_ERROR);
                return;
            }

            SDL.SDL_RenderSetLogicalSize(renderer, rendererWidth, rendererHeight);
        }

        public void Clear()
        {
            SDL.SDL_SetRenderDrawColor(renderer, 0, 0, 0, 0);
            SDL.SDL_RenderClear(renderer);
        }
        public void CopyPixelsTexture()
        {
            UpdateTexture();
            SDL.SDL_RenderCopyEx(renderer, texture, nint.Zero, nint.Zero, 0.0, nint.Zero, SDL.SDL_RendererFlip.SDL_FLIP_NONE);
        }
        public void Present()
        {
            SDL.SDL_RenderPresent(renderer);
        }

        private void UpdateTexture()
        {
            nint pixelsPtr;
            int pitch;
            SDL.SDL_LockTexture(texture, nint.Zero, out pixelsPtr, out pitch);

            unsafe
            {
                uint* texturePixels = (uint*)pixelsPtr;
                for (int i = 0; i < Pixels.Length; i++)
                {
                    texturePixels[i] = Pixels[i];
                    Pixels[i] = 0;
                }
            }

            SDL.SDL_UnlockTexture(texture);
        }

        public static nint GetRendererPtr()
        {
            return renderer;
        }

        public void Destroy()
        {
            SDL.SDL_DestroyRenderer(renderer);
            SDL.SDL_DestroyTexture(texture);
        }
    }
}
