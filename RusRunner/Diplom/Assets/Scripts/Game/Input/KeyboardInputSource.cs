using UnityEngine;

namespace RusRunner.Game.Input
{
    public sealed class KeyboardInputSource : IInputSource
    {
        public bool JumpPressed()
        {
            return UnityEngine.Input.GetKeyDown(KeyCode.Space) || UnityEngine.Input.GetKeyDown(KeyCode.UpArrow);
        }

        public bool SlidePressed()
        {
            return UnityEngine.Input.GetKeyDown(KeyCode.DownArrow) || UnityEngine.Input.GetKeyDown(KeyCode.S);
        }

        public bool DashPressed()
        {
            return UnityEngine.Input.GetKeyDown(KeyCode.LeftShift) || UnityEngine.Input.GetKeyDown(KeyCode.D);
        }
    }
}
