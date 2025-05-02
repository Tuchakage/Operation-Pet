using UnityEngine;
using Photon.Pun;
using static teamsEnum;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using System.Collections;
using Unity.VisualScripting;

public class PetFood : MonoBehaviourPunCallbacks
{
    public TeamModelScriptableObject foodModelScriptableObject;


    //Set in Unity Editor, Indicates which team the food is for
    public teams foodFor;

    //Randomly determines if the food is fake
    public bool isFake;

    //Is used for deathmatch, if set to true then anyone can pick this up
    public bool anyoneCanPickUp;



    [SerializeField] private float explosionRadius = 5f;
    [SerializeField] private float explosionForce = 500;

    ScoreManager scoreManager;
    RoundManager roundManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        scoreManager = GameObject.Find("ScoreManager").GetComponent<ScoreManager>();
        roundManager = GameObject.Find("RoundManager").GetComponent<RoundManager>();
        //Randomly choose if the food is fake or not
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player") 
        {
            Debug.Log(other.tag + "Collided");
            if (!isFake)
            {
                //If its not a deathmatch then not everyone can pick up the food
                if (!anyoneCanPickUp)
                {
                    //Go into the score manager and call the increase score function for the food that was picked up
                    scoreManager.photonView.RPC("IncreaseScore", RpcTarget.All, foodFor);
                    
                }
                else 
                {
                    //Get the player information
                    Player p = other.GetComponent<PhotonView>().Controller;
                    teams playerTeam = GetTeam(p);

                    // Check Game winner is called in RoundManager in NextRound() function
                    scoreManager.photonView.RPC("IncreaseScore", RpcTarget.All, playerTeam);
                }


                
                photonView.RPC("DestroyFood", RpcTarget.All);
            }
            else 
            {
                photonView.RPC("Explode", RpcTarget.All);
            }
        }
    }

    teams GetTeam(Player player)
    {
        object isInTeam;
        //Check the custom properties of that player to see what team they are apart of
        if (player.CustomProperties.TryGetValue("Team Name", out isInTeam))
        {
            //Debug.Log("team = " + (teams)isInTeam);
            return (teams)isInTeam;
        }

        return teams.Unassigned;
    }

    [PunRPC]
    void Explode() 
    {
        //Play explosion effect

        //Push Player back
        var surroundingPlayers = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (var player in surroundingPlayers) 
        {
            //Get the rigid body of the player
            var rb = player.GetComponent<Rigidbody>();
            //If they dont have a rigidbody then just skip to the next player in the list
            if (rb == null) continue;

            // Throw the players away
            rb.AddExplosionForce(explosionForce, transform.position, explosionRadius);
            Destroy(this.gameObject);
        }

        //Destroy Object
    }

    [PunRPC]
    void DestroyFood() 
    {
        Destroy(this.gameObject);
    }

    [PunRPC]
    void SetAsMine() 
    {
        isFake = true;
    }

    [PunRPC]
    void EveryoneCanPickUp() 
    {
        anyoneCanPickUp = true;
    }

    [PunRPC]
    public void CallSetFoodModel(teams foodToSet) 
    {
        StartCoroutine(SetFoodModel(foodToSet));



    }

    IEnumerator SetFoodModel(teams foodToSet) 
    {
        //Set which team the food is for so that they will gain points when they pick it up
        foodFor = foodToSet;

        //Set the Mesh of the food
        SetMesh(foodToSet);

        BoxCollider boxCollider = GetComponent<BoxCollider>();
        //Destroy the box collider
        Destroy(boxCollider);

        //Wait until the Box Collider has been destroyed to continue
        yield return boxCollider.IsDestroyed();

        //Respawn it so that it is resized to the Mesh size automatically
        gameObject.AddComponent<BoxCollider>();

        GetComponent<BoxCollider>().isTrigger = true;

        
    }

    public void SetMesh(teams teamName) 
    {
        //Set the mesh for the food
        Mesh thisMesh = GetComponent<MeshFilter>().mesh;
        switch (teamName)
        {
            case teams.Unassigned: //If Unassigned then it is a mine
                GetComponent<MeshFilter>().mesh =  foodModelScriptableObject.foodModels[5];
                break;
            case teams.Dog:
                //Debug.Log(foodModelScriptableObject.foodModels[0].name);
                GetComponent<MeshFilter>().mesh = foodModelScriptableObject.foodModels[0];
                break;
            case teams.Cat:
                //Debug.Log(foodModelScriptableObject.foodModels[1].name);
                GetComponent<MeshFilter>().mesh = foodModelScriptableObject.foodModels[1];
                break;

            case teams.Mouse:
                GetComponent<MeshFilter>().mesh = foodModelScriptableObject.foodModels[2];
                break;

            case teams.Squirrel:
                GetComponent<MeshFilter>().mesh = foodModelScriptableObject.foodModels[3];
                break;

            case teams.Horse:
                GetComponent<MeshFilter>().mesh = foodModelScriptableObject.foodModels[4];
                break;
        }
    }


}
