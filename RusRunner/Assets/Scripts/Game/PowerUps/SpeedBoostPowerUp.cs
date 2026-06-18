using RusRunner.Core;
using RusRunner.Core.Contracts;

namespace RusRunner.Game.PowerUps
{
    public sealed class SpeedBoostPowerUp : IPowerUp
    {
        private readonly float _multiplierBonus;

        public string Id => "speed_boost";
        public float DurationSeconds { get; }

        public SpeedBoostPowerUp(float durationSeconds, float multiplierBonus)
        {
            DurationSeconds = durationSeconds;
            _multiplierBonus = multiplierBonus;
        }

        public void Apply(GameContext context)
        {
            context.SpeedMultiplier += _multiplierBonus;
        }

        public void Expire(GameContext context)
        {
            context.SpeedMultiplier -= _multiplierBonus;
            if (context.SpeedMultiplier < 1f)
            {
                context.SpeedMultiplier = 1f;
            }
        }
    }
}
