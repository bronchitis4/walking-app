using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace RoomGame
{
    /// <summary>Nickname and skin, written by the owner and visible to all.</summary>
    public class PlayerData : NetworkBehaviour
    {
        /// <summary>Choice made on the connect screen before joining.</summary>
        public static string LocalNickname = "Player";
        public static int LocalSkin;

        public RuntimeAnimatorController[] skins;
        public Animator animator;
        public PlayerWorldUI worldUI;

        public readonly NetworkVariable<FixedString32Bytes> Nickname = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public readonly NetworkVariable<int> Skin = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        public override void OnNetworkSpawn()
        {
            Nickname.OnValueChanged += (_, v) => ApplyName(v.ToString());
            Skin.OnValueChanged += (_, v) => ApplySkin(v);

            if (IsOwner)
            {
                var nick = string.IsNullOrWhiteSpace(LocalNickname) ? "Player" : LocalNickname.Trim();
                if (nick.Length > 20) nick = nick.Substring(0, 20);
                Nickname.Value = new FixedString32Bytes(nick);
                Skin.Value = Mathf.Clamp(LocalSkin, 0, skins.Length - 1);
            }

            ApplyName(Nickname.Value.ToString());
            ApplySkin(Skin.Value);
        }

        void ApplyName(string n)
        {
            if (worldUI != null) worldUI.SetName(n);
            gameObject.name = "Player_" + n;
        }

        void ApplySkin(int i)
        {
            if (skins == null || skins.Length == 0 || animator == null) return;
            animator.runtimeAnimatorController = skins[Mathf.Clamp(i, 0, skins.Length - 1)];
            var pa = GetComponent<PlayerAnimator>();
            if (pa != null) pa.Refresh();
        }
    }
}
