using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class ReticleTargeting : MonoBehaviourPunCallbacks
{
    public Image reticle; // UI Image for the reticle
    public Color defaultColor = Color.white;
    public Color targetColor = Color.red;
    public float detectionRange = 5.0f;
    public LayerMask targetLayer; // Assign the target layer in Inspector

    private Camera mainCamera;

    void Start()
    {
        if (!photonView.IsMine) // Prevents non-local players from updating reticle
        {
            enabled = false;
            return;
        }

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