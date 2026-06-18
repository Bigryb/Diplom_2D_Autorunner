using RusRunner.Core;
using RusRunner.Core.Contracts;

namespace RusRunner.Game.Abilities
{
    public sealed class DashAbility : IAbility
    {
        private readonly float _cooldownSeconds;
        private readonly float _durationSeconds;
        private readonly float _speedMultiplierBonus;

        private float _cooldownLeft;
        private float _durationLeft;
        private bool _active;

        public string Id => "dash";

        public DashAbility(float cooldownSeconds, float durationSeconds, float speedMultiplierBonus)
        {
            _cooldownSeconds = cooldownSeconds;
            _durationSeconds = durationSeconds;
            _speedMultiplierBonus = speedMultiplierBonus;
            _cooldownLeft = 0f;
            _durationLeft = 0f;
        }

        public bool CanActivate(GameContext context)
        {
            return !_active && _cooldownLeft <= 0f && context.IsAlive;
        }

        public void Activate(GameContext context)
        {
            _active = true;
            _durationLeft = _durationSeconds;
            context.SpeedMultiplier += _speedMultiplierBonus;
        }

        public void Tick(GameContext context, float deltaTime)
        {
            if (_cooldownLeft > 0f)
            {
                _cooldownLeft -= deltaTime;
            }

            if (!_active)
            {
                return;
            }

            _durationLeft -= deltaTime;
            if (_durationLeft > 0f)
            {
                return;
            }

            Deactivate(context);
        }

        public void Deactivate(GameContext context)
        {
            if (!_active)
            {
                return;
            }

            _active = false;
            _cooldownLeft = _cooldownSeconds;
            context.SpeedMultiplier -= _speedMultiplierBonus;
            if (context.SpeedMultiplier < 0.2f)
            {
                context.SpeedMultiplier = 0.2f;
            }
        }
    }
}
