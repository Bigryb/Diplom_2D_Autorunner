using RusRunner.Core;
using RusRunner.Core.Contracts;

namespace RusRunner.Game.Abilities
{
    // Gives one additional air jump while active.
    public sealed class SecondWindAbility : IAbility
    {
        private readonly float _cooldownSeconds;
        private float _cooldownLeft;
        private bool _active;

        public string Id => "second_wind";

        public SecondWindAbility(float cooldownSeconds)
        {
            _cooldownSeconds = cooldownSeconds;
            _cooldownLeft = 0f;
            _active = false;
        }

        public bool CanActivate(GameContext context)
        {
            return !_active && _cooldownLeft <= 0f && context.IsAlive;
        }

        public void Activate(GameContext context)
        {
            _active = true;
            context.AirJumpsLeft = 1;
        }

        public void Tick(GameContext context, float deltaTime)
        {
            if (_cooldownLeft > 0f)
            {
                _cooldownLeft -= deltaTime;
            }

            if (_active && context.IsGrounded)
            {
                Deactivate(context);
            }
        }

        public void Deactivate(GameContext context)
        {
            if (!_active)
            {
                return;
            }

            _active = false;
            _cooldownLeft = _cooldownSeconds;
            if (context.AirJumpsLeft > 0)
            {
                context.AirJumpsLeft = 0;
            }
        }
    }
}
