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
            return UnityEngine.Input.GetKey(KeyCode.DownArrow) || UnityEngine.Input.GetKey(KeyCode.S);
        }

        public bool DashPressed()
        {
            return false;
        }
    }
}
