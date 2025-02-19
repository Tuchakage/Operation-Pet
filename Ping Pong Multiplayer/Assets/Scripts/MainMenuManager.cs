using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Unity.Burst.Intrinsics.X86;

public class MainMenuManager : MonoBehaviourPunCallbacks, IPunObservable
{
    //Variable that is shared between all instances
    public static MainMenuManager menuInstance;

    string gameVersion = "0.9";


    //This will be the child named "Content" from the scroll view
    [Header("UI")] public Transform roomListParent;
    public GameObject roomListItemPrefab;

    //Stores the List of Rooms
    List<RoomInfo> createdRooms = new List<RoomInfo>();

    public GameObject canvas;

    public GameObject roomListPanel;
    public GameObject createRoomBtn;
    public GameObject roomInfo;

    //Used to turn on the indicator to show that a player is ready
    public GameObject pOneReadyCircle;
    public GameObject pTwoReadyCircle;



    Vector2 roomListScroll = Vector2.zero;
    bool joiningRoom = false;

    [SerializeField]
    private TMP_InputField roomName;

    [SerializeField]
    private TMP_InputField playerName;

    [SerializeField]
    private TMP_Text amntPlayerstxt;

    [SerializeField]
    private GameObject playerCardPrefab;



    //Variables used for keeping count how many players are ready in the lobby
    private int maxReadyPlayers;
    private int currentReadyPlayers;

    //Indicates whether they are P1 Or P2
    private int playerNumber;

    //Holds the names of the Player
    public string pnameOne;
    public string pnameTwo;

    //Keep a hold of all the Player cards in the room
    private Dictionary<int, GameObject> playersInRoom;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        menuInstance = this;
        currentReadyPlayers = 0;
        maxReadyPlayers = 2;

        //Makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same rom sync their level automatically
        PhotonNetwork.AutomaticallySyncScene = true;

        if (!PhotonNetwork.IsConnected)
        {

            //Set the App version before connecting
            PhotonNetwork.PhotonServerSettings.AppSettings.AppVersion = gameVersion;
            //Connect to photon master-server. Uses the settings saved in PhotonServerSettings (An asset file in project)
            PhotonNetwork.ConnectUsingSettings();

            if (photonView.IsMine)
            {
                //Debug.Log("Player ID:" + photonView.ViewID);
            }

        }


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

    #region Photon Callbacks
    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("OnFailedToConnectToPhoton. StatusCode: " + cause.ToString() + "ServerAddress: " + PhotonNetwork.ServerAddress);
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("OnConnectedToMaster");
        Debug.Log("Connection made to " + PhotonNetwork.CloudRegion + "server.");

        //After we connect to Master server, join the lobby
        PhotonNetwork.JoinLobby(TypedLobby.Default);
    }


    // roomList variable will already be populated automatically by Photon (Only sends things that change)
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {

        //Debug.Log("Room Amount " + roomList.Count);
        createdRooms = roomList;

        if (createdRooms.Count <= 0) //Create Room list
        {
            //After this callback, the room list is being updated into "createdRooms" variable
            createdRooms = roomList;
            Debug.Log("Populating List");
        }
        else //Updating changes in room list
        {

            foreach (RoomInfo room in roomList)
            {
                Debug.Log(room.Name);
                for (int i = 0; i < createdRooms.Count; i++)
                {
                    //If the name of the room is the same as the room stored in the Pun classes
                    if (createdRooms[i].Name == room.Name)
                    {
                        //Temporary List so we can modify the main list
                        List<RoomInfo> newList = createdRooms;

                        if (room.RemovedFromList) //If the room has been removed from the list
                        {
                            newList.Remove(newList[i]);
                        }
                        else //If the room changed
                        {
                            //Debug.Log("Room has changed" + room);
                            //tempList[i] = room;
                            //Debug.Log(newList[i]);

                        }

                        //createdRooms = tempList;
                    }
                }
            }
        }

        UpdateUI();

    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene(0);
    }

    void UpdateUI()
    {
        //Destroy all the Rooms displayed
        foreach (Transform roomItem in roomListParent)
        {
            Destroy(roomItem.gameObject);
        }


        //Add Rooms
        foreach (var room in createdRooms)
        {
            GameObject roomItem = Instantiate(roomListItemPrefab, roomListParent);

            //Set the text of the Child Text Objects
            roomItem.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = room.Name;
            roomItem.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = room.PlayerCount + "/2";

        }
    }


    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        
        roomListPanel.SetActive(false);
        createRoomBtn.SetActive(false);
        roomInfo.SetActive(true);

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

            GameObject playerCard = Instantiate(playerCardPrefab);

            GameObject playerCardGroup = GameObject.Find("Player Card Group");

            if (playerCardGroup) 
            {
                playerCard.transform.SetParent(playerCardGroup.transform);
            }
            

            playerCard.transform.localPosition = new Vector3(0, 0, 0);

            //Initialise Player card
            playerCard.GetComponent<PlayerCardEntry>().Init(p.ActorNumber, p.NickName);

            object isPlayerReady;

            //Set the Ready status by Trying to get the value from the key "Player Ready"
            if (p.CustomProperties.TryGetValue("Player Ready", out isPlayerReady))
            {
                //Set the ready status
                playerCard.GetComponent<PlayerCardEntry>().SetReadyStatus((bool) isPlayerReady);
            }

            //Add to list
            playersInRoom.Add(p.ActorNumber, playerCard);

            Debug.Log("Actor Name: " + p.NickName + " Player Actor Number: " + p.ActorNumber);
        }
        

    }


    public override void OnJoinRoomFailed(short returnCode, string message)
    {

        joiningRoom = false;

        //Disable player name input field
        playerName.gameObject.SetActive(true);
        roomName.gameObject.SetActive(true);


    }
    public override void OnPlayerEnteredRoom(Player other)
    {
        //Not seen if you're the player joining 
        Debug.LogFormat("OnPlayerEnteredRoom() {0}", other.NickName);

        GameObject playerCard = Instantiate(playerCardPrefab);

        GameObject playerTwoSpawnpoint = GameObject.Find("Player 2 Name Card Spawnpoint");
        playerCard.transform.SetParent(playerTwoSpawnpoint.transform);
        playerCard.transform.localPosition = new Vector3(0, 0, 0);
        playerCard.GetComponent<PlayerCardEntry>().Init(other.ActorNumber, other.NickName);

        //Add to list
        playersInRoom.Add(other.ActorNumber, playerCard);
    }

    public override void OnPlayerLeftRoom(Player other)
    {
        Debug.LogFormat("OnPlayerLeftRoom() {0}", other.NickName); // seen when other disconnects

        GameObject playerCard;
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
        
        Debug.Log("My Actor Number: " + PhotonNetwork.LocalPlayer.ActorNumber);
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
            
        }

        //Call Check Ready Players
        if (CheckReadyPlayers()) 
        {
            StartGame();
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
        if (playerName.text == "")
        {
            // If player name is empty then dont do anything

            return;
        }

        if (roomName.text == "") //If Room Name is empty
        {
            //Set a default name that includes the player name
            roomName.text = playerName.text + "'s Room";
        }

        //Player has joined the room
        joiningRoom = true;

        //Shows the Room as an option to join in lobby list
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.IsOpen = true;
        roomOptions.IsVisible = true;
        roomOptions.MaxPlayers = 2;

        //canvas.SetActive(false);

        PhotonNetwork.CreateRoom(roomName.text, roomOptions, TypedLobby.Default);

        //Disable player name input field
        playerName.gameObject.SetActive(false);
        roomName.gameObject.SetActive(false);
    }

    public void JoinRoom(string RoomName)
    {

        if (playerName.text == "")
        {
            // If player name is empty then dont do anything

            return;
        }


        joiningRoom = true;


        //canvas.SetActive(false);

        PhotonNetwork.JoinRoom(RoomName);

        //Disable player name input field
        playerName.gameObject.SetActive(false);
        roomName.gameObject.SetActive(false);
    }

    public void LeaveRoom() 
    {
        PhotonNetwork.LeaveRoom();

        //Rest of functionality will be in the PUN Callback "OnPlayerLeaves"
    }
    void RefreshRooms()
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

    #endregion
    public void StartGame()
    {

        //If not a master client then don't load the level (PhotonNetwork.AutomaticallySyncScene, will make everyone in the room load the level)
        if (!PhotonNetwork.IsMasterClient) 
        {
            Debug.Log("test");
            Debug.LogError("Only master client can load the level");
            return;
        }

        Debug.Log("Loading Level");
        //Go To The Actual Game
        PhotonNetwork.LoadLevel(1);

        //Make sure no one can join whilst game is going on
        PhotonNetwork.CurrentRoom.IsVisible = false;
        PhotonNetwork.CurrentRoom.IsOpen = false;
    }

    //Function used to keep track of how many players have readied up
    public void SetReadyPlayer(bool isReady)
    {
        if (isReady)
        {
            currentReadyPlayers++;
        }
        else
        {
            currentReadyPlayers--;
        }
    }

    //Function to check if the players are ready (Uses Photons Custom Properties)
    bool CheckReadyPlayers() 
    {
        //If there is more than one player
        if (PhotonNetwork.CurrentRoom.PlayerCount > 1) 
        {
            //Make sure only the Master Client is checking which players are ready
            if (!PhotonNetwork.IsMasterClient)
            {
                return false;
            }

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
