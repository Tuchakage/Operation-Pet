using Photon.Pun;
using UnityEngine;

public class ThirdPersonControllers : MonoBehaviourPunCallbacks
{
    public float moveSpeed = 5.0f; // Speed for character movement
    public float rotationSpeed = 720.0f; // Speed for character rotation
    public float cameraFollowSpeed = 5.0f; // Speed for camera movement
    public float mouseSensitivity = 3.0f; // Sensitivity for camera rotation
    public Transform cameraTarget; // Target for the camera to follow (e.g., player's head or body)
    public Camera playerCamera; // Reference to the camera following the character

    private CharacterController characterController; // Unity's Character Controller
    private float cameraPitch = 0f; // Tracks vertical camera rotation
    private bool allowCameraRotation = true; // Flag to enable/disable camera rotation

    void Start()
    {
        // Check if this is the local player
        if (!photonView.IsMine)
        {
            // Disable components for remote players
            if (playerCamera != null)
            {
                playerCamera.enabled = false;
            }
            enabled = false; // Disable this script for non-local players
            return;
        }

        // Get CharacterController component
        characterController = GetComponent<CharacterController>();
        if (playerCamera == null || cameraTarget == null)
        {
            Debug.LogError("Player Camera or Camera Target is not assigned! Please assign them in the Inspector.");
        }

        // Lock the cursor for better mouse control
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (!photonView.IsMine) return;
        // Handle player movement
        HandleMovement();

        // Allow or disable camera rotation based on input
        allowCameraRotation = Mathf.Abs(Input.GetAxis("Horizontal")) < 0.1f;

        // Rotate camera if allowed
        if (allowCameraRotation)
        {
            RotateCamera();
        }

        // Follow the player
        FollowCamera();
    }

    private void HandleMovement()
    {
        // Get player input for movement
        float horizontal = Input.GetAxis("Horizontal"); // Left/Right or A/D
        float vertical = Input.GetAxis("Vertical"); // Forward/Backward or W/S

        Vector3 movementDirection = new Vector3(horizontal, 0, vertical).normalized;

        if (movementDirection.magnitude > 0.1f)
        {
            // Rotate character to face movement direction relative to the camera
            Vector3 targetDirection = playerCamera.transform.forward * vertical + playerCamera.transform.right * horizontal;
            targetDirection.y = 0; // Ensure no vertical tilt
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // Apply character movement
        Vector3 moveVector = transform.forward * movementDirection.magnitude * moveSpeed;
        characterController.Move(moveVector * Time.deltaTime);

        // Apply gravity
        characterController.Move(Vector3.down * 9.81f * Time.deltaTime);
    }

    private void RotateCamera()
    {
        // Get mouse input for camera rotation
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Horizontal rotation (yaw) around the character
        transform.Rotate(0, mouseX, 0);

        // Vertical rotation (pitch) within clamped limits
        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, -30f, 60f); // Prevent extreme angles

        // Apply vertical rotation to the camera
        playerCamera.transform.localRotation = Quaternion.Euler(cameraPitch, 0, 0);

    }

    private void FollowCamera()
    {
        // Smoothly follow the player with the camera
        Vector3 desiredPosition = cameraTarget.position - playerCamera.transform.forward * 5.0f + Vector3.up * 2.0f; // Adjust camera position
        playerCamera.transform.position = Vector3.Lerp(playerCamera.transform.position, desiredPosition, cameraFollowSpeed * Time.deltaTime);
    }
}