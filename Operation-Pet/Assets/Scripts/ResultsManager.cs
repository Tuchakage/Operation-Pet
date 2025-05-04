using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static teamsEnum;

public class ResultsManager : MonoBehaviourPun
{
    public TeamModelScriptableObject PlayerModelScriptableObject;

    private Dictionary<teams, int> sortedRankedTeams;
    private Dictionary<teams, int> possibleWinners;

    //FOR TESTING
    //public teams[] testArray;

    [SerializeField]
    private List<GameObject> SetsList;

    [SerializeField]
    private GameObject[] modelsToSpawn;

    //Stores the index of the Set from the SetList that should not be destroyed
    private int setToKeep;

    //For Keeping track of the highest score when sorting the array (Made a global variable otherwise the Sort Team() function will look too messy)
    private int highScore;
    void Awake() 
    {

    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        sortedRankedTeams = new Dictionary<teams, int>();
        possibleWinners = new Dictionary<teams, int>();

        //Sort the teams in the "sortedRankedTeams" dictionary
        SortTeams();

        //Choose where the Players will be spawned on the Results Screen
        SelectSet();

        SpawnTeam();

        //No need to Sync Scenes because anyone can go back to the main menu at any time
        PhotonNetwork.AutomaticallySyncScene = false;

    }
    // Update is called once per frame
    void Update()
    {
        
    }

    //Function that finds all the teams in the game and sorts them depending on their score
    void SortTeams() 
    {
        int highScore = 0;
        //Check through every team in the "teams" Enum that was created
        foreach (teams teamName in Enum.GetValues(typeof(teams)))
        {
            object teamCount;
            //Try to get the value from the Hashtable and put it into 'teamCount'
            if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(teamName.ToString(), out teamCount))
            {
                //If the team is in the game  
                if ((int)teamCount > 1)
                {
                    object teamScore;
                    string keyName = teamName.ToString() + " Rounds";
                    //We try to get the score of the team
                    if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(keyName, out teamScore))
                    {
                        //Add the team and the score to the Dictionary
                        sortedRankedTeams.Add(teamName, (int)teamScore);

                        //Check who the winners of the game are and populate and manage the possibleWinners Dictionary
                        CheckAmntWinners(teamName, (int)teamScore);

                        Debug.Log("Team Score For " + teamName + " : " + (int)teamScore);
                    }
                }
            }
        }

        //Order the Dictionary which will convert it to a IOrderedEnumerable List and then convert it back to a Dictionary using the original keys and the original values
        sortedRankedTeams = sortedRankedTeams.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);

        /////////// FOR TESTING DELETE LATER ///////////////
        //testArray = sortedRankedTeams.Keys.ToArray();
    }

    void SelectSet() 
    {
        //Index of the Set that should be used from SetsArray Array
        //index 0 is also set 1
        setToKeep = 0;
        switch (possibleWinners.Count) 
        {
            case 2:
                //Use Set 2
                setToKeep = 1;
                break;

            case 3:
                //Use Set 3
                setToKeep = 2;
                break;

            case 4:
                //Use Set 4
                setToKeep = 3;
                break;

            case 5:
                //Use Set 5
                setToKeep = 4;
                break;
        }
        Debug.Log(SetsList[setToKeep] + " has been selected since " +possibleWinners.Count + " winners");



        for (int i = 0; i < SetsList.Count; i++) 
        {
            //If the set being checked is not the same as the set that needs to stay
            if (SetsList[i] != SetsList[setToKeep]) 
            {
                //Debug.Log(SetsList[i].name + " is going to be removed");
                //Get rid of the set in the list
                Destroy(SetsList[i]);
                //SetsList[i] = null;
                
            }
        }

    }

    void SpawnTeam() 
    {

        for (int i = 0; i < sortedRankedTeams.Count; i++) 
        {
            //Get the child of the set list in order
            Transform spawnpoint = SetsList[setToKeep].transform.GetChild(i);
            GameObject model = Instantiate(CheckModelToSpawn(sortedRankedTeams.ElementAt(i).Key), spawnpoint);
            model.transform.localPosition = new Vector3(0, 0, 0);
            model.transform.localScale = new Vector3(3, 3, 3);
            model.transform.localEulerAngles = new Vector3(0, 90, 0);
        }

        //Let the Master Client tell everyone to update match won stat if they won
        if (PhotonNetwork.IsMasterClient) 
        {
            //Update Matches Won Stat in the database
            photonView.RPC("IncreaseMatchWonStat", RpcTarget.All);
            
        }
        

    }

    GameObject CheckModelToSpawn(teams teamName)
    {
        Debug.Log("Spawning " + teamName.ToString());
        switch (teamName)
        {
            case teams.Dog:
                Debug.Log(modelsToSpawn[0].name);
                return modelsToSpawn[0];
            case teams.Cat:
                Debug.Log("Spawn Model: " + modelsToSpawn[1].name);
                return modelsToSpawn[1];

            case teams.Mouse:
                Debug.Log("Spawn Model: " + modelsToSpawn[2].name);
                return modelsToSpawn[2];

            case teams.Squirrel:
                return modelsToSpawn[3];

            case teams.Horse:
                return modelsToSpawn[4];
        }

        return null;
    }
    //Function to check who the winners of the game are
    void CheckAmntWinners(teams team, int teamRoundScore) 
    {
        if ((int)teamRoundScore > highScore)
        {
            //set this score to be highest
            highScore = teamRoundScore;

            //Add this as a team that could potentially be the winner of the game
            possibleWinners.Add(team, teamRoundScore);


            //If there are multiple gane winners
            if (possibleWinners.Count > 1)
            {
                //Check all their values and if they are lower than the current Highest Score then remove them from the list
                RemoveFromWinningTeams(highScore);
            }

        }
        else if ((int)teamRoundScore == highScore) //If the score is the same as the highest
        {
            //Add the team to the list
            possibleWinners.Add(team, highScore);
        }
    }

    void RemoveFromWinningTeams(int currHighScore)
    {
        //Check through all the possible winners
        foreach (teams teamName in possibleWinners.Keys.ToArray())
        {

            //If anyone in the team has a lower score than the current Highest Score 
            int teamScore;
            if (possibleWinners.TryGetValue(teamName, out teamScore))
            {

                if (teamScore < currHighScore)
                {
                    //Remove it from the list
                    possibleWinners.Remove(teamName);
                }
            }
        }

    }


    [PunRPC]
    void IncreaseMatchWonStat() 
    {
        //Get the team this player is on
        teams myTeam = GetTeam(PhotonNetwork.LocalPlayer);

        //If the possible winners key contains the team of this player
        if (possibleWinners.ContainsKey(myTeam)) 
        {
            Debug.Log(PhotonNetwork.NickName + " Is a Winner");
            //Call the Firebase Database and increase matches won
            StartCoroutine(FirebaseManager.Instance.UpdateMatchesWonDatabase());
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
}
