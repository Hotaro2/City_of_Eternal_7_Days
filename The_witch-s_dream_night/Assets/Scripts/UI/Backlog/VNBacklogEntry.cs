using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VN
{
    public sealed class VNBacklogEntry : MonoBehaviour
    {
        [SerializeField] private TMP_Text speakerText;
        [SerializeField] private TMP_Text contentText;

        public void SetEntry(string speaker, string content)
        {
            if (speakerText != null)
            {
                speakerText.text = string.IsNullOrWhiteSpace(speaker) ? "" : speaker;
            }

            if (contentText != null)
            {
                contentText.text = content ?? "";
                
                // 줄바꿈이 가로 폭에 맞춰지도록 강제 설정
                contentText.textWrappingMode = TextWrappingModes.Normal;
                contentText.overflowMode = TextOverflowModes.Overflow;
            }
        }
    }
}
