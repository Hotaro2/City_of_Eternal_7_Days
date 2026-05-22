using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using VN;

namespace VN.Editor
{
    public static class VNPrologueUISetupTool
    {
        [MenuItem("VN Tools/Setup Prologue UI")]
        public static void SetupPrologueUI()
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
            var overlay = GameObject.Find("PrologueOverlay");
            if (overlay == null)
            {
                overlay = CreateUIObject("PrologueOverlay", canvas.transform, typeof(Image), typeof(Button));
                Undo.RegisterCreatedObjectUndo(overlay, "Create Prologue UI");
            }

            overlay.transform.SetParent(canvas.transform, false);
            overlay.transform.SetAsLastSibling();
            var overlayRect = overlay.GetComponent<RectTransform>();
            Stretch(overlayRect);

            var background = overlay.GetComponent<Image>();
            background.color = new Color(0.035f, 0.028f, 0.033f, 1f);
            background.raycastTarget = true;

            var advanceButton = overlay.GetComponent<Button>();
            advanceButton.transition = Selectable.Transition.None;

            var textObject = overlay.transform.Find("PrologueText")?.gameObject;
            if (textObject == null)
            {
                textObject = CreateUIObject("PrologueText", overlay.transform, typeof(TextMeshProUGUI), typeof(Shadow));
                Undo.RegisterCreatedObjectUndo(textObject, "Create Prologue Text");
            }

            var textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0.08f, 0.32f);
            textRect.anchorMax = new Vector2(0.92f, 0.86f);
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            var prologueText = textObject.GetComponent<TextMeshProUGUI>();
            prologueText.text = "";
            if (fontAsset != null) prologueText.font = fontAsset;
            prologueText.fontSize = 34f;
            prologueText.alignment = TextAlignmentOptions.Center;
            prologueText.color = new Color(0.94f, 0.94f, 0.94f, 1f);
            prologueText.textWrappingMode = TextWrappingModes.Normal;
            prologueText.overflowMode = TextOverflowModes.Overflow;
            prologueText.lineSpacing = 20f;
            prologueText.richText = true;
            prologueText.raycastTarget = false;

            var textShadow = textObject.GetComponent<Shadow>();
            textShadow.effectColor = new Color(1f, 1f, 1f, 0.22f);
            textShadow.effectDistance = Vector2.zero;

            var choicesObject = overlay.transform.Find("PrologueChoices")?.gameObject;
            if (choicesObject == null)
            {
                choicesObject = CreateUIObject("PrologueChoices", overlay.transform, typeof(HorizontalLayoutGroup));
                Undo.RegisterCreatedObjectUndo(choicesObject, "Create Prologue Choices");
            }

            var choicesRect = choicesObject.GetComponent<RectTransform>();
            choicesRect.anchorMin = new Vector2(0.08f, 0.16f);
            choicesRect.anchorMax = new Vector2(0.92f, 0.26f);
            choicesRect.offsetMin = Vector2.zero;
            choicesRect.offsetMax = Vector2.zero;

            var layoutGroup = choicesObject.GetComponent<HorizontalLayoutGroup>();
            layoutGroup.childAlignment = TextAnchor.MiddleCenter;
            layoutGroup.spacing = 64f;
            layoutGroup.childControlWidth = false;
            layoutGroup.childControlHeight = true;
            layoutGroup.childForceExpandWidth = false;
            layoutGroup.childForceExpandHeight = false;

            Button firstChoice = CreateChoiceSlot(choicesObject.transform, "Choice_1", "맞아, 있었어.", fontAsset);
            Button secondChoice = CreateChoiceSlot(choicesObject.transform, "Choice_2", "아니, 그런 적 없는데.", fontAsset);

            var so = new SerializedObject(ui);
            so.FindProperty("prologueRoot").objectReferenceValue = overlay;
            so.FindProperty("prologueText").objectReferenceValue = prologueText;
            so.FindProperty("prologueAdvanceButton").objectReferenceValue = advanceButton;

            var buttonsProp = so.FindProperty("prologueChoiceButtons");
            buttonsProp.ClearArray();
            buttonsProp.InsertArrayElementAtIndex(0);
            buttonsProp.GetArrayElementAtIndex(0).objectReferenceValue = firstChoice;
            buttonsProp.InsertArrayElementAtIndex(1);
            buttonsProp.GetArrayElementAtIndex(1).objectReferenceValue = secondChoice;
            so.ApplyModifiedProperties();

            firstChoice.gameObject.SetActive(false);
            secondChoice.gameObject.SetActive(false);
            overlay.SetActive(false);

            EditorUtility.SetDirty(ui);
            EditorUtility.SetDirty(overlay);
            EditorSceneManager.MarkSceneDirty(overlay.scene);
            Selection.activeGameObject = overlay;
            Debug.Log("[VN Tools] Prologue UI를 Scene 오브젝트로 생성하고 VNUIController에 연결했습니다.");
        }

        private static Button CreateChoiceSlot(Transform parent, string name, string previewText, TMP_FontAsset fontAsset)
        {
            var slot = parent.Find(name)?.gameObject;
            if (slot == null)
            {
                slot = CreateUIObject(name, parent, typeof(Image), typeof(Button), typeof(LayoutElement), typeof(CanvasGroup), typeof(VNChoiceSelectionEffect));
                Undo.RegisterCreatedObjectUndo(slot, "Create Prologue Choice");
            }
            else if (slot.GetComponent<VNChoiceSelectionEffect>() == null)
            {
                slot.AddComponent<VNChoiceSelectionEffect>();
            }

            if (slot.GetComponent<CanvasGroup>() == null) slot.AddComponent<CanvasGroup>();

            var rect = slot.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(360f, 52f);

            var layout = slot.GetComponent<LayoutElement>();
            layout.minWidth = 260f;
            layout.preferredWidth = 360f;
            layout.minHeight = 52f;
            layout.preferredHeight = 52f;

            var image = slot.GetComponent<Image>();
            image.color = new Color(1f, 1f, 1f, 0f);

            var button = slot.GetComponent<Button>();
            var colors = button.colors;
            colors.normalColor = new Color(1f, 1f, 1f, 0f);
            colors.highlightedColor = new Color(1f, 1f, 1f, 0.08f);
            colors.pressedColor = new Color(1f, 1f, 1f, 0.16f);
            colors.selectedColor = colors.highlightedColor;
            button.colors = colors;

            var labelObject = slot.transform.Find("Label")?.gameObject;
            if (labelObject == null)
            {
                labelObject = CreateUIObject("Label", slot.transform, typeof(TextMeshProUGUI), typeof(Shadow));
                Undo.RegisterCreatedObjectUndo(labelObject, "Create Prologue Choice Label");
            }

            Stretch(labelObject.GetComponent<RectTransform>());
            var label = labelObject.GetComponent<TextMeshProUGUI>();
            label.text = previewText;
            if (fontAsset != null) label.font = fontAsset;
            label.fontSize = 24f;
            label.alignment = TextAlignmentOptions.Center;
            label.color = new Color(0.92f, 0.88f, 0.88f, 1f);
            label.raycastTarget = false;

            var shadow = labelObject.GetComponent<Shadow>();
            shadow.effectColor = new Color(1f, 0.83f, 0.72f, 0.32f);
            shadow.effectDistance = Vector2.zero;

            return button;
        }

        private static GameObject CreateUIObject(string name, Transform parent, params System.Type[] components)
        {
            var obj = new GameObject(name, typeof(RectTransform));
            foreach (var component in components) obj.AddComponent(component);
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
    }
}
