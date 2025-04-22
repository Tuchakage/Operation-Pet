using UnityEngine;

public class FloatingPlayerMine : MonoBehaviour
{
    public GameObject minePrefab; // Prefab of the mine to place
    public float placeDistance = 2.0f; // Distance in front of the floating player where the mine is placed
    public float explosionForce = 10.0f; // Force of the mine explosion
    public float explosionRadius = 5.0f; // Radius within which the mine affects objects
    public float upwardModifier = 2.0f; // Adds upward force to the explosion

    void Update()
    {
        // Check if the player presses the right mouse button to place a mine
        if (Input.GetMouseButtonDown(1)) // Right mouse button
        {
            PlaceMine();
        }
    }

    private void PlaceMine()
    {
        // Calculate the placement position in front of the player
        Vector3 placePosition = transform.position + transform.forward * placeDistance;

        // Instantiate the mine prefab at the calculated position
        Instantiate(minePrefab, placePosition, Quaternion.identity);
    }
}