using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Animator))]
public class CharacterMovement : MonoBehaviour
{
    // Movement Parameters
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float dashForce = 10f;
    [SerializeField] private float dashCooldown = 1f;

    // Ground Check Parameters
    [Header("Ground Detection")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    // Component References
    private Rigidbody rb;
    private Animator animator;

    // State Variables
    private bool isGrounded;
    private bool canDash = true;
    private float dashCooldownTimer = 0f;
    private Vector3 moveDirection;

    // Animation Parameter IDs (for efficiency)
    private int animIsMovingId;
    private int animIsGroundedId;
    private int animJumpId;
    private int animDashId;

    private void Awake()
    {
        // Automatically get or add required components
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        // Create ground check if it doesn't exist
        if (groundCheck == null)
        {
            GameObject checkObject = new GameObject("GroundCheck");
            checkObject.transform.SetParent(transform);
            checkObject.transform.localPosition = new Vector3(0, -1f, 0);
            groundCheck = checkObject.transform;
            Debug.Log("Ground check created automatically");
        }

        // Set up physics properly
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        // Cache animator parameter IDs for better performance
        animIsMovingId = Animator.StringToHash("IsMoving");
        animIsGroundedId = Animator.StringToHash("IsGrounded");
        animJumpId = Animator.StringToHash("Jump");
        animDashId = Animator.StringToHash("Dash");
    }

    private void Start()
    {
        // Verify the ground layer is set up
        if (groundLayer.value == 0)
        {
            Debug.LogWarning("Ground layer not set! Creating and assigning a default ground layer.");

            // Create a ground layer if it doesn't exist
            if (LayerMask.NameToLayer("Ground") == -1)
            {
                // Note: In actual runtime, you can't create layers dynamically
                // This is just for notification purposes
                Debug.LogError("Please create a 'Ground' layer in your project settings and assign it to your ground objects.");
            }

            groundLayer = LayerMask.GetMask("Ground");
        }
    }

    private void Update()
    {
        // Handle input
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        // Calculate movement direction
        moveDirection = new Vector3(horizontalInput, 0f, verticalInput).normalized;

        // Handle jumping
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            Jump();
        }

        // Handle dashing
        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
        {
            Dash();
        }

        // Handle dash cooldown
        if (!canDash)
        {
            dashCooldownTimer -= Time.deltaTime;
            if (dashCooldownTimer <= 0f)
            {
                canDash = true;
            }
        }

        // Update animations
        UpdateAnimations();
    }

    private void FixedUpdate()
    {
        // Check if character is grounded
        CheckGrounded();

        // Apply movement in FixedUpdate for consistent physics
        Move();
    }

    private void CheckGrounded()
    {
        // Use a sphere overlap to check if the character is grounded
        isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);
    }

    private void Move()
    {
        // Only control horizontal movement
        Vector3 horizontalVelocity = moveDirection * moveSpeed;
        rb.linearVelocity = new Vector3(horizontalVelocity.x, rb.linearVelocity.y, horizontalVelocity.z);
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z); // Reset Y velocity
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

        // Trigger jump animation
        animator.SetTrigger(animJumpId);
    }

    private void Dash()
    {
        Vector3 dashDirection = moveDirection;

        // If no direction is input, dash forward
        if (dashDirection.magnitude < 0.1f)
        {
            dashDirection = transform.forward;
        }

        // Apply dash force
        rb.AddForce(dashDirection.normalized * dashForce, ForceMode.Impulse);

        // Start cooldown
        canDash = false;
        dashCooldownTimer = dashCooldown;

        // Trigger dash animation
        animator.SetTrigger(animDashId);
    }

    private void UpdateAnimations()
    {
        // Update animator parameters
        animator.SetBool(animIsMovingId, moveDirection.magnitude > 0.1f);
        animator.SetBool(animIsGroundedId, isGrounded);
    }

    // Visual debugging
    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            // Visualize ground check radius
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}