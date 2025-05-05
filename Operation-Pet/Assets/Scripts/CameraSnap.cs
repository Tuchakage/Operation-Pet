using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using static teamsEnum;
using System.Collections;

public class PetFoodHighlighter : MonoBehaviourPun
{
    public Material highlightMaterial; // Highlighted appearance for local player's team

    private MeshRenderer meshRenderer;
    private teams foodFor;
    private bool isInitialized = false;
    private bool isMineFood = false;
    private bool canHighlight = true; // Cooldown control

    bool fakeFood;

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        foodFor = GetComponent<PetFood>().foodFor;

        // Check if the food is fake
        fakeFood = GetComponent<PetFood>().isFake;

        // Try to initialize immediately if custom properties are available
        TryInitialize();
    }

    void Update()
    {
        if (!isInitialized)
        {
            TryInitialize();
            return;
        }

        if (!fakeFood && canHighlight)
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                photonView.RPC("HighlightFoodRPC", RpcTarget.AllBuffered);
                StartCoroutine(HighlightCooldown());
            }
        }
    }

    [PunRPC]
    private void HighlightFoodRPC()
    {
        if (isMineFood)
        {
            SetMaterial(highlightMaterial);
        }
        else
        {
            SetMaterial(null); // Removes material if not on the same team
        }
    }

    private IEnumerator HighlightCooldown()
    {
        canHighlight = false;
        yield return new WaitForSeconds(3.0f); // 3-second cooldown
        canHighlight = true;
    }

    void TryInitialize()
    {
        if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("Team Name", out object teamObj))
        {
            teams myTeam = (teams)teamObj;
            isMineFood = (myTeam == foodFor);
            isInitialized = true;
        }
    }

    void SetMaterial(Material mat)
    {
        if (meshRenderer != null)
        {
            meshRenderer.sharedMaterial = mat;
        }
    }
}