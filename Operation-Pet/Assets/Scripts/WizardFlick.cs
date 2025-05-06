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
    public LayerMask targetLayer; // Layer for valid flick targets
    public float flickCooldownDuration = 5.0f; // Cooldown time after flicking
    public float lockOnRange = 10f; // Maximum lock-on range

    private bool isTargetLocked = false; // Tracks if a target is locked on
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

        playerActionAsset.Wizard.LockOnTarget.started += LockOnTargetInView;
        playerActionAsset.Wizard.Flick.started += CallFlickRPC;


    }



    void OnDisable()
    {
        if (!photonView.IsMine) return;
        //Disable the Player Action Map 
        playerActionAsset.Wizard.Disable();

        playerActionAsset.Wizard.LockOnTarget.started -= LockOnTargetInView;
        playerActionAsset.Wizard.Flick.started -= CallFlickRPC;
    }

    void Update()
    {
        if (!photonView.IsMine) return;




        // Flick the locked target when Left Mouse Button is clicked
        if (Input.GetMouseButtonDown(0) && isTargetLocked && canFlick)
        {

        }
    }


    // Lock onto a target in player's view when "R" is pressed
    private void LockOnTargetInView(InputAction.CallbackContext context)
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

    void CallFlickRPC(InputAction.CallbackContext context) 
    {
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
        if (lockedTarget != null)
        {
            Rigidbody rb = lockedTarget.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 direction = (lockedTarget.position - transform.position).normalized;

                // Add upward lift
                Vector3 flickDirection = new Vector3(direction.x, 10f, direction.z).normalized;

                // Apply strong upward + backward force
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