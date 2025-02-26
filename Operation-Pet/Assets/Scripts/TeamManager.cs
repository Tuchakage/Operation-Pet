using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;


//Purpose of this script is to set the teams of the players before the game
public class TeamManager : MonoBehaviourPunCallbacks, IPunObservable 
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

        //Move the player card to be under the team name
        MoveCardToTeam(teamName, ownerid);






    }

    [PunRPC]
    void MoveCardToTeam(string team, int cardOwner) 
    {
        //Switch case used to find the correct Group List to put the Player Card under
        string teamNameCheck = "Unassigned";
        switch (team) 
        {
            case "Unassigned":
                break;
            case "Red":
                teamNameCheck = "Red Team";
            break;
        }

        //Get reference to the Team Group List (Depending on what was passed through the parameter)
        Transform TeamGroupList = GameObject.Find(teamNameCheck).transform;
        

        GameObject[] cardArray;

        cardArray = GameObject.FindGameObjectsWithTag("Player Card");

        if (cardArray.Length > 0)
        {
            foreach (GameObject card in cardArray)
            {
                //Get the Actor number of the Player Card
                int playerCardID = card.GetComponent<PlayerCardEntry>().ownerId;

                //If the player card is the local players card that moved, then move it under the team name.
                if (playerCardID == cardOwner)
                {
                    card.transform.SetParent(TeamGroupList);
                    card.transform.localPosition = new Vector3(0, 0, 0);
                    card.transform.localScale = new Vector3(1, 1, 1);
                }
            }
        }
        // Debug.Log("Moving " + PhotonNetwork.CurrentRoom.GetPlayer(cardOwner).NickName + " To Red Team");


        photonView.RPC("MoveCardToTeam", RpcTarget.OthersBuffered, team, cardOwner);
        
    }

    //This function allows the variables inside to be sent over the network (Used as Observed component in photon view, this reads/writes the variables)
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            //We own this player so send the other computers the data

        }
        else
        {
            //Network player that receives the data


        }
    }
}


