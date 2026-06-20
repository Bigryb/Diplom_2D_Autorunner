using NUnit.Framework;
using RusRunner.Core;

namespace RusRunner.Tests.EditMode.Core
{
    public sealed class GameContextTests
    {
        private const float Tolerance = 0.0001f;

        [Test]
        public void NewContext_HasGameplaySafeDefaults()
        {
            var context = new GameContext();

            Assert.That(context.CurrentSpeed, Is.EqualTo(0f).Within(Tolerance));
            Assert.That(context.DistanceMeters, Is.EqualTo(0f).Within(Tolerance));
            Assert.That(context.Score, Is.EqualTo(0f).Within(Tolerance));
            Assert.That(context.IsAlive, Is.True);
            Assert.That(context.IsGrounded, Is.False);
            Assert.That(context.IsInvulnerable, Is.False);
            Assert.That(context.IsSliding, Is.False);
            Assert.That(context.SpeedMultiplier, Is.EqualTo(1f).Within(Tolerance));
            Assert.That(context.ScoreMultiplier, Is.EqualTo(1f).Within(Tolerance));
            Assert.That(context.AirJumpsLeft, Is.EqualTo(0));

        }

        [Test]
        public void Reset_RestoresDefaultRunState()
        {
            var context = new GameContext
            {
                CurrentSpeed = 12f,
                DistanceMeters = 42f,
                Score = 150f,
                IsGrounded = false,
                IsAlive = false,
                IsInvulnerable = true,
                IsSliding = true,
                SpeedMultiplier = 2.5f,
                ScoreMultiplier = 3f,
                AirJumpsLeft = 2
            };

            context.Reset();

            Assert.That(context.CurrentSpeed, Is.EqualTo(0f).Within(Tolerance));
            Assert.That(context.DistanceMeters, Is.EqualTo(0f).Within(Tolerance));
            Assert.That(context.Score, Is.EqualTo(0f).Within(Tolerance));
            Assert.That(context.IsGrounded, Is.True);
            Assert.That(context.IsAlive, Is.True);
            Assert.That(context.IsInvulnerable, Is.False);
            Assert.That(context.IsSliding, Is.False);
            Assert.That(context.SpeedMultiplier, Is.EqualTo(1f).Within(Tolerance));
            Assert.That(context.ScoreMultiplier, Is.EqualTo(1f).Within(Tolerance));
            Assert.That(context.AirJumpsLeft, Is.EqualTo(0));

        }
    }
}
