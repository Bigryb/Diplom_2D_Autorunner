using UnityEngine;

namespace RusRunner.Game.Obstacles
{
    public enum ObstacleType
    {
        Jump = 0,
        Slide = 1,
        Air = 2
    }

    [RequireComponent(typeof(Collider2D))]
    public sealed class Obstacle : MonoBehaviour
    {
        [SerializeField] private ObstacleType obstacleType = ObstacleType.Jump;

        public ObstacleType Type => obstacleType;

        public void Configure(ObstacleType type)
        {
            obstacleType = type;
            gameObject.tag = "Obstacle";
            var c = GetComponent<Collider2D>();
            if (c != null)
            {
                c.isTrigger = true;
            }
        }

        public void Deactivate()
        {
            gameObject.SetActive(false);
        }

        private void Awake()
        {
            Configure(obstacleType);
        }
    }
}
