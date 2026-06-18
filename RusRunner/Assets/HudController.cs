using RusRunner.Game.Audio;
using RusRunner.Game.Runner;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RusRunner.Game.UI
{
    public sealed class HudController : MonoBehaviour
    {
        [SerializeField] private RunnerBootstrap runnerBootstrap;
        [SerializeField] private Text scoreText;
        [SerializeField] private Text bestScoreText;
        [SerializeField] private Text distanceText;
        [SerializeField] private Text speedText;
        [SerializeField] private Text stateText;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private Text gameOverText;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button mainMenuButton;

        private Canvas _canvas;
        private Transform _adaptiveRoot;

        private void Awake()
        {
            if (runnerBootstrap == null)
            {
                runnerBootstrap = FindFirstObjectByType<RunnerBootstrap>();
            }

            EnsureAdaptiveUi();
        }

        private void Start()
        {
            if (restartButton != null)
            {
                restartButton.onClick.RemoveListener(Restart);
                restartButton.onClick.AddListener(Restart);
            }

            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.RemoveListener(ReturnToMainMenu);
                mainMenuButton.onClick.AddListener(ReturnToMainMenu);
            }

            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(false);
            }
        }

        private void Update()
        {
            if (runnerBootstrap == null)
            {
                runnerBootstrap = FindFirstObjectByType<RunnerBootstrap>();
                if (runnerBootstrap == null)
                {
                    return;
                }
            }

            if (_adaptiveRoot != null)
            {
                _adaptiveRoot.gameObject.SetActive(runnerBootstrap.IsRunStarted);
            }

            if (!runnerBootstrap.IsRunStarted)
            {
                return;
            }

            if (scoreText != null)
            {
                scoreText.text = $"Очки: {runnerBootstrap.CurrentScore:0}";
            }

            if (bestScoreText != null)
            {
                bestScoreText.text = $"Рекорд: {runnerBootstrap.BestScore:0}";
            }

            if (distanceText != null)
            {
                distanceText.text = $"Дистанция: {runnerBootstrap.CurrentDistance:0.0} м";
            }

            if (speedText != null)
            {
                speedText.text = $"Скорость: {runnerBootstrap.CurrentSpeed:0.0}";
            }

            if (stateText != null)
            {
                if (!runnerBootstrap.IsAlive)
                {
                    stateText.text = "Статус: поражение";
                }
                else
                {
                    var state = runnerBootstrap.IsSliding ? "Подкат" : "Бег";
                    if (runnerBootstrap.IsInvulnerable)
                    {
                        state += " | щит";
                    }

                    if (runnerBootstrap.HasScoreBoost)
                    {
                        state += " | x2";
                    }

                    if (runnerBootstrap.ExtraJumpsLeft > 0)
                    {
                        state += $" | доп.прыжок:{runnerBootstrap.ExtraJumpsLeft}";
                    }

                    stateText.text = "Статус: " + state;
                }
            }

            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(!runnerBootstrap.IsAlive);
            }

            if (gameOverText != null && !runnerBootstrap.IsAlive)
            {
                gameOverText.text =
                    $"Очки: {runnerBootstrap.CurrentScore:0}\n" +
                    $"Рекорд: {runnerBootstrap.BestScore:0}\n" +
                    $"Дистанция: {runnerBootstrap.CurrentDistance:0.0} м\n\n" +
                    "Выберите действие ниже или нажмите R";
            }
        }

        private void Restart()
        {
            if (runnerBootstrap != null)
            {
                runnerBootstrap.RestartRun();
                return;
            }

            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void ReturnToMainMenu()
        {
            if (runnerBootstrap != null)
            {
                runnerBootstrap.ReturnToMainMenu();
                return;
            }

            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void OnDestroy()
        {
            if (restartButton != null)
            {
                restartButton.onClick.RemoveListener(Restart);
            }

            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.RemoveListener(ReturnToMainMenu);
            }
        }

        private void EnsureAdaptiveUi()
        {
            var canvasObject = FindObjectByName("Canvas");
            if (canvasObject == null)
            {
                canvasObject = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            }

            _canvas = canvasObject.GetComponent<Canvas>();
            if (_canvas == null)
            {
                _canvas = canvasObject.AddComponent<Canvas>();
            }

            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 200;

            var scaler = canvasObject.GetComponent<CanvasScaler>() ?? canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            HideLegacyCanvasChildren(canvasObject.transform);

            var existingRoot = canvasObject.transform.Find("AdaptiveUIRoot");
            if (existingRoot != null)
            {
                _adaptiveRoot = existingRoot;
                scoreText = FindTextUnder(existingRoot, "AdaptiveScoreText");
                bestScoreText = FindTextUnder(existingRoot, "AdaptiveBestText");
                distanceText = FindTextUnder(existingRoot, "AdaptiveDistanceText");
                speedText = FindTextUnder(existingRoot, "AdaptiveSpeedText");
                stateText = FindTextUnder(existingRoot, "AdaptiveStateText");
                gameOverPanel = FindUnder(existingRoot, "AdaptiveGameOverOverlay");
                gameOverText = FindTextUnder(existingRoot, "AdaptiveGameOverStats");
                var restartObject = FindUnder(existingRoot, "AdaptiveRestartButton");
                restartButton = restartObject != null ? restartObject.GetComponent<Button>() : null;
                var mainMenuObject = FindUnder(existingRoot, "AdaptiveMainMenuButton");
                mainMenuButton = mainMenuObject != null ? mainMenuObject.GetComponent<Button>() : null;
                return;
            }

            _adaptiveRoot = new GameObject("AdaptiveUIRoot", typeof(RectTransform)).transform;
            _adaptiveRoot.SetParent(canvasObject.transform, false);
            StretchFull(_adaptiveRoot as RectTransform);

            var topBar = CreateImage("TopBar", _adaptiveRoot, new Color(0f, 0f, 0f, 0.18f));
            SetAnchoredRect(topBar.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 0f), new Vector2(0f, 96f), new Vector2(0.5f, 1f));

            scoreText = CreateText("AdaptiveScoreText", topBar.transform, 28, TextAnchor.UpperLeft, FontStyle.Bold);
            SetAnchoredRect(scoreText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(32f, -18f), new Vector2(360f, 34f), new Vector2(0f, 1f));
            bestScoreText = CreateText("AdaptiveBestText", topBar.transform, 22, TextAnchor.UpperLeft, FontStyle.Normal);
            SetAnchoredRect(bestScoreText.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(32f, -52f), new Vector2(360f, 28f), new Vector2(0f, 1f));

            distanceText = CreateText("AdaptiveDistanceText", topBar.transform, 28, TextAnchor.UpperCenter, FontStyle.Bold);
            SetAnchoredRect(distanceText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -18f), new Vector2(520f, 34f), new Vector2(0.5f, 1f));

            speedText = CreateText("AdaptiveSpeedText", topBar.transform, 28, TextAnchor.UpperRight, FontStyle.Bold);
            SetAnchoredRect(speedText.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-32f, -18f), new Vector2(360f, 34f), new Vector2(1f, 1f));
            stateText = CreateText("AdaptiveStateText", topBar.transform, 20, TextAnchor.UpperRight, FontStyle.Normal);
            stateText.horizontalOverflow = HorizontalWrapMode.Overflow;
            stateText.verticalOverflow = VerticalWrapMode.Truncate;
            SetAnchoredRect(stateText.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-32f, -54f), new Vector2(760f, 30f), new Vector2(1f, 1f));

            var bottomBar = CreateImage("BottomBar", _adaptiveRoot, new Color(0f, 0f, 0f, 0.42f));
            SetAnchoredRect(bottomBar.GetComponent<RectTransform>(), new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(0f, 54f), new Vector2(0.5f, 0f));
            var hintText = CreateText("AdaptiveHintText", bottomBar.transform, 19, TextAnchor.MiddleCenter, FontStyle.Normal);
            hintText.text = "Space/↑ — прыжок | Q — доп. прыжок | S/↓ — подкат | R — рестарт";
            hintText.color = new Color32(244, 244, 244, 245);
            StretchWithPadding(hintText.rectTransform, 22f, 6f, 22f, 6f);

            gameOverPanel = CreateImage("AdaptiveGameOverOverlay", _adaptiveRoot, new Color(0f, 0f, 0f, 0.34f));
            StretchFull(gameOverPanel.GetComponent<RectTransform>());

            var card = CreateImage("AdaptiveGameOverCard", gameOverPanel.transform, new Color32(225, 231, 243, 248));
            SetAnchoredRect(card.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(640f, 430f), new Vector2(0.5f, 0.5f));
            var cardOutline = card.AddComponent<Outline>();
            cardOutline.effectColor = new Color(0f, 0f, 0f, 0.18f);
            cardOutline.effectDistance = new Vector2(2f, -2f);

            var title = CreateText("AdaptiveGameOverTitle", card.transform, 38, TextAnchor.UpperCenter, FontStyle.Bold);
            title.text = "ЗАБЕГ ЗАВЕРШЁН";
            title.color = new Color32(55, 63, 78, 255);
            title.horizontalOverflow = HorizontalWrapMode.Wrap;
            title.verticalOverflow = VerticalWrapMode.Overflow;
            title.lineSpacing = 1f;
            SetAnchoredRect(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -36f), new Vector2(560f, 58f), new Vector2(0.5f, 1f));

            gameOverText = CreateText("AdaptiveGameOverStats", card.transform, 23, TextAnchor.MiddleCenter, FontStyle.Normal);
            gameOverText.color = new Color32(70, 75, 84, 255);
            gameOverText.lineSpacing = 1.15f;
            SetAnchoredRect(gameOverText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -22f), new Vector2(520f, 150f), new Vector2(0.5f, 0.5f));

            restartButton = CreateButton("AdaptiveRestartButton", card.transform, "Играть снова");
            SetAnchoredRect(restartButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-150f, 44f), new Vector2(260f, 60f), new Vector2(0.5f, 0f));

            mainMenuButton = CreateButton("AdaptiveMainMenuButton", card.transform, "В меню");
            SetAnchoredRect(mainMenuButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(150f, 44f), new Vector2(260f, 60f), new Vector2(0.5f, 0f));
            gameOverPanel.SetActive(false);
        }

        private static void HideLegacyCanvasChildren(Transform canvasTransform)
        {
            var legacyNames = new[]
            {
                "ScoreText", "BestText", "DistanceText", "SpeedText", "StateText",
                "GameOverPanel", "GeneratedTopBar", "GeneratedBottomBar"
            };

            var transforms = canvasTransform.GetComponentsInChildren<Transform>(true);
            for (var i = 0; i < transforms.Length; i++)
            {
                var current = transforms[i];
                if (current == null || current == canvasTransform)
                {
                    continue;
                }

                for (var j = 0; j < legacyNames.Length; j++)
                {
                    if (current.name == legacyNames[j])
                    {
                        current.gameObject.SetActive(false);
                        break;
                    }
                }
            }
        }

        private static GameObject CreateImage(string name, Transform parent, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.color = color;
            return go;
        }

        private static Text CreateText(string name, Transform parent, int fontSize, TextAnchor anchor, FontStyle fontStyle)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var text = go.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = anchor;
            text.fontStyle = fontStyle;
            text.color = new Color32(50, 50, 50, 255);
            text.resizeTextForBestFit = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        private static Button CreateButton(string name, Transform parent, string caption)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.color = new Color32(246, 247, 252, 255);
            var outline = go.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.12f);
            outline.effectDistance = new Vector2(1f, -1f);

            var button = go.GetComponent<Button>();
            button.onClick.AddListener(GameAudioManager.PlayButtonClick);

            var colors = button.colors;
            colors.normalColor = new Color32(246, 247, 252, 255);
            colors.highlightedColor = new Color32(255, 255, 255, 255);
            colors.pressedColor = new Color32(220, 224, 236, 255);
            colors.selectedColor = colors.highlightedColor;
            button.colors = colors;

            var text = CreateText(name + "Text", go.transform, 26, TextAnchor.MiddleCenter, FontStyle.Bold);
            text.text = caption;
            text.color = new Color32(62, 68, 81, 255);
            StretchFull(text.rectTransform);
            return button;
        }

        private static void SetAnchoredRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 size, Vector2 pivot)
        {
            if (rect == null)
            {
                return;
            }

            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
            rect.localScale = Vector3.one;
        }

        private static void StretchFull(RectTransform rect)
        {
            if (rect == null)
            {
                return;
            }

            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;
        }

        private static void StretchWithPadding(RectTransform rect, float left, float bottom, float right, float top)
        {
            if (rect == null)
            {
                return;
            }

            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(-right, -top);
            rect.localScale = Vector3.one;
        }

        private static Text FindTextUnder(Transform root, string objectName)
        {
            var go = FindUnder(root, objectName);
            return go == null ? null : go.GetComponent<Text>();
        }

        private static GameObject FindUnder(Transform root, string objectName)
        {
            if (root == null)
            {
                return null;
            }

            var transforms = root.GetComponentsInChildren<Transform>(true);
            for (var i = 0; i < transforms.Length; i++)
            {
                if (transforms[i].name == objectName)
                {
                    return transforms[i].gameObject;
                }
            }

            return null;
        }

        private static GameObject FindObjectByName(string objectName)
        {
            var scene = SceneManager.GetActiveScene();
            var roots = scene.GetRootGameObjects();
            for (var i = 0; i < roots.Length; i++)
            {
                var transforms = roots[i].GetComponentsInChildren<Transform>(true);
                for (var j = 0; j < transforms.Length; j++)
                {
                    if (transforms[j].name == objectName)
                    {
                        return transforms[j].gameObject;
                    }
                }
            }

            return null;
        }
    }
}
