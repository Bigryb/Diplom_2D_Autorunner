using NUnit.Framework;
using RusRunner.Core;
using UnityEngine;

namespace RusRunner.Tests.EditMode.Core
{
    public sealed class GameSettingsTests
    {
        private const float Tolerance = 0.0001f;
        private const string MusicVolumeKey = "RusRunner.MusicVolume";
        private const string EffectsVolumeKey = "RusRunner.EffectsVolume";
        private const string TargetFpsKey = "RusRunner.TargetFps";

        private int _originalTargetFrameRate;
        private int _originalVSyncCount;
        private float _originalAudioListenerVolume;

        [SetUp]
        public void SetUp()
        {
            _originalTargetFrameRate = Application.targetFrameRate;
            _originalVSyncCount = QualitySettings.vSyncCount;
            _originalAudioListenerVolume = AudioListener.volume;
            ClearSettings();
        }

        [TearDown]
        public void TearDown()
        {
            ClearSettings();
            Application.targetFrameRate = _originalTargetFrameRate;
            QualitySettings.vSyncCount = _originalVSyncCount;
            AudioListener.volume = _originalAudioListenerVolume;
        }

        [Test]
        public void VolumeProperties_AreClampedToRange()
        {
            GameSettings.MusicVolume = -1f;
            GameSettings.EffectsVolume = 2f;

            Assert.That(GameSettings.MusicVolume, Is.EqualTo(0f).Within(Tolerance));
            Assert.That(GameSettings.EffectsVolume, Is.EqualTo(1f).Within(Tolerance));

        }

        [TestCase(60, 60)]
        [TestCase(120, 120)]
        [TestCase(180, 180)]
        [TestCase(75, 60)]
        [TestCase(0, 60)]
        public void TargetFps_NormalizesUnsupportedValues(int requestedFps, int expectedFps)
        {
            GameSettings.TargetFps = requestedFps;

            Assert.That(GameSettings.TargetFps, Is.EqualTo(expectedFps));
        }

        [Test]
        public void Save_PersistsSettingsAndAppliesFrameRate()
        {
            GameSettings.Save(musicVolume: 0.4f, effectsVolume: 0.6f, targetFps: 120);

            Assert.That(GameSettings.MusicVolume, Is.EqualTo(0.4f).Within(Tolerance));
            Assert.That(GameSettings.EffectsVolume, Is.EqualTo(0.6f).Within(Tolerance));
            Assert.That(GameSettings.TargetFps, Is.EqualTo(120));
            Assert.That(Application.targetFrameRate, Is.EqualTo(120));
            Assert.That(QualitySettings.vSyncCount, Is.EqualTo(0));
            Assert.That(AudioListener.volume, Is.EqualTo(1f).Within(Tolerance));

        }

        private static void ClearSettings()
        {
            PlayerPrefs.DeleteKey(MusicVolumeKey);
            PlayerPrefs.DeleteKey(EffectsVolumeKey);
            PlayerPrefs.DeleteKey(TargetFpsKey);
            PlayerPrefs.Save();
        }
    }
}
