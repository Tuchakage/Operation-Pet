using UnityEngine;

public class LightBeam : MonoBehaviour
{
    public GameObject beamPrefab; // Prefab of the beam
    public float beamDuration = 5.0f; // Duration before the beam disappears
    public float beamWidth = 1.0f; // Width of the beam
    public Color beamColor = Color.white; // Color of the beam
    public Camera playerCamera; // Player's camera for reticle targeting
    public LayerMask targetLayer; // Layer of objects where the beam should spawn

    void Update()
    {
        // Check for player input to create the beam (e.g., left mouse button)
        if (Input.GetMouseButtonDown(0)) // Left mouse button
        {
            CreateBeam();
        }
    }

    private void CreateBeam()
    {
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // Perform a raycast to find the target position
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, targetLayer))
        {
            Vector3 beamPosition = hit.point; // Position where reticle is aimed
            GameObject beam = Instantiate(beamPrefab, beamPosition, Quaternion.identity);

            // Adjust the beam's scale and positioning
            beam.transform.localScale = new Vector3(beamWidth, 5000f, beamWidth);
            beam.GetComponent<Renderer>().material.color = beamColor;

            // Destroy the beam after the duration
            Destroy(beam, beamDuration);
        }
    }
}
