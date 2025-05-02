using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;

public class PetController : MonoBehaviourPun
{
    InputSystem_Actions playerActionAsset;
    InputAction move;
    ThirdPersonAnimation animationScript; //Set the animation values within the script

    Vector2 currentMovement;
    private float horizontalMovement;
    private float verticalMovement;
    bool movementPressed; //Indications whether the button to move has been pressed

    [Header("Movement")]
    private Rigidbody rb;
    [SerializeField]
    private float movementForce = 1f;
    [SerializeField]
    private float maxSpeed = 5f;

    private Vector3 forceDirection = Vector3.zero;

    [SerializeField]
    private Camera playerCamera;

    void Awake()
    {
        
        playerActionAsset = new InputSystem_Actions();

        if (photonView.IsMine) 
        {
            rb = GetComponent<Rigidbody>();
            playerCamera.enabled = true;
        }

        animationScript = GetComponent<ThirdPersonAnimation>();

        //performed is a callback function and will return the current state of the callback of the player input (Stored in ctx)
        //Code Syntax for callbacks is "+ = ctx"
        //input.Player.Move.performed += ctx => 
        //{ 
        //    //Store the value of the button being pressed
        //    currentMovement = ctx.ReadValue<Vector2>();

        //    // If the values are not 0 then that means the input to move is being pressed
        //    movementPressed = currentMovement.x != 0 || currentMovement.y != 0;
        //};
    }

    void OnEnable()
    {
        //Get the inputs for the move action
        move = playerActionAsset.Player.Move;
        //Enable the Player Action Map 
        playerActionAsset.Player.Enable();
    }

    void OnDisable()
    {
        //Disable the Player Action Map 
        playerActionAsset.Player.Disable();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animationScript.SetMaxSpeed(maxSpeed);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {
        if (!photonView.IsMine)
        {
            return;
        }
        //Depends on whether we are moving left or right relative to the camera
        forceDirection += move.ReadValue<Vector2>().x * GetCameraRight(playerCamera) * movementForce;


        forceDirection += move.ReadValue<Vector2>().y * GetCameraForward(playerCamera) * movementForce;
        //Apply the movement
        rb.AddForce(forceDirection, ForceMode.Impulse);

        //When we let go of our movement input button then the player will stop moving
        forceDirection = Vector3.zero;

        Vector3 horizontalVelocity = rb.linearVelocity;
        horizontalVelocity.y = 0f;

        //Is the square magnitude more than the movement Speed squared
        if (horizontalVelocity.sqrMagnitude > maxSpeed * maxSpeed) 
        {
            rb.linearVelocity = horizontalVelocity.normalized * maxSpeed + Vector3.up * rb.linearVelocity.y;
        }

        LookAt();
    }

    Vector3 GetCameraRight(Camera camera)
    {
        Vector3 right = playerCamera.transform.right;
        right.y = 0;

        return right.normalized;
    }

    Vector3 GetCameraForward(Camera camera) 
    {
        Vector3 forward = playerCamera.transform.forward;
        forward.y = 0;

        return forward.normalized;
    }

    void LookAt() 
    {
        Vector3 direction = rb.linearVelocity;
        //Make sure the player doesn't rotate up or down
        direction.y = 0f;

        //as long as the player is moving and input is being pressed
        if (move.ReadValue<Vector2>().sqrMagnitude > 0.1f && direction.sqrMagnitude > 0.1f)
        {
            //Change the direction of where the player looks
            rb.rotation = Quaternion.LookRotation(direction, Vector3.up);
        }
        else 
        {
            rb.angularVelocity = Vector3.zero;
        }

    }



}
