using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace RoomGame
{
    /// <summary>Side-view movement (A/D or arrows, Space to jump). Owner-authoritative.</summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerMovement : NetworkBehaviour
    {
        public float moveSpeed = 3.5f;
        public float jumpVelocity = 7f;
        public LayerMask groundMask = ~0;

        Rigidbody2D rb;
        Collider2D col;
        readonly RaycastHit2D[] groundHits = new RaycastHit2D[4];

        public float HorizontalVelocity => rb != null ? rb.linearVelocity.x : 0f;
        public bool IsGrounded { get; private set; }

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<Collider2D>();
        }

        public override void OnNetworkSpawn()
        {
            // Only the owner simulates physics; remote copies are driven by NetworkTransform.
            rb.bodyType = IsOwner ? RigidbodyType2D.Dynamic : RigidbodyType2D.Kinematic;
            if (IsOwner && GameManager.Instance != null)
            {
                transform.position = GameManager.Instance.GetSpawnPosition();
                GameManager.Instance.OnLocalPlayerSpawned(this);
            }
        }

        void Update()
        {
            if (!IsSpawned || !IsOwner) return;

            IsGrounded = CheckGrounded();
            var kb = Keyboard.current;
            float x = 0f;
            bool jump = false;
            if (kb != null && !ChatUI.IsTyping)
            {
                if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) x -= 1f;
                if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) x += 1f;
                jump = kb.spaceKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame || kb.upArrowKey.wasPressedThisFrame;
            }

            var v = rb.linearVelocity;
            v.x = x * moveSpeed;
            if (jump && IsGrounded) v.y = jumpVelocity;
            rb.linearVelocity = v;
        }

        bool CheckGrounded()
        {
            if (col == null) return true;
            if (rb.linearVelocity.y > 0.01f) return false;
            var b = col.bounds;
            // Ignore triggers (camera bounds, lamp area) and our own collider.
            var filter = new ContactFilter2D { useTriggers = false, useLayerMask = true, layerMask = groundMask };
            int n = Physics2D.BoxCast(new Vector2(b.center.x, b.min.y + 0.05f), new Vector2(b.size.x * 0.8f, 0.05f), 0f, Vector2.down, filter, groundHits, 0.08f);
            for (int i = 0; i < n; i++)
                if (groundHits[i].collider != col) return true;
            return false;
        }
    }
}
