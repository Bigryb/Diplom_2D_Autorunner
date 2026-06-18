using UnityEngine;

namespace RusRunner.Game.Input
{
    public sealed class MobileInputSource : IInputSource
    {
        public bool JumpPressed()
        {
            return DetectSwipe(Vector2.up);
        }

        public bool SlidePressed()
        {
            return DetectSwipe(Vector2.down);
        }

        public bool DashPressed()
        {
            return false;
        }

        private static bool DetectSwipe(Vector2 direction)
        {
            if (UnityEngine.Input.touchCount == 0)
            {
                return false;
            }

            var touch = UnityEngine.Input.GetTouch(0);
            if (touch.phase != TouchPhase.Ended)
            {
                return false;
            }

            var delta = touch.position - touch.rawPosition;
            return delta.sqrMagnitude > 3600f && Vector2.Dot(delta.normalized, direction) > 0.7f;
        }
    }
}
