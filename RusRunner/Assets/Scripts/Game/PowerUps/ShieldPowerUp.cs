using RusRunner.Core;
using RusRunner.Core.Contracts;

namespace RusRunner.Game.PowerUps
{
    public sealed class ShieldPowerUp : IPowerUp
    {
        public string Id => "shield";
        public float DurationSeconds { get; }

        public ShieldPowerUp(float durationSeconds)
        {
            DurationSeconds = durationSeconds;
        }

        public void Apply(GameContext context)
        {
            context.IsInvulnerable = true;
        }

        public void Expire(GameContext context)
        {
            context.IsInvulnerable = false;
        }
    }
}
