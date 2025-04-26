using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using static Photon.Pun.UtilityScripts.PunTeams;
using static teamsEnum;

public class TestRoleManager : MonoBehaviour
{
    public TeamModelScriptableObject PlayerModelScriptableObject;
    //public GameObject[] playerModels;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SelectPet()
    {
        teams myTeam = GetTeam(PhotonNetwork.LocalPlayer);



        //Find Spawnpoint for corresponding Team (The Gameobjects name would be like "Dog Pet Spawnpoint")
        string spawnPointName = myTeam.ToString() + " " + roles.Pet.ToString() + " Spawnpoint";
        GameObject spawnPoint = GameObject.Find(spawnPointName);

        //Check player Model array and make sure you spawn in player of correct team and correct role
        string playerModel = CheckModelToSpawn(myTeam, roles.Pet);

        //Instantiate the Player and disable UI
        GameObject roleSelectionPanel = GameObject.Find("Test Role Selection Panel");
        roleSelectionPanel.SetActive(false);

        PhotonNetwork.Instantiate(playerModel, spawnPoint.transform.position, Quaternion.identity, 0);
    }

    public void SelectWizard() 
    {

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
                    //Debug.Log(playerModelsScript.playerModels[0].name);
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
}
