using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEngine.InputSystem;

public class FloatingPlayerMine : MonoBehaviourPunCallbacks
{
    InputSystem_Actions playerActionAsset;

    public GameObject minePrefab; // Prefab of the mine
    public Camera playerCamera; // Camera for reticle targeting
    public LayerMask targetLayer; // Layer for valid mine placement
    public float minePlacementCooldown = 3.0f; // Cooldown duration in seconds

    private bool canPlaceMine = true; // Track whether mine placement is allowed

    void Awake()
    {
        playerActionAsset = new InputSystem_Actions();
    }

    void OnEnable()
    {
        if (!photonView.IsMine) return;

        //Enable the Player Action Map 
        playerActionAsset.Wizard.Enable();

        playerActionAsset.Wizard.PlaceMine.started += CallPlaceMine;


    }



    void OnDisable()
    {
        if (!photonView.IsMine) return;
        //Disable the Player Action Map 
        playerActionAsset.Wizard.Disable();

        playerActionAsset.Wizard.PlaceMine.started -= CallPlaceMine;
    }

    void CallPlaceMine(InputAction.CallbackContext context) 
    {
        if (canPlaceMine) 
        {
            PlaceMine();
            StartCoroutine(StartCooldown());
        }
    }

   
    private void PlaceMine()
    {
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, targetLayer))
        {
            Vector3 spawnPos = hit.point;
            PhotonNetwork.Instantiate("Mine", spawnPos, Quaternion.identity);
            StartCoroutine(StartCooldown());
        }
    }

    private IEnumerator StartCooldown()
    {
        canPlaceMine = false; // Disable mine placement
        yield return new WaitForSeconds(minePlacementCooldown); // Wait for cooldown duration
        canPlaceMine = true; // Re-enable mine placement
    }
}