using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using VN;
using System.Linq;
using UnityEditor.Events;

public class VNBacklogSetupTool : EditorWindow
{
    [MenuItem("VN Tools/Setup Final Improved Backlog (Left Aligned)")]
    public static void Setup()
    {
        // 1. 기존 잔재 청소
        string[] oldNames = { "BacklogRoot_Final", "BacklogRoot_Improved", "BacklogRoot_v3", "ImprovedBacklog" };
        foreach (var name in oldNames) { GameObject old = GameObject.Find(name); if (old != null) DestroyImmediate(old); }

        // 2. 한글 폰트 검색
        TMP_FontAsset koreanFont = AssetDatabase.FindAssets("t:TMP_FontAsset")
            .Select(guid => AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(guid)))
            .FirstOrDefault(f => f.name != "LiberationSans SDF");

        // 3. 백로그 항목 프리팹 (좌측 정렬 가로 레이아웃)
        string prefabPath = "Assets/Prefabs/BacklogEntryPrefab.prefab";
        GameObject entryObj = new GameObject("BacklogEntry_Template", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(ContentSizeFitter), typeof(VNBacklogEntry));
        
        var hlg = entryObj.GetComponent<HorizontalLayoutGroup>();
        hlg.childControlWidth = true; hlg.childControlHeight = true; hlg.childForceExpandWidth = false; hlg.childForceExpandHeight = false;
        hlg.childAlignment = TextAnchor.UpperLeft; // 전체 요소를 왼쪽 상단으로 정렬
        hlg.padding = new RectOffset(30, 30, 15, 15); hlg.spacing = 25;
        entryObj.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // 화자 (왼쪽 고정 폭, 좌측 정렬)
        GameObject sObj = new GameObject("Speaker", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
        sObj.transform.SetParent(entryObj.transform);
        var sText = sObj.GetComponent<TextMeshProUGUI>(); if (koreanFont != null) sText.font = koreanFont;
        sText.fontSize = 30; sText.fontStyle = FontStyles.Bold; sText.color = new Color(1f, 0.85f, 0.3f);
        sText.alignment = TextAlignmentOptions.TopLeft; // 화자 이름 왼쪽 정렬
        sText.text = "Speaker";
        var sLe = sObj.GetComponent<LayoutElement>();
        sLe.minWidth = 180; sLe.preferredWidth = 180; sLe.flexibleWidth = 0;

        // 내용 (오른쪽 자동 줄바꿈, 좌측 정렬)
        GameObject cObj = new GameObject("Content", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
        cObj.transform.SetParent(entryObj.transform);
        var cText = cObj.GetComponent<TextMeshProUGUI>(); if (koreanFont != null) cText.font = koreanFont;
        cText.fontSize = 28; cText.color = Color.white; 
        cText.alignment = TextAlignmentOptions.TopLeft; // 대화 내용 왼쪽 정렬
        cText.textWrappingMode = TextWrappingModes.Normal;
        cObj.GetComponent<LayoutElement>().flexibleWidth = 1;

        // 참조 연결
        var entryScript = entryObj.GetComponent<VNBacklogEntry>();
        var soEntry = new SerializedObject(entryScript);
        soEntry.FindProperty("speakerText").objectReferenceValue = sText;
        soEntry.FindProperty("contentText").objectReferenceValue = cText;
        soEntry.ApplyModifiedProperties();

        GameObject prefabAsset = PrefabUtility.SaveAsPrefabAsset(entryObj, prefabPath);
        DestroyImmediate(entryObj);

        // 4. 최상위 UI 구조 구축
        VNUIController ui = Object.FindFirstObjectByType<VNUIController>();
        VNBacklogManager manager = ui.GetComponent<VNBacklogManager>() ?? ui.gameObject.AddComponent<VNBacklogManager>();

        GameObject root = new GameObject("BacklogRoot_Final", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster));
        root.transform.SetParent(null);
        var canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.overrideSorting = true; canvas.sortingOrder = 32000;

        // 배경 닫기 버튼
        GameObject bg = new GameObject("Background_Close", typeof(RectTransform), typeof(Image), typeof(Button));
        bg.transform.SetParent(root.transform, false);
        bg.GetComponent<Image>().color = new Color(0, 0, 0, 0.96f);
        bg.GetComponent<RectTransform>().anchorMin = Vector2.zero; bg.GetComponent<RectTransform>().anchorMax = Vector2.one; bg.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
        UnityEventTools.AddPersistentListener(bg.GetComponent<Button>().onClick, manager.ToggleBacklog);

        // 스크롤 뷰 (중앙 배치)
        GameObject scrollView = new GameObject("BacklogScroll", typeof(RectTransform), typeof(ScrollRect));
        scrollView.transform.SetParent(root.transform, false);
        var sr = scrollView.GetComponent<ScrollRect>(); sr.horizontal = false;
        var rtScroll = scrollView.GetComponent<RectTransform>();
        rtScroll.anchorMin = new Vector2(0.05f, 0.1f); rtScroll.anchorMax = new Vector2(0.95f, 0.9f); rtScroll.sizeDelta = Vector2.zero;

        GameObject viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
        viewport.transform.SetParent(scrollView.transform, false);
        viewport.GetComponent<RectTransform>().anchorMin = Vector2.zero; viewport.GetComponent<RectTransform>().anchorMax = Vector2.one; viewport.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
        viewport.GetComponent<Image>().color = new Color(0,0,0,0);
        sr.viewport = viewport.GetComponent<RectTransform>();

        // Content (좌측 정렬 강제)
        GameObject content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        content.transform.SetParent(viewport.transform, false);
        var rtContent = content.GetComponent<RectTransform>(); rtContent.anchorMin = new Vector2(0, 1); rtContent.anchorMax = new Vector2(1, 1); rtContent.pivot = new Vector2(0, 1); // Pivot을 0,1로 설정하여 왼쪽 상단 고정
        
        var cvlg = content.GetComponent<VerticalLayoutGroup>();
        cvlg.childControlWidth = true; cvlg.childControlHeight = true; cvlg.childForceExpandWidth = true; cvlg.childForceExpandHeight = false;
        cvlg.childAlignment = TextAnchor.UpperLeft; // 자식 요소들을 왼쪽 상단부터 배치
        cvlg.spacing = 15; cvlg.padding = new RectOffset(20, 20, 30, 30);
        content.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        sr.content = rtContent;

        // 매니저 참조 연결
        var soManager = new SerializedObject(manager);
        soManager.FindProperty("backlogRoot").objectReferenceValue = root;
        soManager.FindProperty("container").objectReferenceValue = rtContent;
        soManager.FindProperty("scrollRect").objectReferenceValue = sr;
        soManager.FindProperty("entryPrefab").objectReferenceValue = prefabAsset.GetComponent<VNBacklogEntry>();
        soManager.ApplyModifiedProperties();

        var soUI = new SerializedObject(ui);
        soUI.FindProperty("backlogManager").objectReferenceValue = manager;
        soUI.ApplyModifiedProperties();

        root.SetActive(false);
        Debug.Log("백로그 좌측 정렬 및 가독성 개선 완료!");
    }
}
