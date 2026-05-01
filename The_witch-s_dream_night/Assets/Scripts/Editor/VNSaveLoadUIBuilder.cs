using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace VN.Editor
{
    public static class VNSaveLoadUIBuilder
    {
        [MenuItem("VN Tools/Build SaveLoad UI Only")]
        public static void BuildFullUI()
        {
            var uiController = Object.FindFirstObjectByType<VNUIController>();
            Canvas canvas = uiController != null
                ? uiController.GetComponentInParent<Canvas>()
                : Object.FindFirstObjectByType<Canvas>();

            if (canvas == null)
            {
                var canvasObject = new GameObject("VN_Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvas = canvasObject.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }

            Transform oldPanel = canvas.transform.Find("SaveLoadPanel");
            if (oldPanel != null) Object.DestroyImmediate(oldPanel.gameObject);

            GameObject panelObject = CreatePanel(canvas.transform, "SaveLoadPanel", new Color(0.1f, 0.1f, 0.12f, 0.95f));
            var saveLoadPanel = panelObject.AddComponent<VNSaveLoadPanel>();

            GameObject tabs = new GameObject("Tabs", typeof(RectTransform));
            tabs.transform.SetParent(panelObject.transform, false);
            tabs.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 400f);
            var tabLayout = tabs.AddComponent<HorizontalLayoutGroup>();
            tabLayout.spacing = 50f;
            tabLayout.childAlignment = TextAnchor.MiddleCenter;

            Button saveTab = CreateBtn(tabs.transform, "Tab_Save", "SAVE", new Vector2(200f, 80f));
            Button loadTab = CreateBtn(tabs.transform, "Tab_Load", "LOAD", new Vector2(200f, 80f));

            GameObject scrollView = CreateScrollView(panelObject.transform);
            Transform gridContent = scrollView.GetComponent<ScrollRect>().content;
            var grid = gridContent.gameObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(350f, 200f);
            grid.spacing = new Vector2(40f, 40f);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 3;
            grid.padding = new RectOffset(50, 50, 50, 50);
            grid.childAlignment = TextAnchor.UpperCenter;

            Button closeButton = CreateBtn(panelObject.transform, "Btn_Close", "CLOSE", new Vector2(150f, 50f));
            closeButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -450f);

            GameObject slotTemplate = CreateSlotTemplate();
            slotTemplate.transform.SetParent(uiController != null ? uiController.transform : canvas.transform, false);
            slotTemplate.name = "SaveSlotItem_Template";
            slotTemplate.SetActive(false);

            var panelObjectData = new SerializedObject(saveLoadPanel);
            panelObjectData.FindProperty("director").objectReferenceValue = Object.FindFirstObjectByType<VNDirector>();
            panelObjectData.FindProperty("slotRoot").objectReferenceValue = gridContent;
            panelObjectData.FindProperty("slotPrefab").objectReferenceValue = slotTemplate;
            panelObjectData.FindProperty("closeButton").objectReferenceValue = closeButton;
            panelObjectData.FindProperty("saveTabButton").objectReferenceValue = saveTab;
            panelObjectData.FindProperty("loadTabButton").objectReferenceValue = loadTab;
            panelObjectData.ApplyModifiedProperties();

            if (uiController != null)
            {
                var uiObject = new SerializedObject(uiController);
                uiObject.FindProperty("saveLoadPanel").objectReferenceValue = saveLoadPanel;
                uiObject.ApplyModifiedProperties();
            }

            panelObject.SetActive(false);
            Debug.Log("<color=green>[VN Tools] SaveLoad UI build complete.</color>");
        }

        private static GameObject CreatePanel(Transform parent, string name, Color color)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Image));
            obj.transform.SetParent(parent, false);

            var rectTransform = obj.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            obj.GetComponent<Image>().color = color;
            return obj;
        }

        private static Button CreateBtn(Transform parent, string name, string label, Vector2 size)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            obj.transform.SetParent(parent, false);
            obj.GetComponent<RectTransform>().sizeDelta = size;
            obj.GetComponent<Image>().color = new Color(0.25f, 0.25f, 0.25f, 1f);

            GameObject textObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(obj.transform, false);

            var text = textObject.GetComponent<TextMeshProUGUI>();
            text.text = label;
            text.fontSize = 24f;
            text.alignment = TextAlignmentOptions.Center;
            textObject.GetComponent<RectTransform>().sizeDelta = size;

            return obj.GetComponent<Button>();
        }

        private static GameObject CreateScrollView(Transform parent)
        {
            GameObject scrollObject = new GameObject("SlotGrid", typeof(RectTransform), typeof(ScrollRect), typeof(Image));
            scrollObject.transform.SetParent(parent, false);
            scrollObject.GetComponent<RectTransform>().sizeDelta = new Vector2(1200f, 700f);
            scrollObject.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.1f);

            GameObject viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Mask), typeof(Image));
            viewport.transform.SetParent(scrollObject.transform, false);
            var viewportRect = viewport.GetComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = Vector2.zero;
            viewport.GetComponent<Image>().color = Color.white;
            viewport.GetComponent<Mask>().showMaskGraphic = false;

            GameObject content = new GameObject("Content", typeof(RectTransform));
            content.transform.SetParent(viewport.transform, false);
            var contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0.5f, 1f);
            contentRect.anchorMax = new Vector2(0.5f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.sizeDelta = new Vector2(1100f, 800f);

            ScrollRect scrollRect = scrollObject.GetComponent<ScrollRect>();
            scrollRect.viewport = viewportRect;
            scrollRect.content = contentRect;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;

            return scrollObject;
        }

        private static GameObject CreateSlotTemplate()
        {
            GameObject slot = new GameObject("SaveSlotItem", typeof(RectTransform), typeof(Image), typeof(Button), typeof(VNSaveSlotItem));
            slot.GetComponent<RectTransform>().sizeDelta = new Vector2(350f, 200f);
            slot.GetComponent<Image>().color = new Color(0.18f, 0.18f, 0.2f, 1f);

            GameObject plus = new GameObject("PlusIcon", typeof(RectTransform), typeof(TextMeshProUGUI));
            plus.transform.SetParent(slot.transform, false);
            var plusText = plus.GetComponent<TextMeshProUGUI>();
            plusText.text = "+";
            plusText.fontSize = 80f;
            plusText.alignment = TextAlignmentOptions.Center;
            plusText.color = new Color(1f, 1f, 1f, 0.3f);
            plus.GetComponent<RectTransform>().sizeDelta = new Vector2(100f, 100f);

            GameObject contentGroup = new GameObject("ContentGroup", typeof(RectTransform));
            contentGroup.transform.SetParent(slot.transform, false);
            var contentRect = contentGroup.GetComponent<RectTransform>();
            contentRect.anchorMin = Vector2.zero;
            contentRect.anchorMax = Vector2.one;
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;
            var contentLayout = contentGroup.AddComponent<HorizontalLayoutGroup>();
            contentLayout.padding = new RectOffset(18, 18, 18, 18);
            contentLayout.spacing = 18f;
            contentLayout.childAlignment = TextAnchor.UpperLeft;
            contentLayout.childControlWidth = true;
            contentLayout.childControlHeight = false;
            contentLayout.childForceExpandWidth = false;
            contentLayout.childForceExpandHeight = false;

            GameObject thumb = new GameObject("Thumbnail", typeof(RectTransform), typeof(Image));
            thumb.transform.SetParent(contentGroup.transform, false);
            var thumbRect = thumb.GetComponent<RectTransform>();
            thumbRect.sizeDelta = new Vector2(124f, 124f);
            var thumbLayout = thumb.AddComponent<LayoutElement>();
            thumbLayout.minWidth = 124f;
            thumbLayout.preferredWidth = 124f;
            thumbLayout.minHeight = 124f;
            thumbLayout.preferredHeight = 124f;
            thumb.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.1f);
            thumb.GetComponent<Image>().preserveAspect = true;

            GameObject infoColumn = new GameObject("InfoColumn", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement));
            infoColumn.transform.SetParent(contentGroup.transform, false);
            var infoLayout = infoColumn.GetComponent<VerticalLayoutGroup>();
            infoLayout.padding = new RectOffset(0, 0, 0, 0);
            infoLayout.spacing = 8f;
            infoLayout.childAlignment = TextAnchor.UpperLeft;
            infoLayout.childControlWidth = true;
            infoLayout.childControlHeight = false;
            infoLayout.childForceExpandWidth = true;
            infoLayout.childForceExpandHeight = false;
            var infoElement = infoColumn.GetComponent<LayoutElement>();
            infoElement.flexibleWidth = 1f;
            infoElement.preferredWidth = 170f;

            GameObject title = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
            title.transform.SetParent(infoColumn.transform, false);
            var titleText = title.GetComponent<TextMeshProUGUI>();
            titleText.text = "Chapter 1";
            titleText.fontSize = 24f;
            titleText.color = Color.white;
            titleText.alignment = TextAlignmentOptions.TopLeft;
            titleText.textWrappingMode = TextWrappingModes.NoWrap;
            titleText.overflowMode = TextOverflowModes.Ellipsis;
            var titleRect = title.GetComponent<RectTransform>();
            titleRect.sizeDelta = new Vector2(0f, 34f);
            var titleLayout = title.AddComponent<LayoutElement>();
            titleLayout.minHeight = 34f;
            titleLayout.preferredHeight = 34f;

            GameObject time = new GameObject("Time", typeof(RectTransform), typeof(TextMeshProUGUI));
            time.transform.SetParent(infoColumn.transform, false);
            var timeText = time.GetComponent<TextMeshProUGUI>();
            timeText.text = "Playtime  00:00:00";
            timeText.fontSize = 16f;
            timeText.color = new Color(0.78f, 0.78f, 0.78f, 1f);
            timeText.alignment = TextAlignmentOptions.TopLeft;
            timeText.textWrappingMode = TextWrappingModes.NoWrap;
            timeText.overflowMode = TextOverflowModes.Ellipsis;
            time.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 28f);
            var timeLayout = time.AddComponent<LayoutElement>();
            timeLayout.minHeight = 28f;
            timeLayout.preferredHeight = 28f;

            GameObject date = new GameObject("Date", typeof(RectTransform), typeof(TextMeshProUGUI));
            date.transform.SetParent(infoColumn.transform, false);
            var dateText = date.GetComponent<TextMeshProUGUI>();
            dateText.text = "2026-04-18 12:34";
            dateText.fontSize = 14f;
            dateText.color = new Color(0.65f, 0.65f, 0.65f, 1f);
            dateText.alignment = TextAlignmentOptions.TopLeft;
            dateText.textWrappingMode = TextWrappingModes.NoWrap;
            dateText.overflowMode = TextOverflowModes.Ellipsis;
            date.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 24f);
            var dateLayout = date.AddComponent<LayoutElement>();
            dateLayout.minHeight = 24f;
            dateLayout.preferredHeight = 24f;

            var item = slot.GetComponent<VNSaveSlotItem>();
            var itemObject = new SerializedObject(item);
            itemObject.FindProperty("chapterTitleText").objectReferenceValue = titleText;
            itemObject.FindProperty("playTimeText").objectReferenceValue = timeText;
            itemObject.FindProperty("dateText").objectReferenceValue = dateText;
            itemObject.FindProperty("thumbnailImage").objectReferenceValue = thumb.GetComponent<Image>();
            itemObject.FindProperty("plusIcon").objectReferenceValue = plus;
            itemObject.FindProperty("contentGroup").objectReferenceValue = contentGroup;
            itemObject.FindProperty("actionButton").objectReferenceValue = slot.GetComponent<Button>();
            itemObject.ApplyModifiedProperties();

            return slot;
        }
    }
}
