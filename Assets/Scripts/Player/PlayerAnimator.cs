using Unity.Netcode;
using UnityEngine;

namespace RoomGame
{
    /// <summary>Picks idle/walk/jump/typing on the owner and syncs it to everyone via NetworkVariables.</summary>
    public class PlayerAnimator : NetworkBehaviour
    {
        public enum State : byte { Idle, Walk, Jump, Typing, JumpFront }

        static readonly string[] StateNames = { "idle", "walk", "jump_side", "typing", "jump" };

        public Animator animator;
        public SpriteRenderer spriteRenderer;
        PlayerMovement movement;

        readonly NetworkVariable<State> state = new(State.Idle, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        readonly NetworkVariable<bool> facingLeft = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        State playing = (State)255;
        bool wasAirborne;
        bool jumpFront;

        void Awake()
        {
            movement = GetComponent<PlayerMovement>();
            if (animator == null) animator = GetComponentInChildren<Animator>();
            if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        void Update()
        {
            if (!IsSpawned) return;

            if (IsOwner)
            {
                float vx = movement != null ? movement.HorizontalVelocity : 0f;
                State s;
                bool airborne = movement != null && !movement.IsGrounded;
                // Decide jump style at takeoff: standing still -> front jump, moving -> side jump.
                if (airborne && !wasAirborne) jumpFront = Mathf.Abs(vx) <= 0.1f;
                wasAirborne = airborne;

                if (ChatUI.IsTyping && !airborne) s = State.Typing;
                else if (airborne) s = jumpFront ? State.JumpFront : State.Jump;
                else if (Mathf.Abs(vx) > 0.1f) s = State.Walk;
                else s = State.Idle;
                if (state.Value != s) state.Value = s;
                if (vx < -0.1f && !facingLeft.Value) facingLeft.Value = true;
                else if (vx > 0.1f && facingLeft.Value) facingLeft.Value = false;
            }

            Apply();
        }

        void Apply()
        {
            // Walk/jump are side-view sprites facing right; mirror them for left. Idle/typing face the viewer.
            bool sideView = state.Value == State.Walk || state.Value == State.Jump;
            spriteRenderer.flipX = sideView && facingLeft.Value;

            if (playing != state.Value && animator != null && animator.runtimeAnimatorController != null)
            {
                playing = state.Value;
                animator.Play(StateNames[(int)playing], 0, 0f);
            }
        }

        /// <summary>Forces the current state to replay (used after a skin swap).</summary>
        public void Refresh() => playing = (State)255;
    }
}
