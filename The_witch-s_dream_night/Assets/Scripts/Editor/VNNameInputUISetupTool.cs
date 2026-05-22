using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using VN;

namespace VN.Editor
{
    public static class VNNameInputUISetupTool
    {
        [MenuItem("VN Tools/Setup Name Input UI")]
        public static void SetupNameInputUI()
        {
            var ui = Object.FindFirstObjectByType<VNUIController>(FindObjectsInactive.Include);
            if (ui == null)
            {
                Debug.LogError("[VN Tools] Scene에 VNUIController가 없습니다.");
                return;
            }

            var canvas = ui.GetComponentInParent<Canvas>();
            if (canvas == null) canvas = Object.FindFirstObjectByType<Canvas>(FindObjectsInactive.Include);
            if (canvas == null)
            {
                Debug.LogError("[VN Tools] Scene에 Canvas가 없습니다.");
                return;
            }

            TMP_FontAsset fontAsset = null;

            var panel = GameObject.Find("NameInputPanel");
            if (panel == null)
            {
                panel = CreateUIObject("NameInputPanel", canvas.transform, typeof(Image), typeof(VNNameInputPanel));
                Undo.RegisterCreatedObjectUndo(panel, "Create Name Input UI");
            }

            panel.transform.SetParent(canvas.transform, false);
            panel.transform.SetAsLastSibling();

            var panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(520f, 260f);
            panelRect.anchoredPosition = Vector2.zero;

            var panelImage = panel.GetComponent<Image>();
            panelImage.color = new Color(0.06f, 0.055f, 0.06f, 0.96f);

            var title = EnsureText(panel.transform, "Title", "이름 입력", fontAsset, 30f, new Color(0.94f, 0.94f, 0.94f, 1f));
            var titleRect = title.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.offsetMin = new Vector2(30f, -70f);
            titleRect.offsetMax = new Vector2(-30f, -20f);

            var inputObject = panel.transform.Find("NameInput")?.gameObject;
            if (inputObject == null)
            {
                inputObject = CreateUIObject("NameInput", panel.transform, typeof(Image), typeof(TMP_InputField));
                Undo.RegisterCreatedObjectUndo(inputObject, "Create Name Input Field");
            }

            var inputRect = inputObject.GetComponent<RectTransform>();
            inputRect.anchorMin = new Vector2(0f, 0.5f);
            inputRect.anchorMax = new Vector2(1f, 0.5f);
            inputRect.offsetMin = new Vector2(48f, -20f);
            inputRect.offsetMax = new Vector2(-48f, 36f);

            var inputImage = inputObject.GetComponent<Image>();
            inputImage.color = new Color(0.95f, 0.95f, 0.95f, 1f);

            var inputText = EnsureText(inputObject.transform, "Text Area/Text", "지휘사", fontAsset, 26f, Color.black);
            var textArea = inputObject.transform.Find("Text Area") as RectTransform;
            if (textArea != null) Stretch(textArea, Vector2.zero, Vector2.zero);
            Stretch(inputText.GetComponent<RectTransform>(), new Vector2(16f, 4f), new Vector2(-16f, -4f));

            var placeholder = EnsureText(inputObject.transform, "Placeholder", "이름을 입력하세요.", fontAsset, 22f, new Color(0.25f, 0.25f, 0.25f, 0.55f));
            Stretch(placeholder.GetComponent<RectTransform>(), new Vector2(16f, 4f), new Vector2(-16f, -4f));

            var inputField = inputObject.GetComponent<TMP_InputField>();
            inputField.textViewport = textArea;
            inputField.textComponent = inputText.GetComponent<TextMeshProUGUI>();
            inputField.placeholder = placeholder.GetComponent<TextMeshProUGUI>();
            inputField.characterLimit = 8;

            var confirmButton = CreateButton(panel.transform, "ConfirmButton", "확인", fontAsset, new Vector2(-95f, -82f));
            var cancelButton = CreateButton(panel.transform, "CancelButton", "기본값", fontAsset, new Vector2(95f, -82f));

            var panelScript = panel.GetComponent<VNNameInputPanel>();
            var panelSo = new SerializedObject(panelScript);
            panelSo.FindProperty("root").objectReferenceValue = panel;
            panelSo.FindProperty("inputField").objectReferenceValue = inputField;
            panelSo.FindProperty("confirmButton").objectReferenceValue = confirmButton;
            panelSo.FindProperty("cancelButton").objectReferenceValue = cancelButton;
            panelSo.FindProperty("titleText").objectReferenceValue = title.GetComponent<TextMeshProUGUI>();
            panelSo.ApplyModifiedProperties();

            var uiSo = new SerializedObject(ui);
            uiSo.FindProperty("nameInputPanel").objectReferenceValue = panelScript;
            uiSo.ApplyModifiedProperties();

            VNKoreanFontFallback.ApplyToAllIn(panel);

            panel.SetActive(false);
            EditorUtility.SetDirty(ui);
            EditorUtility.SetDirty(panel);
            EditorUtility.SetDirty(panelScript);
            EditorSceneManager.MarkSceneDirty(panel.scene);
            Selection.activeGameObject = panel;
            Debug.Log("[VN Tools] Name Input UI를 생성하고 VNUIController에 연결했습니다.");
        }

        private static Button CreateButton(Transform parent, string name, string label, TMP_FontAsset fontAsset, Vector2 position)
        {
            var buttonObject = parent.Find(name)?.gameObject;
            if (buttonObject == null)
            {
                buttonObject = CreateUIObject(name, parent, typeof(Image), typeof(Button));
                Undo.RegisterCreatedObjectUndo(buttonObject, "Create Name Input Button");
            }

            var rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(150f, 46f);
            rect.anchoredPosition = position;

            buttonObject.GetComponent<Image>().color = new Color(0.78f, 0.78f, 0.78f, 1f);
            var text = EnsureText(buttonObject.transform, "Label", label, fontAsset, 22f, Color.black);
            Stretch(text.GetComponent<RectTransform>(), Vector2.zero, Vector2.zero);
            return buttonObject.GetComponent<Button>();
        }

        private static GameObject EnsureText(Transform parent, string path, string text, TMP_FontAsset fontAsset, float size, Color color)
        {
            Transform current = parent;
            string[] parts = path.Split('/');
            for (int i = 0; i < parts.Length; i++)
            {
                var child = current.Find(parts[i]);
                if (child == null)
                {
                    var obj = CreateUIObject(parts[i], current, i == parts.Length - 1 ? typeof(TextMeshProUGUI) : typeof(RectTransform));
                    Undo.RegisterCreatedObjectUndo(obj, "Create Name Input Text");
                    child = obj.transform;
                }
                current = child;
            }

            var tmp = current.GetComponent<TextMeshProUGUI>();
            if (tmp == null) tmp = current.gameObject.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            if (fontAsset != null) tmp.font = fontAsset;
            tmp.fontSize = size;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = color;
            return current.gameObject;
        }

        private static GameObject CreateUIObject(string name, Transform parent, params System.Type[] components)
        {
            var obj = new GameObject(name, typeof(RectTransform));
            foreach (var component in components)
            {
                if (component != typeof(RectTransform)) obj.AddComponent(component);
            }
            obj.transform.SetParent(parent, false);
            return obj;
        }

        private static void Stretch(RectTransform rect, Vector2 offsetMin, Vector2 offsetMax)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }
    }
}
