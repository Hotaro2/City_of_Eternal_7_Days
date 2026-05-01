using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using VN;

namespace VN.Editor
{
    public static class VNUISetupTool
    {
        [MenuItem("VN Tools/Setup UI Modules (Final Fix)")]
        public static void SetupUI()
        {
            var ui = Object.FindFirstObjectByType<VNUIController>();
            if (ui == null) return;
            Undo.RecordObject(ui.gameObject, "Setup UI Fix");
            var typer = ui.GetComponent<VNTextTyper>() ?? ui.gameObject.AddComponent<VNTextTyper>();
            var choice = ui.GetComponent<VNChoicePanel>() ?? ui.gameObject.AddComponent<VNChoicePanel>();
            var backlog = Object.FindFirstObjectByType<VNBacklogManager>();
            SerializedObject so = new SerializedObject(ui);
            so.FindProperty("typer").objectReferenceValue = typer;
            so.FindProperty("choicePanel").objectReferenceValue = choice;
            so.FindProperty("backlogManager").objectReferenceValue = backlog;
            so.ApplyModifiedProperties();
        }

        [MenuItem("VN Tools/Setup Option UI (Paused & Applied Build)")]
        public static void SetupOptionUI() => BuildOptionUI(true);

        public static void BuildOptionUI(bool createMenuRoot)
        {
            var optionPanel = Object.FindFirstObjectByType<VNOptionPanel>(FindObjectsInactive.Include);
            if (optionPanel == null)
            {
                var canvas = Object.FindFirstObjectByType<Canvas>();
                if (canvas == null) return;
                GameObject obj = new GameObject("OptionPanel");
                obj.transform.SetParent(canvas.transform, false);
                optionPanel = obj.AddComponent<VNOptionPanel>();
            }

            Undo.RecordObject(optionPanel.gameObject, "Setup Option UI Final");
            var fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NanumSquareRoundB SDF.asset");
            for (int i = optionPanel.transform.childCount - 1; i >= 0; i--) Object.DestroyImmediate(optionPanel.transform.GetChild(i).gameObject);
            
            GameObject mainBox = CreateUIObject("MainFrame", optionPanel.transform, typeof(Image));
            mainBox.GetComponent<Image>().color = new Color(0.92f, 0.88f, 0.84f, 1f);
            var boxRT = mainBox.GetComponent<RectTransform>(); boxRT.sizeDelta = new Vector2(1000, 700);
            
            GameObject contentArea = CreateUIObject("ContentArea", mainBox.transform, typeof(HorizontalLayoutGroup));
            var cRT = contentArea.GetComponent<RectTransform>(); cRT.anchorMin = Vector2.zero; cRT.anchorMax = Vector2.one;
            cRT.offsetMin = new Vector2(40, 120); cRT.offsetMax = new Vector2(-40, -100);
            contentArea.GetComponent<HorizontalLayoutGroup>().spacing = 40; contentArea.GetComponent<HorizontalLayoutGroup>().childControlWidth = true;
            
            Transform col1 = CreateColumn(contentArea.transform, "Col1"); Transform col2 = CreateColumn(contentArea.transform, "Col2");
            Transform col3 = CreateColumn(contentArea.transform, "Col3"); Transform col4 = CreateColumn(contentArea.transform, "Col4");
            
            CreateLabel(col1, "Lbl1", "오디오 설정", fontAsset, 24, Color.black);
            Slider master = CreateDetailSlider(col1, "Master", "마스터 볼륨", fontAsset); Slider bgm = CreateDetailSlider(col1, "BGM", "배경음 볼륨", fontAsset); Slider sfx = CreateDetailSlider(col1, "SFX", "효과음 볼륨", fontAsset);
            CreateLabel(col2, "Lbl2", "화면 설정", fontAsset, 24, Color.black); TMP_Dropdown resDdl = CreateDropdown(col2, "ResDdl", fontAsset); Toggle fsTgl = CreateToggle(col2, "FsTgl", "전체 화면", fontAsset);
            CreateLabel(col3, "Lbl3", "게임플레이", fontAsset, 24, Color.black); Slider textSpeed = CreateDetailSlider(col3, "TextSpeed", "대사 속도", fontAsset); Slider autoWait = CreateDetailSlider(col3, "AutoWait", "자동 대기", fontAsset);
            CreateLabel(col4, "Lbl4", "시스템", fontAsset, 24, Color.black); CreateDetailButton(col4, "BtnInfo", "버전 정보", fontAsset); CreateDetailButton(col4, "BtnCredit", "크레딧", fontAsset);
            
            GameObject footer = CreateUIObject("Footer", mainBox.transform, typeof(HorizontalLayoutGroup));
            var fRT = footer.GetComponent<RectTransform>(); fRT.anchorMin = new Vector2(0, 0); fRT.anchorMax = new Vector2(1, 0);
            fRT.anchoredPosition = new Vector2(0, 30); fRT.sizeDelta = new Vector2(-60, 60);
            footer.GetComponent<HorizontalLayoutGroup>().spacing = 20; footer.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleCenter;
            
            Button applyBtn = CreateDetailButton(footer.transform, "BtnApply", "설정 적용 (APPLY)", fontAsset);
            Button closeBtn = CreateDetailButton(footer.transform, "BtnClose", "닫기 (CLOSE)", fontAsset);
            
            GameObject menuRoot = null;
            Button mSave = null, mLoad = null, mOpt = null, mRes = null, mQuit = null;

            if (createMenuRoot)
            {
                menuRoot = CreateUIObject("MenuRoot", optionPanel.transform, typeof(VerticalLayoutGroup));
                var mRT = menuRoot.GetComponent<RectTransform>(); mRT.sizeDelta = new Vector2(350, 450);
                var mVLG = menuRoot.GetComponent<VerticalLayoutGroup>(); mVLG.spacing = 15; mVLG.childAlignment = TextAnchor.MiddleCenter;
                mSave = CreateDetailButton(menuRoot.transform, "MSave", "SAVE", fontAsset); 
                mLoad = CreateDetailButton(menuRoot.transform, "MLoad", "LOAD", fontAsset);
                mOpt = CreateDetailButton(menuRoot.transform, "MOpt", "OPTIONS", fontAsset); 
                mRes = CreateDetailButton(menuRoot.transform, "MResume", "RESUME", fontAsset); 
                mQuit = CreateDetailButton(menuRoot.transform, "MQuit", "QUIT", fontAsset);
            }
            
            SerializedObject so = new SerializedObject(optionPanel);
            so.FindProperty("menuRoot").objectReferenceValue = menuRoot; so.FindProperty("settingsRoot").objectReferenceValue = mainBox;
            so.FindProperty("ui").objectReferenceValue = Object.FindFirstObjectByType<VNUIController>(); so.FindProperty("presenter").objectReferenceValue = Object.FindFirstObjectByType<VNPresenter>();
            if (createMenuRoot)
            {
                so.FindProperty("saveMenuButton").objectReferenceValue = mSave; so.FindProperty("loadMenuButton").objectReferenceValue = mLoad;
                so.FindProperty("openSettingsButton").objectReferenceValue = mOpt; so.FindProperty("resumeButton").objectReferenceValue = mRes; so.FindProperty("quitButton").objectReferenceValue = mQuit;
            }
            so.FindProperty("applyButton").objectReferenceValue = applyBtn; so.FindProperty("closeButton").objectReferenceValue = closeBtn;
            so.FindProperty("masterSlider").objectReferenceValue = master; so.FindProperty("bgmSlider").objectReferenceValue = bgm; so.FindProperty("sfxSlider").objectReferenceValue = sfx;
            so.FindProperty("textSpeedSlider").objectReferenceValue = textSpeed; so.FindProperty("autoWaitSlider").objectReferenceValue = autoWait;
            so.FindProperty("resolutionDropdown").objectReferenceValue = resDdl; so.FindProperty("fullscreenToggle").objectReferenceValue = fsTgl;
            so.ApplyModifiedProperties();
            
            optionPanel.gameObject.SetActive(false);
        }

        [MenuItem("VN Tools/Setup Title Scene (Pro Detailed Build)")]
        public static void SetupTitleUI()
        {
            if (Object.FindFirstObjectByType<EventSystem>() == null) new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

            var canvasObj = GameObject.Find("TitleCanvas");
            if (canvasObj == null)
            {
                canvasObj = new GameObject("TitleCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvasObj.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.GetComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasObj.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
            }

            var manager = Object.FindFirstObjectByType<VNTitleManager>() ?? new GameObject("TitleManager").AddComponent<VNTitleManager>();
            Undo.RecordObject(manager.gameObject, "Setup Title UI Pro");

            for (int i = canvasObj.transform.childCount - 1; i >= 0; i--) 
            {
                var child = canvasObj.transform.GetChild(i);
                if (child.name != "OptionPanel" && child.name != "SaveLoadPanel")
                    Object.DestroyImmediate(child.gameObject);
            }

            GameObject bgImg = CreateUIObject("BackgroundImage", canvasObj.transform, typeof(Image));
            bgImg.GetComponent<Image>().color = new Color(0.1f, 0.1f, 0.12f, 1f);
            bgImg.GetComponent<RectTransform>().anchorMin = Vector2.zero; bgImg.GetComponent<RectTransform>().anchorMax = Vector2.one;
            bgImg.GetComponent<RectTransform>().offsetMin = bgImg.GetComponent<RectTransform>().offsetMax = Vector2.zero;

            var fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/NanumSquareRoundB SDF.asset");
            GameObject logo = CreateUIObject("Logo", canvasObj.transform, typeof(TextMeshProUGUI));
            var logoTmp = logo.GetComponent<TextMeshProUGUI>();
            logoTmp.text = "THE WITCH'S DREAM NIGHT"; logoTmp.font = fontAsset; logoTmp.fontSize = 90; logoTmp.alignment = TextAlignmentOptions.Center;
            logo.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 300); logo.GetComponent<RectTransform>().sizeDelta = new Vector2(1400, 200);

            GameObject menuGrp = CreateUIObject("MenuButtons", canvasObj.transform, typeof(VerticalLayoutGroup));
            var mRT = menuGrp.GetComponent<RectTransform>(); mRT.sizeDelta = new Vector2(450, 500); mRT.anchoredPosition = new Vector2(0, -150);
            var vlg = menuGrp.GetComponent<VerticalLayoutGroup>(); vlg.spacing = 20; vlg.childAlignment = TextAnchor.MiddleCenter; vlg.childControlHeight = false; vlg.childControlWidth = true;

            Button btnStart = CreateProMenuButton(menuGrp.transform, "StartBtn", "NEW GAME", fontAsset);
            Button btnCont = CreateProMenuButton(menuGrp.transform, "ContinueBtn", "CONTINUE", fontAsset);
            Button btnLoad = CreateProMenuButton(menuGrp.transform, "LoadBtn", "LOAD GAME", fontAsset);
            Button btnOpt = CreateProMenuButton(menuGrp.transform, "OptionBtn", "OPTIONS", fontAsset);
            Button btnQuit = CreateProMenuButton(menuGrp.transform, "QuitBtn", "QUIT", fontAsset);

            GameObject ver = CreateUIObject("VersionText", canvasObj.transform, typeof(TextMeshProUGUI));
            var verTmp = ver.GetComponent<TextMeshProUGUI>();
            verTmp.text = "DEVELOPMENT BUILD v1.0"; verTmp.font = fontAsset; verTmp.fontSize = 24; verTmp.alignment = TextAlignmentOptions.BottomRight;
            var verRT = ver.GetComponent<RectTransform>(); verRT.anchorMin = verRT.anchorMax = new Vector2(1, 0); verRT.pivot = new Vector2(1, 0); verRT.anchoredPosition = new Vector2(-30, 30);

            GameObject fader = CreateUIObject("SceneFader", canvasObj.transform, typeof(Image));
            var faderImg = fader.GetComponent<Image>(); faderImg.color = Color.black; faderImg.raycastTarget = false;
            fader.GetComponent<RectTransform>().anchorMin = Vector2.zero; fader.GetComponent<RectTransform>().anchorMax = Vector2.one; fader.GetComponent<RectTransform>().offsetMin = fader.GetComponent<RectTransform>().offsetMax = Vector2.zero;

            // 타이틀 씬은 MenuRoot 없는 옵션 패널 생성
            BuildOptionUI(false);
            VNSaveLoadUIBuilder.BuildFullUI();

            var optionPanel = Object.FindFirstObjectByType<VNOptionPanel>(FindObjectsInactive.Include);
            var saveLoadPanel = Object.FindFirstObjectByType<VNSaveLoadPanel>(FindObjectsInactive.Include);

            SerializedObject so = new SerializedObject(manager);
            so.FindProperty("startButton").objectReferenceValue = btnStart;
            so.FindProperty("continueButton").objectReferenceValue = btnCont;
            so.FindProperty("loadButton").objectReferenceValue = btnLoad;
            so.FindProperty("optionButton").objectReferenceValue = btnOpt;
            so.FindProperty("quitButton").objectReferenceValue = btnQuit;
            so.FindProperty("fader").objectReferenceValue = faderImg;
            so.FindProperty("versionText").objectReferenceValue = verTmp;
            so.FindProperty("optionPanel").objectReferenceValue = optionPanel;
            so.FindProperty("saveLoadPanel").objectReferenceValue = saveLoadPanel;
            so.ApplyModifiedProperties();

            // 이벤트 정리 및 강제 연결
            Button[] buttons = { btnStart, btnCont, btnLoad, btnOpt, btnQuit };
            foreach (var b in buttons) {
                int count = b.onClick.GetPersistentEventCount();
                for (int i = count - 1; i >= 0; i--) UnityEditor.Events.UnityEventTools.RemovePersistentListener(b.onClick, i);
            }
            UnityEditor.Events.UnityEventTools.AddPersistentListener(btnOpt.onClick, optionPanel.ShowSettings);
            
            Debug.Log("<color=orange>[VN Tools] 타이틀 화면 리빌드 완료! MenuRoot가 제거된 설정 전용 패널이 셋업되었습니다.</color>");
        }

        private static Button CreateProMenuButton(Transform parent, string name, string label, TMP_FontAsset font)
        {
            Button btn = CreateDetailButton(parent, name, label, font);
            btn.gameObject.AddComponent<VNButtonEffects>();
            return btn;
        }

        private static GameObject CreateUIObject(string name, Transform parent, params System.Type[] components)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform));
            foreach (var c in components) obj.AddComponent(c);
            obj.transform.SetParent(parent, false);
            return obj;
        }

        private static Transform CreateColumn(Transform parent, string name)
        {
            GameObject col = CreateUIObject(name, parent, typeof(VerticalLayoutGroup));
            col.GetComponent<VerticalLayoutGroup>().spacing = 20;
            col.GetComponent<VerticalLayoutGroup>().childControlHeight = false;
            col.GetComponent<VerticalLayoutGroup>().childControlWidth = true;
            return col.transform;
        }

        private static void CreateLabel(Transform parent, string name, string text, TMP_FontAsset font, float size, Color color)
        {
            GameObject obj = CreateUIObject(name, parent, typeof(TextMeshProUGUI));
            var tmp = obj.GetComponent<TextMeshProUGUI>();
            tmp.text = text; tmp.font = font; tmp.fontSize = size; tmp.color = color;
        }

        private static Slider CreateDetailSlider(Transform parent, string name, string label, TMP_FontAsset font)
        {
            CreateLabel(parent, name + "_L", label, font, 18, Color.black);
            GameObject sObj = CreateUIObject(name, parent, typeof(Slider));
            sObj.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 25);
            var slider = sObj.GetComponent<Slider>();
            GameObject bg = CreateUIObject("BG", sObj.transform, typeof(Image));
            bg.GetComponent<Image>().color = Color.gray;
            var bgRT = bg.GetComponent<RectTransform>(); bgRT.anchorMin = new Vector2(0, 0.4f); bgRT.anchorMax = new Vector2(1, 0.6f); bgRT.sizeDelta = Vector2.zero;
            GameObject fArea = CreateUIObject("FA", sObj.transform);
            var faRT = fArea.GetComponent<RectTransform>(); faRT.anchorMin = new Vector2(0, 0.4f); faRT.anchorMax = new Vector2(1, 0.6f); faRT.sizeDelta = new Vector2(-10, 0);
            GameObject fill = CreateUIObject("F", fArea.transform, typeof(Image));
            fill.GetComponent<Image>().color = new Color(0.4f, 0.3f, 0.2f); fill.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
            GameObject hArea = CreateUIObject("HA", sObj.transform);
            var haRT = hArea.GetComponent<RectTransform>(); haRT.anchorMin = Vector2.zero; haRT.anchorMax = Vector2.one; haRT.sizeDelta = new Vector2(-10, 0);
            GameObject handle = CreateUIObject("H", hArea.transform, typeof(Image));
            handle.GetComponent<Image>().color = Color.white; handle.GetComponent<RectTransform>().sizeDelta = new Vector2(20, 20);
            slider.fillRect = fill.GetComponent<RectTransform>(); slider.handleRect = handle.GetComponent<RectTransform>(); slider.targetGraphic = handle.GetComponent<Image>();
            return slider;
        }

        private static Button CreateDetailButton(Transform parent, string name, string label, TMP_FontAsset font)
        {
            GameObject obj = CreateUIObject(name, parent, typeof(Image), typeof(Button));
            obj.GetComponent<Image>().color = new Color(0.3f, 0.25f, 0.2f);
            obj.GetComponent<RectTransform>().sizeDelta = new Vector2(160, 50);
            GameObject txt = CreateUIObject("T", obj.transform, typeof(TextMeshProUGUI));
            var tmp = txt.GetComponent<TextMeshProUGUI>();
            tmp.text = label; tmp.font = font; tmp.fontSize = 20; tmp.color = Color.white; tmp.alignment = TextAlignmentOptions.Center;
            txt.GetComponent<RectTransform>().anchorMin = Vector2.zero; txt.GetComponent<RectTransform>().anchorMax = Vector2.one; txt.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
            return obj.GetComponent<Button>();
        }

        private static TMP_Dropdown CreateDropdown(Transform parent, string name, TMP_FontAsset font)
        {
            GameObject obj = CreateUIObject(name, parent, typeof(Image), typeof(TMP_Dropdown));
            obj.GetComponent<Image>().color = Color.white;
            obj.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 35);
            GameObject lbl = CreateUIObject("L", obj.transform, typeof(TextMeshProUGUI));
            var tmp = lbl.GetComponent<TextMeshProUGUI>();
            tmp.text = "Loading..."; tmp.font = font; tmp.fontSize = 16; tmp.color = Color.black;
            lbl.GetComponent<RectTransform>().offsetMin = new Vector2(10, 0);
            obj.GetComponent<TMP_Dropdown>().captionText = tmp; return obj.GetComponent<TMP_Dropdown>();
        }

        private static Toggle CreateToggle(Transform parent, string name, string text, TMP_FontAsset font)
        {
            GameObject row = CreateUIObject(name + "R", parent, typeof(HorizontalLayoutGroup));
            row.GetComponent<HorizontalLayoutGroup>().spacing = 10;
            GameObject bg = CreateUIObject("BG", row.transform, typeof(Image), typeof(Toggle));
            bg.GetComponent<Image>().color = Color.white; bg.GetComponent<RectTransform>().sizeDelta = new Vector2(25, 25);
            GameObject ck = CreateUIObject("C", bg.transform, typeof(Image));
            ck.GetComponent<Image>().color = Color.black; ck.GetComponent<RectTransform>().sizeDelta = new Vector2(15, 15);
            bg.GetComponent<Toggle>().graphic = ck.GetComponent<Image>();
            CreateLabel(row.transform, "L", text, font, 16, Color.black);
            return bg.GetComponent<Toggle>();
        }
    }
}
