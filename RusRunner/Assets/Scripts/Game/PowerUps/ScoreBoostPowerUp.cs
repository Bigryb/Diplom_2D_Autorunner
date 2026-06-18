using RusRunner.Core;
using RusRunner.Core.Contracts;

namespace RusRunner.Game.PowerUps
{
    public sealed class ScoreBoostPowerUp : IPowerUp
    {
        private readonly float _multiplierBonus;

        public string Id => "score_boost";
        public float DurationSeconds { get; }

        public ScoreBoostPowerUp(float durationSeconds, float multiplierBonus)
        {
            DurationSeconds = durationSeconds;
            _multiplierBonus = multiplierBonus;
        }

        public void Apply(GameContext context)
        {
            context.ScoreMultiplier += _multiplierBonus;
        }

        public void Expire(GameContext context)
        {
            context.ScoreMultiplier -= _multiplierBonus;
            if (context.ScoreMultiplier < 1f)
            {
                context.ScoreMultiplier = 1f;
            }
        }
    }
}
