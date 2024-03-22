using SDL2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpNES.src.window
{
    public class Input
    {
        private List<SDL.SDL_Scancode> keyPressed = new List<SDL.SDL_Scancode>();
        private List<SDL.SDL_Scancode> keyHold = new List<SDL.SDL_Scancode>();

        public void HandleEvent(SDL.SDL_Event sdlEvent)
        {
            var scanCode = sdlEvent.key.keysym.scancode;

            if (sdlEvent.type == SDL.SDL_EventType.SDL_KEYDOWN)
            {
                if (!keyPressed.Contains(scanCode))
                {
                    keyPressed.Add(scanCode);
                }
            }
            else if (sdlEvent.type == SDL.SDL_EventType.SDL_KEYUP)
            {
                if (keyPressed.Contains(scanCode))
                {
                    keyPressed.Remove(scanCode);
                    keyHold.Remove(scanCode);
                }
            }
        }

        public bool IsKeyPressed(SDL.SDL_Scancode scanCode)
        {

            return keyPressed.Contains(scanCode);
        }

        public bool IsKeyClicked(SDL.SDL_Scancode scanCode)
        {
            if (keyPressed.Contains(scanCode) && !keyHold.Contains(scanCode))
            {
                keyHold.Add(scanCode);
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
