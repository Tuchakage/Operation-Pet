using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;


//Purpose of this script is to set the teams of the players before the game
public class TeamManager : MonoBehaviourPunCallbacks
{
    private string teamName = "Unassigned";
    Player localPlayer;
    void Start()
    {

        //Initialise a Hashtable first to use for the SetCustomProperties Function
        Hashtable properties = new Hashtable()
        {
            {"Team Name",  teamName}
        };
    }

    public override void OnJoinedRoom() 
    {
        localPlayer = PhotonNetwork.CurrentRoom.GetPlayer(PhotonNetwork.LocalPlayer.ActorNumber);
    }
    
    public void JoinRedTeam() 
    {
        teamName = "Red";

        //Update Hash Table 
        Hashtable properties = new Hashtable()
        {
            {"Team Name",  teamName}
        };

        //Update the Hashtable that is being tracked by PUN
        PhotonNetwork.LocalPlayer.SetCustomProperties(properties);



        //Get reference to the Red Team Group List
        Transform redTeamGroupList = GameObject.Find("Red Team ").transform;

        //Will be used to check the value of the Team Name property and it will be assigned
        object teamNameProperty;
        if (localPlayer.CustomProperties.TryGetValue("Team Name", out teamNameProperty))
        {
            Debug.Log("Has joined Team " + (string)teamNameProperty);
        }

        //Get reference to the Player Card


    }
}
