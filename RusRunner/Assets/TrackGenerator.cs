using System.Collections.Generic;
using RusRunner.Game.Obstacles;
using RusRunner.Game.Pickups;
using RusRunner.Game.Presentation;
using UnityEngine;

namespace RusRunner.Game.Track
{
    public sealed class TrackGenerator : MonoBehaviour
    {
        [SerializeField] private List<TrackSegmentConfig> segmentConfigs = new List<TrackSegmentConfig>();
        [SerializeField] private int initialSegments = 8;
        [SerializeField] private float despawnDistance = 18f;
        [SerializeField] private float lookAheadDistance = 28f;
        [SerializeField] private float defaultSegmentLength = 12f;

        private sealed class GroundSegment
        {
            public GameObject GameObject;
            public float Length;
        }

        private sealed class SpawnedObstacle
        {
            public GameObject GameObject;
            public ObstacleType Type;
        }

        private sealed class SpawnedPickup
        {
            public GameObject GameObject;
            public PickupType Type;
        }

        private readonly List<GroundSegment> _activeGroundSegments = new List<GroundSegment>();
        private readonly List<SpawnedObstacle> _activeObstacles = new List<SpawnedObstacle>();
        private readonly List<SpawnedPickup> _activePickups = new List<SpawnedPickup>();
        private readonly Stack<GameObject> _groundPool = new Stack<GameObject>();
        private readonly Stack<GameObject> _jumpObstaclePool = new Stack<GameObject>();
        private readonly Stack<GameObject> _slideObstaclePool = new Stack<GameObject>();
        private readonly Stack<GameObject> _airObstaclePool = new Stack<GameObject>();
        private readonly Stack<GameObject> _shieldPickupPool = new Stack<GameObject>();
        private readonly Stack<GameObject> _scorePickupPool = new Stack<GameObject>();
        private readonly Stack<GameObject> _speedPickupPool = new Stack<GameObject>();

        private Transform _runner;
        private Transform _generatedGroundRoot;
        private Transform _generatedObstacleRoot;
        private Transform _generatedPickupRoot;
        private GameObject _groundTemplate;
        private float _spawnX;
        private float _difficulty01;
        private float _groundY;
        private float _groundTopY;
        private float _runnerX;
        private float _scrollSpeed;
        private bool _initialized;

        public void Initialize(Transform runnerTransform)
        {
            _runner = runnerTransform;
            _runnerX = runnerTransform.position.x;
            EnsureDefaultConfigs();
            PrepareTemplatesAndRoots();
            CleanupLegacyScenePlaceholders();
            BuildInitialTrack();
            _initialized = true;
        }

        public void SetDifficulty(float difficulty01)
        {
            _difficulty01 = Mathf.Clamp01(difficulty01);
        }

        public void SetScrollSpeed(float speed)
        {
            _scrollSpeed = Mathf.Max(0f, speed);
        }

        private void Update()
        {
            if (!_initialized || _runner == null)
            {
                return;
            }

            _runnerX = _runner.position.x;
            var delta = _scrollSpeed * Time.deltaTime;
            if (delta > 0f)
            {
                ScrollWorld(delta);
                _spawnX -= delta;
            }

            while (_spawnX - _runnerX < lookAheadDistance)
            {
                SpawnNextSegment();
            }

            RecycleOffscreen();
        }

        private void ScrollWorld(float delta)
        {
            var move = Vector3.left * delta;

            for (var i = 0; i < _activeGroundSegments.Count; i++)
            {
                if (_activeGroundSegments[i].GameObject != null)
                {
                    _activeGroundSegments[i].GameObject.transform.position += move;
                }
            }

            for (var i = 0; i < _activeObstacles.Count; i++)
            {
                if (_activeObstacles[i].GameObject != null)
                {
                    _activeObstacles[i].GameObject.transform.position += move;
                }
            }

            for (var i = 0; i < _activePickups.Count; i++)
            {
                if (_activePickups[i].GameObject != null)
                {
                    _activePickups[i].GameObject.transform.position += move;
                }
            }
        }

        private void RecycleOffscreen()
        {
            for (var i = _activeGroundSegments.Count - 1; i >= 0; i--)
            {
                var segment = _activeGroundSegments[i];
                if (segment.GameObject == null)
                {
                    _activeGroundSegments.RemoveAt(i);
                    continue;
                }

                var rightEdge = segment.GameObject.transform.position.x + segment.Length * 0.5f;
                if (rightEdge >= _runnerX - despawnDistance)
                {
                    continue;
                }

                segment.GameObject.SetActive(false);
                if (segment.GameObject != _groundTemplate)
                {
                    _groundPool.Push(segment.GameObject);
                }
                _activeGroundSegments.RemoveAt(i);
            }

            for (var i = _activeObstacles.Count - 1; i >= 0; i--)
            {
                var obstacle = _activeObstacles[i];
                if (obstacle.GameObject == null)
                {
                    _activeObstacles.RemoveAt(i);
                    continue;
                }

                if (obstacle.GameObject.transform.position.x >= _runnerX - despawnDistance)
                {
                    continue;
                }

                obstacle.GameObject.SetActive(false);
                GetObstaclePool(obstacle.Type).Push(obstacle.GameObject);
                _activeObstacles.RemoveAt(i);
            }

            for (var i = _activePickups.Count - 1; i >= 0; i--)
            {
                var pickup = _activePickups[i];
                if (pickup.GameObject == null)
                {
                    _activePickups.RemoveAt(i);
                    continue;
                }

                if (pickup.GameObject.transform.position.x >= _runnerX - despawnDistance)
                {
                    continue;
                }

                pickup.GameObject.SetActive(false);
                GetPickupPool(pickup.Type).Push(pickup.GameObject);
                _activePickups.RemoveAt(i);
            }
        }

        private void EnsureDefaultConfigs()
        {
            if (segmentConfigs != null && segmentConfigs.Count > 0)
            {
                return;
            }

            segmentConfigs = new List<TrackSegmentConfig>
            {
                CreateRuntimeConfig("VillageEasy", 12f, 0.48f, 0.14f, 0.18f),
                CreateRuntimeConfig("ForestMid", 12.5f, 0.62f, 0.20f, 0.45f),
                CreateRuntimeConfig("OutpostHard", 11.5f, 0.74f, 0.22f, 0.75f),
                CreateRuntimeConfig("Rush", 10.5f, 0.82f, 0.28f, 1f)
            };
        }

        private static TrackSegmentConfig CreateRuntimeConfig(string name, float length, float obstacleDensity, float collectibleDensity, float difficultyWeight)
        {
            var config = ScriptableObject.CreateInstance<TrackSegmentConfig>();
            config.name = name;
            config.Length = length;
            config.ObstacleDensity = obstacleDensity;
            config.CollectibleDensity = collectibleDensity;
            config.DifficultyWeight = difficultyWeight;
            return config;
        }

        private void PrepareTemplatesAndRoots()
        {
            _groundTemplate = GameObject.Find("Ground");
            if (_groundTemplate != null)
            {
                _groundY = _groundTemplate.transform.position.y;
                var renderer = _groundTemplate.GetComponent<SpriteRenderer>();
                var collider = _groundTemplate.GetComponent<Collider2D>();
                var height = collider != null ? collider.bounds.size.y : (renderer != null ? renderer.bounds.size.y : 1f);
                if (height <= 0.2f)
                {
                    height = 1f;
                }

                _groundTopY = _groundY + height * 0.2f;
            }
            else
            {
                _groundY = -2f;
                _groundTopY = -2f;
            }

            _generatedGroundRoot = (GameObject.Find("GeneratedGround") ?? new GameObject("GeneratedGround")).transform;
            _generatedObstacleRoot = (GameObject.Find("Obstacles") ?? new GameObject("Obstacles")).transform;
            _generatedPickupRoot = (GameObject.Find("Pickups") ?? new GameObject("Pickups")).transform;
        }

        private void CleanupLegacyScenePlaceholders()
        {
            var sceneRoots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            for (var i = 0; i < sceneRoots.Length; i++)
            {
                var transforms = sceneRoots[i].GetComponentsInChildren<Transform>(true);
                for (var j = 0; j < transforms.Length; j++)
                {
                    var current = transforms[j];
                    if (current == null)
                    {
                        continue;
                    }

                    if (current.name.StartsWith("Square"))
                    {
                        current.gameObject.SetActive(false);
                    }
                }
            }
        }

        private void BuildInitialTrack()
        {
            _activeGroundSegments.Clear();
            _activeObstacles.Clear();
            _activePickups.Clear();

            var firstLength = GetSegmentLength();
            var firstStart = _runnerX - firstLength * 0.5f;
            CreateGroundSegment(firstStart, firstLength, true);
            _spawnX = firstStart + firstLength;

            for (var i = 1; i < initialSegments; i++)
            {
                SpawnNextSegment();
            }
        }

        private void SpawnNextSegment()
        {
            var length = GetSegmentLength();
            var startX = _spawnX;
            CreateGroundSegment(startX, length, false);

            var obstacleX = TrySpawnObstacle(startX, length);
            TrySpawnPickup(startX, length, obstacleX);

            _spawnX += length;
        }

        private void CreateGroundSegment(float startX, float length, bool useExistingGround)
        {
            GameObject groundObject = null;

            if (useExistingGround && _groundTemplate != null)
            {
                groundObject = _groundTemplate;
            }
            else if (_groundPool.Count > 0)
            {
                groundObject = _groundPool.Pop();
            }
            else if (_groundTemplate != null)
            {
                groundObject = Instantiate(_groundTemplate, _generatedGroundRoot);
            }

            if (groundObject == null)
            {
                return;
            }

            groundObject.SetActive(true);
            groundObject.transform.SetParent(_generatedGroundRoot, true);
            groundObject.transform.position = new Vector3(startX + length * 0.5f, _groundY, 0f);
            groundObject.transform.localScale = new Vector3(length, 1f, 1f);

            var collider = groundObject.GetComponent<BoxCollider2D>();
            if (collider != null)
            {
                collider.size = new Vector2(1f, 1f);
                collider.offset = Vector2.zero;
            }

            _activeGroundSegments.Add(new GroundSegment
            {
                GameObject = groundObject,
                Length = length
            });
        }

        private float? TrySpawnObstacle(float startX, float length)
        {
            var config = SelectSegmentByDifficulty();
            var density = config != null ? config.ObstacleDensity : Mathf.Lerp(0.4f, 0.85f, _difficulty01);
            if (Random.value > density)
            {
                return null;
            }

            var typeRoll = Random.value;
            var type = typeRoll < 0.44f
                ? ObstacleType.Jump
                : (typeRoll < 0.76f ? ObstacleType.Slide : ObstacleType.Air);

            var minX = startX + 5.2f;
            var maxX = startX + length - 2.6f;
            if (maxX <= minX)
            {
                return null;
            }

            var x = Random.Range(minX, maxX);
            var obstacle = GetPooledObstacle(type);
            obstacle.name = "ObstacleRuntime";
            obstacle.SetActive(true);
            obstacle.transform.SetParent(_generatedObstacleRoot, true);
            obstacle.transform.position = new Vector3(x, GetObstacleY(type), 0f);

            _activeObstacles.Add(new SpawnedObstacle
            {
                GameObject = obstacle,
                Type = type
            });

            return x;
        }

        private void TrySpawnPickup(float startX, float length, float? obstacleX)
        {
            var config = SelectSegmentByDifficulty();
            var density = config != null ? config.CollectibleDensity : Mathf.Lerp(0.14f, 0.3f, _difficulty01);
            if (Random.value > density)
            {
                return;
            }

            var pickupTypeRoll = Random.value;
            var pickupType = pickupTypeRoll < 0.34f
                ? PickupType.Shield
                : (pickupTypeRoll < 0.67f ? PickupType.ScoreBoost : PickupType.SpeedBoost);

            var minX = startX + 4.2f;
            var maxX = startX + length - 2.2f;
            if (maxX <= minX)
            {
                return;
            }

            var x = Random.Range(minX, maxX);
            if (obstacleX.HasValue && Mathf.Abs(x - obstacleX.Value) < 2.2f)
            {
                x = Mathf.Clamp(obstacleX.Value + 2.6f, minX, maxX);
            }

            var y = _groundTopY + Random.Range(1.15f, 1.55f);
            var pickup = GetPooledPickup(pickupType);
            pickup.name = "PickupRuntime";
            pickup.SetActive(true);
            pickup.transform.SetParent(_generatedPickupRoot, true);
            pickup.transform.position = new Vector3(x, y, 0f);

            _activePickups.Add(new SpawnedPickup
            {
                GameObject = pickup,
                Type = pickupType
            });
        }

        private float GetObstacleY(ObstacleType type)
        {
            switch (type)
            {
                case ObstacleType.Slide:
                    return _groundTopY + 1.55f;
                case ObstacleType.Air:
                    return _groundTopY + 3f;
                default:
                    return _groundTopY + 0.34f;
            }
        }

        private GameObject GetPooledObstacle(ObstacleType type)
        {
            var pool = GetObstaclePool(type);
            if (pool.Count > 0)
            {
                return pool.Pop();
            }

            switch (type)
            {
                case ObstacleType.Slide:
                    return CreateSlideObstacleObject();
                case ObstacleType.Air:
                    return CreateAirObstacleObject();
                default:
                    return CreateJumpObstacleObject();
            }
        }

        private Stack<GameObject> GetObstaclePool(ObstacleType type)
        {
            switch (type)
            {
                case ObstacleType.Slide:
                    return _slideObstaclePool;
                case ObstacleType.Air:
                    return _airObstaclePool;
                default:
                    return _jumpObstaclePool;
            }
        }

        private GameObject GetPooledPickup(PickupType type)
        {
            var pool = GetPickupPool(type);
            if (pool.Count > 0)
            {
                return pool.Pop();
            }

            return CreatePickupObject(type);
        }

        private Stack<GameObject> GetPickupPool(PickupType type)
        {
            switch (type)
            {
                case PickupType.Shield:
                    return _shieldPickupPool;
                case PickupType.ScoreBoost:
                    return _scorePickupPool;
                default:
                    return _speedPickupPool;
            }
        }

        private GameObject CreateJumpObstacleObject()
        {
            var go = new GameObject("ObstacleRuntimeJump");
            var collider = go.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = new Vector2(0.88f, 0.68f);

            var obstacle = go.AddComponent<Obstacle>();
            obstacle.Configure(ObstacleType.Jump);
            if (!CreateSpriteVisual(go.transform, "Obstacles/crate", Vector3.zero, 12, 0.92f, 0.82f))
            {
                CreateFallbackRect("Body", go.transform, Vector3.zero, new Vector2(0.86f, 0.66f), new Color32(126, 88, 48, 255), 12);
                CreateFallbackRect("BandTop", go.transform, new Vector3(0f, 0.14f, 0f), new Vector2(0.88f, 0.08f), new Color32(90, 62, 36, 255), 13);
                CreateFallbackRect("BandMid", go.transform, new Vector3(0f, -0.04f, 0f), new Vector2(0.88f, 0.08f), new Color32(90, 62, 36, 255), 13);
                CreateFallbackRect("StrapLeft", go.transform, new Vector3(-0.18f, 0f, 0f), new Vector2(0.08f, 0.66f), new Color32(90, 62, 36, 255), 13);
                CreateFallbackRect("StrapRight", go.transform, new Vector3(0.18f, 0f, 0f), new Vector2(0.08f, 0.66f), new Color32(90, 62, 36, 255), 13);
            }
            return go;
        }

        private GameObject CreateSlideObstacleObject()
        {
            var go = new GameObject("ObstacleRuntimeSlide");
            var collider = go.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.offset = Vector2.zero;
            collider.size = new Vector2(1.95f, 0.28f);

            var obstacle = go.AddComponent<Obstacle>();
            obstacle.Configure(ObstacleType.Slide);
            if (!CreateSpriteVisual(go.transform, "Obstacles/beam", Vector3.zero, 12, 2.35f, 1.9f))
            {
                CreateFallbackRect("Beam", go.transform, Vector3.zero, new Vector2(1.95f, 0.22f), new Color32(108, 72, 39, 255), 12);
                CreateFallbackRect("BeamShade", go.transform, new Vector3(0f, -0.07f, 0f), new Vector2(1.95f, 0.06f), new Color32(82, 54, 30, 255), 13);
                CreateFallbackRect("ClothL", go.transform, new Vector3(-0.62f, -0.24f, 0f), new Vector2(0.2f, 0.24f), new Color32(159, 52, 42, 255), 13);
                CreateFallbackRect("ClothR", go.transform, new Vector3(0.62f, -0.24f, 0f), new Vector2(0.2f, 0.24f), new Color32(159, 52, 42, 255), 13);
            }
            return go;
        }

        private GameObject CreateAirObstacleObject()
        {
            var go = new GameObject("ObstacleRuntimeAir");
            var collider = go.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = new Vector2(1.1f, 0.4f);

            var obstacle = go.AddComponent<Obstacle>();
            obstacle.Configure(ObstacleType.Air);
            if (!CreateSpriteVisual(go.transform, "Obstacles/bird", new Vector3(0f, 0.02f, 0f), 14, 1.32f, 0.9f))
            {
                CreateFallbackRect("Body", go.transform, Vector3.zero, new Vector2(0.72f, 0.2f), new Color32(74, 67, 83, 255), 14);
                CreateFallbackRect("WingLeft", go.transform, new Vector3(-0.34f, 0.05f, 0f), new Vector2(0.42f, 0.14f), new Color32(91, 84, 100, 255), 14).transform.rotation = Quaternion.Euler(0f, 0f, 22f);
                CreateFallbackRect("WingRight", go.transform, new Vector3(0.34f, 0.05f, 0f), new Vector2(0.42f, 0.14f), new Color32(91, 84, 100, 255), 14).transform.rotation = Quaternion.Euler(0f, 0f, -22f);
                CreateFallbackRect("Beak", go.transform, new Vector3(0.42f, 0.02f, 0f), new Vector2(0.12f, 0.06f), new Color32(217, 163, 71, 255), 15);
            }
            return go;
        }

        private GameObject CreatePickupObject(PickupType type)
        {
            var go = new GameObject("PickupRuntime");
            var collider = go.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.34f;

            var pickup = go.AddComponent<PowerUpPickup>();
            pickup.Configure(type);

            CreatePickupVisual(go.transform, type);
            return go;
        }

        private void CreatePickupVisual(Transform root, PickupType type)
        {
            var spritePath = GetPickupSpritePath(type);
            if (CreateSpriteVisual(root, spritePath, Vector3.zero, 18, 0.86f, 0.86f))
            {
                return;
            }

            var fallbackColor = new Color32(242, 222, 118, 255);
            if (type == PickupType.Shield)
            {
                fallbackColor = new Color32(119, 189, 255, 255);
            }
            else if (type == PickupType.SpeedBoost)
            {
                fallbackColor = new Color32(144, 225, 145, 255);
            }

            CreateFallbackRect("PickupFallback", root, Vector3.zero, new Vector2(0.5f, 0.5f), fallbackColor, 18);
        }

        private static string GetPickupSpritePath(PickupType type)
        {
            switch (type)
            {
                case PickupType.Shield:
                    return "Pickups/shield";
                case PickupType.ScoreBoost:
                    return "Pickups/coin";
                default:
                    return "Pickups/speed";
            }
        }

        private static bool CreateSpriteVisual(Transform parent, string spritePath, Vector3 localPosition, int sortingOrder, float maxWidth, float maxHeight)
        {
            var sprite = ArtSpriteLibrary.Load(spritePath);
            if (sprite == null)
            {
                return false;
            }

            var go = new GameObject("Visual");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = Color.white;
            sr.sortingOrder = sortingOrder;

            var spriteSize = sprite.bounds.size;
            if (spriteSize.x > 0.0001f && spriteSize.y > 0.0001f)
            {
                var uniformScale = Mathf.Min(maxWidth / spriteSize.x, maxHeight / spriteSize.y);
                go.transform.localScale = Vector3.one * uniformScale;
            }

            return true;
        }

        private static GameObject CreateFallbackRect(string name, Transform parent, Vector3 localPosition, Vector2 size, Color color, int sortingOrder)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localScale = new Vector3(size.x, size.y, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = ScenePresentationBootstrap.GetPixelSprite();
            sr.color = color;
            sr.sortingOrder = sortingOrder;
            return go;
        }

        private float GetSegmentLength()
        {
            var config = SelectSegmentByDifficulty();
            return config != null ? config.Length : defaultSegmentLength;
        }

        private TrackSegmentConfig SelectSegmentByDifficulty()
        {
            if (segmentConfigs == null || segmentConfigs.Count == 0)
            {
                return null;
            }

            var bestScore = float.MinValue;
            TrackSegmentConfig best = segmentConfigs[0];
            for (var i = 0; i < segmentConfigs.Count; i++)
            {
                var cfg = segmentConfigs[i];
                var score = 1f - Mathf.Abs(cfg.DifficultyWeight - _difficulty01);
                if (score > bestScore)
                {
                    bestScore = score;
                    best = cfg;
                }
            }

            return best;
        }
    }
}
