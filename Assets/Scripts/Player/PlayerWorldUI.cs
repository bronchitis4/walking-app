using TMPro;
using UnityEngine;

namespace RoomGame
{
    /// <summary>Name tag and chat bubble shown above a player (world-space canvas).</summary>
    public class PlayerWorldUI : MonoBehaviour
    {
        public TMP_Text nameText;
        public CanvasGroup bubbleGroup;
        public TMP_Text bubbleText;
        public float bubbleSeconds = 4f;
        public float fadeSeconds = 0.5f;
        public float maxBubbleWidth = 260f;

        float hideAt = -1f;

        void Awake()
        {
            if (bubbleGroup != null) bubbleGroup.alpha = 0f;
        }

        public void SetName(string n)
        {
            if (nameText != null) nameText.text = n;
        }

        public void ShowBubble(string message)
        {
            if (bubbleGroup == null) return;
            bubbleText.text = message;
            var layout = bubbleText.GetComponent<UnityEngine.UI.LayoutElement>();
            if (layout != null) layout.preferredWidth = Mathf.Min(bubbleText.GetPreferredValues(message).x + 2f, maxBubbleWidth);
            bubbleGroup.alpha = 1f;
            hideAt = Time.time + bubbleSeconds;
        }

        void Update()
        {
            if (hideAt < 0f || bubbleGroup == null) return;
            float t = Time.time - hideAt;
            if (t >= 0f)
            {
                bubbleGroup.alpha = Mathf.Clamp01(1f - t / fadeSeconds);
                if (t >= fadeSeconds) hideAt = -1f;
            }
        }
    }
}
