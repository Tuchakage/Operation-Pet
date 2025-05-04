using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;

public class FloatingPlayerMine : MonoBehaviourPunCallbacks
{
    public GameObject minePrefab; // Prefab of the mine
    public Camera playerCamera; // Camera for reticle targeting
    public LayerMask targetLayer; // Layer for valid mine placement
    public float minePlacementCooldown = 3.0f; // Cooldown duration in seconds

    private bool canPlaceMine = true; // Track whether mine placement is allowed

    void Update()
    {
        if (!photonView.IsMine) return;

        // Place mine only if cooldown has ended
        if (Input.GetMouseButtonDown(1) && canPlaceMine) // Right Mouse Button
        {
            photonView.RPC("PlaceMine", RpcTarget.AllBuffered);
            StartCoroutine(StartCooldown());
        }
    }

    [PunRPC]
    private void PlaceMine()
    {
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // Perform raycast to find the target position
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, targetLayer))
        {
            Vector3 minePosition = hit.point; // Position where reticle is aimed
            PhotonNetwork.Instantiate("Mine", minePosition, Quaternion.identity);
        }
    }

    private IEnumerator StartCooldown()
    {
        canPlaceMine = false; // Disable mine placement
        yield return new WaitForSeconds(minePlacementCooldown); // Wait for cooldown duration
        canPlaceMine = true; // Re-enable mine placement
    }
}