using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon; // For Hashtable
using static teamsEnum;
using System.Collections;

[RequireComponent(typeof(PetFood))]
[RequireComponent(typeof(MeshRenderer))]
public class PetFoodHighlighter : MonoBehaviourPun
{
    [Header("Highlight Settings")]
    public Material defaultMaterial;
    public Material highlightMaterial;

    private MeshRenderer meshRenderer;
    private PetFood petFood;

    private teams foodFor = teams.Unassigned;
    private bool isMineFood = false;
    private bool isFakeFood = false;
    private bool isPlayerTagged = false;
    private bool isInitialized = false;

    private void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        petFood = GetComponent<PetFood>();

        StartCoroutine(InitializeWhenReady());
    }

    private IEnumerator InitializeWhenReady()
    {
        // Wait until PetFood has assigned the team
        while (petFood.foodFor == teams.Unassigned)
        {
            yield return null;
        }

        foodFor = petFood.foodFor;
        isFakeFood = petFood.isFake;

        // Wait until custom properties are available
        while (!PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("Tag") ||
               !PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("Team Name"))
        {
            yield return null;
        }

        // Validate tag
        string tag = PhotonNetwork.LocalPlayer.CustomProperties["Tag"] as string;
        isPlayerTagged = (tag == "Player");

        // Validate team
        teams myTeam = (teams)PhotonNetwork.LocalPlayer.CustomProperties["Team Name"];
        isMineFood = (myTeam == foodFor);

        isInitialized = true;

        Debug.Log($"[Highlighter] Initialized - FoodFor: {foodFor}, MyTeam: {myTeam}, IsMineFood: {isMineFood}, IsFake: {isFakeFood}");
    }

    private void Update()
    {
        if (!isInitialized || !isPlayerTagged || isFakeFood)
            return;

        if (Input.GetKey(KeyCode.Q))
        {
            SetMaterial(isMineFood ? highlightMaterial : defaultMaterial);
        }
        else
        {
            SetMaterial(defaultMaterial);
        }
    }

    private void SetMaterial(Material mat)
    {
        if (meshRenderer != null && mat != null && meshRenderer.sharedMaterial != mat)
        {
            meshRenderer.sharedMaterial = mat;
            // Debug.Log($"[Highlighter] Material set to: {mat.name}");
        }
    }
}
