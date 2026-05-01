using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

namespace VN
{
    public sealed class VNSaveLoadPanel : MonoBehaviour
    {
        public enum PanelMode { Save, Load }

        [Header("Components")]
        [SerializeField] private VNDirector director;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private Transform slotRoot; // 슬롯들이 배치될 Content 트랜스폼
        [SerializeField] private GameObject slotPrefab; // 슬롯 템플릿 프리팹
        [SerializeField] private Button closeButton;

        [Header("Top Toggles")]
        [SerializeField] private Button saveTabButton;
        [SerializeField] private Button loadTabButton;

        [Header("Settings")]
        [SerializeField] private int maxSlots = 9;

        private PanelMode currentMode;
        private List<VNSaveSlotItem> slotItems = new List<VNSaveSlotItem>();

        private void Awake()
        {
            if (closeButton != null) closeButton.onClick.AddListener(Close);
            if (saveTabButton != null) saveTabButton.onClick.AddListener(() => ChangeMode(PanelMode.Save));
            if (loadTabButton != null) loadTabButton.onClick.AddListener(() => ChangeMode(PanelMode.Load));
            RefreshTabVisibility();
        }

        public void Open(PanelMode mode)
        {
            gameObject.SetActive(true);
            RefreshTabVisibility();
            ChangeMode(mode);
        }

        private void ChangeMode(PanelMode mode)
        {
            if (!CanSaveInCurrentScene() && mode == PanelMode.Save)
                mode = PanelMode.Load;

            currentMode = mode;
            if (titleText != null) titleText.text = currentMode.ToString().ToUpper();
            
            // 탭 강조 효과
            if (saveTabButton != null) saveTabButton.image.color = currentMode == PanelMode.Save ? Color.white : new Color(1, 1, 1, 0.4f);
            if (loadTabButton != null) loadTabButton.image.color = currentMode == PanelMode.Load ? Color.white : new Color(1, 1, 1, 0.4f);

            RefreshSlots();
        }

        public void Close() => gameObject.SetActive(false);

        public void RefreshSlots()
        {
            if (slotRoot == null || slotPrefab == null)
            {
                Debug.LogError($"[VNSaveLoadPanel] Critical: SlotRoot or SlotPrefab is NOT assigned! Root: {slotRoot}, Prefab: {slotPrefab}");
                return;
            }

            // 1. 기존 슬롯 제거
            foreach (var item in slotItems) 
            {
                if (item != null) Destroy(item.gameObject);
            }
            slotItems.Clear();

            // 2. 강제로 부모 아래의 다른 자식들도 청소 (중복 생성 방지)
            foreach (Transform child in slotRoot)
            {
                Destroy(child.gameObject);
            }

            // 3. 새 슬롯 생성
            for (int i = 0; i < maxSlots; i++)
            {
                GameObject slotObj = Instantiate(slotPrefab, slotRoot);
                slotObj.SetActive(true); // 중요: 템플릿이 꺼져있을 수 있으므로 켭니다.
                
                // 레이아웃이 깨지지 않도록 스케일 초기화
                slotObj.transform.localScale = Vector3.one;

                var slotItem = slotObj.GetComponent<VNSaveSlotItem>();
                if (slotItem != null)
                {
                    VNSaveData data = VNSaveService.Load(i);
                    slotItem.Setup(i, data, OnSlotClicked);
                    slotItems.Add(slotItem);
                }
            }

            // 4. 레이아웃 강제 업데이트 (그리드 정렬 보장)
            LayoutRebuilder.ForceRebuildLayoutImmediate(slotRoot as RectTransform);
            Debug.Log($"[VNSaveLoadPanel] Refreshed {maxSlots} slots in {currentMode} mode.");
        }

        private void RefreshTabVisibility()
        {
            bool canSave = CanSaveInCurrentScene();

            if (saveTabButton != null)
                saveTabButton.gameObject.SetActive(canSave);

            if (!canSave && currentMode == PanelMode.Save)
                currentMode = PanelMode.Load;

            if (saveTabButton != null && saveTabButton.transform.parent is RectTransform tabsRect)
                LayoutRebuilder.ForceRebuildLayoutImmediate(tabsRect);
        }

        private bool CanSaveInCurrentScene()
        {
            return director != null;
        }

        private void OnSlotClicked(int slot)
        {
            if (currentMode == PanelMode.Save)
            {
                if (director != null)
                {
                    director.Save(slot);
                    RefreshSlots();
                }
                else
                {
                    Debug.LogWarning("[VNSaveLoadPanel] Director is missing. Cannot save in this scene.");
                }
            }
            else
            {
                if (VNSaveService.Exists(slot))
                {
                    if (director != null)
                    {
                        director.Load(slot);
                        Close();
                    }
                    else
                    {
                        // 타이틀 씬 대응: 슬롯 정보를 저장하고 게임 씬 로드
                        PlayerPrefs.SetInt("LoadOnStart_Slot", slot);
                        UnityEngine.SceneManagement.SceneManager.LoadScene("SampleScene");
                    }
                }
                else
                {
                    Debug.LogWarning($"[VNSaveLoadPanel] Slot {slot} is empty, cannot load.");
                }
            }
        }
    }
}
