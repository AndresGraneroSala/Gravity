using UnityEngine;
using System.Runtime.InteropServices;

public class FullscreenToggle : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern void ToggleFullscreen();

    public void Toggle()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        ToggleFullscreen();
#else
        Screen.fullScreen = !Screen.fullScreen;
#endif
    }
}