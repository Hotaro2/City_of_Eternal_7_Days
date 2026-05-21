using System;
using System.Collections.Generic;
using Ink.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VN
{
    /// <summary>
    /// 선택지 버튼 생성 및 표시를 담당하는 모듈입니다.
    /// </summary>
    public sealed class VNChoicePanel : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private Transform container;
        [SerializeField] private Button choiceButtonPrefab;

        public void Show(IReadOnlyList<Choice> choices, Action<int> onSelect)
        {
            Clear();

            if (choices == null || choices.Count == 0)
            {
                SetVisible(false);
                return;
            }

            SetVisible(true);

            for (int i = 0; i < choices.Count; i++)
            {
                int index = i;
                var btn = Instantiate(choiceButtonPrefab, container);
                var textObj = btn.GetComponentInChildren<TMP_Text>();
                if (textObj != null)
                {
                    VNKoreanFontFallback.ApplyTo(textObj);
                    textObj.text = choices[i].text;
                }

                btn.onClick.AddListener(() =>
                {
                    SetVisible(false);
                    onSelect?.Invoke(index);
                });
            }
        }

        public void Clear()
        {
            if (container == null) return;
            foreach (Transform child in container)
            {
                Destroy(child.gameObject);
            }
        }

        public void SetVisible(bool visible)
        {
            if (root != null) root.SetActive(visible);
        }
    }
}
