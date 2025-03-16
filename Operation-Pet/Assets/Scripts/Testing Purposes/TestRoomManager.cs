using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using static teamsEnum;
//The purpose of this script is to test things out in a room without having to go through the whole team select
public class TestRoomManager : MonoBehaviourPunCallbacks
{
    string gameVersion = "0.9";
    teamsEnum.teams teams;

    public GameObject testTeamSelectionPanel;
    public GameObject roleSelectionPanel;

    void Awake()
    {
        // #Critical
        // this makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same room sync their level automatically
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Connect();
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    public void Connect()
    {
        // we check if we are connected or not, we join if we are , else we initiate the connection to the server.
        if (PhotonNetwork.IsConnected)
        {
            // #Critical we need at this point to attempt joining a Random Room. If it fails, we'll get notified in OnJoinRandomFailed() and we'll create one.
            PhotonNetwork.JoinRandomRoom();
            Debug.Log("Join Room");
        }
        else
        {
            // #Critical, we must first and foremost connect to Photon Online Server.
            PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.PhotonServerSettings.AppSettings.AppVersion = gameVersion;
        }
    }

    #region PUN Callbacks

    public override void OnConnectedToMaster()
    {
        // #Critical: The first we try to do is to join a potential existing room. If there is, good, else, we'll be called back with OnJoinRandomFailed()
        PhotonNetwork.JoinRandomRoom();
        Debug.Log("PUN Basics Tutorial/Launcher: OnConnectedToMaster() was called by PUN");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarningFormat("PUN Basics Tutorial/Launcher: OnDisconnected() was called by PUN with reason {0}", cause);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("PUN Basics Tutorial/Launcher:OnJoinRandomFailed() was called by PUN. No random room available, so we create one.\nCalling: PhotonNetwork.CreateRoom");

        // #Critical: we failed to join a random room, maybe none exists or they are all full. No worries, we create a new room.
        PhotonNetwork.CreateRoom(null, new RoomOptions());
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Room");
    }

    #endregion

    #region Team Button Testing
    public void JoinDogTeam() 
    {
        //Change what team the player is on
        Hashtable properties = new Hashtable()
        {
            {"Team Name",  teams.Dog}
        };

        //Update the Hashtable that is being tracked by PUN to keep track of what team the player is on
        PhotonNetwork.LocalPlayer.SetCustomProperties(properties);

        testTeamSelectionPanel.SetActive(false);
        roleSelectionPanel.SetActive(true);
    }

    public void JoinCatTeam()
    {
        //Change what team the player is on
        Hashtable properties = new Hashtable()
        {
            {"Team Name",  teams.Cat}
        };

        //Update the Hashtable that is being tracked by PUN to keep track of what team the player is on
        PhotonNetwork.LocalPlayer.SetCustomProperties(properties);

        testTeamSelectionPanel.SetActive(false);
        roleSelectionPanel.SetActive(true);
    }

    public void JoinMouseTeam()
    {
        //Change what team the player is on
        Hashtable properties = new Hashtable()
        {
            {"Team Name",  teams.Mouse}
        };

        //Update the Hashtable that is being tracked by PUN to keep track of what team the player is on
        PhotonNetwork.LocalPlayer.SetCustomProperties(properties);

        testTeamSelectionPanel.SetActive(false);
        roleSelectionPanel.SetActive(true);
    }

    public void JoinSquirrelTeam()
    {
        //Change what team the player is on
        Hashtable properties = new Hashtable()
        {
            {"Team Name",  teams.Squirrel}
        };

        //Update the Hashtable that is being tracked by PUN to keep track of what team the player is on
        PhotonNetwork.LocalPlayer.SetCustomProperties(properties);

        testTeamSelectionPanel.SetActive(false);
        roleSelectionPanel.SetActive(true);
    }

    public void JoinHorseTeam()
    {
        //Change what team the player is on
        Hashtable properties = new Hashtable()
        {
            {"Team Name",  teams.Horse}
        };

        //Update the Hashtable that is being tracked by PUN to keep track of what team the player is on
        PhotonNetwork.LocalPlayer.SetCustomProperties(properties);

        testTeamSelectionPanel.SetActive(false);
        roleSelectionPanel.SetActive(true);
    }

    #endregion
}
