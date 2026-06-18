using RusRunner.Core;
using UnityEngine;

namespace RusRunner.Game.Audio
{
    public sealed class GameAudioManager : MonoBehaviour
    {
        private const string MenuMusicPath = "Audio/menu_theme";
        private const string GameplayMusicPath = "Audio/gameplay_theme";
        private const string JumpPath = "Audio/jump";
        private const string AirJumpPath = "Audio/air_jump";
        private const string SlidePath = "Audio/slide";
        private const string PickupPath = "Audio/pickup";
        private const string ShieldPath = "Audio/shield";
        private const string SpeedPath = "Audio/speed";
        private const string HitPath = "Audio/hit";
        private const string ShieldBlockPath = "Audio/shield_block";
        private const string ButtonClickPath = "Audio/button_click";
        private const string GameOverPath = "Audio/game_over";

        private static GameAudioManager _instance;

        [Header("Music")]
        [SerializeField] private AudioClip menuMusic;
        [SerializeField] private AudioClip gameplayMusic;

        [Header("Effects")]
        [SerializeField] private AudioClip jumpClip;
        [SerializeField] private AudioClip airJumpClip;
        [SerializeField] private AudioClip slideClip;
        [SerializeField] private AudioClip pickupClip;
        [SerializeField] private AudioClip shieldClip;
        [SerializeField] private AudioClip speedClip;
        [SerializeField] private AudioClip hitClip;
        [SerializeField] private AudioClip shieldBlockClip;
        [SerializeField] private AudioClip buttonClickClip;
        [SerializeField] private AudioClip gameOverClip;

        private AudioSource _musicSource;
        private AudioSource _effectsSource;

        public static GameAudioManager EnsureInScene()
        {
            if (_instance != null)
            {
                return _instance;
            }

            _instance = FindFirstObjectByType<GameAudioManager>(FindObjectsInactive.Include);
            if (_instance != null)
            {
                return _instance;
            }

            var go = new GameObject("GameAudioManager");
            _instance = go.AddComponent<GameAudioManager>();
            return _instance;
        }

        public void SetGameplayMode(bool isGameplay)
        {
            EnsureSources();
            LoadDefaultClips();
            ApplySavedVolumes();
            PlayMusic(isGameplay ? gameplayMusic : menuMusic);
        }

        public static void ApplySavedVolumes()
        {
            EnsureInScene().ApplyVolumesFromSettings();
        }

        public static void SetPreviewMusicVolume(float volume)
        {
            EnsureInScene().SetMusicVolume(volume);
        }

        public static void SetPreviewEffectsVolume(float volume)
        {
            EnsureInScene().SetEffectsVolume(volume);
        }

        public static void PlayJump()
        {
            var audio = EnsureInScene();
            audio.LoadDefaultClips();
            audio.PlayEffect(audio.jumpClip);
        }

        public static void PlayAirJump()
        {
            var audio = EnsureInScene();
            audio.LoadDefaultClips();
            audio.PlayEffect(audio.airJumpClip);
        }

        public static void PlaySlide()
        {
            var audio = EnsureInScene();
            audio.LoadDefaultClips();
            audio.PlayEffect(audio.slideClip);
        }

        public static void PlayPickup()
        {
            var audio = EnsureInScene();
            audio.LoadDefaultClips();
            audio.PlayEffect(audio.pickupClip);
        }

        public static void PlayShield()
        {
            var audio = EnsureInScene();
            audio.LoadDefaultClips();
            audio.PlayEffect(audio.shieldClip);
        }

        public static void PlaySpeedBoost()
        {
            var audio = EnsureInScene();
            audio.LoadDefaultClips();
            audio.PlayEffect(audio.speedClip);
        }

        public static void PlayHit()
        {
            var audio = EnsureInScene();
            audio.LoadDefaultClips();
            audio.PlayEffect(audio.hitClip);
        }

        public static void PlayShieldBlock()
        {
            var audio = EnsureInScene();
            audio.LoadDefaultClips();
            audio.PlayEffect(audio.shieldBlockClip);
        }

        public static void PlayButtonClick()
        {
            var audio = EnsureInScene();
            audio.LoadDefaultClips();
            audio.PlayEffect(audio.buttonClickClip);
        }

        public static void PlayGameOver()
        {
            var audio = EnsureInScene();
            audio.LoadDefaultClips();
            audio.PlayEffect(audio.gameOverClip);
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
            EnsureSources();
            LoadDefaultClips();
            ApplyVolumesFromSettings();
            PlayMusic(menuMusic);
        }

        private void EnsureSources()
        {
            if (_musicSource == null)
            {
                _musicSource = gameObject.AddComponent<AudioSource>();
                _musicSource.playOnAwake = false;
                _musicSource.loop = true;
                _musicSource.spatialBlend = 0f;
                _musicSource.priority = 128;
            }

            if (_effectsSource == null)
            {
                _effectsSource = gameObject.AddComponent<AudioSource>();
                _effectsSource.playOnAwake = false;
                _effectsSource.loop = false;
                _effectsSource.spatialBlend = 0f;
                _effectsSource.priority = 64;
            }
        }

        private void LoadDefaultClips()
        {
            if (menuMusic == null)
            {
                menuMusic = Resources.Load<AudioClip>(MenuMusicPath);
            }

            if (gameplayMusic == null)
            {
                gameplayMusic = Resources.Load<AudioClip>(GameplayMusicPath);
            }

            if (jumpClip == null)
            {
                jumpClip = Resources.Load<AudioClip>(JumpPath);
            }

            if (airJumpClip == null)
            {
                airJumpClip = Resources.Load<AudioClip>(AirJumpPath);
            }

            if (slideClip == null)
            {
                slideClip = Resources.Load<AudioClip>(SlidePath);
            }

            if (pickupClip == null)
            {
                pickupClip = Resources.Load<AudioClip>(PickupPath);
            }

            if (shieldClip == null)
            {
                shieldClip = Resources.Load<AudioClip>(ShieldPath);
            }

            if (speedClip == null)
            {
                speedClip = Resources.Load<AudioClip>(SpeedPath);
            }

            if (hitClip == null)
            {
                hitClip = Resources.Load<AudioClip>(HitPath);
            }

            if (shieldBlockClip == null)
            {
                shieldBlockClip = Resources.Load<AudioClip>(ShieldBlockPath);
            }

            if (buttonClickClip == null)
            {
                buttonClickClip = Resources.Load<AudioClip>(ButtonClickPath);
            }

            if (gameOverClip == null)
            {
                gameOverClip = Resources.Load<AudioClip>(GameOverPath);
            }
        }

        private void PlayMusic(AudioClip clip)
        {
            if (clip == null || _musicSource == null)
            {
                return;
            }

            if (_musicSource.clip == clip && _musicSource.isPlaying)
            {
                return;
            }

            _musicSource.Stop();
            _musicSource.clip = clip;
            _musicSource.time = 0f;
            _musicSource.Play();
        }

        private void PlayEffect(AudioClip clip)
        {
            EnsureSources();
            LoadDefaultClips();
            if (clip == null || _effectsSource == null)
            {
                return;
            }

            _effectsSource.PlayOneShot(clip);
        }

        private void ApplyVolumesFromSettings()
        {
            SetMusicVolume(GameSettings.MusicVolume);
            SetEffectsVolume(GameSettings.EffectsVolume);
            AudioListener.volume = 1f;
        }

        private void SetMusicVolume(float volume)
        {
            EnsureSources();
            _musicSource.volume = Mathf.Clamp01(volume);
        }

        private void SetEffectsVolume(float volume)
        {
            EnsureSources();
            _effectsSource.volume = Mathf.Clamp01(volume);
        }
    }
}
