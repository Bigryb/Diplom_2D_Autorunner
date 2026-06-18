using RusRunner.Game.Runner;
using UnityEngine;

namespace RusRunner.Game.Pickups
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class PowerUpPickup : MonoBehaviour
    {
        [SerializeField] private PickupType pickupType;

        public PickupType Type => pickupType;

        public void Configure(PickupType type)
        {
            pickupType = type;
            var collider = GetComponent<Collider2D>();
            if (collider != null)
            {
                collider.isTrigger = true;
            }
        }

        public void Deactivate()
        {
            gameObject.SetActive(false);
        }

        private void Awake()
        {
            Configure(pickupType);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player"))
            {
                return;
            }

            var bootstrap = other.GetComponent<RunnerBootstrap>();
            if (bootstrap == null)
            {
                bootstrap = FindFirstObjectByType<RunnerBootstrap>();
            }

            bootstrap?.CollectPickup(pickupType);
            gameObject.SetActive(false);
        }
    }
}
