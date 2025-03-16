using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    public float moveSpeed = 5.0f;

    void Update()
    {
        if (!gameObject.activeSelf)
        {
            return; // Skip movement if this character is inactive
        }

        // Basic movement using arrow keys or WASD
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(horizontal, 0, vertical) * moveSpeed * Time.deltaTime;
        transform.Translate(movement, Space.World);
    }
}
