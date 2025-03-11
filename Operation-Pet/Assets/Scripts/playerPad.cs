using UnityEngine;
using Photon.Pun;
using UnityEngine.InputSystem;

public class PlayerPad : MonoBehaviourPunCallbacks
{
    [SerializeField] private float speed = 5f;
    private Rigidbody2D rb;
    public InputAction moveAction;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>(); // Cache Rigidbody2D component
    }

    private void OnEnable()
    {
        moveAction.Enable(); // Ensure InputAction is enabled
    }

    private void OnDisable()
    {
        moveAction.Disable(); // Disable InputAction when object is disabled
    }

    private void Update()
    {
        if (photonView.IsMine)
        {
            InputMovement();
        }
    }

    private void InputMovement()
    {
        Vector2 moveValue = moveAction.ReadValue<Vector2>();
<<<<<<< Updated upstream
        GetComponent<Rigidbody2D>().velocity = new Vector2(moveValue.x * speed, moveValue.y * speed);
=======
        rb.velocity = moveValue * speed; // Set Rigidbody2D velocity correctly
>>>>>>> Stashed changes
    }
}

