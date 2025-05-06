using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static teamsEnum;

public class RoleManager : MonoBehaviourPunCallbacks
{
    public TeamModelScriptableObject PlayerModelScriptableObject;

    public GameObject petSelectBtn;
    public GameObject wizardSelectBtn;
    public GameObject roleSelectionScreen;
    public GameObject wizardModel;

    private teamsEnum.teams myTeam;
    RoundManager roundManager;

    //Keep reference to your teammate
    Player teammate;

    //When this variable is set to true then that means the game has passed the first round so you dont get the option to choose the role and instead the roles swap
    bool initRole;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        initRole = false;
        //Store the team that this player is on
        myTeam = GetTeam(PhotonNetwork.LocalPlayer);

        //Depending on what team the player is on change the colour of the button
        petSelectBtn.GetComponent<Image>().color = teamsEnum.ChangeColour(myTeam);
        wizardSelectBtn.GetComponent<Image>().color = teamsEnum.ChangeColour(myTeam);
        SetTeammate();

        roundManager = GameObject.Find("RoundManager").GetComponent<RoundManager>();

        //If this is the first time the player is loading in
        if (!PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("Role Name"))
        {
            SetPlayerRole(roles.Unassigned);
        }
        
    }

    public void SelectPet() 
    {


        //Is our teammate already the Pet?
        //Check that its not the first time we are selecting a spawn
        object teammateRole;
        if (teammate.CustomProperties.TryGetValue("Role Name", out teammateRole))
        {

            if ((roles)teammateRole == roles.Pet && !initRole)
            {
                Debug.Log("Cant be the same role");
                return;
            }
        }

        //Find all the Food in the game
        List<GameObject> foodInGameList = new List<GameObject>();
        foodInGameList = GameObject.FindGameObjectsWithTag("Food").ToList<GameObject>();

        if (roundManager.GetCurrentRound() <= 4) 
        {
            //Foreach food that was found
            foreach (var food in foodInGameList)
            {
                //Set the food model to be the model for the Pets team (They can only see one type of food)
                food.GetComponent<PetFood>().SetMesh(myTeam);
                //If the food is a mine, change the material back to being for food
                food.GetComponent<PetFood>().RevertMaterial();
            }
        }


        //Find Spawnpoint for corresponding Team (The Gameobjects name would be like "Dog Pet Spawnpoint")
        string spawnPointName = myTeam.ToString() + " " + roles.Pet.ToString() + " Spawnpoint";
        GameObject spawnPoint = GameObject.Find(spawnPointName);

        //Check player Model array and make sure you spawn in player of correct team and correct role
        string playerModel = CheckModelToSpawn(myTeam, roles.Pet);

        //Instantiate the Player and disable UI
        roleSelectionScreen.SetActive(false);
        PhotonNetwork.Instantiate(playerModel, spawnPoint.transform.position, Quaternion.identity, 0);

    }

    public void SelectWizard() 
    {
        //Is our teammate already the Pet?
        object teammateRole;
        if (teammate.CustomProperties.TryGetValue("Role Name", out teammateRole))
        {

            if ((roles)teammateRole == roles.Wizard && !initRole)
            {
                Debug.Log("Cant be the same role");
                return;
            }
        }

        string spawnPointName = myTeam.ToString() + " " + roles.Wizard.ToString() + " Spawnpoint";
        GameObject spawnPoint = GameObject.Find(spawnPointName);

        //Instantiate the Player and disable UI
        roleSelectionScreen.SetActive(false);
        SetPlayerRole(roles.Wizard);
        PhotonNetwork.Instantiate(wizardModel.name, spawnPoint.transform.position, Quaternion.identity, 0);
    }

    Player SetTeammate() 
    {
        //Check each player in the Room 
        if (PhotonNetwork.PlayerList.Length > 0)
        {
            foreach (Player p in PhotonNetwork.PlayerList)
            {
                //Check if the player is not me and is in the same team
                if (p != PhotonNetwork.LocalPlayer && GetTeam(p) == myTeam) 
                {
                    //Keep reference to that player
                    teammate = p;
                    //Only need to look for that one player
                    break;
                }
            }
        }

        return teammate;
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

    roles GetRole(Player player) 
    {
        object roleName;
        //Check the custom properties of that player to see what team they are apart of
        if (player.CustomProperties.TryGetValue("Role Name", out roleName))
        {
            //Debug.Log("team = " + (teams)isInTeam);
            return (roles)roleName;
        }

        return roles.Unassigned;
    }

    //Function that will update the Player propertie to tell how many players are in each team
    void SetPlayerRole(teamsEnum.roles gameRole)
    {
        Hashtable playerProp = new Hashtable()
        {
            {"Role Name",  gameRole}
        };
        //Debug.Log("Team Counter =" +teamCounter + " For "+team);
        //Set the Custom Properties for the room so that it knows how many players are in each team
        PhotonNetwork.LocalPlayer.SetCustomProperties(playerProp);

    }

    string CheckModelToSpawn(teams teamName, roles roleName) 
    {
        //If the player is a Pet
        if (roleName.Equals(roles.Pet)) 
        {
            SetPlayerRole(roles.Pet);
            //Check which colour the model should be 
            switch (teamName) 
            {
                case teams.Dog:
                    Debug.Log(PlayerModelScriptableObject.playerModels[0].name);
                    return PlayerModelScriptableObject.playerModels[0].name;
                case teams.Cat:
                    Debug.Log(PlayerModelScriptableObject.playerModels[1].name);
                    return PlayerModelScriptableObject.playerModels[1].name;

                case teams.Mouse:
                    return PlayerModelScriptableObject.playerModels[2].name;

                case teams.Squirrel:
                    return PlayerModelScriptableObject.playerModels[3].name;

                case teams.Horse:
                    return PlayerModelScriptableObject.playerModels[4].name;
            }
        }

        return null;
    }

    public System.Collections.IEnumerator SwapRoles()
    {

        yield return new WaitForSeconds(1f);

        initRole = true;
        //Check to see if the player already has a role assigned
        object playerRole;
        if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("Role Name", out playerRole)) 
        {
            if ((roles)playerRole == roles.Pet)
            {
                SelectWizard();
                

            }
            else if ((roles)playerRole == roles.Wizard) 
            {

                SelectPet();
                
            }
        }
    }
}
