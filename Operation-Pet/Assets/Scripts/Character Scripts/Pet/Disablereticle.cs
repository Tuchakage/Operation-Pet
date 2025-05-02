using UnityEngine;
using UnityEngine.UI;

public class ScreenshotVisibility : MonoBehaviour
{
    public GameObject floatingPlayer; // Floating character GameObject
    public GameObject groundPlayer; // Ground player GameObject
    public RawImage screenshotDisplay; // UI element showing the screenshot

    void Update()
    {
        // Disable screenshot display if the floating player is active
        if (floatingPlayer.activeSelf)
        {
            screenshotDisplay.enabled = false;
        }
        else if (groundPlayer.activeSelf)
        {
            screenshotDisplay.enabled = true;
        }
    }
}
