using UnityEngine;
using Photon.Pun;

public class FloatingPlayertargeting : MonoBehaviourPunCallbacks
{
    public Camera playerCamera; // Camera for aiming
    public LayerMask targetLayer; // Layer for lockable targets
    public float lockOnRange = 10f; // Maximum range for locking onto a target

    public Transform lockedTarget { get; private set; } // The currently locked-on target
    public bool isLockedOn { get; private set; } = false; // Tracks if a target is locked

    void Update()
    {
        if (photonView.IsMine)  // Ensure only the local player can lock on
        {
            if (Input.GetKeyDown(KeyCode.R)) // Press "E" to lock/unlock target
            {
                if (isLockedOn)
                {
                    photonView.RPC("UnlockTargetRPC", RpcTarget.All);
                }
                else
                {
                    photonView.RPC("LockOnTargetRPC", RpcTarget.All);
                }
            }
        }
        
    }

    [PunRPC]
    private void LockOnTargetRPC()
    {
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, lockOnRange, targetLayer))
        {
            lockedTarget = hit.transform; // Lock onto the target
            isLockedOn = true;
            Debug.Log($"Locked onto: {lockedTarget.name}");
        }
        else
        {
            Debug.Log("No valid target found!");
        }
    }

    [PunRPC]
    public void UnlockTargetRPC()
    {
        lockedTarget = null; // Clear the locked target
        isLockedOn = false;
        Debug.Log("Target unlocked.");
    }

    // Public method to safely access `isLockedOn` from external scripts
    public bool GetIsLockedOn()
    {
        return isLockedOn;
    }
}