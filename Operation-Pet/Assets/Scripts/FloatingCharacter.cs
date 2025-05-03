using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
public class FloatingCharacter : MonoBehaviourPunCallbacks
{
    public float hoverHeight = 5.0f; // Height above the terrain
    public float hoverSpeed = 2.0f; // Up-and-down floating speed
    public float moveSpeed = 5.0f; // Movement speed
    public float hoverAmplitude = 0.5f; // Amplitude of the hover motion
    public Camera characterCamera; // Assign the Camera in the Inspector
    public float mouseSensitivity = 2.0f; // Sensitivity of mouse movement

    private float verticalLookRotation = 0f; // Tracks vertical camera tilt
    private float hoverOffset;

    void Start()
    {
        //If the object spawned doesn't belong to the player then do nothing
        if (!photonView.IsMine) 
        {
            return;
        }
        //Enable the camera
        characterCamera.enabled = true;
        // Initialize hover offset for smooth motion
        hoverOffset = Random.Range(0, Mathf.PI * 2);

        // Ensure the camera starts at the character's position and orientation
        if (characterCamera != null)
        {
            characterCamera.transform.position = transform.position;
            characterCamera.transform.rotation = transform.rotation;
        }

        // Lock the cursor to the game window for better camera control
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        //If the object spawned doesn't belong to the player then do nothing
        if (!photonView.IsMine)
        {
            return;
        }
        // Hovering: Smooth up-and-down motion
        float hover = Mathf.Sin(Time.time * hoverSpeed + hoverOffset) * hoverAmplitude;
        Vector3 position = transform.position;
        position.y = hoverHeight + hover;
        transform.position = position;

        // Movement (WASD or arrow keys)
        MoveCharacter();

        // Mouse look: Rotate character horizontally and camera vertically
        LookAround();
    }

    private void MoveCharacter()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(horizontal, 0, vertical).normalized * moveSpeed * Time.deltaTime;
        transform.Translate(movement, Space.World);
    }

    private void LookAround()
    {
        // Horizontal rotation (character rotation)
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        transform.Rotate(0, mouseX, 0);

        // Vertical rotation (camera tilt)
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        verticalLookRotation -= mouseY; // Invert vertical movement for natural control
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -90f, 90f); // Limit vertical rotation

        // Apply vertical rotation to the camera
        if (characterCamera != null)
        {
            characterCamera.transform.localRotation = Quaternion.Euler(verticalLookRotation, 0, 0);
        }
    }
}