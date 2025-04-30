using UnityEngine;

public class GroundPlayerPickup : MonoBehaviour
{
    public string correctItemTag = "CorrectItem"; // Tag for correct objects
    public string wrongItemTag = "WrongItem"; // Tag for incorrect objects
    public int score = 0; // Player's score
    public float knockbackForce = 5.0f; // Force applied when player interacts with a wrong object
    private CharacterController characterController; // Reference to CharacterController

    void Start()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the player collides with an object
        if (other.CompareTag(correctItemTag))
        {
            PickupCorrectItem(other.gameObject);
        }
        else if (other.CompareTag(wrongItemTag))
        {
            KnockbackPlayer();
        }
    }

    private void PickupCorrectItem(GameObject item)
    {
        // Increase score
        score += 1;
        Debug.Log($"Correct item picked up! Score: {score}");

        // Destroy the collected item
        Destroy(item);
    }

    private void KnockbackPlayer()
    {
        Debug.Log("Wrong item! Knocked back!");

        // Apply knockback effect by pushing the player backward
        Vector3 knockbackDirection = -transform.forward; // Push player backward
        characterController.Move(knockbackDirection * knockbackForce * Time.deltaTime);
    }
}
