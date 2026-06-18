using UnityEngine;

namespace RusRunner.Game.Obstacles
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class Obstacle : MonoBehaviour
    {
        [SerializeField] private bool disableOnHit = true;

        private void Reset()
        {
            gameObject.tag = "Obstacle";
            var c = GetComponent<Collider2D>();
            c.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!disableOnHit || !other.CompareTag("Player"))
            {
                return;
            }

            gameObject.SetActive(false);
        }
    }
}
