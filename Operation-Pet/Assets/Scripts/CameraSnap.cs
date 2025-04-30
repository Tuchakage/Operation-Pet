using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GroundPlayerScreenshot : MonoBehaviour
{
    public Camera thirdPersonCamera; // Third-person camera
    public Camera firstPersonCamera; // First-person camera
    public Transform firstPersonSpawnPoint; // First-person camera spawn location
    public RenderTexture renderTexture; // Screenshot texture
    public RawImage uiImage; // UI display for the screenshot
    public GameObject trueItem; // The real item that appears different in the screenshot
    public Material alternateMaterial; // Different material for the true item in screenshot
    private Material originalMaterial; // Store the original material

    void Start()
    {
        // Ensure first-person camera is disabled at start
        firstPersonCamera.enabled = false;

        // Set first-person camera position to the predefined spawn point
        firstPersonCamera.transform.position = firstPersonSpawnPoint.position;
        firstPersonCamera.transform.rotation = firstPersonSpawnPoint.rotation;

        // Store original material of the true item
        originalMaterial = trueItem.GetComponent<Renderer>().material;
    }

    void Update()
    {
        // Press "P" to switch views, apply different look, take a screenshot, and revert
        if (Input.GetKeyDown(KeyCode.P))
        {
            StartCoroutine(CaptureScreenshot());
        }
    }

    private IEnumerator CaptureScreenshot()
    {
        // Switch to first-person view
        thirdPersonCamera.enabled = false;
        firstPersonCamera.enabled = true;
        firstPersonCamera.transform.position = firstPersonSpawnPoint.position;
        firstPersonCamera.transform.rotation = firstPersonSpawnPoint.rotation;

        // Temporarily swap material for the true item
        trueItem.GetComponent<Renderer>().material = alternateMaterial;

        yield return new WaitForSeconds(0.2f); // Short delay for transition

        // Capture screenshot
        firstPersonCamera.targetTexture = renderTexture;
        firstPersonCamera.Render();
        uiImage.texture = renderTexture;

        yield return new WaitForSeconds(0.2f); // Short delay before switching back

        // Restore original material after the screenshot
        trueItem.GetComponent<Renderer>().material = originalMaterial;

        // Switch back to third-person view
        firstPersonCamera.enabled = false;
        thirdPersonCamera.enabled = true;
    }
}
