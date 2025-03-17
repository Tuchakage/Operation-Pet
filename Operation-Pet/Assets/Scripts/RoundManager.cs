using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static teamsEnum;

public class RoundManager : MonoBehaviour
{
    private int currRoundNum;
    private Dictionary<teams, int> roundWon;
    private List<teams> possibleWinner;
    float maxRoundTime;
    float currRoundTime;
    TMP_Text timerTxt;
    TMP_Text roundTxt;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        CheckRoundWinner();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void NextRound() 
    {
        if (currRoundNum < 4) 
        {
            currRoundNum++;

            //Spawn food from Spawn Manager
        }
    }

    //Function that is called after timer runs out
    void CheckRoundWinner() 
    {

    }

    void CheckGameWinner() 
    {
        ////Variable used to compare each team score
        //int highestScore = 0;

        //teams currentHighestTeam = teams.Unassigned;

        //foreach (teams teamName in roundWon.Keys)
        //{
        //    int teamRoundScore;
        //    if (roundWon.TryGetValue(teamName, out teamRoundScore))
        //    {
        //        //If the score of this team is
        //        if (teamRoundScore > highestScore)
        //        {
        //            //set this score to be highest
        //            highestScore = teamRoundScore;

        //            //Add this as someone who can participate in the death match if it happens
        //            possibleWinner.Add(teamName);
        //        }
        //        else if (teamRoundScore == highestScore) //If the score is the same as the highest
        //        {
        //        }
        //    }


        //}
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


}
