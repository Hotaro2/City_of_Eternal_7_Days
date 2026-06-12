#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace VN.Editor
{
    public static class VNLobbySceneSetupTool
    {
        private const string LobbyScenePath = "Assets/Scenes/LobbyScene.unity";
        private const string BackgroundPath = "Assets/Art/Lobby/LobbyBackground.png";
        private const string RegularFontPath = "Assets/Fonts/LINE_SeedKR_2023.09.06/OTF/LINESeedKR-Rg SDF.asset";
        private const string BoldFontPath = "Assets/Fonts/LINE_SeedKR_2023.09.06/OTF/LINESeedKR-Bd SDF.asset";
        private const string GeneratedRootName = "LobbySceneRoot";

        private static TMP_FontAsset regularFont;
        private static TMP_FontAsset boldFont;

        [MenuItem("VN Tools/Lobby/Build Lobby Scene")]
        public static void BuildLobbyScene()
        {
            PrepareBackground();
            LoadFonts();

            Scene scene = EditorSceneManager.OpenScene(LobbyScenePath, OpenSceneMode.Single);
            RemoveGeneratedRoot();

            GameObject root = new GameObject(GeneratedRootName);
            GameObject canvasObject = CreateCanvas(root.transform);
            Transform canvas = canvasObject.transform;

            CreateBackground(canvas);

            Button backButton = CreateNavigationButton(
                canvas,
                "BackButton",
                string.Empty,
                new Vector2(0.01f, 0.91f),
                new Vector2(0.12f, 0.995f),
                0f);

            Button settingsButton = CreateNavigationButton(
                canvas,
                "SettingsButton",
                string.Empty,
                new Vector2(0.19f, 0.91f),
                new Vector2(0.235f, 0.995f),
                0f);

            Button subStoryButton = CreateHotspotButton(
                canvas,
                "SubStoryHotspot",
                "도시 정보",
                "중앙청 보고서",
                new Vector2(0.015f, 0.235f),
                new Vector2(0.285f, 0.54f));

            Button mainStoryButton = CreateHotspotButton(
                canvas,
                "MainStoryHotspot",
                "메인 스토리",
                "중앙청 단말기",
                new Vector2(0.36f, 0.245f),
                new Vector2(0.73f, 0.575f),
                true);

            Button archiveButton = CreateHotspotButton(
                canvas,
                "ArchiveHotspot",
                "기록",
                "지휘사 수첩",
                new Vector2(0.22f, 0.015f),
                new Vector2(0.58f, 0.27f));

            Button miniGameButton = CreateHotspotButton(
                canvas,
                "MiniGameHotspot",
                "미니게임",
                "전술 훈련",
                new Vector2(0.72f, 0.04f),
                new Vector2(0.98f, 0.36f));

            CanvasGroup noticeGroup = CreateNotice(canvas, out TMP_Text noticeText);
            Image fadeImage = CreateFade(canvas);
            EnsureEventSystem(root.transform);

            VNLobbyController controller = root.AddComponent<VNLobbyController>();
            SerializedObject serializedController = new SerializedObject(controller);
            serializedController.FindProperty("backButton").objectReferenceValue = backButton;
            serializedController.FindProperty("mainStoryButton").objectReferenceValue = mainStoryButton;
            serializedController.FindProperty("subStoryButton").objectReferenceValue = subStoryButton;
            serializedController.FindProperty("archiveButton").objectReferenceValue = archiveButton;
            serializedController.FindProperty("miniGameButton").objectReferenceValue = miniGameButton;
            serializedController.FindProperty("settingsButton").objectReferenceValue = settingsButton;
            serializedController.FindProperty("noticeGroup").objectReferenceValue = noticeGroup;
            serializedController.FindProperty("noticeText").objectReferenceValue = noticeText;
            serializedController.FindProperty("fadeImage").objectReferenceValue = fadeImage;
            serializedController.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            UpdateBuildSettings();

            Selection.activeGameObject = root;
            Debug.Log("[VN Lobby] LobbyScene was built and added to Build Settings.");
        }

        private static void PrepareBackground()
        {
            if (AssetImporter.GetAtPath(BackgroundPath) is not TextureImporter importer) return;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = false;
            importer.sRGBTexture = true;
            importer.maxTextureSize = 2048;
            importer.SaveAndReimport();
        }

        private static void LoadFonts()
        {
            regularFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(RegularFontPath);
            boldFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(BoldFontPath);
            if (boldFont == null) boldFont = regularFont;
        }

        private static void RemoveGeneratedRoot()
        {
            GameObject existing = GameObject.Find(GeneratedRootName);
            if (existing != null)
                Object.DestroyImmediate(existing);
        }

        private static GameObject CreateCanvas(Transform parent)
        {
            GameObject canvasObject = CreateUIObject("LobbyCanvas", parent, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            return canvasObject;
        }

        private static void CreateBackground(Transform parent)
        {
            GameObject backgroundObject = CreateUIObject("Background", parent, typeof(Image));
            Stretch(backgroundObject.GetComponent<RectTransform>());

            Image image = backgroundObject.GetComponent<Image>();
            image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(BackgroundPath);
            image.color = Color.white;
            image.preserveAspect = false;
            image.raycastTarget = false;
        }

        private static Button CreateNavigationButton(
            Transform parent,
            string name,
            string label,
            Vector2 anchorMin,
            Vector2 anchorMax,
            float fontSize)
        {
            GameObject buttonObject = CreatePanel(
                name,
                parent,
                anchorMin,
                anchorMax,
                Color.clear);

            Button button = buttonObject.AddComponent<Button>();
            ConfigureButton(button, buttonObject.GetComponent<Image>());

            return button;
        }

        private static Button CreateHotspotButton(
            Transform parent,
            string name,
            string title,
            string subtitle,
            Vector2 anchorMin,
            Vector2 anchorMax,
            bool primary = false)
        {
            GameObject buttonObject = CreatePanel(name, parent, anchorMin, anchorMax, Color.clear);
            Image image = buttonObject.GetComponent<Image>();
            Button button = buttonObject.AddComponent<Button>();
            ConfigureButton(button, image);

            return button;
        }

        private static CanvasGroup CreateNotice(Transform parent, out TMP_Text noticeText)
        {
            GameObject notice = CreatePanel(
                "Notice",
                parent,
                new Vector2(0.335f, 0.025f),
                new Vector2(0.665f, 0.095f),
                new Color(0.025f, 0.03f, 0.09f, 0.94f));

            CanvasGroup group = notice.AddComponent<CanvasGroup>();
            group.alpha = 0f;

            Outline outline = notice.AddComponent<Outline>();
            outline.effectColor = new Color(0.55f, 0.48f, 0.9f, 0.7f);
            outline.effectDistance = new Vector2(1f, -1f);

            noticeText = CreateText(
                "Message",
                notice.transform,
                string.Empty,
                new Vector2(0.04f, 0f),
                new Vector2(0.96f, 1f),
                21f,
                TextAlignmentOptions.Center,
                Color.white,
                false);

            return group;
        }

        private static Image CreateFade(Transform parent)
        {
            GameObject fade = CreatePanel("SceneFade", parent, Vector2.zero, Vector2.one, Color.clear);
            fade.transform.SetAsLastSibling();
            Image image = fade.GetComponent<Image>();
            image.color = Color.clear;
            image.raycastTarget = false;
            return image;
        }

        private static void EnsureEventSystem(Transform parent)
        {
            if (Object.FindFirstObjectByType<EventSystem>() != null) return;

            GameObject eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            eventSystem.transform.SetParent(parent, false);
        }

        private static GameObject CreatePanel(
            string name,
            Transform parent,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Color color)
        {
            GameObject panel = CreateUIObject(name, parent, typeof(Image));
            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            panel.GetComponent<Image>().color = color;
            return panel;
        }

        private static TMP_Text CreateText(
            string name,
            Transform parent,
            string text,
            Vector2 anchorMin,
            Vector2 anchorMax,
            float fontSize,
            TextAlignmentOptions alignment,
            Color color,
            bool bold)
        {
            GameObject textObject = CreateUIObject(name, parent, typeof(TextMeshProUGUI));
            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            TextMeshProUGUI textComponent = textObject.GetComponent<TextMeshProUGUI>();
            textComponent.text = text;
            textComponent.font = bold ? boldFont : regularFont;
            textComponent.fontSize = fontSize;
            textComponent.alignment = alignment;
            textComponent.color = color;
            textComponent.raycastTarget = false;
            textComponent.enableAutoSizing = false;
            textComponent.textWrappingMode = TextWrappingModes.Normal;
            return textComponent;
        }

        private static GameObject CreateUIObject(string name, Transform parent, params System.Type[] components)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform));
            foreach (System.Type component in components)
                obj.AddComponent(component);

            obj.transform.SetParent(parent, false);
            return obj;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void ConfigureButton(Button button, Graphic targetGraphic)
        {
            button.targetGraphic = targetGraphic;
            button.transition = Selectable.Transition.ColorTint;

            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.18f, 1.18f, 1.25f, 1f);
            colors.pressedColor = new Color(0.76f, 0.8f, 1f, 1f);
            colors.selectedColor = colors.highlightedColor;
            colors.disabledColor = new Color(0.5f, 0.5f, 0.55f, 0.45f);
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.12f;
            button.colors = colors;
        }

        private static void UpdateBuildSettings()
        {
            string[] preferredOrder =
            {
                "Assets/Scenes/TitleScene.unity",
                LobbyScenePath,
                "Assets/Scenes/VNScene.unity",
                "Assets/Scenes/MiniGameScene.unity"
            };

            List<EditorBuildSettingsScene> scenes = preferredOrder
                .Where(path => AssetDatabase.LoadAssetAtPath<SceneAsset>(path) != null)
                .Select(path => new EditorBuildSettingsScene(path, true))
                .ToList();

            foreach (EditorBuildSettingsScene existing in EditorBuildSettings.scenes)
            {
                if (scenes.Any(scene => scene.path == existing.path)) continue;
                scenes.Add(existing);
            }

            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
#endif
