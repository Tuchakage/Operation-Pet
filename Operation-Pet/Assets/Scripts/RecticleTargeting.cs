using UnityEngine;
using UnityEngine.UI;

public class ReticleTargeting : MonoBehaviour
{
    public Image reticle; // UI Image for the reticle
    public Color defaultColor = Color.white;
    public Color targetColor = Color.red;
    public float detectionRange = 5.0f;
    public LayerMask targetLayer; // Assign the target layer in Inspector

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        UpdateReticleColor();
    }

    private void UpdateReticleColor()
    {
        Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, detectionRange, targetLayer))
        {
            if (hit.collider.CompareTag("Target"))
            {
                reticle.color = targetColor; // Change color when aiming at a target
            }
            else
            {
                reticle.color = defaultColor; // Default color
            }
        }
        else
        {
            reticle.color = defaultColor; // Default color
        }
    }
}