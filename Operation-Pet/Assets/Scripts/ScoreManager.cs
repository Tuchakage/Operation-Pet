using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using static teamsEnum;

public class ScoreManager : MonoBehaviourPunCallbacks
{
    //Stores the scores for each team
    public Dictionary<teams, int> teamScores;

    //Store all the possible winners of the round
    private Dictionary<teams, int> possibleRoundWinner;
    public TMP_Text DogScoreTxt, CatScoreTxt, MouseScoreTxt, SquirrelScoreTxt, HorseScoreTxt;

    PetFoodSpawner foodSpawner;
    RoundManager roundManager;

    //Determines the score the teams need to get to end the round
    [SerializeField]
    int maxFoodPerTeam;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        teamScores = new Dictionary<teams, int>();
        roundManager = GameObject.Find("RoundManager").GetComponent<RoundManager>();

        
        //Make sure that all the text isn't visible
        DogScoreTxt.enabled = false;
        CatScoreTxt.enabled = false;
        MouseScoreTxt.enabled = false;
        SquirrelScoreTxt.enabled = false;
        HorseScoreTxt.enabled = false;
        //Check through every team in the "teams" Enum that was created
        foreach (teams teamName in Enum.GetValues(typeof(teams)))
        {
            object teamCount;
            if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(teamName.ToString(), out teamCount))
            {
                //If the team is in the game
                if ((int)teamCount > 1) 
                {
                    //Initialise the score
                    teamScores.Add(teamName, 0);
                    ShowScoreText(teamName);
                    SetTeamScoreTxt(teamName, 0);



                    //Debug.Log("Team Name: " + teamName + " Score: " + teamScores[teamName]);
                }

            }
            

        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [PunRPC]
    void IncreaseScore(teams teamThatScored) 
    {
        teamScores[teamThatScored]++;
        SetTeamScoreTxt(teamThatScored, teamScores[teamThatScored]);


        //If team has reached the maximum score they can get
        if (teamScores[teamThatScored] == maxFoodPerTeam) 
        {
            Debug.Log("Round Winner is " + teamThatScored.ToString());
            //
            roundManager.IncreaseTeamRound(teamThatScored);
        }

        //Check if Next Round should be called
        Debug.Log("Increased Score");
    }


    void SetTeamScoreTxt(teams teamScoreUpdate, int score) 
    {
        switch (teamScoreUpdate) 
        {
            case teams.Dog:

                DogScoreTxt.text = "Dog Score: " + score;
                break;

            case teams.Cat:
                CatScoreTxt.text = "Cat Score: " + score;
                break;

            case teams.Mouse:
                MouseScoreTxt.text = "Mouse Score: " + score;
                break;

            case teams.Squirrel:
                SquirrelScoreTxt.text = "Squirrel Score: " + score;
                break;

            case teams.Horse:
                HorseScoreTxt.text = "Horse Score: " + score;
                break;
        }
    }

    void ShowScoreText(teams teamName) 
    {
        switch (teamName)
        {
            case teams.Dog:

                DogScoreTxt.enabled = true;
                break;

            case teams.Cat:
                CatScoreTxt.enabled = true;
                break;

            case teams.Mouse:
                MouseScoreTxt.enabled = true;
                break;

            case teams.Squirrel:
                SquirrelScoreTxt.enabled = true;
                break;

            case teams.Horse:
                HorseScoreTxt.enabled = true;
                break;
        }
    }

    [PunRPC]
    void ResetScore() 
    {
        foreach (teams teamName in teamScores.Keys)
        {
            Debug.Log("Resetting Team " + teamName);
            //int teamScore;
            //if (teamScores.TryGetValue(teamName, out teamScore))
            //{
            //    teamScores[teamName] = 0;
            //}
        }
    }

    public void SetMaxFoodPerTeam(int currentRound) 
    {
        Debug.Log("Round Number is " + currentRound);
        if (currentRound <= 4)
        {
            foodSpawner = GameObject.Find("FoodSpawner").GetComponent<PetFoodSpawner>();
            //Get the amount of points each team needs to collect
            maxFoodPerTeam = foodSpawner.Init();
        }
        else //We are in a deathmatch
        {
            foodSpawner = GameObject.Find("FoodSpawner").GetComponent<PetFoodSpawner>();
            foodSpawner.SpawnFood();
            //Teams only need to collect one food which is the deathmatch food
            maxFoodPerTeam = 1;
        }

    }
}
