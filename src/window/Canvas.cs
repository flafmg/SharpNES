using SDL2;
using System;
using System.Numerics;

namespace SharpNES.src.window
{
    public class Canvas
    {
        private nint texture;
        public uint[] Pixels;
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int x { get; set; }
        public int y { get; set; }
        public bool AutoSize { get; set; }
        public float Scale { get; set; }

        public Canvas(int width, int height)
        {
            Width = width;
            Height = height;
            Pixels = new uint[width * height];
            AutoSize = true;
            Scale = 1.0f;
            CreateTexture();
        }

        private void CreateTexture()
        {
            texture = SDL.SDL_CreateTexture(Renderer.GetRendererPtr(), SDL.SDL_PIXELFORMAT_RGBA8888, (int)SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_STREAMING, Width, Height);
            if (texture == nint.Zero)
            {
                throw new Exception($"Failed to create texture: {SDL.SDL_GetError()}");
            }
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

        public void Clear()
        {
            Array.Clear(Pixels, 0, Pixels.Length);
            UpdateTexture();
        }

        public void Render()
        {
            UpdateTexture();
           
        }
        public nint GetTexture()
        {
            return texture;
        }
        public void Destroy()
        {
            SDL.SDL_DestroyTexture(texture);
        }
    }
}
