using RusRunner.Core;
using RusRunner.Core.Systems;
using RusRunner.Game.Input;
using RusRunner.Game.Audio;
using RusRunner.Game.Obstacles;
using RusRunner.Game.Pickups;
using RusRunner.Game.PowerUps;
using RusRunner.Game.Presentation;
using RusRunner.Game.Track;
using RusRunner.Game.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RusRunner.Game.Runner
{
    public sealed class RunnerBootstrap : MonoBehaviour
    {
        [SerializeField] private TrackGenerator trackGenerator;
        [SerializeField] private Transform runnerTransform;
        [SerializeField] private RunnerController runnerController;
        [SerializeField] private float baseSpeed = 5f;
        [SerializeField] private float speedGainPerSecond = 0.05f;
        [SerializeField] private float difficultyRampPerSecond = 0.015f;
        [SerializeField] private float scorePerMeter = 1f;
        [SerializeField] private float worldScrollMultiplier = 1f;
        [SerializeField] private Vector3 cameraOffset = new Vector3(3.8f, 1.2f, -10f);
        [SerializeField] private float cameraSmooth = 8f;

        private static bool _startImmediatelyAfterReload;

        private Camera _mainCamera;
        private Vector3 _cameraFixedPosition;
        private bool _isRunStarted;

        private readonly GameContext _context = new GameContext();
        private readonly PowerUpSystem _powerUpSystem = new PowerUpSystem();
        private ProgressionSystem _progressionSystem;
        private ScoreSystem _scoreSystem;
        private IInputSource _inputSource;

        public float CurrentScore => _context.Score;
        public float CurrentDistance => _context.DistanceMeters;
        public float CurrentSpeed => _context.IsAlive && _isRunStarted ? _context.CurrentSpeed * _context.SpeedMultiplier : 0f;
        public float BestScore => _scoreSystem == null ? 0f : _scoreSystem.BestScore;
        public bool IsAlive => _context.IsAlive;
        public bool IsInvulnerable => _context.IsInvulnerable;
        public bool IsSliding => _context.IsSliding;
        public bool HasScoreBoost => _context.ScoreMultiplier > 1.01f;
        public int ExtraJumpsLeft => _context.AirJumpsLeft;
        public bool IsRunStarted => _isRunStarted;

        private void Awake()
        {
            GameSettings.Apply();
            GameAudioManager.EnsureInScene().SetGameplayMode(false);
            ScenePresentationBootstrap.EnsureInScene();
            AutoFindReferences();
            MainMenuController.EnsureInScene(this);
        }

        private void Start()
        {
            CreateRuntimeSystems();
            _context.Reset();
            _context.CurrentSpeed = baseSpeed;
            _isRunStarted = false;

            _inputSource = Application.isMobilePlatform
                ? (IInputSource)new MobileInputSource()
                : new KeyboardInputSource();

            _mainCamera = Camera.main;
            if (_mainCamera != null && runnerTransform != null)
            {
                _cameraFixedPosition = new Vector3(runnerTransform.position.x + cameraOffset.x, cameraOffset.y, cameraOffset.z);
                _mainCamera.transform.position = _cameraFixedPosition;
            }

            if (trackGenerator != null && runnerTransform != null)
            {
                trackGenerator.Initialize(runnerTransform);
                trackGenerator.SetScrollSpeed(0f);
            }

            if (_startImmediatelyAfterReload)
            {
                _startImmediatelyAfterReload = false;
                BeginRun();
            }
            else
            {
                GameAudioManager.EnsureInScene().SetGameplayMode(false);
                MainMenuController.EnsureInScene(this).ShowMainMenu();
            }
        }

        private void Update()
        {
            if (!_isRunStarted)
            {
                if (trackGenerator != null)
                {
                    trackGenerator.SetScrollSpeed(0f);
                }

                if (runnerController != null)
                {
                    runnerController.KeepFixedOnGround();
                }

                return;
            }

            if (!_context.IsAlive)
            {
                if (trackGenerator != null)
                {
                    trackGenerator.SetScrollSpeed(0f);
                }

                ProcessRestartInput();
                return;
            }

            var dt = Time.deltaTime;

            if (runnerController != null)
            {
                runnerController.Tick(_context, dt);
            }

            ProcessInput();
            _progressionSystem.Tick(_context, dt);
            _powerUpSystem.Tick(_context, dt);
            _scoreSystem.Tick(_context, dt);

            if (runnerController != null)
            {
                runnerController.MoveForward(CurrentSpeed);
            }

            if (trackGenerator != null)
            {
                trackGenerator.SetDifficulty(_progressionSystem.Difficulty01);
                trackGenerator.SetScrollSpeed(GetWorldScrollSpeed());
            }
        }

        private void LateUpdate()
        {
            if (_mainCamera == null)
            {
                _mainCamera = Camera.main;
                if (_mainCamera == null)
                {
                    return;
                }
            }

            _mainCamera.transform.position = Vector3.Lerp(
                _mainCamera.transform.position,
                _cameraFixedPosition,
                Time.deltaTime * cameraSmooth);
        }

        public void BeginRun()
        {
            GameSettings.Apply();
            GameAudioManager.EnsureInScene().SetGameplayMode(true);
            CreateRuntimeSystems();
            _context.Reset();
            _context.CurrentSpeed = baseSpeed;
            _isRunStarted = true;

            if (runnerController != null)
            {
                runnerController.RestoreAfterMenuOrRestart();
            }

            if (trackGenerator != null)
            {
                trackGenerator.SetDifficulty(0f);
                trackGenerator.SetScrollSpeed(GetWorldScrollSpeed());
            }
        }

        public void RestartRun()
        {
            _startImmediatelyAfterReload = true;
            GameAudioManager.EnsureInScene().SetGameplayMode(true);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void ReturnToMainMenu()
        {
            _startImmediatelyAfterReload = false;
            GameAudioManager.EnsureInScene().SetGameplayMode(false);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void HandleObstacleHit(Obstacle obstacle)
        {
            if (!_isRunStarted || !_context.IsAlive)
            {
                return;
            }

            if (_context.IsInvulnerable)
            {
                GameAudioManager.PlayShieldBlock();
                obstacle?.Deactivate();
                return;
            }

            if (obstacle != null && obstacle.Type == ObstacleType.Slide && _context.IsSliding)
            {
                return;
            }

            _context.IsAlive = false;
            GameAudioManager.PlayHit();
            GameAudioManager.PlayGameOver();
            runnerController?.Halt();
            if (trackGenerator != null)
            {
                trackGenerator.SetScrollSpeed(0f);
            }
            _scoreSystem?.FinalizeRun(_context);
        }

        public void CollectPickup(PickupType pickupType)
        {
            if (!_isRunStarted || !_context.IsAlive)
            {
                return;
            }

            switch (pickupType)
            {
                case PickupType.Shield:
                    _powerUpSystem.Add(new ShieldPowerUp(5f), _context);
                    GameAudioManager.PlayPickup();
                    GameAudioManager.PlayShield();
                    break;
                case PickupType.ScoreBoost:
                    _powerUpSystem.Add(new ScoreBoostPowerUp(6f, 1f), _context);
                    GameAudioManager.PlayPickup();
                    break;
                case PickupType.SpeedBoost:
                    _powerUpSystem.Add(new SpeedBoostPowerUp(4f, 0.25f), _context);
                    GameAudioManager.PlayPickup();
                    GameAudioManager.PlaySpeedBoost();
                    break;
            }
        }

        private float GetWorldScrollSpeed()
        {
            return CurrentSpeed * Mathf.Max(0.1f, worldScrollMultiplier);
        }

        private void CreateRuntimeSystems()
        {
            _progressionSystem = new ProgressionSystem(baseSpeed, speedGainPerSecond, difficultyRampPerSecond);
            _scoreSystem = new ScoreSystem(scorePerMeter);
        }

        private void ProcessInput()
        {
            if (_inputSource == null || runnerController == null)
            {
                return;
            }

            if (_inputSource.JumpPressed() && runnerController.TryGroundJump(_context))
            {
                GameAudioManager.PlayJump();
            }

            if (_inputSource.SlidePressed() && runnerController.Slide(_context))
            {
                GameAudioManager.PlaySlide();
            }

            if (UnityEngine.Input.GetKeyDown(KeyCode.Q) && runnerController.TryAirJump(_context))
            {
                GameAudioManager.PlayAirJump();
            }
        }

        private void ProcessRestartInput()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.R))
            {
                RestartRun();
            }
        }

        private void AutoFindReferences()
        {
            if (trackGenerator == null)
            {
                trackGenerator = FindFirstObjectByType<TrackGenerator>();
            }

            if (runnerTransform == null)
            {
                var runner = GameObject.FindGameObjectWithTag("Player") ?? GameObject.Find("Runner");
                if (runner != null)
                {
                    runnerTransform = runner.transform;
                }
            }

            if (runnerController == null && runnerTransform != null)
            {
                runnerController = runnerTransform.GetComponent<RunnerController>();
            }
        }

        private void OnDisable()
        {
            if (_isRunStarted)
            {
                _scoreSystem?.FinalizeRun(_context);
            }
        }
    }
}
