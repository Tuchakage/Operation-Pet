using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static teamsEnum;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System.IO;
using System;
using Unity.VisualScripting;
using UnityEngine.InputSystem;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class RoundManager : MonoBehaviourPunCallbacks, IPunObservable
{


    //Store all the possible winners of the round
    private Dictionary<teams, int> possibleWinners;

    public float maxRoundTime;
    [SerializeField]private float currRoundTime;

    //Determines whether the round has ended
    private bool roundEnd;

    //Indicate whether the deathmatch has started (Set in PetFoodSpawner Script)
    public bool hasDeathmatchStarted;

    public TMP_Text timerTxt;
    public TMP_Text roundTxt;
    //Indicators for in game to show how many rounds each team has won
    public GameObject roundWonIndicatorPrefab;

    ScoreManager scoreManager;
    PetFoodSpawner foodSpawner;
    RoleManager roleManager;

    //Variable to make sure only 1 set of food is spawned
    public bool foodSpawned;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Now that everyone is in the game, set the player ready property to false
        PlayerCardEntry.UpdatePlayerReadyProp(false);
        //Create Dictionarys
        possibleWinners = new Dictionary<teams, int>();

        scoreManager = GameObject.Find("ScoreManager").GetComponent<ScoreManager>();
        foodSpawner = GameObject.Find("FoodSpawner").GetComponent<PetFoodSpawner>();
        roleManager = GameObject.Find("RoleManager").GetComponent<RoleManager>();
        roundEnd = false;

        foodSpawned = false;
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
                    //If the Hashtable doesn't exist
                    if (!PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(teamName.ToString() + " Rounds"))
                    {
                        //Initialise the table starting the team off with 0 rounds won
                        Hashtable initTeam = new Hashtable()
                        {
                            {teamName.ToString() + " Rounds", 0 }
                        };
                        //Set the Custom Properties for the room so that it knows how many rounds each team has won
                        PhotonNetwork.CurrentRoom.SetCustomProperties(initTeam);
                    }
                    else //If team already has the custom property set
                    {
                        //Update the UI indicating the amount of rounds each team has won
                        UpdateRoundWonUI(teamName);
                    }

                }

            }
            
        }


        //If the Hashtable for keeping track of the current round doesn't exist
        if (!PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("Round Number"))
        {
            //Initialise the table starting the round number at 1
            Hashtable initRound = new Hashtable()
            {
                {"Round Number", 1 }
            };
            //Set the Custom Properties for the room so that it knows how many rounds each team has won
            PhotonNetwork.CurrentRoom.SetCustomProperties(initRound);
        }
        else //If the Hashtable does exist (Round 1 has already been completed)
        {
            if (PhotonNetwork.IsMasterClient) 
            {
                //Set the amount of food each team needs to collect (We put this here so this is done after the round has been 
                scoreManager.SetMaxFoodPerTeam(GetCurrentRound());
            }

            //Swap Roles
            StartCoroutine(roleManager.SwapRoles());




        }
        //Start the countdown of the round
        currRoundTime = maxRoundTime;

    }

    // Update is called once per frame
    void Update()
    {
        if (PhotonNetwork.IsMasterClient) 
        {
            if (currRoundTime > 0)
            {
                currRoundTime -= Time.deltaTime;
            }
            else if (currRoundTime < 0 && !roundEnd && !hasDeathmatchStarted)
            {
                
                roundEnd = true;
                CheckRoundWinner();
            }
            else if (currRoundTime < 0 && !roundEnd && hasDeathmatchStarted) 
            {
                roundEnd = true;
                CheckGameWinner();
            }
        }


        int minutes = Mathf.FloorToInt(currRoundTime / 60);
        int seconds = Mathf.FloorToInt(currRoundTime % 60);
        timerTxt.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }


    public System.Collections.IEnumerator NextRound() 
    {
        Debug.Log("Start New Round");
        //Check what round number the game is in
        if (GetCurrentRound() <= 3)
        {
            //Increase Round Number
            IncreaseRoundNumProperty();
            Debug.Log("Increasing");

        }
        else //After Round 4 Check if there is someone who won the game
        {
            CheckGameWinner();
            yield return null;
            //End Game
        }

        yield return new WaitForSeconds(1f);

        //Go to the Scene Switcher scene (If we don't go to a different scene then the clients won't be able to reload
        PhotonNetwork.LoadLevel("SceneSwitcher");

    }

    #region After Timer ends
    //Function that is called after timer runs out
    public void CheckRoundWinner() 
    {
        int highestScore = -1;
        //Check all the values of the team in the dictionary
        foreach (teams teamName in scoreManager.teamScores.Keys)
        {
            int teamRoundScore;
            if (scoreManager.teamScores.TryGetValue(teamName, out teamRoundScore))
            {
                //If the score of this team is higher than the highest
                if (teamRoundScore > highestScore)
                {

                    //set this score to be highest
                    highestScore = teamRoundScore;
                    //Add team as a possible winner
                    possibleWinners.Add(teamName, teamRoundScore);
                    Debug.Log("Check Round Winner: " + teamName);

                    //If there are multiple round winners
                    if (possibleWinners.Count > 1)
                    {
                        //Check all their values and if they are lower than the current Highest Score then remove them from the list
                        RemoveFromWinningTeams(highestScore);
                    }
                    //Debug.Log(teamName + "Added to winning list with score: "+ teamRoundScore);
                }
                else if (teamRoundScore == highestScore) //If they have the same highscore
                {
                    //Add team as a possible winner
                    possibleWinners.Add(teamName, teamRoundScore);
                }
            }


        }

        //After sorting out the teams with the highest score, Increase the amount of rounds they have won
        IncreasePossibleWinners();
    }

    //Function used when time runs, give every team with the highest score a win for the round
    void IncreasePossibleWinners()
    {
        //For each team that could be possible winners due to having the highest score
        foreach (var teamName in possibleWinners.Keys)
        {
            //The key for the winning team
            string keyName = teamName.ToString() + " Rounds";

            //Get the amount of rounds the winning team has got
            object roundsWon;
            if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(keyName, out roundsWon))
            {
                //Get the value and increment it by 1
                int num = (int)roundsWon;
                num++;

                Debug.Log(teamName + " has won a round");
                //Update Room Properties Hashtable
                StartCoroutine(UpdateRoundProperties(keyName, num));
            }
        }

        //Clear the list because the next time it will be called it will need to be empty
        possibleWinners.Clear();

        //Start the next round
        StartCoroutine(NextRound());
    }

    #endregion

    //Function used when a team collects all the points
    public System.Collections.IEnumerator IncreaseTeamRoundWon(teams winningTeam) 
    {
        
        //The key for the winning team
        string keyName = winningTeam.ToString() + " Rounds";
        
        //Get the amount of rounds the winning team has got
        object roundsWon;
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(keyName, out roundsWon))
        {
            //Get the value and increment it by 1
            int num = (int)roundsWon;
            Debug.Log("Before Increase Team Round of "+ keyName + " to "+num);
            num++;

            //Update Room Property Hashtable
            yield return StartCoroutine(UpdateRoundProperties(keyName, num));
            Debug.Log("Update Round Props "+keyName +" "+num);
        }

        yield return new WaitForSeconds(1f);
        //Start the next round
        StartCoroutine(NextRound());

    }

    //Function called at the end of the game
    void CheckGameWinner() 
    {
        //Make sure list is empty
        possibleWinners.Clear();
        //Variable used to compare each team score
        int highestScore = 0;


        //Check through every team in the "teams" Enum that was created
        foreach (teams teamName in Enum.GetValues(typeof(teams)))
        {
            string keyName = teamName.ToString() + " Rounds";

            //Get the amount of rounds the team being checked has won
            object teamRoundScore;
            if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(keyName, out teamRoundScore))
            {
                //If the score of this team is higher than the highest
                if ((int)teamRoundScore > highestScore)
                {
                    //set this score to be highest
                    highestScore = (int)teamRoundScore;

                    //Add this as a team that could potentially be the winner of the game
                    possibleWinners.Add(teamName, (int)teamRoundScore);

                    Debug.Log("Check Game Winner");

                    //If there are multiple round winners
                    if (possibleWinners.Count > 1)
                    {
                        //Check all their values and if they are lower than the current Highest Score then remove them from the list
                        RemoveFromWinningTeams(highestScore);
                    }

                }
                else if ((int)teamRoundScore == highestScore) //If the score is the same as the highest
                {
                    //Add the team to the list
                    possibleWinners.Add(teamName, highestScore);
                }
            }


        }



        //If there is more than one team in the possibleWinners Dictionary
        if (possibleWinners.Count > 1)
        {
            //If not in the deathmatch
            if (!hasDeathmatchStarted)
            {
                //Increase round by 1 more to start Deathmatch
                IncreaseRoundNumProperty();
                //Start Deathmatch
                Debug.Log("Start Deathmatch");
            }
            else 
            {
                PhotonNetwork.LoadLevel("ResultScreen");
            }
            //Check if currently in deathmatch



        }
        else 
        {
            //The only team in the dictionary is the winner of the game!
            Debug.Log(possibleWinners.First() + "IS THE WINNER!");
            PhotonNetwork.LoadLevel("ResultScreen");
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

    #region Photon Property Related Functions

    //Function to update the Hashtable to increase the amount of Rounds a team has won
    System.Collections.IEnumerator UpdateRoundProperties(string key, int roundsWon) 
    {
        
        //Update Hashtable
        Hashtable properties = new Hashtable()
        {
                {key, roundsWon}
            };

        //Update the Hashtable that is being tracked by PUN
        PhotonNetwork.CurrentRoom.SetCustomProperties(properties);

        yield return null;
    }
    
    //Get the current Round Number of the game
    public int GetCurrentRound() 
    {
        object currentroundNum;
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("Round Number", out currentroundNum)) 
        {
            return (int)currentroundNum;
        }

        return 0;
    }

    //Function to increase the Round Number of the game
    void IncreaseRoundNumProperty() 
    {
        object currentroundNum;
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("Round Number", out currentroundNum))
        {
            //Store current Round Num into this variable whilst also conerting it to integer
            int newRoundNum = (int)currentroundNum;
            newRoundNum++;
            //Debug.Log("New Round number SHOULD be " + newRoundNum);
            //Update Hashtable
            Hashtable properties = new Hashtable()
            {
                {"Round Number", newRoundNum}
            };

            //Update the Hashtable that is being tracked by PUN
            PhotonNetwork.CurrentRoom.SetCustomProperties(properties);
        }

    }

    #endregion

    //Function called when the deathmatch food is picked up, setting the winning team
    [PunRPC]
    public void GameWinner(teams winningTeam) 
    {
        Debug.Log(winningTeam.ToString() + "HAS WON THE GAME");
    }

    //Functions used to update the 
    void UpdateRoundWonUI(teams teamToUpdate) 
    {

        Debug.Log("Checking Team: " + teamToUpdate);
        //Find the empty game object the rounds won indicator will be spawned on
        GameObject spawnLocation = GameObject.Find(teamToUpdate + " Rounds Won");

        if (!spawnLocation) 
        {
            Debug.LogError("Couldnt find game object to hold Round Won Indicators");
            return;
        }

        //The key for checking how many rounds the team has won
        string keyName = teamToUpdate.ToString() + " Rounds";

        //Get the amount of rounds the winning team has got
        object roundsWon;
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(keyName, out roundsWon)) 
        {
            if ((int)roundsWon > 0) 
            {
                //Foreach round that has been won
                for (int i = 0; i < (int)roundsWon; i++)
                {
                    //Spawn in Winning indicator

                    GameObject indicator = Instantiate(roundWonIndicatorPrefab, spawnLocation.transform);
                }
            }



        }
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        //If the round property was changed then spawn the food in (Only make the Master Client do this) This is called when the Rounds are initialised at the start
        if (propertiesThatChanged.ContainsKey("Round Number") && PhotonNetwork.IsMasterClient && !foodSpawned) 
        {
            //Set the amount of food each team needs to collect (We put this here so this is done after the round has been 
            scoreManager.SetMaxFoodPerTeam(GetCurrentRound());
            foodSpawned = true;
        }

        if (propertiesThatChanged.ContainsKey("Round Number")) 
        {
            Debug.Log("We are on Round " + GetCurrentRound());
        }
        //Check the properties that have changed in the Hashtable
        //foreach (var prop in propertiesThatChanged)
        //{
        //    if (PhotonNetwork.IsMasterClient) 
        //    {
        //        Debug.Log("Key: " + prop.Key + " = " + prop.Value);
        //    }
        //}


    }

    //This function allows the variables inside to be sent over the network (Used as Observed component in photon view, this reads/writes the variables)
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            //We own this player so send the other computers the data
            stream.SendNext(currRoundTime);

        }
        else
        {
            //Network player that receives the data
            currRoundTime = (float)stream.ReceiveNext();
        }
    }

}
