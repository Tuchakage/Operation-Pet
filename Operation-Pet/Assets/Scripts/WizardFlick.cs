using UnityEngine;
using Photon.Pun;
using System.Collections;

public class WizardFlick : MonoBehaviourPunCallbacks
{
    public float flickForce = 15.0f; // Force applied to flick the target
    public Camera playerCamera; // Camera used to target opponents
    private Transform lockedTarget; // The currently locked target
    public LayerMask groundPlayerLayer; // Layer for ground players

    private bool isTargetLocked = false; // Tracks if a ground player is locked on

    void Update()
    {
        if (!photonView.IsMine) return;

        // Lock onto the closest ground player when Right Mouse Button is clicked
        if (Input.GetMouseButtonDown(1))
        {
            LockOnGroundPlayer();
        }

        // Flick the locked ground player when Left Mouse Button is clicked
        if (Input.GetMouseButtonDown(0) && isTargetLocked)
        {
            photonView.RPC("FlickTargetRPC", RpcTarget.AllBuffered);
        }
    }

    private void LockOnGroundPlayer()
    {
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // Check if the raycast hits a ground player
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundPlayerLayer))
        {
            if (hit.collider.CompareTag("Player")) // Ensure target is a ground player
            {
                lockedTarget = hit.transform; // Assign the locked target
                isTargetLocked = true;
                Debug.Log($"Locked onto Ground Player: {lockedTarget.name}");
            }
        }
        else
        {
            isTargetLocked = false;
            lockedTarget = null;
            Debug.Log("No Ground Player found.");
        }
    }

    [PunRPC]
    private void FlickTargetRPC()
    {
        if (lockedTarget != null && lockedTarget.CompareTag("Player")) // Ensure flicking only ground players
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
                Debug.LogError($"Target {lockedTarget.name} does not have a Rigidbody!");
            }

            // Unlock target after flicking
            isTargetLocked = false;
            lockedTarget = null;
        }
    }
}