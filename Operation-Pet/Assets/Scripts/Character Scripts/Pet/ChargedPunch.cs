using UnityEngine;
using Photon.Pun;
using System.Collections;
using UnityEngine.InputSystem;
using System;

public class ChargedPunch : MonoBehaviourPunCallbacks
{
    InputSystem_Actions playerActionAsset;

    public float punchForce = 50f; // Knockback force
    public float upwardForce = 5f; // Adds slight upward push
    public float chargeTime = 2f; // Time required to fully charge
    public float disableMovementDuration = 2f; // Opponent loses movement for this time
    public LayerMask playerLayer; // Assigned layer for valid players
    private bool isCharging = false;
    private bool isFullyCharged = false;

    void Awake()
    {
        playerActionAsset = new InputSystem_Actions();
    }

    void Update()
    {
        if (!photonView.IsMine) return;



        // Throw punch when Right Mouse Button (RMB) is released after full charge
        if (Input.GetMouseButtonUp(1) && isFullyCharged)
        {

        }

        // Stop charging if RMB is released before full charge
        if (Input.GetMouseButtonUp(1))
        {
            isCharging = false;
            isFullyCharged = false;
        }
    }

    void OnEnable()
    {
        playerActionAsset.Pet.HeavyAttack.started += CallChargePunch;

        playerActionAsset.Pet.HeavyAttack.canceled += ReleaseChargePunch;

        //Enable the Player Action Map 
        playerActionAsset.Pet.Enable();

    }

    void ReleaseChargePunch(InputAction.CallbackContext context)
    {
        Debug.Log("Released");
        if (isFullyCharged)
        {
            photonView.RPC("ExecutePunchRPC", RpcTarget.All);
            isFullyCharged = false; // Reset charge state
        }
        else 
        {
            isCharging = false;
            isFullyCharged = false;
        }
    }

    void CallChargePunch(InputAction.CallbackContext context)
    {
        Debug.Log("Pressed");
        if (!photonView.IsMine) return;

        isCharging = true;
        StartCoroutine(ChargePunch());
    }

    void OnDisable()
    {
        playerActionAsset.Pet.Disable();

        playerActionAsset.Pet.HeavyAttack.started -= CallChargePunch;

        playerActionAsset.Pet.HeavyAttack.canceled -= ReleaseChargePunch;
    }


    private IEnumerator ChargePunch()
    {
        
        yield return new WaitForSeconds(chargeTime);
        if (isCharging) // Only set as fully charged if RMB is still held
        {
            isFullyCharged = true;
            Debug.Log("Punch is fully charged!");
        }
    }

    [PunRPC]
    private void ExecutePunchRPC()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, 3f, playerLayer)) // Check only valid players
        {
            if (hit.collider.CompareTag("Player")) // Ensure only characters with "Player" tag are affected
            {
                Rigidbody rb = hit.collider.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    FindObjectOfType<AbilityCooldownUI>().photonView.RPC("StartCooldownRPC", RpcTarget.All, "Charged Punch", chargeTime);
                    // Calculate push direction
                    Vector3 pushDirection = (hit.collider.transform.position - transform.position).normalized;
                    pushDirection.y += upwardForce; // Add slight upward force

                    // Apply increased knockback force
                    rb.AddForce(pushDirection * punchForce, ForceMode.Impulse);

                    StartCoroutine(DisableMovement(hit.collider.transform)); // Disable movement for opponent
                    Debug.Log($"Charged punch landed on {hit.collider.name} with force {punchForce}!");
                }
            }
        }
    }

    private IEnumerator DisableMovement(Transform target)
    {
        Rigidbody rb = target.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true; // Disable movement
            yield return new WaitForSeconds(disableMovementDuration);
            rb.isKinematic = false; // Restore movement after 2 seconds
        }
    }
}