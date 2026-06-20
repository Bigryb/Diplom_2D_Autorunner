using NUnit.Framework;
using RusRunner.Core;
using RusRunner.Core.Systems;
using RusRunner.Game.Abilities;

namespace RusRunner.Tests.EditMode.Core.Systems
{
    public sealed class AbilitySystemTests
    {
        private const float Tolerance = 0.0001f;

        [Test]
        public void TryActivate_ReturnsFalseForUnknownAbility()
        {
            var system = new AbilitySystem();
            var context = new GameContext();

            var activated = system.TryActivate("missing", context);

            Assert.That(activated, Is.False);
        }

        [Test]
        public void Register_IgnoresNullAndBlankIdAbilities()
        {
            var system = new AbilitySystem();
            var context = new GameContext();

            system.Register(null);
            system.Register(new SpyAbility(" ", canActivate: true));

            Assert.That(system.TryActivate(" ", context), Is.False);
        }

        [Test]
        public void TryActivate_UsesRegisteredAbilityAndTicksIt()
        {
            var system = new AbilitySystem();
            var context = new GameContext();
            var ability = new SpyAbility("test_ability", canActivate: true);

            system.Register(ability);
            var activated = system.TryActivate("test_ability", context);
            system.Tick(context, deltaTime: 0.25f);

            Assert.That(activated, Is.True);
            Assert.That(ability.ActivateCallCount, Is.EqualTo(1));
            Assert.That(ability.TickCallCount, Is.EqualTo(1));
            Assert.That(ability.LastDeltaTime, Is.EqualTo(0.25f).Within(Tolerance));

        }

        [Test]
        public void TryActivate_ReturnsFalseWhenAbilityCannotActivate()
        {
            var system = new AbilitySystem();
            var context = new GameContext();
            var ability = new SpyAbility("locked_ability", canActivate: false);

            system.Register(ability);
            var activated = system.TryActivate("locked_ability", context);

            Assert.That(activated, Is.False);
            Assert.That(ability.ActivateCallCount, Is.EqualTo(0));

        }

        [Test]
        public void DashAbility_ActivatesTemporarilyAndStartsCooldown()
        {
            var context = new GameContext { IsAlive = true, SpeedMultiplier = 1f };
            var dash = new DashAbility(cooldownSeconds: 1f, durationSeconds: 0.5f, speedMultiplierBonus: 0.75f);

            Assert.That(dash.CanActivate(context), Is.True);

            dash.Activate(context);
            Assert.That(context.SpeedMultiplier, Is.EqualTo(1.75f).Within(Tolerance));

            dash.Tick(context, deltaTime: 0.49f);
            Assert.That(context.SpeedMultiplier, Is.EqualTo(1.75f).Within(Tolerance));

            dash.Tick(context, deltaTime: 0.02f);
            Assert.That(context.SpeedMultiplier, Is.EqualTo(1f).Within(Tolerance));
            Assert.That(dash.CanActivate(context), Is.False);


            dash.Tick(context, deltaTime: 1f);
            Assert.That(dash.CanActivate(context), Is.True);
        }

        [Test]
        public void SecondWindAbility_GrantsOneAirJumpAndThenStartsCooldown()
        {
            var context = new GameContext { IsAlive = true };
            var secondWind = new SecondWindAbility(cooldownSeconds: 1f);

            Assert.That(secondWind.CanActivate(context), Is.True);

            secondWind.Activate(context);
            Assert.That(context.AirJumpsLeft, Is.EqualTo(1));

            context.AirJumpsLeft = 0;
            secondWind.Tick(context, deltaTime: 0.01f);

            Assert.That(context.AirJumpsLeft, Is.EqualTo(0));
            Assert.That(secondWind.CanActivate(context), Is.False);


            secondWind.Tick(context, deltaTime: 1f);
            Assert.That(secondWind.CanActivate(context), Is.True);
        }
    }
}
