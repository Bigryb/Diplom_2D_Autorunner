using UnityEngine;

namespace RusRunner.Game.Runner
{
    public sealed class RunnerCollisionRelay : MonoBehaviour
    {
        [SerializeField] private RunnerBootstrap runnerBootstrap;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (runnerBootstrap == null)
            {
                return;
            }

            if (!other.CompareTag("Obstacle"))
            {
                return;
            }

            runnerBootstrap.HandleObstacleHit();
        }
    }
}
