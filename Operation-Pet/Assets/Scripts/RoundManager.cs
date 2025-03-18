using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static teamsEnum;
using Photon.Pun;
using Photon.Realtime;
using System.IO;
using System;

public class RoundManager : MonoBehaviourPunCallbacks, IPunObservable
{
    private int currRoundNum;
    private Dictionary<teams, int> roundWon;


    //Store all the possible winners of the round
    private Dictionary<teams, int> possibleWinners;

    [SerializeField]private float maxRoundTime;
    [SerializeField]private float currRoundTime;

    //Determines whether the round has ended
    private bool roundEnd;

    public TMP_Text timerTxt;
    public TMP_Text roundTxt;

    ScoreManager scoreManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Create Dictionarys
        roundWon = new Dictionary<teams, int>();
        possibleWinners = new Dictionary<teams, int>();

        currRoundNum = 1;
        scoreManager = GameObject.Find("ScoreManager").GetComponent<ScoreManager>();
        roundEnd = false;


        //Check through every team in the "teams" Enum that was created
        foreach (teams teamName in Enum.GetValues(typeof(teams)))
        {
            object teamCount;
            if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(teamName.ToString(), out teamCount))
            {
                //If the team is in the game
                if ((int)teamCount > 1)
                {
                    //Initialise the amount of rounds they won
                    roundWon.Add(teamName, 0);
                }

            }
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

    void NextRound() 
    {
        //Check what round number the game is in
        if (currRoundNum < 3)
        {
            scoreManager.photonView.RPC("ResetScore", RpcTarget.All);
            currRoundTime = maxRoundTime;
            roundEnd = false;         
            currRoundNum++;
            Debug.Log("We are on Round " + currRoundNum);

            //Spawn food from Spawn Manager
        }
        else 
        {
            CheckGameWinner();
            //End Game
        }
    }

    //Function that is called after timer runs out
    void CheckRoundWinner() 
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
                    Debug.Log(teamName + "Added to winning list with score: "+ teamRoundScore);

                    //Add this as a team that could potentially be the winner of the game
                    possibleWinners.Add(teamName, teamRoundScore);
                    
                    //If there are multiple round winners
                    if (possibleWinners.Count > 1)
                    {
                        //Check all their values and if they are lower than the current Highest Score then remove them from the list
                        RemoveFromWinningTeams(highestScore);
                    }

                }
                else if (teamRoundScore == highestScore) //If the score is the same as the highest
                {
                    //Add the team to the list
                    possibleWinners.Add(teamName, highestScore);
                }
            }


        }

        //After sorting out the teams with the highest score, Increase the amount of rounds they have won
        IncreaseRoundWon();
    }

    void CheckGameWinner() 
    {

        //Variable used to compare each team score
        int highestScore = 0;


        //Check all the values of the team in the dictionary
        foreach (teams teamName in roundWon.Keys)
        {
            int teamRoundScore;
            if (roundWon.TryGetValue(teamName, out teamRoundScore))
            {
                //If the score of this team is higher than the highest
                if (teamRoundScore > highestScore)
                {
                    //set this score to be highest
                    highestScore = teamRoundScore;

                    //Add this as a team that could potentially be the winner of the game
                    possibleWinners.Add(teamName, teamRoundScore);

                    //If there are multiple round winners
                    if (possibleWinners.Count > 1)
                    {
                        //Check all their values and if they are lower than the current Highest Score then remove them from the list
                        RemoveFromWinningTeams(highestScore);
                    }

                }
                else if (teamRoundScore == highestScore) //If the score is the same as the highest
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

    void IncreaseRoundWon() 
    {
        //For each team that could be possible winners due to having the highest score
        foreach (var teamName in possibleWinners.Keys) 
        {
            //Increase the amount of Rounds that team has won
            roundWon[teamName]++;
        }

        //Clear the list because the next time it will be called it will need to be empty
        possibleWinners.Clear();

        //Start the next round
        NextRound();
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
