using UnityEngine;
using System.Collections;
using Photon.Pun;
using Photon.Realtime;

public class WizardFlick : MonoBehaviourPunCallbacks
{
    public float flickForce = 15.0f; // Force applied to flick the target
    public FloatingPlayertargeting targetingScript; // Reference to the targeting script

    void Update()
    {
        // Check if the left mouse button is clicked and a target is locked
        if (targetingScript != null && targetingScript.isLockedOn && Input.GetMouseButtonDown(0)) // Left mouse button
        {
            FlickTarget();
        }
    }

    private void FlickTarget()
    {
        // Get the currently locked target from the targeting script
        Transform lockedTarget = targetingScript.lockedTarget;

        if (lockedTarget != null)
        {
            // Ensure the target has a Rigidbody to apply force
            Rigidbody rb = lockedTarget.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Use the player's forward direction as the flick direction
                Vector3 flickDirection = transform.forward; // Floating player's forward direction

                // Apply the flick force
                rb.AddForce(flickDirection * flickForce, ForceMode.Impulse);
                Debug.Log($"Flicked target: {lockedTarget.name} in direction {flickDirection}");

                // Optionally, unlock the target after the flick
                targetingScript.UnlockTarget();
            }
        }
    }
}