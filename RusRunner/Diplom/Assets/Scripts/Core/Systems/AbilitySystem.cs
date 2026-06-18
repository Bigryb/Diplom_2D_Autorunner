using System.Collections.Generic;
using RusRunner.Core.Contracts;

namespace RusRunner.Core.Systems
{
    public sealed class AbilitySystem
    {
        private readonly Dictionary<string, IAbility> _abilities = new Dictionary<string, IAbility>();
        private readonly HashSet<string> _activeAbilities = new HashSet<string>();

        public void Register(IAbility ability)
        {
            if (ability == null || string.IsNullOrWhiteSpace(ability.Id))
            {
                return;
            }

            _abilities[ability.Id] = ability;
        }

        public bool TryActivate(string abilityId, GameContext context)
        {
            if (!_abilities.TryGetValue(abilityId, out var ability))
            {
                return false;
            }

            if (!ability.CanActivate(context))
            {
                return false;
            }

            ability.Activate(context);
            _activeAbilities.Add(abilityId);
            return true;
        }

        public void Tick(GameContext context, float deltaTime)
        {
            foreach (var abilityId in _activeAbilities)
            {
                _abilities[abilityId].Tick(context, deltaTime);
            }
        }

        public void Deactivate(string abilityId, GameContext context)
        {
            if (!_activeAbilities.Contains(abilityId))
            {
                return;
            }

            _abilities[abilityId].Deactivate(context);
            _activeAbilities.Remove(abilityId);
        }
    }
}
