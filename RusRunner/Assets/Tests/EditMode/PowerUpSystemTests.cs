using NUnit.Framework;
using RusRunner.Core;
using RusRunner.Core.Systems;
using RusRunner.Game.PowerUps;

namespace RusRunner.Tests.EditMode.Core.Systems
{
    public sealed class PowerUpSystemTests
    {
        private const float Tolerance = 0.0001f;

        [Test]
        public void Add_NullPowerUp_DoesNotChangeContext()
        {
            var context = new GameContext();
            var system = new PowerUpSystem();

            system.Add(null, context);
            system.Tick(context, deltaTime: 10f);

            Assert.That(context.IsInvulnerable, Is.False);
            Assert.That(context.SpeedMultiplier, Is.EqualTo(1f).Within(Tolerance));
            Assert.That(context.ScoreMultiplier, Is.EqualTo(1f).Within(Tolerance));

        }

        [Test]
        public void ShieldPowerUp_ExpiresAfterDuration()
        {
            var context = new GameContext();
            var system = new PowerUpSystem();

            system.Add(new ShieldPowerUp(durationSeconds: 0.5f), context);
            Assert.That(context.IsInvulnerable, Is.True);

            system.Tick(context, deltaTime: 0.49f);
            Assert.That(context.IsInvulnerable, Is.True);

            system.Tick(context, deltaTime: 0.02f);
            Assert.That(context.IsInvulnerable, Is.False);
        }

        [Test]
        public void SpeedBoostPowerUp_RestoresMultiplierAfterExpiration()
        {
            var context = new GameContext { SpeedMultiplier = 1f };
            var system = new PowerUpSystem();

            system.Add(new SpeedBoostPowerUp(durationSeconds: 0.5f, multiplierBonus: 0.75f), context);
            Assert.That(context.SpeedMultiplier, Is.EqualTo(1.75f).Within(Tolerance));

            system.Tick(context, deltaTime: 0.5f);
            Assert.That(context.SpeedMultiplier, Is.EqualTo(1f).Within(Tolerance));
        }

        [Test]
        public void ScoreBoostPowerUp_RestoresMultiplierAfterExpiration()
        {
            var context = new GameContext { ScoreMultiplier = 1f };
            var system = new PowerUpSystem();

            system.Add(new ScoreBoostPowerUp(durationSeconds: 0.5f, multiplierBonus: 1f), context);
            Assert.That(context.ScoreMultiplier, Is.EqualTo(2f).Within(Tolerance));

            system.Tick(context, deltaTime: 0.5f);
            Assert.That(context.ScoreMultiplier, Is.EqualTo(1f).Within(Tolerance));
        }

        [Test]
        public void Add_ValidPowerUp_CallsApplyAndExpireOnce()
        {
            var context = new GameContext();
            var system = new PowerUpSystem();
            var powerUp = new SpyPowerUp(durationSeconds: 0.1f);

            system.Add(powerUp, context);
            system.Tick(context, deltaTime: 0.05f);
            system.Tick(context, deltaTime: 0.05f);
            system.Tick(context, deltaTime: 1f);

            Assert.That(powerUp.ApplyCallCount, Is.EqualTo(1));
            Assert.That(powerUp.ExpireCallCount, Is.EqualTo(1));

        }
    }
}
