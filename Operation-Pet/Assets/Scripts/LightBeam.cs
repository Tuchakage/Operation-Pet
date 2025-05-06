using UnityEngine;
using Photon.Pun;
using System.Collections;

public class LightBeam : MonoBehaviourPunCallbacks
{
    public GameObject beamPrefab;           // Prefab of the beam
    public float beamDuration = 5.0f;       // Duration before the beam disappears
    public float beamWidth = 1.0f;          // Width of the beam
    public Color beamColor = Color.white;   // Color of the beam
    public Camera playerCamera;             // Player's camera for targeting
    public LayerMask targetLayer;           // Layer for ground targeting
    public LayerMask groundPlayerLayer;     // Layer for players that should destroy the beam

    void Update()
    {
        if (!photonView.IsMine) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            TryCreateBeam();
        }
    }

    private void TryCreateBeam()
    {
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, targetLayer))
        {
            Vector3 spawnPosition = hit.point;
            GameObject beamInstance = PhotonNetwork.Instantiate(beamPrefab.name, spawnPosition, Quaternion.identity);

            // Scale and color
            beamInstance.transform.localScale = new Vector3(beamWidth, 5000f, beamWidth);
            Renderer renderer = beamInstance.GetComponent<Renderer>();
            if (renderer != null)
                renderer.material.color = beamColor;

            // Attach collision handler and initialize it
            BeamCollisionHandler collisionHandler = beamInstance.AddComponent<BeamCollisionHandler>();
            collisionHandler.Initialize(groundPlayerLayer);

            // Start destruction timer
            StartCoroutine(DestroyBeamAfterTime(beamInstance));
        }
        else
        {
            Debug.Log("No valid ground target hit.");
        }
    }

    private IEnumerator DestroyBeamAfterTime(GameObject beam)
    {
        yield return new WaitForSeconds(beamDuration);

        if (beam != null && beam.GetComponent<PhotonView>().IsMine)
        {
            PhotonNetwork.Destroy(beam);
        }
    }
}
