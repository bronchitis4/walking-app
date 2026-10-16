using System.Runtime.InteropServices;
using UnityEngine;

namespace RoomGame
{
    /// <summary>Copies text to the clipboard; uses the browser clipboard API on WebGL.</summary>
    public static class WebClipboard
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] static extern void RoomGame_CopyToClipboard(string text);
#endif

        public static void Copy(string text)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            RoomGame_CopyToClipboard(text);
#else
            GUIUtility.systemCopyBuffer = text;
#endif
        }
    }
}
