using Unity.Netcode;
using UnityEngine;

namespace RoomGame
{
    /// <summary>Server-authoritative chat: owner -> server (validate/trim) -> everyone shows the bubble.</summary>
    public class PlayerChat : NetworkBehaviour
    {
        public const int MaxLength = 120;

        public PlayerWorldUI worldUI;

        public override void OnNetworkSpawn()
        {
            if (IsOwner && ChatManager.Instance != null) ChatManager.Instance.LocalChat = this;
        }

        public override void OnNetworkDespawn()
        {
            if (IsOwner && ChatManager.Instance != null && ChatManager.Instance.LocalChat == this) ChatManager.Instance.LocalChat = null;
        }

        public void Send(string message)
        {
            if (!IsOwner || string.IsNullOrWhiteSpace(message)) return;
            SubmitRpc(message);
        }

        [Rpc(SendTo.Server)]
        void SubmitRpc(string message, RpcParams rpcParams = default)
        {
            // Only accept messages from the client that owns this player.
            if (rpcParams.Receive.SenderClientId != OwnerClientId) return;
            message = message == null ? "" : message.Trim();
            if (message.Length == 0) return;
            if (message.Length > MaxLength) message = message.Substring(0, MaxLength);
            BroadcastRpc(message);
        }

        [Rpc(SendTo.Everyone)]
        void BroadcastRpc(string message)
        {
            if (worldUI != null) worldUI.ShowBubble(message);
            if (ChatManager.Instance != null) ChatManager.Instance.OnMessage(GetComponent<PlayerData>()?.Nickname.Value.ToString(), message);
        }
    }
}
