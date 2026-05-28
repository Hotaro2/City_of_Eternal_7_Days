using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VN.MiniGames;

namespace VN.Editor
{
    public static class VNMiniGameSetupTool
    {
        private const string DefaultQteId = "qte_defense_001";
        private const string DefaultSequenceQteId = "qte_sequence_001";
        private const string QteDefinitionPath = "Assets/ScriptObjects/MiniGames/QTE_Defense_001.asset";
        private const string SequenceDefinitionPath = "Assets/ScriptObjects/MiniGames/QTE_Sequence_001.asset";

        [MenuItem("VN Tools/MiniGames/Install Overlay In Active Scene")]
        public static void InstallOverlayInActiveScene()
        {
            EnsureEventSystem();

            Canvas canvas = EnsureCanvas();
            MiniGameManager manager = EnsureManager();
            QTEView view = EnsureQtePanel(canvas.transform);
            QTEMiniGame qte = EnsureQteModule(manager.transform, view);
            SequenceQTEView sequenceView = EnsureSequenceQtePanel(canvas.transform);
            SequenceQTEMiniGame sequenceQte = EnsureSequenceQteModule(manager.transform, sequenceView);
            TMP_Text testStatus = EnsureMiniGameSceneTestStatus(canvas.transform);
            QTEMiniGameDefinition definition = EnsureQteDefinition();
            SequenceQTEMiniGameDefinition sequenceDefinition = EnsureSequenceQteDefinition();

            var managerSo = new SerializedObject(manager);
            var definitionsProp = managerSo.FindProperty("definitions");
            definitionsProp.ClearArray();
            definitionsProp.InsertArrayElementAtIndex(0);
            definitionsProp.GetArrayElementAtIndex(0).objectReferenceValue = definition;
            definitionsProp.InsertArrayElementAtIndex(1);
            definitionsProp.GetArrayElementAtIndex(1).objectReferenceValue = sequenceDefinition;
            managerSo.ApplyModifiedProperties();

            var qteSo = new SerializedObject(qte);
            qteSo.FindProperty("view").objectReferenceValue = view;
            qteSo.ApplyModifiedProperties();

            var sequenceQteSo = new SerializedObject(sequenceQte);
            sequenceQteSo.FindProperty("view").objectReferenceValue = sequenceView;
            sequenceQteSo.ApplyModifiedProperties();

            MiniGameTestLauncher testLauncher = EnsureMiniGameSceneTestLauncher(manager, testStatus);

            EditorUtility.SetDirty(manager);
            EditorUtility.SetDirty(qte);
            EditorUtility.SetDirty(view);
            EditorUtility.SetDirty(sequenceQte);
            EditorUtility.SetDirty(sequenceView);
            if (testLauncher != null) EditorUtility.SetDirty(testLauncher);
            EditorUtility.SetDirty(definition);
            EditorUtility.SetDirty(sequenceDefinition);
            EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
            Selection.activeGameObject = manager.gameObject;

            Debug.Log("[VN Tools] 현재 열린 씬에 MiniGameCanvas, MiniGameManager, QTE Definition을 구성했습니다.");
        }

        private static Canvas EnsureCanvas()
        {
            GameObject canvasObject = GameObject.Find("MiniGameCanvas");
            if (canvasObject == null)
            {
                canvasObject = new GameObject("MiniGameCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                Undo.RegisterCreatedObjectUndo(canvasObject, "Create MiniGame Canvas");
            }

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            return canvas;
        }

        private static MiniGameManager EnsureManager()
        {
            GameObject managerObject = GameObject.Find("MiniGameManager");
            if (managerObject == null)
            {
                managerObject = new GameObject("MiniGameManager");
                Undo.RegisterCreatedObjectUndo(managerObject, "Create MiniGame Manager");
            }

            MiniGameManager manager = managerObject.GetComponent<MiniGameManager>();
            if (manager == null) manager = managerObject.AddComponent<MiniGameManager>();
            return manager;
        }

        private static QTEView EnsureQtePanel(Transform canvasTransform)
        {
            GameObject panel = canvasTransform.Find("QTEPanel")?.gameObject;
            if (panel == null)
            {
                panel = CreateUIObject("QTEPanel", canvasTransform, typeof(Image), typeof(QTEView));
                Undo.RegisterCreatedObjectUndo(panel, "Create QTE Panel");
            }

            Stretch(panel.GetComponent<RectTransform>());
            Image panelImage = panel.GetComponent<Image>();
            panelImage.color = new Color(0.035f, 0.038f, 0.05f, 0.96f);

            Image background = EnsureImage(panel.transform, "Background", new Color(0.08f, 0.09f, 0.13f, 1f));
            Stretch(background.rectTransform);

            TMP_Text prompt = EnsureText(panel.transform, "PromptText", "공격을 막아내세요", 42f, Color.white);
            SetAnchored(prompt.rectTransform, new Vector2(0.18f, 0.68f), new Vector2(0.82f, 0.82f), Vector2.zero, Vector2.zero);

            Image keyImage = EnsureImage(panel.transform, "KeyImage", new Color(1f, 1f, 1f, 0.12f));
            SetCentered(keyImage.rectTransform, new Vector2(0f, 40f), new Vector2(220f, 140f));

            TMP_Text keyText = EnsureText(keyImage.transform, "KeyText", "SPACE", 44f, Color.white);
            Stretch(keyText.rectTransform);

            GameObject timerFrame = panel.transform.Find("TimerFrame")?.gameObject;
            if (timerFrame == null)
            {
                timerFrame = CreateUIObject("TimerFrame", panel.transform, typeof(Image));
                Undo.RegisterCreatedObjectUndo(timerFrame, "Create QTE Timer Frame");
            }
            SetAnchored(timerFrame.GetComponent<RectTransform>(), new Vector2(0.28f, 0.28f), new Vector2(0.72f, 0.33f), Vector2.zero, Vector2.zero);
            timerFrame.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.16f);

            Image timerFill = EnsureImage(timerFrame.transform, "TimerFill", new Color(0.8f, 0.15f, 0.22f, 1f));
            Stretch(timerFill.rectTransform);
            timerFill.type = Image.Type.Filled;
            timerFill.fillMethod = Image.FillMethod.Horizontal;
            timerFill.fillOrigin = 0;
            timerFill.fillAmount = 1f;

            TMP_Text timerText = EnsureText(panel.transform, "TimerText", "5.0", 28f, new Color(0.9f, 0.9f, 0.92f, 1f));
            SetAnchored(timerText.rectTransform, new Vector2(0.28f, 0.22f), new Vector2(0.72f, 0.27f), Vector2.zero, Vector2.zero);

            TMP_Text result = EnsureText(panel.transform, "ResultText", "", 42f, Color.white);
            SetAnchored(result.rectTransform, new Vector2(0.18f, 0.08f), new Vector2(0.82f, 0.2f), Vector2.zero, Vector2.zero);

            QTEView view = panel.GetComponent<QTEView>();
            var viewSo = new SerializedObject(view);
            viewSo.FindProperty("root").objectReferenceValue = panel;
            viewSo.FindProperty("promptText").objectReferenceValue = prompt;
            viewSo.FindProperty("keyText").objectReferenceValue = keyText;
            viewSo.FindProperty("timerText").objectReferenceValue = timerText;
            viewSo.FindProperty("resultText").objectReferenceValue = result;
            viewSo.FindProperty("backgroundImage").objectReferenceValue = background;
            viewSo.FindProperty("keyImage").objectReferenceValue = keyImage;
            viewSo.FindProperty("timerFillImage").objectReferenceValue = timerFill;
            viewSo.ApplyModifiedProperties();

            panel.SetActive(false);
            return view;
        }

        private static TMP_Text EnsureMiniGameSceneTestStatus(Transform canvasTransform)
        {
            if (!IsMiniGameScene()) return null;

            TMP_Text status = EnsureText(canvasTransform, "MiniGameTestStatusText", "1: Single QTE   2: Sequence QTE", 22f, new Color(0.78f, 0.8f, 0.86f, 1f));
            SetAnchored(status.rectTransform, new Vector2(0.02f, 0.02f), new Vector2(0.48f, 0.1f), Vector2.zero, Vector2.zero);
            status.alignment = TextAlignmentOptions.BottomLeft;
            return status;
        }

        private static SequenceQTEView EnsureSequenceQtePanel(Transform canvasTransform)
        {
            GameObject panel = canvasTransform.Find("SequenceQTEPanel")?.gameObject;
            if (panel == null)
            {
                panel = CreateUIObject("SequenceQTEPanel", canvasTransform, typeof(Image), typeof(SequenceQTEView));
                Undo.RegisterCreatedObjectUndo(panel, "Create Sequence QTE Panel");
            }

            Stretch(panel.GetComponent<RectTransform>());
            Image panelImage = panel.GetComponent<Image>();
            panelImage.color = new Color(0.025f, 0.028f, 0.038f, 0.97f);

            TMP_Text prompt = EnsureText(panel.transform, "PromptText", "ENTER THE SEQUENCE", 38f, Color.white);
            SetAnchored(prompt.rectTransform, new Vector2(0.18f, 0.72f), new Vector2(0.82f, 0.84f), Vector2.zero, Vector2.zero);

            TMP_Text round = EnsureText(panel.transform, "RoundText", "Round 1 / 2", 26f, new Color(0.82f, 0.84f, 0.9f, 1f));
            SetAnchored(round.rectTransform, new Vector2(0.18f, 0.64f), new Vector2(0.82f, 0.7f), Vector2.zero, Vector2.zero);

            GameObject keyRow = panel.transform.Find("KeyRow")?.gameObject;
            if (keyRow == null)
            {
                keyRow = CreateUIObject("KeyRow", panel.transform);
                Undo.RegisterCreatedObjectUndo(keyRow, "Create Sequence QTE Key Row");
            }
            SetAnchored(keyRow.GetComponent<RectTransform>(), new Vector2(0.18f, 0.43f), new Vector2(0.82f, 0.58f), Vector2.zero, Vector2.zero);

            var keyTexts = new TMP_Text[12];
            for (int i = 0; i < keyTexts.Length; i++)
            {
                TMP_Text key = EnsureText(keyRow.transform, $"Key_{i + 1:00}", i < 5 ? "SPACE" : string.Empty, 28f, Color.white);
                float width = 1f / keyTexts.Length;
                SetAnchored(key.rectTransform, new Vector2(width * i + 0.004f, 0f), new Vector2(width * (i + 1) - 0.004f, 1f), Vector2.zero, Vector2.zero);
                keyTexts[i] = key;
            }

            GameObject timerFrame = panel.transform.Find("TimerFrame")?.gameObject;
            if (timerFrame == null)
            {
                timerFrame = CreateUIObject("TimerFrame", panel.transform, typeof(Image));
                Undo.RegisterCreatedObjectUndo(timerFrame, "Create Sequence QTE Timer Frame");
            }
            SetAnchored(timerFrame.GetComponent<RectTransform>(), new Vector2(0.25f, 0.31f), new Vector2(0.75f, 0.36f), Vector2.zero, Vector2.zero);
            timerFrame.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.16f);

            Image timerFill = EnsureImage(timerFrame.transform, "TimerFill", new Color(0.92f, 0.58f, 0.18f, 1f));
            Stretch(timerFill.rectTransform);
            timerFill.type = Image.Type.Filled;
            timerFill.fillMethod = Image.FillMethod.Horizontal;
            timerFill.fillOrigin = 0;
            timerFill.fillAmount = 1f;

            TMP_Text timer = EnsureText(panel.transform, "TimerText", "15.0", 28f, new Color(0.9f, 0.9f, 0.92f, 1f));
            SetAnchored(timer.rectTransform, new Vector2(0.25f, 0.24f), new Vector2(0.5f, 0.3f), Vector2.zero, Vector2.zero);

            TMP_Text mistakes = EnsureText(panel.transform, "MistakeText", "Mistakes 0 / 2", 28f, new Color(0.9f, 0.9f, 0.92f, 1f));
            SetAnchored(mistakes.rectTransform, new Vector2(0.5f, 0.24f), new Vector2(0.75f, 0.3f), Vector2.zero, Vector2.zero);

            TMP_Text result = EnsureText(panel.transform, "ResultText", "", 42f, Color.white);
            SetAnchored(result.rectTransform, new Vector2(0.18f, 0.08f), new Vector2(0.82f, 0.2f), Vector2.zero, Vector2.zero);

            SequenceQTEView view = panel.GetComponent<SequenceQTEView>();
            var viewSo = new SerializedObject(view);
            viewSo.FindProperty("root").objectReferenceValue = panel;
            viewSo.FindProperty("promptText").objectReferenceValue = prompt;
            viewSo.FindProperty("roundText").objectReferenceValue = round;
            viewSo.FindProperty("timerText").objectReferenceValue = timer;
            viewSo.FindProperty("mistakeText").objectReferenceValue = mistakes;
            viewSo.FindProperty("resultText").objectReferenceValue = result;
            viewSo.FindProperty("timerFillImage").objectReferenceValue = timerFill;

            var keyTextsProp = viewSo.FindProperty("keyTexts");
            keyTextsProp.ClearArray();
            for (int i = 0; i < keyTexts.Length; i++)
            {
                keyTextsProp.InsertArrayElementAtIndex(i);
                keyTextsProp.GetArrayElementAtIndex(i).objectReferenceValue = keyTexts[i];
            }
            viewSo.ApplyModifiedProperties();

            panel.SetActive(false);
            return view;
        }

        private static QTEMiniGame EnsureQteModule(Transform managerTransform, QTEView view)
        {
            Transform existing = managerTransform.Find("QTEMiniGame");
            GameObject qteObject = existing != null ? existing.gameObject : new GameObject("QTEMiniGame");
            if (existing == null)
            {
                Undo.RegisterCreatedObjectUndo(qteObject, "Create QTE MiniGame Module");
                qteObject.transform.SetParent(managerTransform, false);
            }

            QTEMiniGame qte = qteObject.GetComponent<QTEMiniGame>();
            if (qte == null) qte = qteObject.AddComponent<QTEMiniGame>();
            if (qteObject.GetComponent<AudioSource>() == null) qteObject.AddComponent<AudioSource>();

            var qteSo = new SerializedObject(qte);
            qteSo.FindProperty("view").objectReferenceValue = view;
            qteSo.ApplyModifiedProperties();
            return qte;
        }

        private static SequenceQTEMiniGame EnsureSequenceQteModule(Transform managerTransform, SequenceQTEView view)
        {
            Transform existing = managerTransform.Find("SequenceQTEMiniGame");
            GameObject qteObject = existing != null ? existing.gameObject : new GameObject("SequenceQTEMiniGame");
            if (existing == null)
            {
                Undo.RegisterCreatedObjectUndo(qteObject, "Create Sequence QTE MiniGame Module");
                qteObject.transform.SetParent(managerTransform, false);
            }

            SequenceQTEMiniGame qte = qteObject.GetComponent<SequenceQTEMiniGame>();
            if (qte == null) qte = qteObject.AddComponent<SequenceQTEMiniGame>();
            if (qteObject.GetComponent<AudioSource>() == null) qteObject.AddComponent<AudioSource>();

            var qteSo = new SerializedObject(qte);
            qteSo.FindProperty("view").objectReferenceValue = view;
            qteSo.ApplyModifiedProperties();
            return qte;
        }

        private static MiniGameTestLauncher EnsureMiniGameSceneTestLauncher(MiniGameManager manager, TMP_Text statusText)
        {
            if (!IsMiniGameScene()) return null;

            MiniGameTestLauncher launcher = manager.GetComponent<MiniGameTestLauncher>();
            if (launcher == null) launcher = manager.gameObject.AddComponent<MiniGameTestLauncher>();

            var launcherSo = new SerializedObject(launcher);
            launcherSo.FindProperty("manager").objectReferenceValue = manager;
            launcherSo.FindProperty("playOnStartMiniGameId").stringValue = DefaultSequenceQteId;
            launcherSo.FindProperty("playOnStart").boolValue = true;
            launcherSo.FindProperty("singleQteKey").intValue = (int)KeyCode.Alpha1;
            launcherSo.FindProperty("sequenceQteKey").intValue = (int)KeyCode.Alpha2;
            launcherSo.FindProperty("statusText").objectReferenceValue = statusText;
            launcherSo.ApplyModifiedProperties();

            return launcher;
        }

        private static QTEMiniGameDefinition EnsureQteDefinition()
        {
            Directory.CreateDirectory("Assets/ScriptObjects/MiniGames");
            QTEMiniGameDefinition definition = AssetDatabase.LoadAssetAtPath<QTEMiniGameDefinition>(QteDefinitionPath);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<QTEMiniGameDefinition>();
                AssetDatabase.CreateAsset(definition, QteDefinitionPath);
            }

            var so = new SerializedObject(definition);
            so.FindProperty("miniGameId").stringValue = DefaultQteId;
            so.FindProperty("resultVariableName").stringValue = "qte_defense_success";
            so.FindProperty("requiredKey").intValue = (int)KeyCode.Space;
            so.FindProperty("timeLimitSeconds").floatValue = 5f;
            so.FindProperty("successScore").intValue = 100;
            so.FindProperty("failScore").intValue = 0;
            so.FindProperty("successRank").stringValue = "Good";
            so.FindProperty("failRank").stringValue = "Fail";
            so.FindProperty("successFlag").stringValue = "qte_defense_success_flag";
            so.FindProperty("failFlag").stringValue = "qte_defense_fail_flag";
            so.FindProperty("promptText").stringValue = "공격을 막아내세요";
            so.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
            return definition;
        }

        private static SequenceQTEMiniGameDefinition EnsureSequenceQteDefinition()
        {
            Directory.CreateDirectory("Assets/ScriptObjects/MiniGames");
            SequenceQTEMiniGameDefinition definition = AssetDatabase.LoadAssetAtPath<SequenceQTEMiniGameDefinition>(SequenceDefinitionPath);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<SequenceQTEMiniGameDefinition>();
                AssetDatabase.CreateAsset(definition, SequenceDefinitionPath);
            }

            var so = new SerializedObject(definition);
            so.FindProperty("miniGameId").stringValue = DefaultSequenceQteId;
            so.FindProperty("resultVariableName").stringValue = "qte_sequence_success";

            var inputPool = so.FindProperty("inputPool");
            inputPool.ClearArray();
            SetKey(inputPool, 0, KeyCode.UpArrow);
            SetKey(inputPool, 1, KeyCode.DownArrow);
            SetKey(inputPool, 2, KeyCode.LeftArrow);
            SetKey(inputPool, 3, KeyCode.RightArrow);
            SetKey(inputPool, 4, KeyCode.Space);

            so.FindProperty("sequenceLength").intValue = 5;
            so.FindProperty("roundCount").intValue = 2;
            so.FindProperty("timeLimitSeconds").floatValue = 15f;
            so.FindProperty("mistakeLimit").intValue = 2;
            so.FindProperty("successScore").intValue = 200;
            so.FindProperty("failScore").intValue = 0;
            so.FindProperty("successRank").stringValue = "Good";
            so.FindProperty("failRank").stringValue = "Fail";
            so.FindProperty("successFlag").stringValue = "qte_sequence_success_flag";
            so.FindProperty("failFlag").stringValue = "qte_sequence_fail_flag";
            so.FindProperty("promptText").stringValue = "Enter the sequence!";
            so.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
            return definition;
        }

        private static void SetKey(SerializedProperty array, int index, KeyCode key)
        {
            array.InsertArrayElementAtIndex(index);
            array.GetArrayElementAtIndex(index).intValue = (int)key;
        }

        private static bool IsMiniGameScene()
        {
            return string.Equals(SceneManager.GetActiveScene().name, "MiniGameScene", System.StringComparison.OrdinalIgnoreCase);
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindFirstObjectByType<EventSystem>(FindObjectsInactive.Include) != null) return;

            GameObject eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            Undo.RegisterCreatedObjectUndo(eventSystem, "Create EventSystem");
        }

        private static Image EnsureImage(Transform parent, string name, Color color)
        {
            GameObject imageObject = parent.Find(name)?.gameObject;
            if (imageObject == null)
            {
                imageObject = CreateUIObject(name, parent, typeof(Image));
                Undo.RegisterCreatedObjectUndo(imageObject, "Create QTE Image");
            }

            Image image = imageObject.GetComponent<Image>();
            image.color = color;
            return image;
        }

        private static TMP_Text EnsureText(Transform parent, string name, string text, float fontSize, Color color)
        {
            GameObject textObject = parent.Find(name)?.gameObject;
            if (textObject == null)
            {
                textObject = CreateUIObject(name, parent, typeof(TextMeshProUGUI));
                Undo.RegisterCreatedObjectUndo(textObject, "Create QTE Text");
            }

            TextMeshProUGUI tmp = textObject.GetComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = color;
            tmp.raycastTarget = false;
            VNKoreanFontFallback.ApplyTo(tmp);
            return tmp;
        }

        private static GameObject CreateUIObject(string name, Transform parent, params System.Type[] components)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform));
            foreach (System.Type component in components)
            {
                if (component != typeof(RectTransform)) obj.AddComponent(component);
            }

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

        private static void SetCentered(RectTransform rect, Vector2 position, Vector2 size)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        private static void SetAnchored(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }
    }
}
