using RusRunner.Core;
using RusRunner.Core.Contracts;

namespace RusRunner.Tests.EditMode.Core.Systems
{
    internal sealed class SpyAbility : IAbility
    {
        private readonly bool _canActivate;

        public SpyAbility(string id, bool canActivate)
        {
            Id = id;
            _canActivate = canActivate;
        }

        public string Id { get; }
        public int ActivateCallCount { get; private set; }
        public int TickCallCount { get; private set; }
        public int DeactivateCallCount { get; private set; }
        public float LastDeltaTime { get; private set; }

        public bool CanActivate(GameContext context)
        {
            return _canActivate;
        }

        public void Activate(GameContext context)
        {
            ActivateCallCount++;
        }

        public void Tick(GameContext context, float deltaTime)
        {
            TickCallCount++;
            LastDeltaTime = deltaTime;
        }

        public void Deactivate(GameContext context)
        {
            DeactivateCallCount++;
        }
    }

    internal sealed class SpyPowerUp : IPowerUp
    {
        public SpyPowerUp(float durationSeconds)
        {
            DurationSeconds = durationSeconds;
        }

        public string Id => "spy_power_up";
        public float DurationSeconds { get; }
        public int ApplyCallCount { get; private set; }
        public int ExpireCallCount { get; private set; }

        public void Apply(GameContext context)
        {
            ApplyCallCount++;
        }

        public void Expire(GameContext context)
        {
            ExpireCallCount++;
        }
    }
}
