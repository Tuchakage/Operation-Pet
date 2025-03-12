
    using UnityEngine;

public class Seeker : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 7f;
    public float dashForce = 10f;
    public float dashCooldown = 1f;
    
    [Header("Ground Detection")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    
    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private float gravity = -9.81f;
    private bool canDash = true;
    
    private void Start()
    {
        controller = GetComponent<CharacterController>();
        
        // Error prevention: Check if groundCheck is assigned
        if (groundCheck == null)
        {
            Debug.LogError("GroundCheck transform not assigned to SeekerController!");
        }
    }
    
    private void Update()
    {
        Move();
        Jump();
        Dash();
    }
    
    private void Move()
    {
        // Get input axes
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        
        // Create movement vector
        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        
        // Apply movement
        controller.Move(move * moveSpeed * Time.deltaTime);
    }
    
    private void Jump()
    {
        // Check if grounded
        if (groundCheck != null)
        {
            isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);
        }
        
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Keeps the player grounded properly
        }
        
        // Handle jump input
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        }
        
        // Apply gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
    
    private void Dash()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
        {
            // Calculate dash
            Vector3 dashDirection = transform.forward;
            
            // Apply dash movement (fixed by adding Time.deltaTime)
            controller.Move(dashDirection * dashForce * Time.deltaTime);
            
            // Start cooldown
            canDash = false;
            Invoke(nameof(ResetDash), dashCooldown);
        }
    }
    
    private void ResetDash()
    {
        canDash = true;
    }
}

[RequireComponent(typeof(CharacterController))]
public class SimpleMoveTest : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    private CharacterController characterController;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        // Basic WASD movement in world space
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 move = new Vector3(moveX, 0f, moveZ);
        characterController.Move(move * moveSpeed * Time.deltaTime);

        // Lock rotation so there's no spinning
        transform.rotation = Quaternion.identity;
    }
}
