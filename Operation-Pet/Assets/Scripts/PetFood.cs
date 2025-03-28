using UnityEngine;
using Photon.Pun;
using static teamsEnum;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class PetFood : MonoBehaviourPunCallbacks
{
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

                    //
                    scoreManager.photonView.RPC("IncreaseScore", RpcTarget.All, playerTeam);
                    roundManager.photonView.RPC("GameWinner", RpcTarget.All, playerTeam);
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
}
