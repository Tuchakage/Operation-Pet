using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Photon.Pun;

public class GroundPlayerScreenshot : MonoBehaviourPunCallbacks
{
    public Camera thirdPersonCamera; // Third-person camera
    public Camera firstPersonCamera; // First-person camera
    public Transform firstPersonSpawnPoint; // First-person camera spawn location
    public RenderTexture renderTexture; // Screenshot texture
    public RawImage uiImage; // UI display for the screenshot
    public GameObject[] allItems; // Array of all available items
    public Material standardMaterial; // Normal material for all items
    public Material trueItemMaterial; // Special material for the real item when screenshot is taken
    private GameObject trueItem; // The true item for this specific player

    void Start()
    {
        if (!photonView.IsMine) return; // Only allow local players to manage their own true item

        // Disable first-person camera at start
        firstPersonCamera.enabled = false;

        // Set first-person camera position to the predefined spawn point
        firstPersonCamera.transform.position = firstPersonSpawnPoint.position;
        firstPersonCamera.transform.rotation = firstPersonSpawnPoint.rotation;

        // Assign a unique true item per player
        AssignTrueItem();

        // Ensure all items start with the standard material
        foreach (GameObject item in allItems)
        {
            item.GetComponent<Renderer>().material = standardMaterial;
        }
    }

    void Update()
    {
        if (!photonView.IsMine) return; // Only allow local player interaction

        // Press "P" to take a screenshot
        if (Input.GetKeyDown(KeyCode.P))
        {
            photonView.RPC("CaptureScreenshotRPC", RpcTarget.All);
        }
    }

    private void AssignTrueItem()
    {
        int playerID = photonView.ViewID % allItems.Length; // Assign different true items per player
        trueItem = allItems[playerID];
    }

    [PunRPC]
    private void CaptureScreenshotRPC()
    {
        StartCoroutine(CaptureScreenshot());
    }

    private IEnumerator CaptureScreenshot()
    {
        // Switch to first-person view
        thirdPersonCamera.enabled = false;
        firstPersonCamera.enabled = true;
        firstPersonCamera.transform.position = firstPersonSpawnPoint.position;
        firstPersonCamera.transform.rotation = firstPersonSpawnPoint.rotation;

        // Change material only for the local player’s true item
        photonView.RPC("RevealTrueItemRPC", RpcTarget.All, photonView.ViewID);

        yield return new WaitForSeconds(0.2f); // Short delay

        // Capture screenshot
        firstPersonCamera.targetTexture = renderTexture;
        firstPersonCamera.Render();
        uiImage.texture = renderTexture;

        yield return new WaitForSeconds(0.2f); // Short delay before reverting

        // Restore normal materials for all items across clients
        photonView.RPC("ResetItemMaterialsRPC", RpcTarget.All);

        // Switch back to third-person view
        firstPersonCamera.enabled = false;
        thirdPersonCamera.enabled = true;
    }

    [PunRPC]
    private void RevealTrueItemRPC(int playerViewID)
    {
        if (photonView.ViewID == playerViewID) // Ensure only the local player's true item is changed
        {
            trueItem.GetComponent<Renderer>().material = trueItemMaterial;
        }
    }

    [PunRPC]
    private void ResetItemMaterialsRPC()
    {
        foreach (GameObject item in allItems)
        {
            item.GetComponent<Renderer>().material = standardMaterial;
        }
    }
}