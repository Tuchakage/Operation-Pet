using UnityEngine;

public class FloatingPlayerMine : MonoBehaviour
{
    public GameObject minePrefab; // Prefab of the mine
    public Camera playerCamera; // Camera for reticle targeting
    public LayerMask targetLayer; // Layer for valid mine placement

    void Update()
    {
        // Place mine when right mouse button is clicked
        if (Input.GetMouseButtonDown(1)) // Right Mouse Button
        {
            PlaceMine();
        }
    }

    private void PlaceMine()
    {
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // Perform raycast to find the target position
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, targetLayer))
        {
            Vector3 minePosition = hit.point; // Position where reticle is aimed
            Instantiate(minePrefab, minePosition, Quaternion.identity);
        }
    }
}