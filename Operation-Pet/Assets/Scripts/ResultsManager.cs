using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using static teamsEnum;

public class ResultsManager : MonoBehaviour
{
    public TeamModelScriptableObject PlayerModelScriptableObject;

    private Dictionary<teams, int> sortedRankedTeams;
    private Dictionary<teams, int> possibleWinners;

    [SerializeField]
    private List<GameObject> SetsList;

    void Awake() 
    {

    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        sortedRankedTeams = new Dictionary<teams, int>();

        //Sort the teams in the "sortedRankedTeams" dictionary
        SortTeams();

        //Choose where the Players will be spawned on the Results Screen
        SelectSet();

        SpawnTeam();


    }
    // Update is called once per frame
    void Update()
    {
        
    }

    //Function that finds all the teams in the game and sorts them depending on their score
    void SortTeams() 
    {
        
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

                        //Check who the winners of the game are
                        CheckAmntWinners(teamName, (int)teamScore);
                    }
                }
            }
        }

        //Order the Dictionary which will convert it to a IOrderedEnumerable List and then convert it back to a Dictionary using the original keys and the original values
        sortedRankedTeams = sortedRankedTeams.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
    }

    void SelectSet() 
    {
        //Index of the Set that should be used from SetsArray Array
        //index 0 is also set 1
        int index = 0;
        switch (possibleWinners.Count) 
        {
            case 2:
                //Use Set 2
                index = 1;
                break;

            case 3:
                //Use Set 3
                index = 2;
                break;

            case 4:
                //Use Set 4
                index = 3;
                break;

            case 5:
                //Use Set 5
                index = 4;
                break;
        }

        //Check each Set in the SetsList
        foreach (GameObject Set in SetsList) 
        {
            //if they are not equal to the Set that we are using (Indicated by index)
            if (Set != SetsList[index]) 
            {
                //Remove & Destroy from List
                SetsList.Remove(Set);
                Destroy(Set);
            }
        }
    }

    void SpawnTeam() 
    {

        for (int i = 0; i < sortedRankedTeams.Count; i++) 
        {
            //Get the child of the set list in order
            Transform spawnpoint = SetsList[0].transform.GetChild(i);
            GameObject model = Instantiate(CheckModelToSpawn(sortedRankedTeams.ElementAt(i).Key), spawnpoint);
        }

    }

    GameObject CheckModelToSpawn(teams teamName)
    {
        switch (teamName)
        {
            case teams.Dog:
                Debug.Log(PlayerModelScriptableObject.playerModels[0].name);
                return PlayerModelScriptableObject.playerModels[0];
            case teams.Cat:
                Debug.Log(PlayerModelScriptableObject.playerModels[1].name);
                return PlayerModelScriptableObject.playerModels[1];

            case teams.Mouse:
                return PlayerModelScriptableObject.playerModels[2];

            case teams.Squirrel:
                return PlayerModelScriptableObject.playerModels[3];

            case teams.Horse:
                return PlayerModelScriptableObject.playerModels[4];
        }

        return null;
    }
    //Function to check who the winners of the game are
    void CheckAmntWinners(teams team, int teamRoundScore) 
    {
        int highestScore = 0;
        if ((int)teamRoundScore > highestScore)
        {
            //set this score to be highest
            highestScore = teamRoundScore;

            //Add this as a team that could potentially be the winner of the game
            possibleWinners.Add(team, teamRoundScore);

            Debug.Log("Check Game Winner");

            //If there are multiple gane winners
            if (possibleWinners.Count > 1)
            {
                //Check all their values and if they are lower than the current Highest Score then remove them from the list
                RemoveFromWinningTeams(highestScore);
            }

        }
        else if ((int)teamRoundScore == highestScore) //If the score is the same as the highest
        {
            //Add the team to the list
            possibleWinners.Add(team, highestScore);
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
}
