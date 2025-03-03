using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using System.Collections.Generic;
using static Photon.Pun.UtilityScripts.PunTeams;
using UnityEngine.UIElements;


//Purpose of this script is to set the teams of the players before the game
public class TeamManager : MonoBehaviourPunCallbacks, IPunObservable 
{
    private string teamName = "Unassigned";
    public int ownerid;
    private int redTeamCount;
    Player localPlayer;

    //Variable used to check if the player card can be moved (Otherwise initially an Object reference error comes up
    private bool canMoveCard;

    void Start()
    {
        canMoveCard = false;
        redTeamCount = 0;
    }

    void Update() 
    {
        //Debug.Log("Team count = " + redTeamCount);
    }

    public override void OnJoinedRoom() 
    {
        //Get the actor number when the local player joins the room
        ownerid = PhotonNetwork.LocalPlayer.ActorNumber;

        //Get Reference to Player info when local player joins the room
        localPlayer = PhotonNetwork.CurrentRoom.GetPlayer(ownerid);

        teamName = "Unassigned";

        //Update Hash Table 
        Hashtable properties = new Hashtable()
        {
            {"Team Name",  teamName}
        };

        //Update the Hashtable that is being tracked by PUN
        PhotonNetwork.LocalPlayer.SetCustomProperties(properties);



        //Move the Player Cards for all players that have just joined the room
        if (PhotonNetwork.PlayerList.Length > 0) 
        {
            foreach (Player p in PhotonNetwork.PlayerList)
            {
                string playerTeam = GetTeamName(p);
                if (playerTeam != "Unassigned")
                {
                    MoveCardToTeam(playerTeam, p.ActorNumber);
                }
            }
        }



    }

    //Called when there is a change to a Players custom property
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        Debug.Log("Property for " + targetPlayer.NickName + " Changed to " + changedProps["Team Name"]);
        IncreaseTeamCount(targetPlayer);

        //Move the player card to be under the team name
        MoveCardToTeam(GetTeamName(targetPlayer), targetPlayer.ActorNumber);
        //if (canMoveCard) 
        //{

        //    Debug.Log("canMoveCard = " + canMoveCard);
        //}


        
    }

    public override void OnPlayerLeftRoom(Player gonePlayer)
    {
        
    }

    public void JoinUnassigned()
    {
        DecreaseTeamCount(localPlayer);
        Debug.Log("JoinUnassigned");

        teamName = "Unassigned";

        //Update Hash Table 
        Hashtable properties = new Hashtable()
        {
            {"Team Name",  teamName}
        };

        //Update the Hashtable that is being tracked by PUN
        PhotonNetwork.LocalPlayer.SetCustomProperties(properties);

        

    }

    public void JoinRedTeam() 
    {
        if (redTeamCount >= 2)
        {
            return;
        }

        teamName = "Red";

        CheckTeam();



        //Update Hash Table 
        Hashtable properties = new Hashtable()
        {
            {"Team Name",  teamName}
        };

        //Update the Hashtable that is being tracked by PUN
        PhotonNetwork.LocalPlayer.SetCustomProperties(properties);
        //Player Card can be moved
        canMoveCard = true;
    }



    string GetTeamName(Player player)
    {
        object isInTeam;
        //Check the custom properties of that player to see what team they are apart of
        if (player.CustomProperties.TryGetValue("Team Name", out isInTeam))
        {
            return (string)isInTeam;
        }

        return "Null";
    }


    //Function that checks whether they are already in a team and if they do dont run the rest of the code
    void CheckTeam() 
    {
        //if they are already in the same team as the button as they pressed
        if (GetTeamName(localPlayer) == teamName)
        {
            //Ignore them
            return;
        }

    }


    void IncreaseTeamCount(Player playerJoined) 
    {
        switch (GetTeamName(playerJoined))
        {
            case "Unassigned":
                //Debug.Log("Unassigned");
                break;

            case "Red":               
                redTeamCount++;
                Debug.Log("Increased Team Count for: " + GetTeamName(playerJoined) + " = " + redTeamCount);
                break;


        }

        

        //photonView.RPC("IncreaseTeamCount", RpcTarget.OthersBuffered, playerJoined);
    }

    [PunRPC]
    public void DecreaseTeamCount(Player p) 
    {
        Debug.Log("Called Decrease");
        switch (GetTeamName(p))
        {
            case "Unassigned":
                Debug.Log("Decrease Unassigned");
                break;

            case "Red":
                redTeamCount--;
                Debug.Log("Decrease Amount in Red Team = " + redTeamCount);
                break;


        }

        if (!PhotonNetwork.IsMasterClient) 
        {
            //Make sure only the Master Client is changing the team count
            photonView.RPC("DecreaseTeamCount", RpcTarget.MasterClient, p);
        }

    }

    void MoveCardToTeam(string team, int cardOwner)
    {
        //Switch case used to find the correct Group List to put the Player Card under
        string teamNameCheck = "Unassigned";
        switch (team)
        {
            case "Unassigned":
                teamNameCheck = "Unassigned Player Card Group";
                break;
            case "Red":
                //"Red Team" is the name of the GameObject in the Unity Editor where player cards of that team will go under
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
                    card.transform.localScale = new Vector3(1, 1, 1);
                }
            }
        }
        // Debug.Log("Moving " + PhotonNetwork.CurrentRoom.GetPlayer(cardOwner).NickName + " To Red Team");

        //Cant move Player Card anymore
        canMoveCard = false;

        //Debug.Log("Moved ID:" + cardOwner + " to " + team);


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
            ////Network player that receives the data
            redTeamCount = (int)stream.ReceiveNext();
            //Debug.Log("Receive "+ redTeamCount);
        }
    }
}


