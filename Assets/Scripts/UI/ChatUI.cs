using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RoomGame
{
    /// <summary>Enter opens/sends, Esc closes. While open, player movement is disabled.</summary>
    public class ChatUI : MonoBehaviour
    {
        public static bool IsTyping { get; private set; }

        public GameObject root;
        public TMP_InputField input;

        void Start()
        {
            input.characterLimit = PlayerChat.MaxLength;
            Close();
        }

        void Update()
        {
            var kb = Keyboard.current;
            if (kb == null || ChatManager.Instance == null || ChatManager.Instance.LocalChat == null) return;

            if (!IsTyping)
            {
                if (kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame) Open();
                return;
            }

            if (kb.escapeKey.wasPressedThisFrame) { Close(); return; }
            if (kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame)
            {
                var text = input.text;
                if (!string.IsNullOrWhiteSpace(text)) ChatManager.Instance.Send(text);
                Close();
            }
        }

        void Open()
        {
            IsTyping = true;
            root.SetActive(true);
            input.text = "";
            input.ActivateInputField();
            input.Select();
        }

        public void Close()
        {
            IsTyping = false;
            input.DeactivateInputField();
            root.SetActive(false);
        }

        void OnDisable() => IsTyping = false;
    }
}
