using UnityEngine;

public class FloatingPlayertargeting : MonoBehaviour
{
    public Camera playerCamera; // Camera for aiming
    public LayerMask targetLayer; // Layer for lockable targets
    public float lockOnRange = 10f; // Maximum range for locking onto a target

    public Transform lockedTarget { get; private set; } // The currently locked-on target
    public bool isLockedOn { get; private set; } = false; // Tracks if a target is locked

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E)) // Press "E" to lock/unlock target
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
    }

    public void LockOnTarget()
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

    public void UnlockTarget()
    {
        lockedTarget = null; // Clear the locked target
        isLockedOn = false;
        Debug.Log("Target unlocked.");
    }
}