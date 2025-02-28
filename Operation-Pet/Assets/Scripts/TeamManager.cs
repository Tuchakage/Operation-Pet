using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using System.Collections.Generic;


//Purpose of this script is to set the teams of the players before the game
public class TeamManager : MonoBehaviourPunCallbacks, IPunObservable 
{
    private string teamName = "Unassigned";
    public int ownerid;
    private int redTeamCount;
    Player localPlayer;
    void Start()
    {
        //Initialise a Hashtable first to use for the SetCustomProperties Function
        Hashtable properties = new Hashtable()
        {
            {"Team Name",  teamName}
        };
    }

    void Update() 
    {

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

        CheckTeam();

        if (redTeamCount >= 2)
        {
            return;
        }

        IncreaseTeamCount();
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
                //"Red Team" is the name of the GameObject in the Unity Editor where player cards of that team will go under
                teamNameCheck = "Red Team";
                //Debug.Log("Amount in Red Team = " + redTeamCount);
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
                    card.transform.localScale = new Vector3(1, 1, 1);
                }
            }
        }
        // Debug.Log("Moving " + PhotonNetwork.CurrentRoom.GetPlayer(cardOwner).NickName + " To Red Team");


        photonView.RPC("MoveCardToTeam", RpcTarget.OthersBuffered, team, cardOwner);
        
    }

    //Function that checks whether they are already in a team and if they do dont run the rest of the code
    void CheckTeam() 
    {
        object isInTeam;
        //Check the custom properties of that player to see if they are part of a team
        if (localPlayer.CustomProperties.TryGetValue("Team Name", out isInTeam))
        {
            //if they are already in the same team as the button as they pressed
            if ((string)isInTeam == teamName)
            {
                //Ignore them
                return;
            }
        }
    }

    [PunRPC]
    void IncreaseTeamCount() 
    {
        object isInTeam;
        //Check the custom properties of that player to see what team they are apart of
        if (localPlayer.CustomProperties.TryGetValue("Team Name", out isInTeam))
        {
            switch ((string)isInTeam)
            {
                case "Unassigned":
                    break;

                case "Red":
                    redTeamCount++;
                    break;


            }
        }

        //If you are not the Master Client
        if (!photonView.IsMine) 
        {
            //Send to the Master Client so that they can sync the value (Since Local clients dont own the Network Manager)
            photonView.RPC("IncreaseTeamCount", RpcTarget.MasterClient);
        }
        Debug.Log("Amount in Red Team = " + redTeamCount);
    }

    public void DecreaseTeamCount(Player gonePlayer) 
    {
        object isInTeam;
        //Check the custom properties of that player to see what team they are apart of
        if (gonePlayer.CustomProperties.TryGetValue("Team Name", out isInTeam)) 
        {
            switch ((string)isInTeam) 
            {
                case "Unassigned":
                    break;

                case "Red":
                    redTeamCount--;
                    break;


            }
        }

        //If you are not the Master Client
        if (!photonView.IsMine)
        {
            //Send to the Master Client so that they can sync the value (Since Local clients dont own the Network Manager)
            photonView.RPC("IncreaseTeamCount", RpcTarget.MasterClient);
        }

        Debug.Log("Amount in Red Team = " + redTeamCount);
    }

    //This function allows the variables inside to be sent over the network (Used as Observed component in photon view, this reads/writes the variables)
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            //We own this player so send the other computers the data
            stream.SendNext(redTeamCount);
        }
        else
        {
            //Network player that receives the data
            redTeamCount = (int)stream.ReceiveNext();
        }
    }
}


