using UnityEngine;

namespace RoomGame
{
    /// <summary>Routes chat between the UI and the local player's PlayerChat.</summary>
    public class ChatManager : MonoBehaviour
    {
        public static ChatManager Instance { get; private set; }

        public PlayerChat LocalChat { get; set; }

        void Awake() => Instance = this;

        public void Send(string message)
        {
            if (LocalChat != null) LocalChat.Send(message);
        }

        public void OnMessage(string sender, string message)
        {
            Debug.Log($"[Chat] {sender}: {message}");
        }
    }
}
