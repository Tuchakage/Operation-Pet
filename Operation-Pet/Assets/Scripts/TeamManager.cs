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
    public enum teams {Unassigned ,Red, Blue, Yellow, Green, Purple};
    public int ownerid;
    Player localPlayer;

    //Variable used to check if the player card can be moved (Otherwise initially an Object reference error comes up
    private bool canMoveCard;

    void Start()
    {
        canMoveCard = false;
    }

    void Update() 
    {
        //Debug.Log("Team count = " + redTeamCount);
    }

    #region PUN Callbacks

    public override void OnJoinedRoom() 
    {
        //Get the actor number when the local player joins the room
        ownerid = PhotonNetwork.LocalPlayer.ActorNumber;

        //Get Reference to Player info when local player joins the room
        localPlayer = PhotonNetwork.CurrentRoom.GetPlayer(ownerid);

        IncreaseTeamCount(localPlayer, AddPlayerToTeam(teams.Unassigned));

        //Move the Player Cards for all players that have just joined the room
        if (PhotonNetwork.PlayerList.Length > 0) 
        {
            foreach (Player p in PhotonNetwork.PlayerList)
            {
                teams playerTeam = GetTeamName(p);
                if (playerTeam != teams.Unassigned)
                {
                    MoveCardToTeam(playerTeam, p.ActorNumber);
                }
            }
        }



    }

    //Called when there is a change to a Players custom property
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        //Debug.Log("Property for " + targetPlayer.NickName + " Changed to " + changedProps["Team Name"]);
        //IncreaseTeamCount(targetPlayer);

        //Move the player card to be under the team name
        MoveCardToTeam(GetTeamName(targetPlayer), targetPlayer.ActorNumber);
        //if (canMoveCard) 
        //{

        //    Debug.Log("canMoveCard = " + canMoveCard);
        //}


        
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        foreach (var hi in propertiesThatChanged)
        {
            Debug.Log("Key: " + hi.Key+ " = "+ hi.Value);
        }

    }

    #endregion

    #region Button Functions
    public void JoinUnassigned()
    {
        //Before changing the team, remove the count from there previous team
        DecreaseTeamCount(localPlayer);
        Debug.Log("JoinUnassigned");


        IncreaseTeamCount(localPlayer, AddPlayerToTeam(teams.Unassigned));




    }

    public void JoinRedTeam() 
    {
        //Check if the team is full or if they are already on Red Team or not
        if (GetTeamCount(teams.Red) >= 2 || CheckTeam(teams.Red))
        {
            return;
        }

        


        //DecreaseTeamCount(localPlayer);


        //AddPlayerToTeam(teams.Red);

        //Player Card can be moved
        canMoveCard = true;

        //Increase Red Team Count
        IncreaseTeamCount(localPlayer, AddPlayerToTeam(teams.Red));
        //IncreaseTeamCount(localPlayer, teams.Red);
    }
    #endregion

    #region Team Related Functions
    teams GetTeamName(Player player)
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

    int GetTeamCount(teams teamToCheck) 
    {
        
        switch (teamToCheck) 
        {
            case teams.Unassigned:
                break;

            case teams.Red:
                object redTeamCount;
                //Check the custom properties of that player to see what team they are apart of
                if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(teamToCheck.ToString(), out redTeamCount))
                {
                    
                    return (int)redTeamCount;
                }

              break;
        }


        return 0;
    }

    //Function that checks whether they are already in a team and if they do dont run the rest of the code
    bool CheckTeam(teams teamJoinAttempt) 
    {
        //if they are already in the same team as the button as they pressed
        if (GetTeamName(localPlayer) == teamJoinAttempt)
        {
            
            //They are in the same team so ignore them
            return true;
        }
        return false;
    }

    teams AddPlayerToTeam(teams teamToAdd) 
    {
        //Change what team the player is on
        Hashtable properties = new Hashtable()
        {
            {"Team Name",  teamToAdd}
        };

        //Update the Hashtable that is being tracked by PUN to keep track of what team the player is on
        PhotonNetwork.LocalPlayer.SetCustomProperties(properties);

        return teamToAdd;
    }


    void IncreaseTeamCount(Player playerJoined, teams team) 
    {
        //Temporary local variable to increase the amount of players in a team
        int teamCounter = 0;
        //Check which team to increase
        switch (team)
        {
            case teams.Unassigned:
                //Debug.Log("Unassigned");
                break;

            case teams.Red:
                //Get the value that is already stored for that team
                teamCounter = GetTeamCount(teams.Red);
                //Increase Counter by 1
                teamCounter++;
                Debug.Log("Increased Team Count for: " + team + " = " + teamCounter);
                break;


        }

        Hashtable teamProp = new Hashtable()
        {
            {team.ToString(),  teamCounter}
        };
        Debug.Log("Team Counter =" +teamCounter + " For "+team);
        //Set the Custom Properties for the room so that it knows how many players are in each team
        PhotonNetwork.CurrentRoom.SetCustomProperties(teamProp);



        //photonView.RPC("IncreaseTeamCount", RpcTarget.OthersBuffered, playerJoined);
    }

    public void DecreaseTeamCount(Player p) 
    {
        //Temporary local variable to decrease the amount of players in a team
        int teamCounter = 0;
        //Debug.Log("Called Decrease");
        Debug.Log("REDDD: "+ GetTeamCount(teams.Red));
        //Check what team the player is on
        switch (GetTeamName(p))
        {
            case teams.Unassigned:
                Debug.Log("Decrease Unassigned");
                break;

            case teams.Red:
                //Get the value that is already stored for that team
                teamCounter = GetTeamCount(teams.Red);

                //Decrease Counter by 1
                teamCounter--;
                Debug.Log("Decrease Amount in Red Team = " + teamCounter);
                break;


        }

        //Update the hash table telling the Room to decrease the amount of players in the team
        Hashtable teamProp = new Hashtable()
        {
            {GetTeamName(p).ToString(),  teamCounter}
        };

        //Set the Custom Properties for the room so that it knows how many players are in each team
        PhotonNetwork.CurrentRoom.SetCustomProperties(teamProp);

    }

    void MoveCardToTeam(teams teamToJoin, int cardOwner)
    {
        //Switch case used to find the correct Group List to put the Player Card under
        string teamNameCheck = "Unassigned";
        switch (teamToJoin)
        {
            case teams.Unassigned:
                teamNameCheck = "Unassigned Player Card Group";
                break;
            case teams.Red:
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

    #endregion

    //This function allows the variables inside to be sent over the network (Used as Observed component in photon view, this reads/writes the variables)
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            //We own this player so send the other computers the data

        }
        else
        {
            ////Network player that receives the data
        }
    }
}


