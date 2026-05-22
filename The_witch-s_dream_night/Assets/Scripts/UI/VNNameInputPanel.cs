using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VN
{
    public sealed class VNNameInputPanel : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private TMP_Text titleText;

        private const string DefaultName = "지휘사";
        private const string DefaultTitle = "이름 입력";

        private Action<string> onConfirm;
        private string fallbackName = DefaultName;
        private bool confirmListenerRegistered;
        private bool cancelListenerRegistered;

        public bool IsOpen => root != null && root.activeSelf;

        private void Awake()
        {
            EnsureRoot();
            ResolveReferences();
            RegisterListeners();
        }

        public void Open(string defaultName, Action<string> onNameConfirmed)
        {
            EnsureRoot();
            ResolveReferences();
            RegisterListeners();

            fallbackName = string.IsNullOrWhiteSpace(defaultName) ? DefaultName : defaultName.Trim();
            onConfirm = onNameConfirmed;

            root.SetActive(true);
            VNKoreanFontFallback.ApplyToAllIn(root);

            if (titleText != null) titleText.text = DefaultTitle;
            if (inputField != null)
            {
                if (inputField.textComponent != null)
                    VNKoreanFontFallback.ApplyTo(inputField.textComponent);

                if (inputField.placeholder is TMP_Text placeholderText)
                    VNKoreanFontFallback.ApplyTo(placeholderText);

                inputField.text = fallbackName;
                inputField.ActivateInputField();
                inputField.Select();
            }
        }

        public void Close()
        {
            EnsureRoot();
            root.SetActive(false);
            onConfirm = null;
        }

        private void Update()
        {
            if (!IsOpen) return;

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                Confirm();
            }
        }

        private void Confirm()
        {
            string value = inputField != null ? inputField.text : fallbackName;
            value = string.IsNullOrWhiteSpace(value) ? fallbackName : value.Trim();
            onConfirm?.Invoke(value);
            Close();
        }

        private void UseFallback()
        {
            onConfirm?.Invoke(fallbackName);
            Close();
        }

        private void EnsureRoot()
        {
            if (root == null) root = gameObject;
        }

        private void ResolveReferences()
        {
            if (inputField == null)
            {
                inputField = GetComponentInChildren<TMP_InputField>(true);
            }

            if (titleText == null)
            {
                var title = transform.Find("Title");
                if (title != null) titleText = title.GetComponent<TMP_Text>();
            }

            if (confirmButton == null)
            {
                var confirm = transform.Find("ConfirmButton");
                if (confirm != null) confirmButton = confirm.GetComponent<Button>();
            }

            if (cancelButton == null)
            {
                var cancel = transform.Find("CancelButton");
                if (cancel != null) cancelButton = cancel.GetComponent<Button>();
            }
        }

        private void RegisterListeners()
        {
            if (confirmButton != null && !confirmListenerRegistered)
            {
                confirmButton.onClick.AddListener(Confirm);
                confirmListenerRegistered = true;
            }

            if (cancelButton != null && !cancelListenerRegistered)
            {
                cancelButton.onClick.AddListener(UseFallback);
                cancelListenerRegistered = true;
            }
        }
    }
}
