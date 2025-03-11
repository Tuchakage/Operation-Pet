using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using System.Collections.Generic;
using static Photon.Pun.UtilityScripts.PunTeams;
using UnityEngine.UIElements;
using System;
using TMPro;
using Unity.VisualScripting;


//Purpose of this script is to set the teams of the players before the game
public class TeamManager : MonoBehaviourPunCallbacks, IPunObservable 
{
    public enum teams {Unassigned ,Red, Blue, Yellow, Green, Purple};
    //Set the Team Name text in the Unity Editor
    public TMP_Text[] teamText; 
    public int ownerid;
    Player localPlayer;
    PlayerCardEntry localPlayerCard;

    //Variable used to check if the player card can be moved (Otherwise initially an Object reference error comes up
    private bool canMoveCard;

    void Start()
    {
        canMoveCard = false;

        //Make sure player starts in the Unassigned tem
        AddPlayerToTeam(teams.Unassigned);
    }

    void Update() 
    {
        //Debug.Log("Team count = " + redTeamCount);
    }

    #region PUN Callbacks

    public override void OnCreatedRoom()
    {
        //Initialise the Teams
        InitTeams();
    }
    public override void OnJoinedRoom() 
    {
        
        //Get the actor number when the local player joins the room
        ownerid = PhotonNetwork.LocalPlayer.ActorNumber;

        //Get Reference to Player info when local player joins the room
        localPlayer = PhotonNetwork.CurrentRoom.GetPlayer(ownerid);


        IncreaseTeamCount(teams.Unassigned);

        //Move the Player Cards for all players that have just joined the room
        if (PhotonNetwork.PlayerList.Length > 0) 
        {
            foreach (Player p in PhotonNetwork.PlayerList)
            {
                teams playerTeam = GetTeamName(p);
                MoveCardToTeam(playerTeam, p.ActorNumber);
            }
        }

        //Check through every team in the "teams" Enum that was created
        foreach (teams teamName in Enum.GetValues(typeof(teams))) 
        {
            SetTeamText(teamName, GetTeamCount(teamName));
        }

    }

    //Called when there is a change to a Players custom property
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        //Debug.Log("Property for " + targetPlayer.NickName + " Changed to " + changedProps["Team Name"]);

        //Move the player card to be under the team name
        MoveCardToTeam(GetTeamName(targetPlayer), targetPlayer.ActorNumber);
        //if (canMoveCard) 
        //{

        //    Debug.Log("canMoveCard = " + canMoveCard);
        //}


        
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        //When someones joins a team, update the text for everyone in the game
        foreach (var prop in propertiesThatChanged)
        {

            //Debug.Log("Key: " + prop.Key + " = " + prop.Value);
            //Try Convert the object to the teams Enum and store into variable called "teamName"
            if (System.Enum.TryParse<teams>(prop.Key.ToString(), out teams teamName)) 
            {
                //If Successful update Team Text
                SetTeamText(teamName, Convert.ToInt32(prop.Value));
            }
            
            //SetTeamText((teams)prop.Key, (int)prop.Value);


        }


    }

    #endregion

    #region Button Functions
    public void JoinUnassigned()
    {
        //Before changing the team, remove the count from there previous team
        DecreaseTeamCount();


        IncreaseTeamCount(AddPlayerToTeam(teams.Unassigned));




    }

    public void JoinRedTeam() 
    {
        //Check if the team is full or if they are already on Red Team or not
        if (GetTeamCount(teams.Red) >= 2 || CheckTeam(teams.Red))
        {
            return;
        }

        //if the Player is already readied up
        if (localPlayerCard.isPlayerReady) 
        {
            //Reverse it and make it so they arent readied up
            localPlayerCard.ReadyUp();
        }
        

        DecreaseTeamCount();

        //Player Card can be moved
        canMoveCard = true;

        //Increase Red Team Count
        IncreaseTeamCount(AddPlayerToTeam(teams.Red));
        
    }

    public void JoinBlueTeam()
    {
        //Check if the team is full or if they are already on Red Team or not
        if (GetTeamCount(teams.Blue) >= 2 || CheckTeam(teams.Blue))
        {
            return;
        }

        //if the Player is already readied up
        if (localPlayerCard.isPlayerReady)
        {
            //Reverse it and make it so they arent readied up
            localPlayerCard.ReadyUp();
        }

        Debug.Log("Player Card: " + localPlayerCard.isPlayerReady);
        DecreaseTeamCount();

        //Player Card can be moved
        canMoveCard = true;

        //Increase Red Team Count
        IncreaseTeamCount(AddPlayerToTeam(teams.Blue));
    }

    public void JoinYellowTeam()
    {
        //Check if the team is full or if they are already on Red Team or not
        if (GetTeamCount(teams.Yellow) >= 2 || CheckTeam(teams.Yellow))
        {
            return;
        }

        //if the Player is already readied up
        if (localPlayerCard.isPlayerReady)
        {
            //Reverse it and make it so they arent readied up
            localPlayerCard.ReadyUp();
        }

        DecreaseTeamCount();

        //Player Card can be moved
        canMoveCard = true;

        //Increase Red Team Count
        IncreaseTeamCount(AddPlayerToTeam(teams.Yellow));
    }

    public void JoinGreenTeam()
    {
        //Check if the team is full or if they are already on Red Team or not
        if (GetTeamCount(teams.Green) >= 2 || CheckTeam(teams.Green))
        {
            return;
        }

        //if the Player is already readied up
        if (localPlayerCard.isPlayerReady)
        {
            //Reverse it and make it so they arent readied up
            localPlayerCard.ReadyUp();
        }

        DecreaseTeamCount();

        //Player Card can be moved
        canMoveCard = true;

        //Increase Red Team Count
        IncreaseTeamCount(AddPlayerToTeam(teams.Green));
    }

    public void JoinPurpleTeam()
    {
        //Check if the team is full or if they are already on Red Team or not
        if (GetTeamCount(teams.Purple) >= 2 || CheckTeam(teams.Purple))
        {
            return;
        }

        //if the Player is already readied up
        if (localPlayerCard.isPlayerReady)
        {
            //Reverse it and make it so they arent readied up
            localPlayerCard.ReadyUp();
        }

        DecreaseTeamCount();

        //Player Card can be moved
        canMoveCard = true;

        //Increase Red Team Count
        IncreaseTeamCount(AddPlayerToTeam(teams.Purple));
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
        //Debug.Log("Checking Team: " + teamToCheck);
        object teamCount;
        //Check the custom properties of that player to see what team they are apart of
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(teamToCheck.ToString(), out teamCount))
        {
            Debug.Log("Checking Team: " + teamToCheck + " Has " + (int)teamCount);
            return (int)teamCount;
        }

        return 0;
    }

    //Function to check if all teams are ready
    public bool CheckReadyTeams() 
    {
        int readyTeamCount = 0;
       // bool canStartGame = false;

        //Check through every team in the "teams" Enum that was created
        foreach (teams teamName in Enum.GetValues(typeof(teams)))
        {
            object teamCount;
            if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(teamName.ToString(), out teamCount))
            {
                //If anyone is in unassigned then dont start the game cause not everyone is in a team
                if (teamName.ToString() == "Unassigned" && (int)teamCount > 0) 
                {
                    return false;
                }

                if ((int)teamCount == 0)
                {
                    //If there is no one in the team then ignore them
                    continue;
                }
                else if ((int)teamCount == 2)
                {
                    //If there are 2 people in the team that means that team is ready
                    readyTeamCount++;
                }
                else 
                {
                    //If there is 1 Player or more than 2 that means all teams are not ready
                    return false;
                }
            }
            //Debug.Log("Team Name: " + teamName + ": " + (int)teamCount);

        }

        if (readyTeamCount > 1)
        {
            return true;
        }
        else 
        {
            return false;
        }
        
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

    void InitTeams() 
    {
        //Go through each team and set the player count to 0
        foreach (teams teamName in Enum.GetValues(typeof(teams)))
        {
            Hashtable initTeam = new Hashtable()
            {
                {teamName.ToString(), 0 }
            };

            //Set the Custom Properties for the room so that it knows how many players are in each team
            PhotonNetwork.CurrentRoom.SetCustomProperties(initTeam);
        }
    }


    void IncreaseTeamCount(teams team) 
    {
        //Temporary local variable to increase the amount of players in a team
        int teamCounter = 0;

        //Get the current amount of players in the team the player has joined
        teamCounter = GetTeamCount(team);

        //Increase Counter by 1
        teamCounter++;

        Debug.Log("Increased Team Count for: " + team + " = " + teamCounter);

        UpdateTeamProp(team, teamCounter);

        //SetTeamText(team, teamCounter);

        //photonView.RPC("IncreaseTeamCount", RpcTarget.OthersBuffered, playerJoined);
    }

    public void DecreaseTeamCount() 
    {
        //Temporary local variable to increase the amount of players in a team
        int teamCounter = 0;

        teams teamName = GetTeamName(localPlayer);
        //Get the current amount of players in the team the player has joined
        teamCounter = GetTeamCount(teamName);
        
        //Make sure team Counter doesn't go below 0
        if (teamCounter > 0) 
        {
            //Decrease Counter by 1
            teamCounter--;
        }


        Debug.Log("Decreased Team Count for: " + GetTeamName(localPlayer) + " = " + teamCounter);

        //Update the hash table telling the Room to decrease the amount of players in the team
        UpdateTeamProp(teamName, teamCounter);

        //SetTeamText(teamName, teamCounter);
    }

    void MoveCardToTeam(teams teamToJoin, int cardOwner)
    {


        //Use this to find the Game Object Group of the team for the player card
        string teamNameCheck = teamToJoin.ToString() + " Team";

        //Debug.Log("Name of team: " + teamNameCheck);
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

                //If the player card in the array belongs to the local player that is trying to move then move it under the team name.
                if (playerCardID == cardOwner)
                {
                    card.transform.SetParent(TeamGroupList);
                    card.transform.localScale = new Vector3(1, 1, 1);
                    //Keep local reference of player card
                    localPlayerCard = card.GetComponent<PlayerCardEntry>();
                }
            }
        }
        // Debug.Log("Moving " + PhotonNetwork.CurrentRoom.GetPlayer(cardOwner).NickName + " To Red Team");

        //Cant move Player Card anymore
        canMoveCard = false;

        //SetTeamText(teamToJoin, GetTeamCount(teamToJoin));
        //Debug.Log("Moved ID:" + cardOwner + " to " + teamToJoin);


    }

    //Function that will update the Room propertie to tell how many players are in each team
    void UpdateTeamProp(teams teamName, int numOfTeamMems) 
    {
        Hashtable teamProp = new Hashtable()
        {
            {teamName.ToString(),  numOfTeamMems}
        };
        //Debug.Log("Team Counter =" +teamCounter + " For "+team);
        //Set the Custom Properties for the room so that it knows how many players are in each team
        PhotonNetwork.CurrentRoom.SetCustomProperties(teamProp);
        
    }


    void SetTeamText(teams teamName, int teamCounter) 
    {

        //Get the current amount of players in the team the player has joined
        //int teamCounter = GetTeamCount(teamName);
        switch (teamName) 
        {
            case teams.Unassigned:
                teamText[0].text = "Unassigned " + teamCounter;
                break;
            case teams.Red:
                teamText[1].text = "Red Team " + teamCounter + "/2";
                break;
            case teams.Blue:
                teamText[2].text = "Blue Team " + teamCounter + "/2";
                break;
            case teams.Yellow:
                teamText[3].text = "Yellow Team " + teamCounter + "/2";
                break;
            case teams.Green:
                teamText[4].text = "Green Team " + teamCounter + "/2";
                break;
            case teams.Purple:
                teamText[5].text = "Purple Team " + teamCounter + "/2";
                break;

        }

        //Debug.Log(teamName + " Team " + teamCounter + "/2");
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


