using UnityEngine;
using Photon.Pun;
using UnityEngine.InputSystem;

public class PlayerPad : MonoBehaviourPunCallbacks
{
    [Header("Movement Settings")]
    public float speed = 5f;
    private InputAction moveAction;
    private Rigidbody rb; // 3D Rigidbody

    void Start()
    {
        // Get Rigidbody component
        rb = GetComponent<Rigidbody>();

        // Ensure Rigidbody exists
        if (rb == null)
        {
            Debug.LogError("Rigidbody is missing on " + gameObject.name);
        }

        // Find the move action from the Input System
        moveAction = InputSystem.actions.FindAction("Move");

        // Enable the Input Action
        if (moveAction != null)
            moveAction.Enable();
        else
            Debug.LogError("Move action not found in Input System!");
    }

    void Update()
    {
        // Ensure this is the local player before processing movement
        if (photonView.IsMine)
        {
            InputMovement();
        }
    }

    void InputMovement()
    {
        Vector2 moveValue = moveAction.ReadValue<Vector2>();

        // Move the Rigidbody in 3D space
        Vector3 moveDirection = new Vector3(moveValue.x, 0f, moveValue.y); // Y is 0 since it's a 3D ground movement
        rb.linearVelocity = moveDirection * speed;
    }
}

