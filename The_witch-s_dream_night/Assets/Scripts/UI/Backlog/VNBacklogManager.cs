using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VN
{
    public sealed class VNBacklogManager : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject backlogRoot;
        [SerializeField] private Transform container;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private Button closeButton;

        [Header("Prefabs")]
        [SerializeField] private VNBacklogEntry entryPrefab;

        private readonly List<BacklogData> backlogDataList = new();
        private readonly List<VNBacklogEntry> instantiatedEntries = new();
        private Coroutine scrollRoutine;

        private void Awake()
        {
            if (backlogRoot != null)
                VNKoreanFontFallback.ApplyToAllIn(backlogRoot);

            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(CloseBacklog);
            }
        }

        public void AddEntry(string speaker, string content)
        {
            if (entryPrefab == null || container == null) return;

            backlogDataList.Add(new BacklogData { speaker = speaker, text = content });

            var newEntry = Instantiate(entryPrefab, container);
            newEntry.SetEntry(speaker, content);
            instantiatedEntries.Add(newEntry);

            if (instantiatedEntries.Count > 100)
            {
                Destroy(instantiatedEntries[0].gameObject);
                instantiatedEntries.RemoveAt(0);
                backlogDataList.RemoveAt(0);
            }

            UpdateScroll();
        }

        public List<BacklogData> GetBacklogData() => new List<BacklogData>(backlogDataList);

        public void ClearAndRestore(List<BacklogData> data)
        {
            foreach (var entry in instantiatedEntries)
                Destroy(entry.gameObject);

            instantiatedEntries.Clear();
            backlogDataList.Clear();

            foreach (var item in data)
                AddEntry(item.speaker, item.text);

            UpdateScroll(true);
        }

        public void ToggleBacklog()
        {
            if (backlogRoot == null) return;
            if (backlogRoot.activeSelf) CloseBacklog();
            else OpenBacklog();
        }

        public void OpenBacklog()
        {
            if (backlogRoot == null) return;
            backlogRoot.SetActive(true);
            UpdateScroll(true);
        }

        public void CloseBacklog()
        {
            if (backlogRoot == null) return;
            backlogRoot.SetActive(false);
        }

        private void UpdateScroll(bool force = false)
        {
            if (scrollRect == null || !force) return;

            if (scrollRoutine != null)
                StopCoroutine(scrollRoutine);

            scrollRoutine = StartCoroutine(SetScrollNextFrame());
        }

        private IEnumerator SetScrollNextFrame()
        {
            yield return null;

            if (scrollRect != null)
                scrollRect.verticalNormalizedPosition = 0f;

            scrollRoutine = null;
        }
    }
}
