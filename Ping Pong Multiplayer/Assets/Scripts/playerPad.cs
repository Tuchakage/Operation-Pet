using UnityEngine;
using Photon.Pun;
using UnityEngine.InputSystem;

public class playerPad : MonoBehaviourPunCallbacks
{/* New Input System: https://docs.unity3d.com/Packages/com.unity.inputsystem@1.8/manual/QuickStartGuide.html */
   
    public float speed = 5f;
    public InputAction moveAction;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Find the move action from the Default Action System
        moveAction = InputSystem.actions.FindAction("Move");
    }

    // Update is called once per frame
    void Update()
    {
        //If the one triggering this function is me
        if (photonView.IsMine) 
        {
            InputMovement();
        }
    }


    void InputMovement() 
    {
        Vector2 moveValue = moveAction.ReadValue<Vector2>();
        GetComponent<Rigidbody2D>().velocity = new Vector2(moveValue.x * speed, moveValue.y * speed);
    }
}
