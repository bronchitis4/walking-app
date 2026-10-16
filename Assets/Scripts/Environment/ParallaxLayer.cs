using UnityEngine;

namespace RoomGame
{
    /// <summary>Moves a background layer with a fraction of the camera's X movement.</summary>
    public class ParallaxLayer : MonoBehaviour
    {
        [Tooltip("0 = static in world, 1 = moves fully with camera")]
        [Range(0f, 1f)] public float factor = 0.5f;

        Transform cam;
        float startX;
        float camStartX;

        void Start()
        {
            cam = Camera.main != null ? Camera.main.transform : null;
            startX = transform.position.x;
            if (cam != null) camStartX = cam.position.x;
        }

        void LateUpdate()
        {
            if (cam == null) return;
            var p = transform.position;
            p.x = startX + (cam.position.x - camStartX) * factor;
            transform.position = p;
        }
    }
}
