using System.Collections.Generic;
using RusRunner.Core.Contracts;

namespace RusRunner.Core.Systems
{
    public sealed class PowerUpSystem
    {
        private sealed class ActivePowerUp
        {
            public IPowerUp PowerUp;
            public float TimeLeft;
        }

        private readonly List<ActivePowerUp> _active = new List<ActivePowerUp>();

        public void Add(IPowerUp powerUp, GameContext context)
        {
            if (powerUp == null)
            {
                return;
            }

            powerUp.Apply(context);
            _active.Add(new ActivePowerUp
            {
                PowerUp = powerUp,
                TimeLeft = powerUp.DurationSeconds
            });
        }

        public void Tick(GameContext context, float deltaTime)
        {
            for (var i = _active.Count - 1; i >= 0; i--)
            {
                _active[i].TimeLeft -= deltaTime;
                if (_active[i].TimeLeft > 0f)
                {
                    continue;
                }

                _active[i].PowerUp.Expire(context);
                _active.RemoveAt(i);
            }
        }
    }
}
