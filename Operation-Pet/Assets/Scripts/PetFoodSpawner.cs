using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static teamsEnum;

public class PetFoodSpawner : MonoBehaviour
{
    Dictionary<teams, GameObject> petFoodList;
    List<string> teamsInGame;
    List<GameObject> spawnPointList; //This list will store all the spawnpoints in the game and won't delete any

    int amntTeamsinGame;
    int foodPerTeam;

    public GameObject[] foodToSpawn;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        petFoodList = new Dictionary<teams, GameObject>();
        teamsInGame = new List<string>();
        spawnPointList = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //Finds all the spawnpoints and tells the Round Manager how much food each team need to collect
    public int Init()
    {
        //Find all the Spawnpoints in the scene and store them into an array
        spawnPointList = GameObject.FindGameObjectsWithTag("Spawnpoints").ToList<GameObject>();

        //Get the amount of spawnpoints in the list
        int spawnpointAmnt = spawnPointList.Count;

        //This will be the amount of food each team will need to collect
        foodPerTeam = spawnpointAmnt / CountTeams();

        return foodPerTeam;
    }

    public void SpawnFood(bool isFake = false) 
    {

        // This is a temporary list so that we can store each individual food and make sure food don't spawn in the same place
        List<GameObject> temporaryspawnPointList = spawnPointList;


        for (int i = 0; i < temporaryspawnPointList.Count; i++) 
        {
            //Select a random Spawnpoint to spawn the food on
            GameObject randomSpawnpoint = ChooseRandomSpawnpoint(temporaryspawnPointList);
            PhotonNetwork.Instantiate(foodToSpawn[0].name, randomSpawnpoint.transform.position, Quaternion.identity, 0);
        }



        

        //Let the master Client spawn food for everyone so everything spawns in the same place for everyone
    }

    int CountTeams() 
    {
        amntTeamsinGame = 0;

        //Check through every team in the "teams" Enum that was created
        foreach (teams teamName in Enum.GetValues(typeof(teams)))
        {
            object teamCount;
            //Check the custom property of the room to see how many players are in each team
            if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(teamName.ToString(), out teamCount))
            {
                //If there is more than one player in a team that means that team is active in the game
                if ((int)teamCount > 1) 
                {
                    amntTeamsinGame++;
                }
            }
        }
        return amntTeamsinGame;
    }

    void FindSpawnPoints() 
    {

    }

    GameObject ChooseRandomSpawnpoint(List<GameObject> SpawnPointList) 
    {
        //Choose a Random spawnpoint from the list
        int randomIndex = UnityEngine.Random.Range(0, SpawnPointList.Count);

        //Keep hold of the random spawnpoint
        GameObject randomPoint = SpawnPointList[randomIndex];

        //Remove from the list
        SpawnPointList.Remove(randomPoint);
        return randomPoint;
    }

}
