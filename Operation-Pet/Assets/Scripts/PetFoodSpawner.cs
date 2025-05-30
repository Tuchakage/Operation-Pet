using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static teamsEnum;

public class PetFoodSpawner : MonoBehaviour
{
    Dictionary<teams, int> foodSpawnedPerTeam; //Keeps count of how much food has been spawned for each team
    public List<GameObject> spawnPointList; //This list will store all the spawnpoints in the game and won't delete any
    public GameObject deathMatchSpawnPoint; //Spawnpoint for where the food for the deathmatch will spawn

    public GameObject[] foodToSpawn; // Food that will be spawned into the game depdending on Team (0 = Dog, 1 = Cat, 2 = Mouse, 3 = Squrirrel, 4= Horse)

    int amntTeamsinGame;
    int foodPerTeam; //The amount of Food each team will have to collect

    void Awake()
    {
        foodSpawnedPerTeam = new Dictionary<teams, int>();
        spawnPointList = new List<GameObject>();

        ResetFoodSpawned();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

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
        foodPerTeam = (spawnpointAmnt / CountTeams()) / 2;
        Debug.Log("Spawnpoint amnt = " + spawnpointAmnt);
        Debug.Log("Food Per team = " + foodPerTeam);
        //Spawn the Food in
        SpawnFood();

        return foodPerTeam;
    }

    public void SpawnFood(bool isFake = false) 
    {

        //Check the round number to see if it is a death match
        object roundNum;
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("Round Number", out roundNum))
        {
            //Normal Rounds
            if ((int)roundNum >= 0 && (int)roundNum <= 4)
            {
                List<GameObject> tempSpawnList = new List<GameObject>();
                tempSpawnList = GameObject.FindGameObjectsWithTag("Spawnpoints").ToList<GameObject>();

                //Get the amount of spawnpoints in the list
                int spawnpointAmnt = spawnPointList.Count;

                for (int i = 0; i < spawnpointAmnt; i++)
                {
                    //Debug.Log("Spawnpoint Count = " + tempSpawnList.Count + "\n Also i = "+i);
                    //Select a random Spawnpoint to spawn the food on
                    GameObject randomSpawnpoint = ChooseRandomSpawnpoint(tempSpawnList);

                    //Before Instantiating, Check the food that needs to be spawned in
                    GameObject food = PhotonNetwork.Instantiate("PetFood", randomSpawnpoint.transform.position, Quaternion.identity, 0);

                    //Check what teams food needs to be spawned in and increment the counter
                    teams foodForTeam = CheckActiveTeams(food);

                    //Depending on the team set the model for the food
                    food.GetPhotonView().RPC("CallSetFoodModel", RpcTarget.All, foodForTeam);




                }
            }
            else if ((int)roundNum > 4) //Deathmatch Round
            {
                GameObject food = PhotonNetwork.Instantiate("DeathmatchFood", deathMatchSpawnPoint.transform.position, Quaternion.identity, 0);
                //Make it so that anyone can pick up the food
                food.GetPhotonView().RPC("EveryoneCanPickUp", RpcTarget.All);

                RoundManager roundManager = GameObject.Find("RoundManager").GetComponent<RoundManager>();
                //Set the timer of the round
                roundManager.maxRoundTime = 30f;
                roundManager.hasDeathmatchStarted = true;
                Debug.Log("Deathmatch spawn");
            }
        }



        //After everything is spawned, amount of food spawned value per team to 0 so they can be respawned after round is finished
        //ResetFoodSpawned();



        //Let the master Client spawn food for everyone so everything spawns in the same place for everyone
    }

    [PunRPC]
    void SetFoodAsMine(GameObject foodToTurn) 
    {

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

    GameObject ChooseRandomSpawnpoint(List<GameObject> spawningList) 
    {
        //Choose a Random spawnpoint from the list
        int randomIndex = UnityEngine.Random.Range(0, spawningList.Count);
        //Keep hold of the random spawnpoint
        GameObject randomPoint = spawningList[randomIndex];

        //Remove from the list
        spawningList.Remove(randomPoint);
        return randomPoint;
    }

    //Function to set the amount of food spawned value per team to 0
    void ResetFoodSpawned()
    {
        //Check through every team in the "teams" Enum that was created
        foreach (teams teamName in Enum.GetValues(typeof(teams)))
        {
            object teamCount;
            if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(teamName.ToString(), out teamCount))
            {
                //If the team is in the game
                if ((int)teamCount > 1)
                {
                    //Initialise the amount of food spawned for each team
                    foodSpawnedPerTeam.Add(teamName, 0);
                }

            }
        }
    }

    //Purpose of this function is to increment how much of the food was spawned in per team
    teams CheckActiveTeams(GameObject foodRef) 
    {
        //Check each team in dictionary 'foodSpawnedPerTeam'
        foreach (teams team in foodSpawnedPerTeam.Keys) 
        {
            //Check if all food has been spawned in for that team
            int foodSpawned;
            //Try to get the value from the Dictionary and put it into 'foodSpawned
            if (foodSpawnedPerTeam.TryGetValue(team, out foodSpawned)) 
            {
                //Check if all the food for the team has been spawned in
                if (foodSpawned != foodPerTeam) 
                {

                    //Increase the amount of food spawned
                    foodSpawnedPerTeam[team]++;

                    //Debug.Log("Team " + team + " has " + foodSpawnedPerTeam[team] + " spawned");

                    //Return the team that was checked
                    return team;

                }
            }
        }

        //Set the food as a mine for everyone
        foodRef.GetPhotonView().RPC("SetAsMine", RpcTarget.All);
        return teams.Unassigned;
    }

}
