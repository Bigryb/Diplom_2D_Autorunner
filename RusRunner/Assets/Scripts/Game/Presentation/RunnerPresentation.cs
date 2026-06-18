using RusRunner.Game.Runner;
using UnityEngine;

namespace RusRunner.Game.Presentation
{
    public sealed class RunnerPresentation : MonoBehaviour
    {
        private RunnerBootstrap _runnerBootstrap;
        private Rigidbody2D _rigidbody;
        private Transform _visualRoot;
        private Transform _shadow;
        private SpriteRenderer _shadowRenderer;
        private Transform _artTransform;
        private SpriteRenderer _artRenderer;
        private Sprite[] _runFrames;
        private Sprite _jumpSprite;
        private Sprite _slideSprite;
        private float _groundY;

        private static readonly Vector3 NormalRootPosition = new Vector3(0.04f, -0.4f, 0f);
        private static readonly Vector3 SlideRootPosition = new Vector3(0.12f, -0.9f, 0f);
        private static readonly Vector3 NormalArtPosition = new Vector3(0.08f, 0.47f, 0f);
        private static readonly Vector3 SlideArtPosition = new Vector3(0.34f, 0.18f, 0f);

        private void Awake()
        {
            _runnerBootstrap = FindFirstObjectByType<RunnerBootstrap>();
            _rigidbody = GetComponent<Rigidbody2D>();
            LoadSprites();
            BuildVisual();
            CacheVisualParts();
            _groundY = transform.position.y;
        }

        private void LateUpdate()
        {
            if (_artRenderer == null || _visualRoot == null)
            {
                CacheVisualParts();
                if (_artRenderer == null || _visualRoot == null)
                {
                    return;
                }
            }

            if (_runnerBootstrap == null)
            {
                _runnerBootstrap = FindFirstObjectByType<RunnerBootstrap>();
            }

            var runStarted = _runnerBootstrap != null && _runnerBootstrap.IsRunStarted;
            var isAlive = _runnerBootstrap == null || _runnerBootstrap.IsAlive;
            var isSliding = _runnerBootstrap != null && _runnerBootstrap.IsSliding;
            var verticalVelocity = _rigidbody != null ? _rigidbody.linearVelocity.y : 0f;
            var heightAboveGround = Mathf.Max(0f, transform.position.y - _groundY);
            var cycle = Time.time * Mathf.Lerp(5.4f, 9f, GetSpeed01());
            var bob = Mathf.Sin(cycle * 2f) * Mathf.Lerp(0.02f, 0.04f, GetSpeed01());
            var lift = Mathf.Clamp(verticalVelocity * 0.02f, -0.08f, 0.12f);

            if (!runStarted)
            {
                ApplySprite(_runFrames[0], NormalArtPosition, 1.8f, 2.15f, Color.white);
                _visualRoot.localPosition = Vector3.Lerp(_visualRoot.localPosition, NormalRootPosition, Time.deltaTime * 14f);
                transform.rotation = Quaternion.identity;
            }
            else if (!isAlive)
            {
                ApplySprite(_jumpSprite != null ? _jumpSprite : _runFrames[0], NormalArtPosition + new Vector3(0f, 0.06f, 0f), 1.8f, 2.15f, new Color(0.9f, 0.9f, 0.9f, 1f));
                transform.rotation = Quaternion.Euler(0f, 0f, -12f);
            }
            else if (isSliding)
            {
                ApplySprite(_slideSprite != null ? _slideSprite : _runFrames[0], SlideArtPosition, 2.25f, 1.05f, GetRunnerTint());
                _visualRoot.localPosition = Vector3.Lerp(_visualRoot.localPosition, SlideRootPosition, Time.deltaTime * 18f);
                transform.rotation = Quaternion.Euler(0f, 0f, -3f);
            }
            else
            {
                var airborne = heightAboveGround > 0.08f || Mathf.Abs(verticalVelocity) > 0.22f;
                var sprite = airborne ? (_jumpSprite != null ? _jumpSprite : _runFrames[0]) : GetRunFrame(cycle);
                ApplySprite(sprite, NormalArtPosition + new Vector3(0f, bob + lift * 0.35f, 0f), 1.8f, 2.15f, GetRunnerTint());
                _visualRoot.localPosition = Vector3.Lerp(_visualRoot.localPosition, NormalRootPosition, Time.deltaTime * 18f);
                transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Clamp(verticalVelocity * 4f, -10f, 14f) * 0.4f);
            }

            if (_shadowRenderer != null && _shadow != null)
            {
                var squash = Mathf.Clamp01(1f - heightAboveGround * 0.3f);
                _shadow.localScale = new Vector3(0.88f + squash * 0.14f, 0.16f + squash * 0.05f, 1f);
                _shadowRenderer.color = new Color(0f, 0f, 0f, Mathf.Lerp(0.1f, 0.24f, squash));
            }
        }

        private void LoadSprites()
        {
            _runFrames = new[]
            {
                ArtSpriteLibrary.Load("Runner/run_01"),
                ArtSpriteLibrary.Load("Runner/run_02"),
                ArtSpriteLibrary.Load("Runner/run_03"),
                ArtSpriteLibrary.Load("Runner/run_04")
            };

            _jumpSprite = ArtSpriteLibrary.Load("Runner/run");
            _slideSprite = ArtSpriteLibrary.Load("Runner/slide") ?? ArtSpriteLibrary.Load("Runner/crouch");

            for (var i = 0; i < _runFrames.Length; i++)
            {
                if (_runFrames[i] == null)
                {
                    _runFrames[i] = _jumpSprite;
                }
            }
        }

        private void BuildVisual()
        {
            if (transform.Find("VisualRoot") == null)
            {
                _visualRoot = new GameObject("VisualRoot").transform;
                _visualRoot.SetParent(transform, false);
                _visualRoot.localPosition = NormalRootPosition;
            }

            if (transform.Find("Shadow") == null)
            {
                var shadow = new GameObject("Shadow");
                shadow.transform.SetParent(transform, false);
                shadow.transform.localPosition = new Vector3(0f, -1f, 0f);
                shadow.transform.localScale = new Vector3(1f, 0.2f, 1f);

                _shadowRenderer = shadow.AddComponent<SpriteRenderer>();
                _shadowRenderer.sprite = ScenePresentationBootstrap.GetCircleSprite();
                _shadowRenderer.color = new Color(0f, 0f, 0f, 0.22f);
                _shadowRenderer.sortingOrder = 10;
            }

            _visualRoot = transform.Find("VisualRoot");
            if (_visualRoot != null && _visualRoot.Find("RunnerArt") == null)
            {
                var art = new GameObject("RunnerArt");
                art.transform.SetParent(_visualRoot, false);
                _artRenderer = art.AddComponent<SpriteRenderer>();
                _artRenderer.sortingOrder = 24;
                _artRenderer.sprite = _runFrames[0];
            }
        }

        private void CacheVisualParts()
        {
            _visualRoot = transform.Find("VisualRoot");
            _shadow = transform.Find("Shadow");
            _artTransform = _visualRoot == null ? null : _visualRoot.Find("RunnerArt");
            _artRenderer = _artTransform == null ? null : _artTransform.GetComponent<SpriteRenderer>();
            _shadowRenderer = _shadow == null ? null : _shadow.GetComponent<SpriteRenderer>();
        }

        private Sprite GetRunFrame(float cycle)
        {
            if (_runFrames == null || _runFrames.Length == 0)
            {
                return _jumpSprite;
            }

            var frameIndex = Mathf.Abs(Mathf.FloorToInt(cycle)) % _runFrames.Length;
            return _runFrames[frameIndex] != null ? _runFrames[frameIndex] : _jumpSprite;
        }

        private float GetSpeed01()
        {
            if (_runnerBootstrap == null)
            {
                return 0f;
            }

            return Mathf.InverseLerp(5f, 16f, _runnerBootstrap.CurrentSpeed);
        }

        private Color GetRunnerTint()
        {
            if (_runnerBootstrap != null && _runnerBootstrap.IsInvulnerable)
            {
                return new Color(1f, 0.97f, 0.82f, 1f);
            }

            return Color.white;
        }

        private void ApplySprite(Sprite sprite, Vector3 localPosition, float maxWidth, float maxHeight, Color tint)
        {
            if (_artRenderer == null || _artTransform == null || sprite == null)
            {
                return;
            }

            _artRenderer.sprite = sprite;
            _artRenderer.color = tint;
            _artTransform.localPosition = localPosition;

            var spriteSize = sprite.bounds.size;
            if (spriteSize.x <= 0.0001f || spriteSize.y <= 0.0001f)
            {
                _artTransform.localScale = Vector3.one;
                return;
            }

            var widthScale = maxWidth / spriteSize.x;
            var heightScale = maxHeight / spriteSize.y;
            var uniformScale = Mathf.Min(widthScale, heightScale);
            _artTransform.localScale = Vector3.one * uniformScale;
        }
    }
}
