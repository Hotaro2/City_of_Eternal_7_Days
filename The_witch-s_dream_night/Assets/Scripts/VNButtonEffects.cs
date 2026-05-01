using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace VN
{
    public sealed class VNButtonEffects : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
    {
        [Header("Settings")]
        [SerializeField] private float hoverScale = 1.05f;
        [SerializeField] private float animationSpeed = 10f;
        [SerializeField] private string hoverSFX = "hover_click"; // 에셋 데이터베이스에 효과음이 있을 경우

        private Vector3 originalScale;
        private Vector3 targetScale;
        private VNPresenter presenter;

        private void Awake()
        {
            originalScale = transform.localScale;
            targetScale = originalScale;
            presenter = FindFirstObjectByType<VNPresenter>();
        }

        private void Update()
        {
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.unscaledDeltaTime * animationSpeed);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            targetScale = originalScale * hoverScale;
            if (presenter != null && !string.IsNullOrEmpty(hoverSFX))
            {
                // presenter.PlaySFX(hoverSFX); // 효과음 데이터가 준비되면 주석 해제
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            targetScale = originalScale;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            transform.localScale = originalScale * 0.95f; // 클릭 시 살짝 작아짐
        }
    }
}
