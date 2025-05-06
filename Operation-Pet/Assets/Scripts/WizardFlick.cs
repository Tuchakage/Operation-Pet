using UnityEngine;
using Photon.Pun;
using System.Collections;
using UnityEngine.InputSystem;

public class WizardFlick : MonoBehaviourPunCallbacks
{
    InputSystem_Actions playerActionAsset;

    public float flickForce = 10.0f; // Force applied to flick the target
    public Camera playerCamera; // Camera used to target opponents
    private Transform lockedTarget; // The currently locked target
    public LayerMask groundPlayerLayer; // Layer for ground players
    public float flickCooldownDuration = 5.0f; // Cooldown time after flicking

    private bool isTargetLocked = false; // Tracks if a ground player is locked on
    private bool canFlick = true; // Prevents repeated flicking before cooldown

    void Awake()
    {
        playerActionAsset = new InputSystem_Actions();
    }

    void OnEnable()
    {
        if (!photonView.IsMine) return;

        //Enable the Player Action Map 
        playerActionAsset.Wizard.Enable();

        playerActionAsset.Wizard.LockOnTarget.started += LockOnGroundPlayer;
        playerActionAsset.Wizard.Flick.started += CallFlickRPC;


    }



    void OnDisable()
    {
        if (!photonView.IsMine) return;
        //Disable the Player Action Map 
        playerActionAsset.Wizard.Disable();

        playerActionAsset.Wizard.LockOnTarget.started -= LockOnGroundPlayer;
        playerActionAsset.Wizard.Flick.started -= CallFlickRPC;
    }

    // Lock onto the closest ground player when Right Mouse Button is clicked
    private void LockOnGroundPlayer(InputAction.CallbackContext context)
    {
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundPlayerLayer))
        {
            if (hit.collider.CompareTag("Player"))
            {
                lockedTarget = hit.transform;
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

    void CallFlickRPC(InputAction.CallbackContext context) 
    {
        // Flick the locked ground player when Left Mouse Button is clicked
        if (isTargetLocked && canFlick) 
        {
            photonView.RPC("FlickTargetRPC", RpcTarget.AllBuffered);
            canFlick = false; // Start cooldown
            StartCoroutine(FlickCooldown()); // Begin cooldown timer
        }
    }

    [PunRPC]
    private void FlickTargetRPC()
    {
        if (lockedTarget != null && lockedTarget.CompareTag("Player"))
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
            FindObjectOfType<AbilityCooldownUI>().photonView.RPC("StartCooldownRPC", RpcTarget.All, "Wizard Flick", flickCooldownDuration);
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