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

public class RoundManager : MonoBehaviourPunCallbacks, IPunObservable
{


    //Store all the possible winners of the round
    private Dictionary<teams, int> possibleWinners;

    [SerializeField]private float maxRoundTime;
    [SerializeField]private float currRoundTime;

    //Determines whether the round has ended
    private bool roundEnd;

    public TMP_Text timerTxt;
    public TMP_Text roundTxt;

    ScoreManager scoreManager;
    PetFoodSpawner foodSpawner;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Create Dictionarys
        possibleWinners = new Dictionary<teams, int>();

        scoreManager = GameObject.Find("ScoreManager").GetComponent<ScoreManager>();
        foodSpawner = GameObject.Find("FoodSpawner").GetComponent<PetFoodSpawner>();
        roundEnd = false;


        //Check through every team in the "teams" Enum that was created
        foreach (teams teamName in Enum.GetValues(typeof(teams)))
        {
            object teamCount;
            //Try to get the value from the Hashtable and put it into 'teamCount'
            if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(teamName.ToString(), out teamCount))
            {
                //If the team is in the game and this 
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

                }

            }
        }


        //If the Hashtable for keeping track of the current round doesn't exist
        if (!PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("Round Number"))
        {
            //Initialise the table starting the team off with 0 rounds won
            Hashtable initRound = new Hashtable()
            {
                {"Round Number", 1 }
            };
            //Set the Custom Properties for the room so that it knows how many rounds each team has won
            PhotonNetwork.CurrentRoom.SetCustomProperties(initRound);
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
            else if (currRoundTime < 0 && !roundEnd)
            {
                roundEnd = true;
                CheckRoundWinner();
                
            }
        }


        int minutes = Mathf.FloorToInt(currRoundTime / 60);
        int seconds = Mathf.FloorToInt(currRoundTime % 60);
        timerTxt.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void NextRound() 
    {
        //Check what round number the game is in
        if (GetCurrentRound() <= 3)
        {
            //scoreManager.photonView.RPC("ResetScore", RpcTarget.All);
            roundEnd = false;
            //Increase Round Number
            IncreaseRoundNumProperty();
            Debug.Log("We are on Round " + GetCurrentRound());

        }
        else 
        {
            CheckGameWinner();
            //End Game
        }

        PhotonNetwork.LoadLevel(1);
    }

    #region After Timer ends
    //Function that is called after timer runs out
    public void CheckRoundWinner() 
    {
        int highestScore = 0;
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
                    //Debug.Log(teamName + "Added to winning list with score: "+ teamRoundScore);
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
                UpdateRoundProperties(keyName, num);
            }
        }

        //Clear the list because the next time it will be called it will need to be empty
        possibleWinners.Clear();

        //Start the next round
        NextRound();
    }

    #endregion

    //Function used when a team collects all the points
    public void IncreaseTeamRound(teams winningTeam) 
    {
        //The key for the winning team
        string keyName = winningTeam.ToString() + " Rounds";

        //Get the amount of rounds the winning team has got
        object roundsWon;
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(keyName, out roundsWon))
        {
            //Get the value and increment it by 1
            int num = (int)roundsWon;
            num++;

            //Update Room Property Hashtable
            UpdateRoundProperties(keyName, num);
        } 
        



        //Start the next round
        NextRound();

    }

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
    }

    void RemoveFromWinningTeams(int currHighScore)
    {
        //Check through all the possible winners
        foreach (teams teamName in possibleWinners.Keys)
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
    void UpdateRoundProperties(string key, int roundsWon) 
    {
        //Update Hashtable
        Hashtable properties = new Hashtable()
        {
                {key, roundsWon}
            };

        //Update the Hashtable that is being tracked by PUN
        PhotonNetwork.CurrentRoom.SetCustomProperties(properties);
    }
    
    //Get the current Round Number of the game
    int GetCurrentRound() 
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

    void CheckPossibleWinners()
    {

    }

    void SwapRoles() 
    {

    }

    void ResetRound() 
    {

    }

    void StartDeathMatch() 
    {

    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        //Check the properties that have changed in the Hashtable
        foreach (var prop in propertiesThatChanged)
        {
            if (PhotonNetwork.IsMasterClient) 
            {
                Debug.Log("Key: " + prop.Key + " = " + prop.Value);
            }
            
            //Try Convert the object to the teams Enum and store into variable called "teamName"
            //if (System.Enum.TryParse<teams>(prop.Key.ToString(), out teams teamName))
            //{
            //    //The key for the winning team
            //    string keyName = teamName.ToString() + " Rounds";
            //    if (prop.Key.ToString() == keyName) 
            //    {
            //        Debug.Log(keyName);
            //    }
            //    //If Successful update Team Text
            //    //SetTeamText(teamName, Convert.ToInt32(prop.Value));
            //}

            //SetTeamText((teams)prop.Key, (int)prop.Value);


        }


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
