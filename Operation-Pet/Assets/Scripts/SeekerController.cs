using UnityEngine;

public class SeekerCharacterMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;
    public float jumpForce = 7f;
    public float dashSpeed = 10f;
    public float dashCooldown = 1f;

    [Header("Ground Detection")]
    public LayerMask groundLayer;
    private bool isGrounded;

    [Header("Components")]
    private Rigidbody rb;
    private Animator animator;

    void Start()
    {
        // Automatically get required components
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();

        if (rb == null) Debug.LogError("Rigidbody missing on " + gameObject.name);
        if (animator == null) Debug.LogWarning("Animator not assigned on " + gameObject.name);
    }

    void Update()
    {
        HandleMovement();
        HandleJump();
        HandleDash();
    }

    void HandleMovement()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 moveDirection = new Vector3(moveX, 0, moveZ).normalized;

        if (moveDirection.magnitude >= 0.1f)
        {
            transform.Translate(moveDirection * moveSpeed * Time.deltaTime, Space.World);

            // Rotate towards movement direction
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

            if (animator)
                animator.SetBool("isRunning", true);
        }
        else
        {
            if (animator)
                animator.SetBool("isRunning", false);
        }
    }

    void HandleJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded())
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

            if (animator)
                animator.SetTrigger("Jump");
        }
    }

    void HandleDash()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && IsGrounded())
        {
            StartCoroutine(Dash());
        }
    }

    System.Collections.IEnumerator Dash()
    {
        Vector3 dashVelocity = transform.forward * dashSpeed;
        rb.linearVelocity = new Vector3(dashVelocity.x, rb.linearVelocity.y, dashVelocity.z);

        yield return new WaitForSeconds(0.2f); // Dash duration
        rb.linearVelocity = Vector3.zero; // Stop dash

        yield return new WaitForSeconds(dashCooldown);
    }

    // Proper Ground Detection using LayerMask (No Tag Error)
    bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, 1.1f, groundLayer);
    }
}
