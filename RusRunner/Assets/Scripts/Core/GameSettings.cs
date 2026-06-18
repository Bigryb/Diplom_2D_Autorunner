using UnityEngine;

namespace RusRunner.Core
{
    public static class GameSettings
    {
        private const string MusicVolumeKey = "RusRunner.MusicVolume";
        private const string EffectsVolumeKey = "RusRunner.EffectsVolume";
        private const string TargetFpsKey = "RusRunner.TargetFps";

        public static float MusicVolume
        {
            get => Mathf.Clamp01(PlayerPrefs.GetFloat(MusicVolumeKey, 0.75f));
            set => PlayerPrefs.SetFloat(MusicVolumeKey, Mathf.Clamp01(value));
        }

        public static float EffectsVolume
        {
            get => Mathf.Clamp01(PlayerPrefs.GetFloat(EffectsVolumeKey, 0.85f));
            set => PlayerPrefs.SetFloat(EffectsVolumeKey, Mathf.Clamp01(value));
        }

        public static int TargetFps
        {
            get => NormalizeFps(PlayerPrefs.GetInt(TargetFpsKey, 60));
            set => PlayerPrefs.SetInt(TargetFpsKey, NormalizeFps(value));
        }

        public static void Save(float musicVolume, float effectsVolume, int targetFps)
        {
            MusicVolume = musicVolume;
            EffectsVolume = effectsVolume;
            TargetFps = targetFps;
            PlayerPrefs.Save();
            Apply();
        }

        public static void Apply()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = TargetFps;

            // Раздельные уровни музыки и эффектов применяются в GameAudioManager.
            // Глобальный слушатель оставлен на максимуме, чтобы не искажать баланс источников.
            AudioListener.volume = 1f;
        }

        private static int NormalizeFps(int value)
        {
            if (value == 120 || value == 180)
            {
                return value;
            }

            return 60;
        }
    }
}
