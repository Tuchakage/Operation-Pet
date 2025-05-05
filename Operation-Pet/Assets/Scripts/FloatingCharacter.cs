using UnityEngine;
using Photon.Pun;

public class FloatingCharacter : MonoBehaviourPunCallbacks
{
    public float hoverHeight = 5.0f; // Fixed height above terrain
    public float moveSpeed = 5.0f; // Movement speed
    public Camera characterCamera; // Assign the Camera in the Inspector
    public float mouseSensitivity = 2.0f; // Sensitivity of mouse movement
    private float verticalLookRotation = 0f; // Tracks vertical camera tilt

    public LayerMask groundPlayerLayer; // Layer for ground players
    public LayerMask floatingPlayerLayer; // Layer for floating players

    void Start()
    {
        if (!photonView.IsMine) return;

        // Enable camera
        characterCamera.enabled = true;

        // Lock cursor for better camera control
        Cursor.lockState = CursorLockMode.Locked;

        // Set up visibility rules
        photonView.RPC("HideFloatingPlayerFromGroundPlayers", RpcTarget.AllBuffered);
    }

    void Update()
    {
        if (!photonView.IsMine) return;

        // Keep floating player at fixed height
        Vector3 position = transform.position;
        position.y = hoverHeight;
        transform.position = position;

        // Movement and camera rotation
        MoveCharacter();
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
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        transform.Rotate(0, mouseX, 0);

        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        verticalLookRotation -= mouseY;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -90f, 90f);

        if (characterCamera != null)
        {
            characterCamera.transform.localRotation = Quaternion.Euler(verticalLookRotation, 0, 0);
        }
    }

    [PunRPC]
    private void HideFloatingPlayerFromGroundPlayers()
    {
        GameObject floatingPlayer = gameObject;

        // Check if player belongs to floating layer
        if (((1 << floatingPlayer.layer) & floatingPlayerLayer) != 0)
        {
            foreach (GameObject groundPlayer in GameObject.FindGameObjectsWithTag("Player"))
            {
                if (((1 << groundPlayer.layer) & groundPlayerLayer) != 0)
                {
                    floatingPlayer.GetComponent<Renderer>().enabled = false; // Make floating player invisible
                    Physics.IgnoreLayerCollision(floatingPlayer.layer, groundPlayer.layer, true);
                }
            }
        }
    }
}