using RusRunner.Core;
using RusRunner.Core.Systems;
using RusRunner.Game.Abilities;
using RusRunner.Game.Input;
using RusRunner.Game.PowerUps;
using RusRunner.Game.Track;
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
        [SerializeField] private float speedGainPerSecond = 0.06f;
        [SerializeField] private float difficultyRampPerSecond = 0.015f;
        [SerializeField] private float scorePerMeter = 1f;

        private readonly GameContext _context = new GameContext();
        private readonly AbilitySystem _abilitySystem = new AbilitySystem();
        private readonly PowerUpSystem _powerUpSystem = new PowerUpSystem();
        private ProgressionSystem _progressionSystem;
        private ScoreSystem _scoreSystem;
        private IInputSource _inputSource;
        
        public float CurrentScore => _context.Score;
        public float CurrentDistance => _context.DistanceMeters;
        public float CurrentSpeed => _context.CurrentSpeed * _context.SpeedMultiplier;
        public float BestScore => _scoreSystem == null ? 0f : _scoreSystem.BestScore;
        public bool IsAlive => _context.IsAlive;
        public bool IsInvulnerable => _context.IsInvulnerable;

        private void Start()
        {
            _context.Reset();
            _context.CurrentSpeed = baseSpeed;
            _progressionSystem = new ProgressionSystem(baseSpeed, speedGainPerSecond, difficultyRampPerSecond);
            _scoreSystem = new ScoreSystem(scorePerMeter);

            _abilitySystem.Register(new DashAbility(2.2f, 0.45f, 0.8f));
            _abilitySystem.Register(new SecondWindAbility(3.5f));

            _inputSource = Application.isMobilePlatform
                ? (IInputSource)new MobileInputSource()
                : new KeyboardInputSource();

            if (trackGenerator != null && runnerTransform != null)
            {
                trackGenerator.Initialize(runnerTransform);
            }
        }

        private void Update()
        {
            if (!_context.IsAlive)
            {
                ProcessRestartInput();
                return;
            }

            var dt = Time.deltaTime;
            ProcessInput();
            _progressionSystem.Tick(_context, dt);

            _abilitySystem.Tick(_context, dt);
            _powerUpSystem.Tick(_context, dt);
            _scoreSystem.Tick(_context, dt);

            if (runnerController != null)
            {
                runnerController.Tick(_context, dt);
                runnerController.MoveForward(_context.CurrentSpeed * _context.SpeedMultiplier);
            }

            if (trackGenerator != null)
            {
                trackGenerator.SetDifficulty(_progressionSystem.Difficulty01);
            }
        }

        public void RestartRun()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void HandleObstacleHit()
        {
            if (!_context.IsAlive || _context.IsInvulnerable)
            {
                return;
            }

            _context.IsAlive = false;
            if (_scoreSystem != null)
            {
                _scoreSystem.FinalizeRun(_context);
            }
        }

        private void ProcessInput()
        {
            if (_inputSource == null || runnerController == null)
            {
                return;
            }

            if (_inputSource.JumpPressed())
            {
                runnerController.Jump(_context);
            }

            if (_inputSource.SlidePressed())
            {
                runnerController.Slide();
            }

            if (_inputSource.DashPressed())
            {
                _abilitySystem.TryActivate("dash", _context);
            }

            if (UnityEngine.Input.GetKeyDown(KeyCode.Q))
            {
                _abilitySystem.TryActivate("second_wind", _context);
            }

            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha1))
            {
                _powerUpSystem.Add(new ShieldPowerUp(5f), _context);
            }

            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha2))
            {
                _powerUpSystem.Add(new ScoreBoostPowerUp(6f, 1f), _context);
            }
        }

        private void ProcessRestartInput()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.R))
            {
                RestartRun();
                return;
            }

            if (Application.isMobilePlatform && UnityEngine.Input.touchCount > 0)
            {
                var touch = UnityEngine.Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    RestartRun();
                }
            }
        }

        private void OnDisable()
        {
            if (_scoreSystem != null)
            {
                _scoreSystem.FinalizeRun(_context);
            }
        }
    }
}
