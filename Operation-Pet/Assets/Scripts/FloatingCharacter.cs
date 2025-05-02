using UnityEngine;
using Photon.Pun;

public class FloatingCharacter : MonoBehaviourPunCallbacks
{
    public float hoverHeight = 5.0f; // Height above the terrain
    public float hoverSpeed = 2.0f; // Floating speed
    public float moveSpeed = 5.0f; // Movement speed
    public float hoverAmplitude = 0.5f; // Amplitude of hover motion
    public Camera characterCamera; // Assign the Camera in Inspector
    public float mouseSensitivity = 2.0f; // Camera sensitivity
    private float verticalLookRotation = 0f;
    private float hoverOffset;

    void Start()
    {
        if (!photonView.IsMine)
        {
            // Disable local controls for other players' characters
            characterCamera.gameObject.SetActive(false);
            return;
        }

        hoverOffset = Random.Range(0, Mathf.PI * 2);
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (!photonView.IsMine) return;

        HandleHovering();
        HandleMovement();
        HandleCameraControl();
    }

    private void HandleHovering()
    {
        float hover = Mathf.Sin(Time.time * hoverSpeed + hoverOffset) * hoverAmplitude;
        Vector3 position = transform.position;
        position.y = hoverHeight + hover;
        transform.position = position;
    }

    private void HandleMovement()
    {
        Vector3 moveDirection = Vector3.zero;

        if (Input.GetKey(KeyCode.A))
        {
            moveDirection = -transform.right;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            moveDirection = transform.right;
        }
        if (Input.GetKey(KeyCode.W))
        {
            moveDirection += transform.forward;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            moveDirection -= transform.forward;
        }

        if (moveDirection != Vector3.zero)
        {
            transform.Translate(moveDirection.normalized * moveSpeed * Time.deltaTime, Space.World);
        }
    }

    private void HandleCameraControl()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(0, mouseX, 0);
        verticalLookRotation -= mouseY;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -90f, 90f);
        characterCamera.transform.localRotation = Quaternion.Euler(verticalLookRotation, 0, 0);
    }
}