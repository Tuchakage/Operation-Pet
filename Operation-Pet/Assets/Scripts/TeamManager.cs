using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;


//Purpose of this script is to set the teams of the players before the game
public class TeamManager : MonoBehaviourPunCallbacks
{
    private string teamName = "Unassigned";
    public int ownerid;
    Player localPlayer;
    void Start()
    {

        //Initialise a Hashtable first to use for the SetCustomProperties Function
        Hashtable properties = new Hashtable()
        {
            {"Team Name",  teamName}
        };
    }

    public override void OnJoinedRoom() 
    {
        //Get the actor number when the local player joins the room
        ownerid = PhotonNetwork.LocalPlayer.ActorNumber;

        //Get Reference to Player info when local player joins the room
        localPlayer = PhotonNetwork.CurrentRoom.GetPlayer(ownerid);

        
    }

    [PunRPC]
    public void JoinRedTeam() 
    {
        teamName = "Red";

        //Update Hash Table 
        Hashtable properties = new Hashtable()
        {
            {"Team Name",  teamName}
        };

        //Update the Hashtable that is being tracked by PUN
        PhotonNetwork.LocalPlayer.SetCustomProperties(properties);



        //Get reference to the Red Team Group List
        Transform redTeamGroupList = GameObject.Find("Red Team ").transform;

        GameObject[] cardArray;

        cardArray = GameObject.FindGameObjectsWithTag("Player Card");

        if (cardArray.Length > 0)
        {
            foreach (GameObject card in cardArray)
            {
                //Get the Actor number of the Player Card
                int playerCardID = card.GetComponent<PlayerCardEntry>().ownerId;

                if (playerCardID == ownerid)
                {                  
                    card.transform.SetParent(redTeamGroupList);
                    card.transform.localPosition = new Vector3(0, 0, 0);
                    card.transform.localScale = new Vector3(1, 1, 1);
                }
            }
        }



        //Get reference to the Player Card


    }

}
