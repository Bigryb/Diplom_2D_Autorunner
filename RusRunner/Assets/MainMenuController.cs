using RusRunner.Core;
using RusRunner.Game.Audio;
using RusRunner.Game.Runner;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RusRunner.Game.UI
{
    public sealed class MainMenuController : MonoBehaviour
    {
        private RunnerBootstrap _runnerBootstrap;
        private Canvas _canvas;
        private Transform _root;
        private GameObject _mainPanel;
        private GameObject _settingsPanel;
        private Slider _musicSlider;
        private Slider _effectsSlider;
        private Text _musicValueText;
        private Text _effectsValueText;
        private Text _fpsValueText;
        private int _selectedFps;

        public static MainMenuController EnsureInScene(RunnerBootstrap runnerBootstrap = null)
        {
            var existing = FindFirstObjectByType<MainMenuController>(FindObjectsInactive.Include);
            if (existing != null)
            {
                if (runnerBootstrap != null)
                {
                    existing.SetRunnerBootstrap(runnerBootstrap);
                }

                return existing;
            }

            var go = new GameObject("MainMenuController");
            var controller = go.AddComponent<MainMenuController>();
            controller.SetRunnerBootstrap(runnerBootstrap);
            return controller;
        }

        public void SetRunnerBootstrap(RunnerBootstrap runnerBootstrap)
        {
            _runnerBootstrap = runnerBootstrap;
        }

        private void Awake()
        {
            if (_runnerBootstrap == null)
            {
                _runnerBootstrap = FindFirstObjectByType<RunnerBootstrap>();
            }

            GameSettings.Apply();
            GameAudioManager.EnsureInScene().SetGameplayMode(false);
            EnsureEventSystem();
            EnsureUi();
        }

        private void Start()
        {
            if (_runnerBootstrap == null)
            {
                _runnerBootstrap = FindFirstObjectByType<RunnerBootstrap>();
            }

            if (_runnerBootstrap == null || !_runnerBootstrap.IsRunStarted)
            {
                ShowMainMenu();
            }
            else
            {
                HideAll();
            }
        }

        public void ShowMainMenu()
        {
            EnsureUi();
            GameAudioManager.EnsureInScene().SetGameplayMode(false);

            if (_root != null)
            {
                _root.gameObject.SetActive(true);
                _root.SetAsLastSibling();
            }

            if (_mainPanel != null)
            {
                _mainPanel.SetActive(true);
            }

            if (_settingsPanel != null)
            {
                _settingsPanel.SetActive(false);
            }
        }

        public void HideAll()
        {
            if (_root != null)
            {
                _root.gameObject.SetActive(false);
            }
        }

        private void StartGame()
        {
            HideAll();
            if (_runnerBootstrap == null)
            {
                _runnerBootstrap = FindFirstObjectByType<RunnerBootstrap>();
            }

            _runnerBootstrap?.BeginRun();
        }

        private void OpenSettings()
        {
            EnsureUi();
            _selectedFps = GameSettings.TargetFps;
            if (_musicSlider != null)
            {
                _musicSlider.value = GameSettings.MusicVolume;
            }

            if (_effectsSlider != null)
            {
                _effectsSlider.value = GameSettings.EffectsVolume;
            }

            RefreshSettingsLabels();

            if (_mainPanel != null)
            {
                _mainPanel.SetActive(false);
            }

            if (_settingsPanel != null)
            {
                _settingsPanel.SetActive(true);
            }
        }

        private void SaveSettings()
        {
            var music = _musicSlider != null ? _musicSlider.value : GameSettings.MusicVolume;
            var effects = _effectsSlider != null ? _effectsSlider.value : GameSettings.EffectsVolume;
            GameSettings.Save(music, effects, _selectedFps);
            GameAudioManager.ApplySavedVolumes();
            ShowMainMenu();
        }

        private void BackFromSettings()
        {
            _selectedFps = GameSettings.TargetFps;
            GameAudioManager.ApplySavedVolumes();
            ShowMainMenu();
        }

        private void SelectFps(int fps)
        {
            _selectedFps = fps;
            RefreshSettingsLabels();
        }

        private void RefreshSettingsLabels()
        {
            if (_musicValueText != null)
            {
                var value = _musicSlider != null ? _musicSlider.value : GameSettings.MusicVolume;
                _musicValueText.text = Mathf.RoundToInt(value * 100f) + "%";
            }

            if (_effectsValueText != null)
            {
                var value = _effectsSlider != null ? _effectsSlider.value : GameSettings.EffectsVolume;
                _effectsValueText.text = Mathf.RoundToInt(value * 100f) + "%";
            }

            if (_fpsValueText != null)
            {
                _fpsValueText.text = "Выбрано: " + _selectedFps + " FPS";
            }
        }

        private void ExitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void EnsureUi()
        {
            if (_root != null)
            {
                return;
            }

            var canvasObject = FindObjectByName("Canvas");
            if (canvasObject == null)
            {
                canvasObject = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            }

            _canvas = canvasObject.GetComponent<Canvas>() ?? canvasObject.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 300;

            var scaler = canvasObject.GetComponent<CanvasScaler>() ?? canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            _root = new GameObject("MainMenuRoot", typeof(RectTransform)).transform;
            _root.SetParent(canvasObject.transform, false);
            StretchFull(_root as RectTransform);

            var dim = CreateImage("MainMenuDim", _root, new Color(0f, 0f, 0f, 0.42f));
            StretchFull(dim.GetComponent<RectTransform>());

            var card = CreateImage("MainMenuCard", _root, new Color32(226, 233, 244, 250));
            SetAnchoredRect(card.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(700f, 700f), new Vector2(0.5f, 0.5f));
            var outline = card.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.18f);
            outline.effectDistance = new Vector2(2f, -2f);

            var title = CreateText("MainMenuTitle", card.transform, 50, TextAnchor.UpperCenter, FontStyle.Bold);
            title.text = "2D-АВТОРАННЕР";
            title.color = new Color32(48, 58, 76, 255);
            SetAnchoredRect(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -46f), new Vector2(560f, 70f), new Vector2(0.5f, 1f));

            var subtitle = CreateText("MainMenuSubtitle", card.transform, 22, TextAnchor.UpperCenter, FontStyle.Normal);
            subtitle.text = "Линейный казуальный раннер";
            subtitle.color = new Color32(82, 91, 108, 255);
            SetAnchoredRect(subtitle.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -112f), new Vector2(520f, 42f), new Vector2(0.5f, 1f));

            _mainPanel = new GameObject("MainMenuButtons", typeof(RectTransform));
            _mainPanel.transform.SetParent(card.transform, false);
            SetAnchoredRect(_mainPanel.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -40f), new Vector2(440f, 330f), new Vector2(0.5f, 0.5f));

            var startButton = CreateButton("StartGameButton", _mainPanel.transform, "Начать игру");
            SetAnchoredRect(startButton.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -10f), new Vector2(360f, 68f), new Vector2(0.5f, 1f));
            startButton.onClick.AddListener(StartGame);

            var settingsButton = CreateButton("SettingsButton", _mainPanel.transform, "Настройки");
            SetAnchoredRect(settingsButton.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -98f), new Vector2(360f, 68f), new Vector2(0.5f, 1f));
            settingsButton.onClick.AddListener(OpenSettings);

            var exitButton = CreateButton("ExitButton", _mainPanel.transform, "Выход");
            SetAnchoredRect(exitButton.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -186f), new Vector2(360f, 68f), new Vector2(0.5f, 1f));
            exitButton.onClick.AddListener(ExitGame);

            var hint = CreateText("MainMenuHint", card.transform, 19, TextAnchor.LowerCenter, FontStyle.Normal);
            hint.text = "Управление: Space/↑ — прыжок, Q — доп. прыжок, S/↓ — подкат";
            hint.color = new Color32(91, 96, 111, 255);
            SetAnchoredRect(hint.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 28f), new Vector2(560f, 44f), new Vector2(0.5f, 0f));

            BuildSettingsPanel(card.transform);
            ShowMainMenu();
        }

        private void BuildSettingsPanel(Transform parent)
        {
            _settingsPanel = new GameObject("SettingsPanel", typeof(RectTransform));
            _settingsPanel.transform.SetParent(parent, false);
            SetAnchoredRect(_settingsPanel.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -62f), new Vector2(620f, 470f), new Vector2(0.5f, 0.5f));

            var title = CreateText("SettingsTitle", _settingsPanel.transform, 32, TextAnchor.UpperCenter, FontStyle.Bold);
            title.text = "Настройки";
            title.color = new Color32(48, 58, 76, 255);
            SetAnchoredRect(title.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -8f), new Vector2(540f, 46f), new Vector2(0.5f, 1f));

            CreateSettingSlider("Music", "Громкость музыки", 0, out _musicSlider, out _musicValueText);
            CreateSettingSlider("Effects", "Громкость звуков", 1, out _effectsSlider, out _effectsValueText);

            _musicSlider.value = GameSettings.MusicVolume;
            _effectsSlider.value = GameSettings.EffectsVolume;
            _musicSlider.onValueChanged.AddListener(value =>
            {
                RefreshSettingsLabels();
                GameAudioManager.SetPreviewMusicVolume(value);
            });
            _effectsSlider.onValueChanged.AddListener(value =>
            {
                RefreshSettingsLabels();
                GameAudioManager.SetPreviewEffectsVolume(value);
            });

            var fpsLabel = CreateText("FpsLabel", _settingsPanel.transform, 24, TextAnchor.MiddleLeft, FontStyle.Bold);
            fpsLabel.text = "Частота кадров";
            fpsLabel.color = new Color32(58, 66, 82, 255);
            SetAnchoredRect(fpsLabel.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(42f, -218f), new Vector2(250f, 38f), new Vector2(0f, 1f));

            _fpsValueText = CreateText("FpsValue", _settingsPanel.transform, 20, TextAnchor.MiddleRight, FontStyle.Normal);
            _fpsValueText.color = new Color32(79, 87, 102, 255);
            SetAnchoredRect(_fpsValueText.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-42f, -218f), new Vector2(260f, 38f), new Vector2(1f, 1f));

            CreateFpsButton(60, -150f);
            CreateFpsButton(120, 0f);
            CreateFpsButton(180, 150f);

            var saveButton = CreateButton("SaveSettingsButton", _settingsPanel.transform, "Применить");
            SetAnchoredRect(saveButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-118f, 26f), new Vector2(220f, 58f), new Vector2(0.5f, 0f));
            saveButton.onClick.AddListener(SaveSettings);

            var backButton = CreateButton("BackSettingsButton", _settingsPanel.transform, "Назад");
            SetAnchoredRect(backButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(118f, 26f), new Vector2(220f, 58f), new Vector2(0.5f, 0f));
            backButton.onClick.AddListener(BackFromSettings);

            _selectedFps = GameSettings.TargetFps;
            RefreshSettingsLabels();
            _settingsPanel.SetActive(false);
        }

        private void CreateSettingSlider(string id, string caption, int index, out Slider slider, out Text valueText)
        {
            var y = -78f - index * 88f;
            var label = CreateText(id + "Label", _settingsPanel.transform, 24, TextAnchor.MiddleLeft, FontStyle.Bold);
            label.text = caption;
            label.color = new Color32(58, 66, 82, 255);
            SetAnchoredRect(label.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(42f, y), new Vector2(250f, 42f), new Vector2(0f, 1f));

            valueText = CreateText(id + "Value", _settingsPanel.transform, 22, TextAnchor.MiddleRight, FontStyle.Normal);
            valueText.color = new Color32(79, 87, 102, 255);
            SetAnchoredRect(valueText.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-42f, y), new Vector2(80f, 42f), new Vector2(1f, 1f));

            var sliderObject = new GameObject(id + "Slider", typeof(RectTransform), typeof(Slider));
            sliderObject.transform.SetParent(_settingsPanel.transform, false);
            SetAnchoredRect(sliderObject.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(302f, y - 4f), new Vector2(172f, 32f), new Vector2(0f, 1f));

            var background = CreateImage("Background", sliderObject.transform, new Color32(193, 202, 218, 255));
            StretchWithPadding(background.GetComponent<RectTransform>(), 0f, 11f, 0f, 11f);

            var fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(sliderObject.transform, false);
            StretchWithPadding(fillArea.GetComponent<RectTransform>(), 6f, 11f, 6f, 11f);

            var fill = CreateImage("Fill", fillArea.transform, new Color32(97, 146, 74, 255));
            StretchFull(fill.GetComponent<RectTransform>());

            var handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
            handleArea.transform.SetParent(sliderObject.transform, false);
            StretchWithPadding(handleArea.GetComponent<RectTransform>(), 8f, 0f, 8f, 0f);

            var handle = CreateImage("Handle", handleArea.transform, new Color32(246, 247, 252, 255));
            SetAnchoredRect(handle.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(28f, 28f), new Vector2(0.5f, 0.5f));
            handle.AddComponent<Outline>().effectColor = new Color(0f, 0f, 0f, 0.18f);

            slider = sliderObject.GetComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.wholeNumbers = false;
            slider.fillRect = fill.GetComponent<RectTransform>();
            slider.handleRect = handle.GetComponent<RectTransform>();
            slider.targetGraphic = handle.GetComponent<Image>();
            slider.direction = Slider.Direction.LeftToRight;
        }

        private void CreateFpsButton(int fps, float x)
        {
            var button = CreateButton("Fps" + fps + "Button", _settingsPanel.transform, fps.ToString());
            SetAnchoredRect(button.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(x, -278f), new Vector2(126f, 52f), new Vector2(0.5f, 1f));
            button.onClick.AddListener(() => SelectFps(fps));
        }

        private static void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null)
            {
                return;
            }

            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        private static GameObject CreateImage(string name, Transform parent, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = color;
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
            outline.effectColor = new Color(0f, 0f, 0f, 0.14f);
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

        private static GameObject FindObjectByName(string objectName)
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
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
