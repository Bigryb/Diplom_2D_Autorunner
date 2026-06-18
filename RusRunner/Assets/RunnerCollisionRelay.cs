using RusRunner.Game.Obstacles;
using UnityEngine;

namespace RusRunner.Game.Runner
{
    public sealed class RunnerCollisionRelay : MonoBehaviour
    {
        [SerializeField] private RunnerBootstrap runnerBootstrap;

        private void Awake()
        {
            if (runnerBootstrap == null)
            {
                runnerBootstrap = FindFirstObjectByType<RunnerBootstrap>();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (runnerBootstrap == null || !other.CompareTag("Obstacle"))
            {
                return;
            }

            var obstacle = other.GetComponent<Obstacle>();
            runnerBootstrap.HandleObstacleHit(obstacle);
        }
    }
}
