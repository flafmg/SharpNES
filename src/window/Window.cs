using SDL2;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace SharpNES.src.window
{
    public class Window
    {
        public static int Width;
        public static int Height;

        private nint windowPtr;
        private bool isFullscreen = false;


        public Window(string title, int windowHeight, int windowWidth, bool resizable)
        {
            SDL.SDL_WindowFlags flags = SDL.SDL_WindowFlags.SDL_WINDOW_SHOWN;
            if (resizable)
            {
                flags |= SDL.SDL_WindowFlags.SDL_WINDOW_RESIZABLE;
            }

            windowPtr = SDL.SDL_CreateWindow(title, SDL.SDL_WINDOWPOS_UNDEFINED, SDL.SDL_WINDOWPOS_UNDEFINED, windowWidth, windowHeight, flags);

            if (windowPtr == nint.Zero)
            {
                ShowMessageBox("SDL Window Creation Error", $"Failed to create SDL window: {SDL.SDL_GetError()}", SDL.SDL_MessageBoxFlags.SDL_MESSAGEBOX_ERROR);
                return;
            }

            Width = windowWidth;
            Height = windowHeight;
        }

        public void ShowMessageBox(string title, string message, SDL.SDL_MessageBoxFlags MBFlags)
        {
            SDL.SDL_ShowSimpleMessageBox(MBFlags, title, message, nint.Zero);
        }

        public void SetFullscreen(bool fullscreen)
        {
            isFullscreen = fullscreen;
            SDL.SDL_SetWindowFullscreen(windowPtr, isFullscreen ? (uint)SDL.SDL_WindowFlags.SDL_WINDOW_FULLSCREEN_DESKTOP : 0);
            UpdateWindowSize();
        }

        public void SetWindowTitle(string title)
        {
            SDL.SDL_SetWindowTitle(windowPtr, title);
        }

        public void SetWindowPosition(int x, int y)
        {
            SDL.SDL_SetWindowPosition(windowPtr, x, y);
            UpdateWindowSize();
        }

        public void SetWindowSize(int width, int height)
        {
            SDL.SDL_SetWindowSize(windowPtr, width, height);
            UpdateWindowSize();
        }

        private void UpdateWindowSize()
        {
            SDL.SDL_GetWindowSize(windowPtr, out int width, out int height);
            Width = width;
            Height = height;
        }

        public void ToggleFullscreen()
        {
            SetFullscreen(!isFullscreen);
            UpdateWindowSize();
        }

        public nint GetWindowPtr()
        {
            return windowPtr;
        }

        public void Destroy()
        {
            SDL.SDL_DestroyWindow(windowPtr);
        }
    }

}
