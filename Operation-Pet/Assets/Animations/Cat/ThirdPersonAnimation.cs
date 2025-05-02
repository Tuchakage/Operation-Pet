using UnityEngine;

public class ThirdPersonAnimation : MonoBehaviour
{
    private Animator animator;
    private Rigidbody rb;
    private float maxSpeed =5f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        // We divide it by max speed so that if the maximum speed value changes then we it will also be shown in the animator
        animator.SetFloat("speed", rb.linearVelocity.magnitude / maxSpeed);
    }

    //Sets the Max Speed variable for animation
    public void SetMaxSpeed(float speed) 
    {
        maxSpeed = speed;
    }
}
