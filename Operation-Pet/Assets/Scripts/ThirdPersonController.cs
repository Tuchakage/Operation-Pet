using Photon.Pun;
using UnityEngine;

public class ThirdPersonControllers : MonoBehaviourPunCallbacks
{

    public float moveSpeed = 5.0f;
    public float rotationSpeed = 720.0f;
    public float mouseSensitivity = 3.0f;
    public Camera playerCamera;

    private CharacterController characterController;
    private float cameraPitch = 0f;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked; // Locks the mouse for smooth camera control
    }

    void Update()
    {
        HandleMovement();
        HandleCameraControl();
    }

    private void HandleMovement()
    {
        Vector3 moveDirection = Vector3.zero;

        // Move Left (A) and Move Right (D)
        if (Input.GetKey(KeyCode.A))
        {
            moveDirection = -transform.right;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            moveDirection = transform.right;
        }

        // Move Forward (W) and Move Backward (S)
        if (Input.GetKey(KeyCode.W))
        {
            moveDirection += transform.forward;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            moveDirection -= transform.forward;
        }

        // Apply movement
        if (moveDirection != Vector3.zero)
        {
            characterController.Move(moveDirection.normalized * moveSpeed * Time.deltaTime);
        }
    }

    private void HandleCameraControl()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Rotate player based on horizontal mouse movement
        transform.Rotate(0, mouseX, 0);

        // Adjust camera pitch based on vertical mouse movement
        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, -30f, 60f); // Limits vertical rotation range
        playerCamera.transform.localRotation = Quaternion.Euler(cameraPitch, 0, 0);
    }
}