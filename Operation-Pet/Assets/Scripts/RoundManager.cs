using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static teamsEnum;

public class RoundManager : MonoBehaviour
{
    private int currRoundNum;
    private Dictionary<teams, int> roundWon;
    float maxRoundTime;
    float currRoundTime;
    TMP_Text timerTxt;
    TMP_Text roundTxt;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
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

    void CheckRoundWinner() 
    {
        //Check the first element in the dictionary
        int highestScore = roundWon[0];
        foreach (int teamScore in roundWon.Values) 
        {
            
            
        }
    }

    void CheckGameWinner() 
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
