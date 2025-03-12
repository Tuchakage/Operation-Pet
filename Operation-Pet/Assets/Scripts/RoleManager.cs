using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using UnityEngine;
using UnityEngine.UI;
using static teamsEnum;

public class RoleManager : MonoBehaviourPunCallbacks
{

    public GameObject[] playerModels;
    public GameObject petSelectBtn;
    public GameObject wizardSelectBtn;

    private teamsEnum.teams myTeam;

    //Keep reference to your teammate
    Player teammate;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
        //Store the team that this player is on
        myTeam = GetTeam(PhotonNetwork.LocalPlayer);

        //Depending on what team the player is on change the colour of the button
        petSelectBtn.GetComponent<Image>().color = teamsEnum.ChangeColour(myTeam);
        SetTeammate();
        SetPlayerRole(roles.Unassigned);
    }

    public void SelectPet() 
    {


        //Is our teammate already the Pet?
        object teammateRole;
        if (teammate.CustomProperties.TryGetValue("Role Name", out teammateRole))
        {

            if ((roles)teammateRole == roles.Pet)
            {
                return;
            }
        }

        //Find Spawnpoint for corresponding Team (The Gameobjects name would be like "Red Pet Spawnpoint")
        string spawnPointName = myTeam.ToString() + " " + roles.Pet.ToString() + " Spawnpoint";
        GameObject spawnPoint = GameObject.Find(spawnPointName);
        Debug.Log(spawnPointName);

        //Check player Model array and make sure you spawn in player of correct team and correct role
        GameObject playerModel = CheckModelToSpawn(GetTeam(PhotonNetwork.LocalPlayer), GetRole(PhotonNetwork.LocalPlayer));

        //Instantiate the Player and disable UI
    }

    public void SelectWizard() 
    {

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

    GameObject CheckModelToSpawn(teams teamName, roles roleName) 
    {
        //If the player is a Pet
        if (roleName.Equals(roles.Pet)) 
        {
            //Check which colour the model should be 
            switch (teamName) 
            {
                case teams.Red:
                    return playerModels[0];
                case teams.Blue:
                    return playerModels[1];

                case teams.Yellow:
                    return playerModels[2];

                case teams.Green:
                    return playerModels[3];

                case teams.Purple:
                    return playerModels[4];
            }
        }

        return null;
    }

}
