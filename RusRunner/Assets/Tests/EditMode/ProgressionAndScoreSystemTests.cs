using NUnit.Framework;
using RusRunner.Core;
using RusRunner.Core.Systems;
using UnityEngine;

namespace RusRunner.Tests.EditMode.Core.Systems
{
    public sealed class ProgressionAndScoreSystemTests
    {
        private const float Tolerance = 0.0001f;
        private const string BestScoreKey = "rusrunner_best_score";

        [SetUp]
        public void SetUp()
        {
            PlayerPrefs.DeleteKey(BestScoreKey);
            PlayerPrefs.Save();
        }

        [TearDown]
        public void TearDown()
        {
            PlayerPrefs.DeleteKey(BestScoreKey);
            PlayerPrefs.Save();
        }

        [Test]
        public void ProgressionSystem_Tick_SetsBaseSpeedAndIncreasesDifficulty()
        {
            var context = new GameContext();
            var progression = new ProgressionSystem(baseSpeed: 7f, speedGainPerSecond: 2f, difficultyRampPerSecond: 0.25f);

            progression.Tick(context, deltaTime: 2f);

            Assert.That(context.CurrentSpeed, Is.EqualTo(11f).Within(Tolerance));
            Assert.That(progression.Difficulty01, Is.EqualTo(0.5f).Within(Tolerance));

        }

        [Test]
        public void ProgressionSystem_Tick_DifficultyIsClampedToOne()
        {
            var context = new GameContext();
            var progression = new ProgressionSystem(baseSpeed: 5f, speedGainPerSecond: 1f, difficultyRampPerSecond: 0.5f);

            progression.Tick(context, deltaTime: 5f);

            Assert.That(progression.Difficulty01, Is.EqualTo(1f).Within(Tolerance));
        }

        [Test]
        public void ScoreSystem_Tick_UsesSpeedAndMultipliers()
        {
            var context = new GameContext
            {
                CurrentSpeed = 10f,
                SpeedMultiplier = 1.5f,
                ScoreMultiplier = 2f
            };
            var scoreSystem = new ScoreSystem(scorePerMeter: 3f);

            scoreSystem.Tick(context, deltaTime: 2f);

            Assert.That(context.DistanceMeters, Is.EqualTo(30f).Within(Tolerance));
            Assert.That(context.Score, Is.EqualTo(180f).Within(Tolerance));

        }

        [Test]
        public void ScoreSystem_FinalizeRun_SavesOnlyBestScore()
        {
            var scoreSystem = new ScoreSystem(scorePerMeter: 1f);

            scoreSystem.FinalizeRun(new GameContext { Score = 100f });
            Assert.That(scoreSystem.BestScore, Is.EqualTo(100f).Within(Tolerance));

            scoreSystem.FinalizeRun(new GameContext { Score = 80f });
            Assert.That(scoreSystem.BestScore, Is.EqualTo(100f).Within(Tolerance));

            var reloadedScoreSystem = new ScoreSystem(scorePerMeter: 1f);
            Assert.That(reloadedScoreSystem.BestScore, Is.EqualTo(100f).Within(Tolerance));
        }
    }
}
