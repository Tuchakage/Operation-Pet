using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon; // Required for Hashtable
using static teamsEnum;

public class PetFoodHighlighter : MonoBehaviourPun
{
    public Material defaultMaterial;         // Neutral/default appearance
    public Material highlightMaterial;       // Highlighted appearance for local player’s team
    private MeshRenderer meshRenderer;
    private teams foodFor;

    private bool isInitialized = false;
    private bool isMineFood = false;
    private bool fakeFood;
    private bool isPlayerTagged = false;

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
        // If team info hasn't been loaded yet, try again
        if (!isInitialized)
        {
            TryInitialize();
            return;
        }

        // Only proceed if the player has the "PLAYER" tag
        if (!isPlayerTagged)
        {
            return;
        }

        if (!fakeFood)
        {
            // On key press, reveal or hide based on team match
            if (Input.GetKey(KeyCode.Q))
            {
                if (isMineFood)
                {
                    SetMaterial(highlightMaterial);
                }
                else
                {
                    SetMaterial(defaultMaterial);
                }
            }
            else
            {
                SetMaterial(defaultMaterial);
            }
        }
    }

    void TryInitialize()
    {
        // Check if the local player's custom properties contain the "Tag" key
        if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("Tag", out object tagObj))
        {
            string tagValue = tagObj as string;
            isPlayerTagged = tagValue == "Player";
        }
        else
        {
            isPlayerTagged = false;
        }

        // Proceed only if the player is tagged as "PLAYER"
        if (!isPlayerTagged)
        {
            return;
        }

        // Check if the local player's custom properties contain the "Team Name" key
        if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("Team Name", out object teamObj))
        {
            teams myTeam = (teams)teamObj;
            isMineFood = (myTeam == foodFor);
            isInitialized = true;
        }
    }

    void SetMaterial(Material mat)
    {
        if (meshRenderer != null && meshRenderer.sharedMaterial != mat)
        {
            meshRenderer.sharedMaterial = mat;
        }
    }
}
