using UnityEngine;
using System.Collections;
using Photon.Pun;
using Photon.Realtime;

public class WizardFlick : MonoBehaviourPunCallbacks
{
    public Camera playerCamera; // Camera used for aiming
    public float lockOnRange = 10f; // Maximum range to lock on to a target
    public float pushForce = 10f; // Force applied to the target
    public Transform reticle; // Reticle for visualizing lock-on
    public LayerMask lockableLayers; // Layers to determine lockable targets
    public float cooldownTime = 1.0f; // Cooldown time between pushes

    private Transform lockedTarget; // The currently locked-on target
    private bool isLockedOn = false; // Whether the player has locked on
    private bool canPush = true; // Cooldown state

    void Update()
    {
        // Lock-on mechanic (press "E" to lock/unlock a target)
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (isLockedOn)
            {
                UnlockTarget();
            }
            else
            {
                LockOnTarget();
            }
        }

        // Push mechanic (use right mouse button to push target)
        if (isLockedOn && Input.GetMouseButtonDown(1) && canPush) // Right mouse button
        {
            PushTarget();
        }

        // Update the reticle position to match the locked target
        if (reticle != null)
        {
            reticle.gameObject.SetActive(isLockedOn && lockedTarget != null);
            if (isLockedOn && lockedTarget != null)
            {
                reticle.position = lockedTarget.position;
            }
        }
    }

    // Perform a raycast to find the nearest lockable target
    private void LockOnTarget()
    {
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, lockOnRange, lockableLayers))
        {
            lockedTarget = hit.transform;                                           // Lock onto the target
            isLockedOn = true;
            Debug.Log($"Locked onto: {lockedTarget.name}");
        }
        else
        {
            Debug.Log("No valid target in range to lock onto!");
        }
    }

    private void UnlockTarget()
    {
        // Unlock the current target
        lockedTarget = null;
        isLockedOn = false;
        Debug.Log("Target unlocked.");
    }

    private void PushTarget()
    {
        if (lockedTarget != null)
        {
            Rigidbody rb = lockedTarget.GetComponent<Rigidbody>();              // Ensure the target has a Rigidbody to apply force
            if (rb != null)
            {
                Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);   // Calculate push direction based on mouse position and camera
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    Vector3 pushDirection = (hit.point - lockedTarget.position).normalized;
                    pushDirection.y = 0;                                                            // Optional: Keep the push horizontal

                    rb.AddForce(pushDirection * pushForce, ForceMode.Impulse);
                    Debug.Log($"Pushed target: {lockedTarget.name} in direction {pushDirection}");

                                                                                                   // Start cooldown after push
                    StartCoroutine(PushCooldown());
                }
            }
        }
    }

    private IEnumerator PushCooldown()
    {
        canPush = false;
        yield return new WaitForSeconds(cooldownTime);
        canPush = true;
    }
}