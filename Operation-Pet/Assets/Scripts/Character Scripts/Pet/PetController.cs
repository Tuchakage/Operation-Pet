using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;
using Cinemachine;

[RequireComponent(typeof(Rigidbody))]
public class PetController : MonoBehaviourPun
{
    
    InputSystem_Actions playerActionAsset;
    InputAction move;

    [SerializeField]
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
    private float jumpForce = 5f;
    [SerializeField]
    private float maxSpeed = 5f;
    //[SerializeField, Range(0.2f, 50f)]
    //private float rayCastDistance = 1.2f; //Distance for the Raycast that checks if the player is Grounded


    private Vector3 forceDirection = Vector3.zero;

    [SerializeField]
    private Camera playerCamera;

    void Awake()
    {
        
        playerActionAsset = new InputSystem_Actions();

        if (photonView.IsMine) 
        {
            playerCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();

            //Get access ot the CineMachine Free Look Component so that we can set what the camera needs to follow and Look At
            CinemachineFreeLook cineMachine = playerCamera.GetComponent<CinemachineFreeLook>();
            cineMachine.Follow = this.transform;
            cineMachine.LookAt = this.transform;

            rb = GetComponent<Rigidbody>();
            Cursor.lockState = CursorLockMode.Locked;
        }


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
        //Subscribe the Jump Event to Jump function
        playerActionAsset.Player.Jump.started += Jump;
        playerActionAsset.Player.Attack.started += Attack;

        //Get the inputs for the move action
        move = playerActionAsset.Player.Move;
        //Enable the Player Action Map 
        playerActionAsset.Player.Enable();
    }


    void OnDisable()
    {
        //Disable the Player Action Map 
        playerActionAsset.Player.Disable();

        //UnSubscribe the Jump Event from the Jump function
        playerActionAsset.Player.Jump.started += Jump;
        playerActionAsset.Player.Attack.started -= Attack;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (animationScript != null)
        {
            //Make sure the Max Speed for the animation is the same as the maxSpeed set in this scriptvcd
            animationScript.SetMaxSpeed(maxSpeed);
        }
        else 
        {
            Debug.LogWarning("No Animator is attached. Please drag and drop from the Inspector");
        }
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

        //Increase acceleration as we are falling
        if (rb.linearVelocity.y < 0f) 
        {
            rb.linearVelocity -= Vector3.down * Physics.gravity.y * Time.deltaTime;
        }

        Vector3 horizontalVelocity = rb.linearVelocity;
        horizontalVelocity.y = 0f;

        //Is the square magnitude more than the movement Speed squared
        if (horizontalVelocity.sqrMagnitude > maxSpeed * maxSpeed) 
        {
            rb.linearVelocity = horizontalVelocity.normalized * maxSpeed + Vector3.up * rb.linearVelocity.y;
        }

        LookAt();
    }

     void Jump(InputAction.CallbackContext context)
    {
        Debug.Log(IsGrounded());
        if (IsGrounded()) 
        {
            //Jump upwards
            forceDirection += Vector3.up * jumpForce;
        }
    }

    bool IsGrounded() 
    {
        //Use Raycast to check what is below us
        // transform.position + Vector3.up * 0.25f = Makes sure we ray cast from above
        Vector3 origin = transform.position + Vector3.up * .25f;
        Ray ray = new Ray(origin, Vector3.down);

        //Spawn in the raycast and if it hit something return true
        if (Physics.Raycast(ray, out RaycastHit hit, 1.6f))
        {
            Debug.DrawLine(origin, Vector3.down, Color.red);
            return true;
        }
        else 
        {
            Debug.DrawLine(origin, Vector3.down, Color.green);
            return false;
        }
        
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

    void Attack(InputAction.CallbackContext context)
    {
        if (animationScript != null)
        {
            animationScript.Attack();
        }
        else
        {
            Debug.LogWarning("Animation Script is not attached. Please drag and drop from the Inspector");
        }
    }



}
