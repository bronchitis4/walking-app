using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RoomGame
{
    /// <summary>Connect screen (nickname, skin, create/join) and the in-game room code panel.</summary>
    public class ConnectUI : MonoBehaviour
    {
        [Header("Connect panel")]
        public GameObject connectPanel;
        public TMP_InputField nicknameInput;
        public TMP_InputField codeInput;
        public Button createButton;
        public Button joinButton;
        public Button[] skinButtons;
        public TMP_Text statusText;

        [Header("In game")]
        public GameObject roomPanel;
        public TMP_Text roomCodeText;
        public Button copyButton;
        public Button leaveButton;

        string code;

        void Awake()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            // Only take keyboard input while the game canvas has focus, so browser shortcuts keep working elsewhere.
            // WebGLInput lives in UnityEngine.WebGLModule, which isn't referenced at compile time here.
            var webGLInput = System.Type.GetType("UnityEngine.WebGLInput, UnityEngine.WebGLModule");
            var capture = webGLInput?.GetProperty("captureAllKeyboardInput", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            if (capture != null) capture.SetValue(null, false);
            else Debug.LogWarning("WebGLInput.captureAllKeyboardInput not found");
#endif
        }

        void Start()
        {
            nicknameInput.text = PlayerPrefs.GetString("nick", "");
            nicknameInput.characterLimit = 20;
            codeInput.characterLimit = 12;
            SelectSkin(PlayerPrefs.GetInt("skin", 0));
            for (int i = 0; i < skinButtons.Length; i++)
            {
                int idx = i;
                skinButtons[i].onClick.AddListener(() => SelectSkin(idx));
            }
            createButton.onClick.AddListener(() => Run(true));
            joinButton.onClick.AddListener(() => Run(false));
            copyButton.onClick.AddListener(Copy);
            leaveButton.onClick.AddListener(Leave);
            SessionManager.Instance.Disconnected += OnDisconnected;
            ShowConnect("");
        }

        void SelectSkin(int i)
        {
            PlayerData.LocalSkin = Mathf.Clamp(i, 0, skinButtons.Length - 1);
            for (int b = 0; b < skinButtons.Length; b++)
                skinButtons[b].GetComponent<Image>().color = b == PlayerData.LocalSkin ? new Color(1f, 0.78f, 0.45f) : new Color(0.35f, 0.3f, 0.35f);
        }

        async void Run(bool create)
        {
            var nick = nicknameInput.text.Trim();
            if (nick.Length == 0) { statusText.text = "Enter a nickname"; return; }
            if (!create && codeInput.text.Trim().Length == 0) { statusText.text = "Enter a room code"; return; }

            PlayerData.LocalNickname = nick;
            PlayerPrefs.SetString("nick", nick);
            PlayerPrefs.SetInt("skin", PlayerData.LocalSkin);
            SetInteractable(false);
            statusText.text = create ? "Creating room..." : "Joining...";
            try
            {
                code = create ? await SessionManager.Instance.CreateRoom() : await SessionManager.Instance.JoinRoom(codeInput.text);
                connectPanel.SetActive(false);
                roomPanel.SetActive(true);
                roomCodeText.text = "Room: " + code;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                statusText.text = "Failed: " + e.Message;
            }
            finally
            {
                SetInteractable(true);
            }
        }

        void SetInteractable(bool on)
        {
            createButton.interactable = on;
            joinButton.interactable = on;
        }

        void Copy()
        {
            if (string.IsNullOrEmpty(code)) return;
            WebClipboard.Copy(code);
            roomCodeText.text = "Room: " + code + "  (copied)";
        }

        async void Leave()
        {
            await SessionManager.Instance.Leave();
            ShowConnect("");
        }

        void OnDisconnected(string reason) => ShowConnect(reason);

        void ShowConnect(string message)
        {
            connectPanel.SetActive(true);
            roomPanel.SetActive(false);
            statusText.text = message;
        }
    }
}
