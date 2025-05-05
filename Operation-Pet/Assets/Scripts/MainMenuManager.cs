using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static TeamManager;
using static teamsEnum;
using static Unity.Burst.Intrinsics.X86;

public class MainMenuManager : MonoBehaviourPunCallbacks, IPunObservable
{
    #region Manager Scripts
    //Variable that is shared between all instances
    public static MainMenuManager menuInstance;
    TeamManager teamManager;
    MapManager mapManager;
    #endregion

    #region Int Variables

    #endregion


    [Header("General")]
    string gameVersion = "0.9";
    //Variables used for keeping count how many players are ready in the lobby
    public int maxPlayers;
    private int currentReadyPlayers;
    [SerializeField]
    private GameObject playerCardPrefab;




    [Header("Room List")]

    public Transform roomListParent;
    public GameObject roomListItemPrefab;
    [SerializeField]
    private TMP_Text amntPlayerstxt;

    //Stores the List of Rooms
    private Dictionary<string, RoomInfo> cachedRoomList;
    //Keep a hold of all the Player cards in the room
    private Dictionary<int, GameObject> playersInRoom;


    [SerializeField]
    private TMP_InputField roomName;
    [SerializeField]
    private TMP_Text roomNameDisplayText;

    //This will be the child named "Content" from the scroll view
    [Header("UI")]
    [SerializeField]
    private TMP_InputField nameInputField;

    public GameObject canvas;
    public GameObject lobbyPanel;
    public GameObject roomInfo;
    public GameObject buttonPanel; //Variable storing all the Buttons in the main menu so we can activate them when connected to Photon

    public Button signOutBtn;
    public Button statsBtn;

    [Header("UserData Display")]
    public TMP_Text matchesPlayedtext;
    public TMP_Text matchesWontext;
    public TMP_Text playernameforStats;

    [SerializeField]
    private TMP_Text playerUsername;






    //List<RoomInfo> cachedRoomList = new List<RoomInfo>();
    #region Debug Variables
    [Header("Debug Variables")]
    public TMP_Text regionTxt;

    
    [SerializeField]
    private TMP_Text playerErrorMessage; //Variable used to display error
    #endregion





    #region Unity

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        buttonPanel.SetActive(false);
        //Depending on whether the player is playing without an account or not determines whether the input field for player name shows up
        nameInputField.gameObject.SetActive(FirebaseManager.Instance.playWithoutAccount);

        cachedRoomList = new Dictionary<string, RoomInfo>();
        menuInstance = this;
        currentReadyPlayers = 0;

        lobbyPanel = GameObject.Find("Lobby Panel");
        teamManager = GameObject.Find("TeamManager").GetComponent<TeamManager>();
        mapManager = GameObject.Find("MapManager").GetComponent<MapManager>();
        //Make sure the stats button is not visible
        statsBtn.gameObject.SetActive(false);

        //Makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same rom sync their level automatically
        PhotonNetwork.AutomaticallySyncScene = true;

        if (!PhotonNetwork.IsConnected)
        {

            //Set the App version before connecting
            PhotonNetwork.PhotonServerSettings.AppSettings.AppVersion = gameVersion;
            //Connect to photon master-server. Uses the settings saved in PhotonServerSettings (An asset file in project)
            PhotonNetwork.ConnectUsingSettings();
        }
        else //If already connected
        {
            buttonPanel.SetActive(true);
            Debug.Log("Already Connected");
            if (regionTxt)
            {
                regionTxt.text = "Region = " + PhotonNetwork.CloudRegion;
            }

            //If player decided to log in with an account
            if (!nameInputField.gameObject.activeSelf)
            {
                //Set Photon Nickname to be the same as signed in Username
                PhotonNetwork.NickName = FirebaseManager.Instance.User.DisplayName;

                //Display Username on screen
                playerUsername.text = PhotonNetwork.NickName;
                //Debug.Log("Name = " + PhotonNetwork.NickName);

                //Allow the Stats button to appear
                statsBtn.gameObject.SetActive(true);
            }

            //Make sure Roles for players are reset
            Hashtable playerProp = new Hashtable()
            {
            {"Role Name",  roles.Unassigned}
            };
            //Debug.Log("Team Counter =" +teamCounter + " For "+team);
            //Set the Custom Properties for the room so that it knows how many players are in each team
            PhotonNetwork.LocalPlayer.SetCustomProperties(playerProp);
        }



        signOutBtn.onClick.AddListener(FirebaseManager.Instance.SignOutButton);

        FirebaseManager.Instance.onLoginScene = false;
    }

    // Update is called once per frame
    void Update()
    {
        //If we are in a room
        if (PhotonNetwork.InRoom)
        {
            amntPlayerstxt.text = PhotonNetwork.CurrentRoom.PlayerCount + " / " + PhotonNetwork.CurrentRoom.MaxPlayers;

            //if (currentReadyPlayers == maxReadyPlayers)
            //{
            //    Debug.Log("Hello");
            //}
        }
    }

    #endregion

    #region Photon Callbacks

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("OnFailedToConnectToPhoton. StatusCode: " + cause.ToString() + "ServerAddress: " + PhotonNetwork.ServerAddress);
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("OnConnectedToMaster");
        Debug.Log("Connection made to " + PhotonNetwork.CloudRegion + "server.");


        buttonPanel.SetActive(true);
        if (regionTxt)
        {
            regionTxt.text = "Region = " + PhotonNetwork.CloudRegion;
        }

        //If player decided to log in with an account
        if (!nameInputField.gameObject.activeSelf)
        {
            //Set Photon Nickname to be the same as signed in Username
            PhotonNetwork.NickName = FirebaseManager.Instance.User.DisplayName;

            //Display Username on screen
            playerUsername.text = PhotonNetwork.NickName;
            //Debug.Log("Name = " + PhotonNetwork.NickName);

            //Allow the Stats button to appear
            statsBtn.gameObject.SetActive(true);
        }

        //After we connect to Master server, join the lobby
        PhotonNetwork.JoinLobby(TypedLobby.Default);
    }

    // roomList variable will already be populated automatically by Photon (Only sends things that change)
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        //Debug.Log("Room Amount " + roomList.Count);
        ClearRoomListView();
        
        //Keep track of the new rooms
        UpdateCachedRoomList(roomList);

        //Update room player cards
        UpdateRoomListView();

    }

    public override void OnLeftRoom()
    {
        //teamManager.JoinUnassigned();
        SceneManager.LoadScene(1);
        Debug.Log("Left Room");
    }


    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        UIManager.Instance.ShowRoomInfoScreen();

        //Debug.Log("Player Name is " + playerName.text);

        if (playersInRoom == null)
        {
            playersInRoom = new Dictionary<int, GameObject>();
        }

        //When player joins a room it will check for all the players in the room and spawn in player card
        foreach (Player p in PhotonNetwork.PlayerList)
        {
            //Spawn in Player Card

            if (p.IsMasterClient) 
            {
                Debug.Log("Master Client is "+ p.NickName);
            }

            SpawnPlayerCards(p);
        }

        roomNameDisplayText.text = PhotonNetwork.CurrentRoom.Name;

    }


    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        //Disable room name input field
        roomName.gameObject.SetActive(true);


    }

    //Not called if you're the player joining 
    public override void OnPlayerEnteredRoom(Player other)
    {
        //Not seen if you're the player 
        SpawnPlayerCards(other);
    }

    public override void OnPlayerLeftRoom(Player other)
    {
        Debug.LogFormat("OnPlayerLeftRoom() {0}", other.NickName); // seen when other disconnects

        

        GameObject playerCard;
        //Try to get Player Card value from Hash table and put it in playerCard variable
        if (playersInRoom.TryGetValue(other.ActorNumber, out playerCard)) 
        {

            object isPlayerReady;
            //Check the custom properties of that player to see if it has the "isPlayerReady" property
            if (other.CustomProperties.TryGetValue("Player Ready", out isPlayerReady)) 
            {
                //if they were readied up
                if ((bool)isPlayerReady) 
                {
                    currentReadyPlayers--;
                }
            }

            //Remove Player card from dictionairy
            playersInRoom.Remove(other.ActorNumber);

            //Destroy the player card
            Destroy(playerCard);

            
        }

        //Check if Game can start when a player leaves (They might be the reason the game can't start
        if (teamManager.CheckReadyTeams() && CheckReadyPlayers())
        {
            StartCoroutine(StartGame());
            Debug.Log("Start the level");
        }

    }

    //Called when there is a change to a Players custom property
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        GameObject playerCard;
        //Go into the Player Card Game Object of the target player (If it exists)

        if (playersInRoom.TryGetValue(targetPlayer.ActorNumber, out playerCard))
        {
            object isPlayerReady;
            //Access the Hash table and get the value
            if (changedProps.TryGetValue("Player Ready", out isPlayerReady)) 
            {
                playerCard.GetComponent<PlayerCardEntry>().SetReadyStatus((bool)isPlayerReady);
            }
            
        };

        //Check if the Game can start when someone readies up
        if (PhotonNetwork.IsMasterClient && teamManager.CheckReadyTeams() && CheckReadyPlayers()) 
        {
            //Tell each player to go into the database and increase the amount of matches they have played by 1
            photonView.RPC("IncreaseMatchPlayedForAll", RpcTarget.All);

            StartCoroutine(StartGame());
            Debug.Log("Start the level");

        }
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        Debug.Log("New Master Client is " + newMasterClient.NickName);
        //if (PhotonNetwork.LocalPlayer.ActorNumber == newMasterClient.ActorNumber)
        //{
        //   
        //}
    }




    #endregion

    #region Button Functions
    public void CreateRoom()
    {
        if (!PhotonNetwork.IsConnectedAndReady) 
        {
            Debug.LogError("Not connected to Photon Network");
            return;
        }

        //If Name Input field is active && Player name is empty
        if (nameInputField.gameObject.activeInHierarchy && nameInputField.text == "")
        {
            //Show error message
            StartCoroutine(ShowErrorMessage());

            Debug.LogError("Empty Name");
            return;
        }


        if (roomName.text == "") //If Room Name is empty
        {
            //Set a default name that includes the player name
            roomName.text = PhotonNetwork.NickName + "'s Room";
        }

        //Shows the Room as an option to join in lobby list
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.IsOpen = true;
        roomOptions.IsVisible = true;
        roomOptions.MaxPlayers = maxPlayers;

        //canvas.SetActive(false);

        PhotonNetwork.CreateRoom(roomName.text, roomOptions, TypedLobby.Default);

        //Disable Lobby Panel
        //lobbyPanel.SetActive(false);
    }

    public void JoinRoom(string RoomName)
    {

        //If Name Input field is active && Player name is empty
        if (nameInputField.gameObject.activeInHierarchy && nameInputField.text == "")
        {
            //Show error message
            StartCoroutine(ShowErrorMessage());

            return;
        }

        //canvas.SetActive(false);

        PhotonNetwork.JoinRoom(RoomName);

        //Disable Lobby Panel
        //lobbyPanel.SetActive(false);
    }

    public void LeaveRoom() 
    {
        teamManager.DecreaseTeamCount();
        TeamManager.AddPlayerToTeam(teams.Unassigned);

        //Reset Properties
        Hashtable properties = new Hashtable(){
            {"Player Ready", false}
        };

        //Update the Hashtable that is being tracked by PUN
        PhotonNetwork.LocalPlayer.SetCustomProperties(properties);


        PhotonNetwork.LeaveRoom();

        //Rest of functionality will be in the PUN Callback "OnPlayerLeaves"
    }


    public void RefreshRooms()
    {
        if (PhotonNetwork.IsConnected)
        {
            //Re-join lobby to get the latest Room List
            PhotonNetwork.JoinLobby(TypedLobby.Default);
        }
        else
        {
            //We are not connected, establish a new connection
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public void ShowStats() 
    {
        playernameforStats.text = PhotonNetwork.NickName + "'s Stats";
        matchesPlayedtext.text = "Matches Played: " + FirebaseManager.Instance.matchesplayed.ToString();
        matchesWontext.text = "Matches Won: " + FirebaseManager.Instance.matcheswon.ToString();
        UIManager.Instance.ShowStatScreen();
    }


    #endregion

    #region Room List Functions
    //Update the createdRooms Variable with the new Rooms
    private void UpdateCachedRoomList(List<RoomInfo> roomList)
    {
        //Check through every Room in the PUN list
        foreach (RoomInfo room in roomList) 
        {
            //Remove room from cached list if it goes closed
            if (room.RemovedFromList) 
            {
                //If Cached list has the room name
                if (cachedRoomList.ContainsKey(room.Name)) 
                {
                    cachedRoomList.Remove(room.Name);
                }

                //Skip to next iteration
                continue;
            }

            //If the cached Room List doesn't have the room stored
            if (!cachedRoomList.ContainsKey(room.Name))
            {
                //Add it to the list
                cachedRoomList.Add(room.Name, room);
            }
            else 
            {
                //Update the values
                cachedRoomList[room.Name] = room;
            }
        }
        
    }

    public void ClearRoomListView()
    {
        //Destroy all the Rooms displayed
        foreach (Transform roomItem in roomListParent)
        {
            Destroy(roomItem.gameObject);
            Debug.Log("Destroyed " + roomItem.gameObject.name);
        }

    }

    void UpdateRoomListView()
    {
        Debug.Log("Rooms in cached list = " + cachedRoomList.Count); 

        //Iterate over the Room info values
        foreach (var room in cachedRoomList.Values)
        {
            GameObject roomItem = Instantiate(roomListItemPrefab, roomListParent);
            //Set the text of the Child Text Objects
            roomItem.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = room.Name;
            roomItem.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = room.PlayerCount + "/"+room.MaxPlayers;

        }

    }

    #endregion

    public System.Collections.IEnumerator StartGame()
    {
        //Find and Disable Leave Room Button
        //If not a master client then don't load the level (PhotonNetwork.AutomaticallySyncScene, will make everyone in the room load the level)
        if (!PhotonNetwork.IsMasterClient) 
        {
            Debug.Log("test");
            Debug.LogError("Only master client can load the level");
            yield return null;
        }

        Debug.Log("Loading Level");

        // Wait a few seconds
        yield return new WaitForSeconds(1f);

        //One final check before starting the game
        if (teamManager.CheckReadyTeams() && CheckReadyPlayers())
        {
            //Make sure no one can join whilst game is going on
            PhotonNetwork.CurrentRoom.IsVisible = false;
            PhotonNetwork.CurrentRoom.IsOpen = false;


            //Select a random Map
            PhotonNetwork.LoadLevel(mapManager.SelectRandomMap());

            //Go To The Actual Game
            //PhotonNetwork.LoadLevel("Street");
        }

    }

    void SpawnPlayerCards(Player player) 
    {
        GameObject playerCard = Instantiate(playerCardPrefab);

        //GameObject playerCardGroup = GameObject.Find("Unassigned Player Card Group");

        //if (playerCardGroup)
        //{
        //    playerCard.transform.SetParent(playerCardGroup.transform);
        //}


        playerCard.transform.localPosition = new Vector3(0, 0, 0);
        playerCard.transform.localScale = new Vector3(1, 1, 1);

        //Initialise Player card
        playerCard.GetComponent<PlayerCardEntry>().Init(player.ActorNumber, player.NickName);

        object isPlayerReady;

        //Set the Ready status by Trying to get the value from the key "Player Ready"
        if (player.CustomProperties.TryGetValue("Player Ready", out isPlayerReady))
        {
            //Set the ready status
            playerCard.GetComponent<PlayerCardEntry>().SetReadyStatus((bool)isPlayerReady);
        }


        //Add the Player Card to list
        playersInRoom.Add(player.ActorNumber, playerCard);

        Debug.Log("Actor Name: " + player.NickName + " Player Actor Number: " + player.ActorNumber);
    }

    //Function to check if the players are ready (Uses Photons Custom Properties)
    bool CheckReadyPlayers() 
    {
        //If there is more than one player
        if (PhotonNetwork.CurrentRoom.PlayerCount > 1) 
        {
            //Check each players Custom Property if one of them is false then that means everyone is not ready
            foreach (Player p in PhotonNetwork.PlayerList)
            {

                //Variable to store the value from the key
                object isPlayerReady;

                //Try to get the value from the key "Player Ready"
                if (p.CustomProperties.TryGetValue("Player Ready", out isPlayerReady))
                {
                    //If the value from the key is false 
                    if (!(bool)isPlayerReady)
                    {
                        return false;
                    }
                }
                else //If the key doesn't exist
                {
                    return false;
                }
            }

            return true;
        }

        return false;

    }

    [PunRPC]
    void IncreaseMatchPlayedForAll() 
    {
        if (FirebaseManager.Instance.playWithoutAccount) 
        {
            return;
        }
        //Increase the amount of games the player has played
        StartCoroutine(FirebaseManager.Instance.UpdateMatchPlayedDatabase());
    }

    System.Collections.IEnumerator ShowErrorMessage() 
    {
        playerErrorMessage.text = "Player name is Empty";

        yield return new WaitForSeconds(1f);

        playerErrorMessage.text = "";
    }



    //This function allows the variables inside to be sent over the network (Used as Observed component in photon view, this reads/writes the variables)
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            //We own this player so send the other computers the data


        }
        else
        {
            //Network player that receives the data

        }
    }
}
