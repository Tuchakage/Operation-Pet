using UnityEngine;
using Photon.Pun;

public class WizardFlick : MonoBehaviourPunCallbacks
{
    public float flickForce = 15.0f; // Force applied to flick the target
    public FloatingPlayertargeting targetingScript; // Reference to the targeting script
    public LayerMask groundPlayerLayer; // Layer for ground players

    void Update()
    {
        if (!photonView.IsMine) return;

        // Check if left mouse button is clicked and a target is locked
        if (targetingScript != null && targetingScript.GetIsLockedOn() && Input.GetMouseButtonDown(0))
        {
            photonView.RPC("FlickTargetRPC", RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    private void FlickTargetRPC()
    {
        // Get the currently locked target from the targeting script
        Transform lockedTarget = targetingScript.lockedTarget;

        if (lockedTarget != null)
        {
            // Check if the target is in the Ground Player layer
            if ((groundPlayerLayer.value & (1 << lockedTarget.gameObject.layer)) > 0)
            {
                Rigidbody rb = lockedTarget.GetComponent<Rigidbody>();

                if (rb != null)
                {
                    Vector3 flickDirection = (lockedTarget.position - transform.position).normalized;
                    rb.AddForce(flickDirection * flickForce, ForceMode.Impulse);
                    Debug.Log($"Flicked Ground Player: {lockedTarget.name} in direction {flickDirection}");
                }
                else
                {
                    Debug.LogError($"Target {lockedTarget.name} is on the ground player layer but does not have a Rigidbody!");
                }
            }
            else
            {
                Debug.Log($"Target {lockedTarget.name} is not in the Ground Player layer.");
            }

            // Unlock the target after flicking
            targetingScript.UnlockTargetRPC();
        }
    }
}