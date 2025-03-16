using UnityEngine;
using Photon.Pun;
using static teamsEnum;
using Photon.Realtime;

public class PetFood : MonoBehaviourPunCallbacks
{
    //Set in Unity Editor, Indicates which team the food is for
    public teams foodFor;

    //Randomly determines if the food is fake
    private bool isFake;

    ScoreManager scoreManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        scoreManager = GameObject.Find("ScoreManager").GetComponent<ScoreManager>();
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
            if (!isFake)
            {
                //Get the player information
                Player p = other.GetComponent<PhotonView>().Controller;
                teams playerTeam = GetTeam(p);
                //If the team the player is on matches who the food is for
                if (playerTeam == foodFor)
                {
                    //Go into the score manager and call the increase score function for everyone
                    scoreManager.photonView.RPC("IncreaseScore", RpcTarget.All, playerTeam);
                }
                else 
                {
                    //Go into the score manager and call the decrease score function for everyone
                    scoreManager.photonView.RPC("DecreaseScore", RpcTarget.All, playerTeam);
                }

                //NOT SURE IF THIS WORKS BUT TEST ANYWAYS
                photonView.RPC("Destroy", RpcTarget.All);
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

        //Destroy Object
    }
}
