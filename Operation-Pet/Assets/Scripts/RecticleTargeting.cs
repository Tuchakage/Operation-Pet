using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class ReticleTargeting : MonoBehaviourPunCallbacks
{
    public RectTransform reticle; // Assign the UI image for the reticle
    public Color defaultColor = Color.white;
    public Color targetColor = Color.red;
    public float detectionRange = 5.0f; // Range within which targets are highlighted

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
        Ray ray = mainCamera.ScreenPointToRay(reticle.position);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, detectionRange))
        {
            if (hit.collider.CompareTag("Target")) // Set the target object's tag to "Target"
            {
                reticle.GetComponent<UnityEngine.UI.Image>().color = targetColor; // Change color when aiming at a target
            }
            else
            {
                reticle.GetComponent<UnityEngine.UI.Image>().color = defaultColor; // Default color
            }
        }
        else
        {
            reticle.GetComponent<UnityEngine.UI.Image>().color = defaultColor; // Default color
        }
    }
}
