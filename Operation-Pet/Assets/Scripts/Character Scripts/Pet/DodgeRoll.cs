using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DodgeRoll : MonoBehaviourPunCallbacks
{
    InputSystem_Actions playerActionAsset;
    public float rollDistance = 5.0f; // Distance covered in the dodge roll
    public float rollSpeed = 10.0f; // Speed of the dodge roll
    public float rollCooldown = 1.5f; // Time before the player can roll again
    private bool isRolling = false; // Tracks if the player is currently rolling
    private bool canRoll = true; // Tracks if the player can roll after cooldown
    private Rigidbody rb; // Reference to Rigidbody

    void Awake()
    {
        playerActionAsset = new InputSystem_Actions();
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>(); // Get Rigidbody component
        rb.interpolation = RigidbodyInterpolation.Interpolate; // Smoother movement
    }

    void Update()
    {

    }

    void OnEnable()
    {
        if (!photonView.IsMine) return;

        //Enable the Player Action Map 
        playerActionAsset.Pet.Enable();

        //Subscribe the Jump Event to Jump function
        playerActionAsset.Pet.Dodge.started += RPCPerformRoll;



    }


    void OnDisable()
    {
        if (!photonView.IsMine) return;
        //Disable the Player Action Map 
        playerActionAsset.Pet.Disable();

        //UnSubscribe the Jump Event from the Jump function
        playerActionAsset.Pet.Dodge.started -= RPCPerformRoll;
    }

    void RPCPerformRoll(InputAction.CallbackContext context) 
    {
        if (!photonView.IsMine) return;

        // Check for player input to dodge roll (e.g., pressing Right Shift)
        if (canRoll)
        {
            photonView.RPC("PerformRoll", RpcTarget.All);
        }
    }
    [PunRPC]
    private void PerformRoll()
    {
        StartCoroutine(Roll());
    }

    private IEnumerator Roll()
    {
        canRoll = false; // Disable rolling during cooldown
        isRolling = true;

        Vector3 rollDirection = transform.forward; // Roll in the direction the player is facing
        float rollTime = rollDistance / rollSpeed; // Duration of the roll

        float elapsedTime = 0;
        while (elapsedTime < rollTime)
        {
            rb.MovePosition(rb.position + rollDirection * rollSpeed * Time.deltaTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        isRolling = false;
        FindObjectOfType<AbilityCooldownUI>().photonView.RPC("StartCooldownRPC", RpcTarget.All, "Dodge Roll", rollCooldown);
        // Start cooldown
        yield return new WaitForSeconds(rollCooldown);
        canRoll = true;
    }
}