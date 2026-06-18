using UnityEngine;
using UnityEngine.UI;

namespace RusRunner.Game.Presentation
{
    public sealed class ScenePresentationBootstrap : MonoBehaviour
    {
        private static Sprite _pixelSprite;
        private static Sprite _circleSprite;

        private Camera _camera;
        private Transform _generatedRoot;
        private bool _applied;

        public static void EnsureInScene()
        {
            var existing = FindFirstObjectByType<ScenePresentationBootstrap>();
            if (existing != null)
            {
                existing.Apply();
                return;
            }

            var host = GameObject.Find("GameRoot") ?? new GameObject("GameRoot");
            var bootstrap = host.GetComponent<ScenePresentationBootstrap>();
            if (bootstrap == null)
            {
                bootstrap = host.AddComponent<ScenePresentationBootstrap>();
            }

            bootstrap.Apply();
        }

        private void Awake()
        {
            Apply();
        }

        public void Apply()
        {
            if (_applied)
            {
                return;
            }

            _applied = true;
            _camera = Camera.main;
            EnsureGeneratedRoot();
            CleanupLegacyPlaceholders();
            StyleCamera();
            BuildParallaxBackground();
            StyleGround();
            StyleRunner();
            StyleCanvas();
        }

        private void EnsureGeneratedRoot()
        {
            if (_generatedRoot != null)
            {
                return;
            }

            _generatedRoot = (GameObject.Find("GeneratedPresentation") ?? new GameObject("GeneratedPresentation")).transform;
        }

        private void CleanupLegacyPlaceholders()
        {
            var roots = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            for (var i = 0; i < roots.Length; i++)
            {
                var transforms = roots[i].GetComponentsInChildren<Transform>(true);
                for (var j = 0; j < transforms.Length; j++)
                {
                    if (transforms[j].name.StartsWith("Square"))
                    {
                        transforms[j].gameObject.SetActive(false);
                    }
                }
            }
        }

        private void StyleCamera()
        {
            if (_camera == null)
            {
                _camera = Camera.main;
            }

            if (_camera == null)
            {
                return;
            }

            _camera.orthographic = true;
            _camera.orthographicSize = 4.2f;
            _camera.backgroundColor = new Color32(166, 193, 232, 255);
        }

        private void BuildParallaxBackground()
        {
            if (_generatedRoot.Find("Background") != null)
            {
                return;
            }

            var backgroundRoot = new GameObject("Background").transform;
            backgroundRoot.SetParent(_generatedRoot, false);

            CreateCloudLayer(backgroundRoot, "Clouds", 24f, 24f, 0.05f, -12, 3.05f);
            CreateStripLayer(backgroundRoot, "Forest", 12f, 12f, 0.11f, "Background/forest", -1.95f, -9);
            CreateStripLayer(backgroundRoot, "Village", 19f, 19f, 0.18f, "Background/village", -2.42f, -7);
            CreateStripLayer(backgroundRoot, "Fence", 12f, 12f, 0.27f, "Background/fence", -2.42f, -5);
        }

        private void CreateCloudLayer(
            Transform parent,
            string name,
            float visualWidth,
            float loopWidth,
            float factor,
            int sortingOrder,
            float centerY)
        {
            var cloudSprite = ArtSpriteLibrary.Load("Background/clouds");
            if (cloudSprite == null)
            {
                return;
            }

            CreateScrollingLayer(parent, name, loopWidth, factor, tile =>
            {
                CreateSpriteAtCenter("CloudStrip", tile, cloudSprite, Vector3.zero, visualWidth, sortingOrder, 0.92f);
                tile.localPosition = new Vector3(tile.localPosition.x, centerY, 0f);
            });
        }

       private void CreateStripLayer(
        Transform parent,
        string name,
        float visualWidth,
        float loopWidth,
        float factor,
        string spritePath,
        float baselineY,
        int sortingOrder)
    {
        var sprite = ArtSpriteLibrary.Load(spritePath);
        if (sprite == null)
        {
            return;
        }

        CreateScrollingLayer(parent, name, loopWidth, factor, tile =>
        {
            CreateSpriteBottomAligned("Strip", tile, sprite, Vector3.zero, visualWidth, baselineY, sortingOrder, 1f);
        });
    }

private void CreateScrollingLayer(
    Transform parent,
    string name,
    float loopWidth,
    float factor,
    System.Action<Transform> builder)
{
    var layerRoot = new GameObject(name).transform;
    layerRoot.SetParent(parent, false);

    var cameraWidth = 20f;
    if (Camera.main != null && Camera.main.orthographic)
    {
        cameraWidth = Camera.main.orthographicSize * 2f * Camera.main.aspect;
    }

    var tileCount = Mathf.Max(3, Mathf.CeilToInt(cameraWidth / loopWidth) + 3);
    var tiles = new Transform[tileCount];

    for (var i = 0; i < tileCount; i++)
    {
        var tile = new GameObject("Tile" + i).transform;
        tile.SetParent(layerRoot, false);
        tile.localPosition = new Vector3(i * loopWidth, 0f, 0f);
        builder(tile);
        tiles[i] = tile;
    }

    var scroller = layerRoot.gameObject.AddComponent<ParallaxLoopLayer>();
    scroller.Configure(factor, loopWidth, tiles);
}

        private void StyleGround()
        {
            var ground = GameObject.Find("Ground");
            if (ground == null)
            {
                return;
            }

            var groundLayer = LayerMask.NameToLayer("Ground");
            if (groundLayer >= 0)
            {
                ground.layer = groundLayer;
            }

            var renderer = ground.GetComponent<SpriteRenderer>() ?? ground.AddComponent<SpriteRenderer>();
            var sprite = ArtSpriteLibrary.Load("Background/ground");
            renderer.sprite = sprite;
            renderer.color = sprite != null ? Color.white : new Color32(122, 88, 50, 255);
            renderer.sortingOrder = -1;

            if (sprite != null)
            {
                renderer.drawMode = SpriteDrawMode.Sliced;
                renderer.size = new Vector2(1.04f, 1f);
            }
        }

        private void StyleRunner()
        {
            var runner = GameObject.Find("Runner");
            if (runner == null)
            {
                return;
            }

            runner.tag = "Player";
            var renderer = runner.GetComponent<SpriteRenderer>() ?? runner.AddComponent<SpriteRenderer>();
            renderer.sprite = null;
            renderer.color = new Color(1f, 1f, 1f, 0f);
            renderer.sortingOrder = 20;

            if (runner.GetComponent<RunnerPresentation>() == null)
            {
                runner.AddComponent<RunnerPresentation>();
            }
        }

        private void StyleCanvas()
        {
            var canvasObject = GameObject.Find("Canvas");
            if (canvasObject == null)
            {
                return;
            }

            canvasObject.transform.localScale = Vector3.one;
            var scaler = canvasObject.GetComponent<CanvasScaler>() ?? canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            var canvas = canvasObject.GetComponent<Canvas>();
            if (canvas != null)
            {
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 100;
            }
        }

        private static void CreateSpriteBottomAligned(string name, Transform parent, Sprite sprite, Vector3 localOffset, float targetWidth, float baselineY, int sortingOrder, float alpha)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = new Color(1f, 1f, 1f, alpha);
            sr.sortingOrder = sortingOrder;

            var scale = GetWidthScale(sprite, targetWidth);
            go.transform.localScale = Vector3.one * scale;

            var height = sprite.bounds.size.y * scale;
            go.transform.localPosition = localOffset + new Vector3(0f, baselineY + height * 0.5f, 0f);
        }

        private static void CreateSpriteAtCenter(string name, Transform parent, Sprite sprite, Vector3 localPosition, float targetWidth, int sortingOrder, float alpha)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = new Color(1f, 1f, 1f, alpha);
            sr.sortingOrder = sortingOrder;

            var scale = GetWidthScale(sprite, targetWidth);
            go.transform.localScale = Vector3.one * scale;
            go.transform.localPosition = localPosition;
        }

        private static float GetWidthScale(Sprite sprite, float targetWidth)
        {
            if (sprite == null || sprite.bounds.size.x <= 0.0001f)
            {
                return 1f;
            }

            return targetWidth / sprite.bounds.size.x;
        }

        public static Sprite GetPixelSprite()
        {
            if (_pixelSprite != null)
            {
                return _pixelSprite;
            }

            var texture = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            for (var y = 0; y < 16; y++)
            {
                for (var x = 0; x < 16; x++)
                {
                    texture.SetPixel(x, y, Color.white);
                }
            }

            texture.Apply();
            texture.filterMode = FilterMode.Bilinear;
            _pixelSprite = Sprite.Create(texture, new Rect(0f, 0f, 16f, 16f), new Vector2(0.5f, 0.5f), 16f);
            return _pixelSprite;
        }

        public static Sprite GetCircleSprite()
        {
            if (_circleSprite != null)
            {
                return _circleSprite;
            }

            const int size = 24;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
            var radius = size * 0.48f;
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var distance = Vector2.Distance(new Vector2(x, y), center);
                    texture.SetPixel(x, y, distance <= radius ? Color.white : Color.clear);
                }
            }

            texture.Apply();
            texture.filterMode = FilterMode.Bilinear;
            _circleSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 16f);
            return _circleSprite;
        }
    }
}
