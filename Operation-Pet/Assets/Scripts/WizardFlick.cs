using UnityEngine;
using Photon.Pun;
using System.Collections;

public class WizardFlick : MonoBehaviourPunCallbacks
{
    public float flickForce = 10.0f; // Force applied to flick the target
    public Camera playerCamera; // Camera used to target opponents
    private Transform lockedTarget; // The currently locked target
    public LayerMask targetLayer; // Layer for valid flick targets
    public float flickCooldownDuration = 5.0f; // Cooldown time after flicking
    public float lockOnRange = 10f; // Maximum lock-on range

    private bool isTargetLocked = false; // Tracks if a target is locked on
    private bool canFlick = true; // Prevents repeated flicking before cooldown

    void Update()
    {
        if (!photonView.IsMine) return;

        // Lock onto a target in player's view when "R" is pressed
        if (Input.GetKeyDown(KeyCode.R))
        {
            LockOnTargetInView();
        }

        // Flick the locked target when Left Mouse Button is clicked
        if (Input.GetMouseButtonDown(0) && isTargetLocked && canFlick)
        {
            photonView.RPC("FlickTargetRPC", RpcTarget.AllBuffered);
            canFlick = false; // Start cooldown
            StartCoroutine(FlickCooldown()); // Begin cooldown timer
        }
    }

    private void LockOnTargetInView()
    {
        Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, lockOnRange, targetLayer))
        {
            lockedTarget = hit.transform;
            isTargetLocked = true;
            Debug.Log($"Locked onto Target: {lockedTarget.name}");
        }
        else
        {
            isTargetLocked = false;
            lockedTarget = null;
            Debug.Log("No valid target found!");
        }
    }

    [PunRPC]
    private void FlickTargetRPC()
    {
        if (lockedTarget != null)
        {
            Rigidbody rb = lockedTarget.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 flickDirection = (lockedTarget.position - transform.position).normalized;
                rb.AddForce(flickDirection * flickForce, ForceMode.Impulse);
                Debug.Log($"Flicked Target: {lockedTarget.name} in direction {flickDirection}");
            }
            else
            {
                Debug.LogError($"Target {lockedTarget.name} does not have a Rigidbody!");
            }

            isTargetLocked = false;
            lockedTarget = null;
        }
    }

    private IEnumerator FlickCooldown()
    {
        yield return new WaitForSeconds(flickCooldownDuration);
        canFlick = true; // Reset cooldown, allowing another flick
    }
}