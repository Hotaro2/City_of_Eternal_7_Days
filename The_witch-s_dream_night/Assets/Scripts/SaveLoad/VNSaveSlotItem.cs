using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VN
{
    public sealed class VNSaveSlotItem : MonoBehaviour
    {
        private const string InfoColumnName = "InfoColumn";

        [Header("UI Elements")]
        [SerializeField] private TMP_Text chapterTitleText;
        [SerializeField] private TMP_Text playTimeText;
        [SerializeField] private TMP_Text dateText;
        [SerializeField] private Image thumbnailImage;
        [SerializeField] private GameObject plusIcon;
        [SerializeField] private GameObject contentGroup;
        [SerializeField] private Button actionButton;

        private int currentSlot;
        private Action<int> onSlotClicked;
        private VNAssetDatabase assetDatabase;

        public void Setup(int slot, VNSaveData data, Action<int> onClick)
        {
            currentSlot = slot;
            onSlotClicked = onClick;

            EnsureReferences();
            VNKoreanFontFallback.ApplyToAllIn(gameObject);

            if (data != null)
            {
                if (plusIcon != null) plusIcon.SetActive(false);
                if (contentGroup != null) contentGroup.SetActive(true);

                if (chapterTitleText != null)
                    chapterTitleText.text = string.IsNullOrEmpty(data.chapterTitle) ? $"Chapter {slot + 1}" : data.chapterTitle;

                if (dateText != null)
                    dateText.text = string.IsNullOrEmpty(data.saveDate) ? "Unknown Save Time" : data.saveDate;

                if (playTimeText != null)
                    playTimeText.text = GetPlayTimeString(data.playTime);

                ApplyThumbnail(data.currentBackground);
            }
            else
            {
                if (plusIcon != null) plusIcon.SetActive(true);
                if (contentGroup != null) contentGroup.SetActive(false);
                HideThumbnail();
            }

            if (actionButton != null)
            {
                actionButton.onClick.RemoveAllListeners();
                actionButton.onClick.AddListener(() => onSlotClicked?.Invoke(currentSlot));
            }
        }

        private void EnsureReferences()
        {
            if (actionButton == null) actionButton = GetComponent<Button>();
            if (plusIcon == null) plusIcon = transform.Find("PlusIcon")?.gameObject;
            if (contentGroup == null) contentGroup = transform.Find("ContentGroup")?.gameObject;

            if (contentGroup == null) return;

            if (thumbnailImage == null)
                thumbnailImage = FindNamedComponentInChildren<Image>(contentGroup.transform, "Thumbnail");

            if (chapterTitleText == null)
                chapterTitleText = FindNamedComponentInChildren<TMP_Text>(contentGroup.transform, "Title");

            if (playTimeText == null)
                playTimeText = FindNamedComponentInChildren<TMP_Text>(contentGroup.transform, "Time");

            if (dateText == null)
                dateText = FindNamedComponentInChildren<TMP_Text>(contentGroup.transform, "Date");

            if (dateText == null)
                dateText = CreateDateLabel(contentGroup.transform);

            EnsureLayoutStructure();
        }

        private TMP_Text CreateDateLabel(Transform parent)
        {
            var dateObject = new GameObject("Date", typeof(RectTransform), typeof(TextMeshProUGUI));
            dateObject.transform.SetParent(parent, false);

            var text = dateObject.GetComponent<TextMeshProUGUI>();
            text.text = "Unknown Save Time";
            text.fontSize = playTimeText != null ? playTimeText.fontSize : 16f;
            text.alignment = TextAlignmentOptions.TopLeft;
            text.color = playTimeText != null ? playTimeText.color : new Color(0.75f, 0.75f, 0.75f, 1f);

            if (playTimeText is TextMeshProUGUI playTimeTmp)
            {
                text.font = playTimeTmp.font;
                text.fontSharedMaterial = playTimeTmp.fontSharedMaterial;
            }

            return text;
        }

        private void EnsureLayoutStructure()
        {
            ConfigureSlotFrame();
            ConfigurePlusIcon();

            if (contentGroup == null) return;

            var contentRect = contentGroup.GetComponent<RectTransform>();
            if (contentRect != null)
            {
                contentRect.anchorMin = Vector2.zero;
                contentRect.anchorMax = Vector2.one;
                contentRect.offsetMin = Vector2.zero;
                contentRect.offsetMax = Vector2.zero;
            }

            var rootLayout = GetOrAddComponent<HorizontalLayoutGroup>(contentGroup);
            rootLayout.padding = new RectOffset(18, 18, 18, 18);
            rootLayout.spacing = 18f;
            rootLayout.childAlignment = TextAnchor.UpperLeft;
            rootLayout.childControlWidth = true;
            rootLayout.childControlHeight = false;
            rootLayout.childForceExpandWidth = false;
            rootLayout.childForceExpandHeight = false;

            GameObject infoColumn = EnsureInfoColumn(contentGroup.transform);
            var infoLayout = GetOrAddComponent<VerticalLayoutGroup>(infoColumn);
            infoLayout.padding = new RectOffset(0, 0, 0, 0);
            infoLayout.spacing = 8f;
            infoLayout.childAlignment = TextAnchor.UpperLeft;
            infoLayout.childControlWidth = true;
            infoLayout.childControlHeight = false;
            infoLayout.childForceExpandWidth = true;
            infoLayout.childForceExpandHeight = false;

            var infoLayoutElement = GetOrAddComponent<LayoutElement>(infoColumn);
            infoLayoutElement.flexibleWidth = 1f;
            infoLayoutElement.minWidth = 0f;
            infoLayoutElement.preferredWidth = 170f;

            ConfigureThumbnail();

            ReparentIfNeeded(chapterTitleText, infoColumn.transform);
            ReparentIfNeeded(playTimeText, infoColumn.transform);
            ReparentIfNeeded(dateText, infoColumn.transform);

            ConfigureTitleText();
            ConfigurePlayTimeText();
            ConfigureDateText();

            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
        }

        private void ConfigureSlotFrame()
        {
            var rect = GetComponent<RectTransform>();
            if (rect != null)
                rect.sizeDelta = new Vector2(350f, 200f);

            var background = GetComponent<Image>();
            if (background != null)
                background.color = new Color(0.18f, 0.18f, 0.2f, 1f);
        }

        private void ConfigurePlusIcon()
        {
            if (plusIcon == null) return;

            var rect = plusIcon.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = Vector2.zero;
                rect.sizeDelta = new Vector2(100f, 100f);
            }

            if (plusIcon.TryGetComponent(out TMP_Text plusText))
            {
                plusText.fontSize = 80f;
                plusText.alignment = TextAlignmentOptions.Center;
                plusText.color = new Color(1f, 1f, 1f, 0.3f);
            }
        }

        private void ConfigureThumbnail()
        {
            if (thumbnailImage == null) return;

            var rect = thumbnailImage.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(124f, 124f);

            var layoutElement = GetOrAddComponent<LayoutElement>(thumbnailImage.gameObject);
            layoutElement.minWidth = 124f;
            layoutElement.preferredWidth = 124f;
            layoutElement.flexibleWidth = 0f;
            layoutElement.minHeight = 124f;
            layoutElement.preferredHeight = 124f;
            layoutElement.flexibleHeight = 0f;
        }

        private void ConfigureTitleText()
        {
            if (chapterTitleText == null) return;

            var rect = chapterTitleText.rectTransform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.sizeDelta = new Vector2(0f, 34f);

            var layoutElement = GetOrAddComponent<LayoutElement>(chapterTitleText.gameObject);
            layoutElement.minHeight = 34f;
            layoutElement.preferredHeight = 34f;
            layoutElement.flexibleHeight = 0f;

            chapterTitleText.fontSize = 24f;
            chapterTitleText.alignment = TextAlignmentOptions.TopLeft;
            chapterTitleText.textWrappingMode = TextWrappingModes.NoWrap;
            chapterTitleText.overflowMode = TextOverflowModes.Ellipsis;
            chapterTitleText.color = Color.white;
        }

        private void ConfigurePlayTimeText()
        {
            if (playTimeText == null) return;

            var rect = playTimeText.rectTransform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.sizeDelta = new Vector2(0f, 28f);

            var layoutElement = GetOrAddComponent<LayoutElement>(playTimeText.gameObject);
            layoutElement.minHeight = 28f;
            layoutElement.preferredHeight = 28f;
            layoutElement.flexibleHeight = 0f;

            playTimeText.fontSize = 16f;
            playTimeText.alignment = TextAlignmentOptions.TopLeft;
            playTimeText.textWrappingMode = TextWrappingModes.NoWrap;
            playTimeText.overflowMode = TextOverflowModes.Ellipsis;
            playTimeText.color = new Color(0.78f, 0.78f, 0.78f, 1f);
        }

        private void ConfigureDateText()
        {
            if (dateText == null) return;

            var rect = dateText.rectTransform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.sizeDelta = new Vector2(0f, 24f);

            var layoutElement = GetOrAddComponent<LayoutElement>(dateText.gameObject);
            layoutElement.minHeight = 24f;
            layoutElement.preferredHeight = 24f;
            layoutElement.flexibleHeight = 0f;

            dateText.fontSize = 14f;
            dateText.alignment = TextAlignmentOptions.TopLeft;
            dateText.textWrappingMode = TextWrappingModes.NoWrap;
            dateText.overflowMode = TextOverflowModes.Ellipsis;
            dateText.color = new Color(0.65f, 0.65f, 0.65f, 1f);
        }

        private GameObject EnsureInfoColumn(Transform parent)
        {
            Transform existing = parent.Find(InfoColumnName);
            if (existing != null)
                return existing.gameObject;

            var infoColumn = new GameObject(InfoColumnName, typeof(RectTransform));
            infoColumn.transform.SetParent(parent, false);
            return infoColumn;
        }

        private static void ReparentIfNeeded(Component child, Transform parent)
        {
            if (child == null || child.transform.parent == parent) return;
            child.transform.SetParent(parent, false);
        }

        private static T FindNamedComponentInChildren<T>(Transform root, string objectName) where T : Component
        {
            if (root == null) return null;

            foreach (T component in root.GetComponentsInChildren<T>(true))
            {
                if (component.name == objectName)
                    return component;
            }

            return null;
        }

        private static T GetOrAddComponent<T>(GameObject target) where T : Component
        {
            T component = target.GetComponent<T>();
            return component != null ? component : target.AddComponent<T>();
        }

        private void ApplyThumbnail(string backgroundKey)
        {
            if (thumbnailImage == null) return;

            thumbnailImage.gameObject.SetActive(true);
            thumbnailImage.preserveAspect = true;

            Sprite sprite = ResolveBackgroundSprite(backgroundKey);
            thumbnailImage.sprite = sprite;

            if (sprite != null)
            {
                thumbnailImage.color = Color.white;
            }
            else
            {
                thumbnailImage.color = new Color(1f, 1f, 1f, 0.08f);
            }
        }

        private void HideThumbnail()
        {
            if (thumbnailImage != null)
                thumbnailImage.gameObject.SetActive(false);
        }

        private Sprite ResolveBackgroundSprite(string backgroundKey)
        {
            if (string.IsNullOrWhiteSpace(backgroundKey)) return null;

            assetDatabase ??= ResolveAssetDatabase();
            return assetDatabase != null ? assetDatabase.GetBackground(backgroundKey) : null;
        }

        private VNAssetDatabase ResolveAssetDatabase()
        {
            var presenter = UnityEngine.Object.FindFirstObjectByType<VNPresenter>(FindObjectsInactive.Include);
            if (presenter != null && presenter.AssetDatabase != null)
                return presenter.AssetDatabase;

            var titleManager = UnityEngine.Object.FindFirstObjectByType<VNTitleManager>(FindObjectsInactive.Include);
            if (titleManager != null && titleManager.AssetDatabase != null)
                return titleManager.AssetDatabase;

            return null;
        }

        private string GetPlayTimeString(float seconds)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(seconds);
            return $"Playtime  {timeSpan.Hours:D2}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
        }
    }
}
