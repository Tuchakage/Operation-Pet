using UnityEngine;
using UnityEngine.UI;
using TMPro;

using ExitGames.Client.Photon;
using Photon.Realtime;
using Photon.Pun;
using Photon.Pun.UtilityScripts;

//Script attached to Player Card to set the info

public class PlayerCardEntry : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text PlayerNameText;
    public int ownerId;
    public bool isPlayerReady = false;


    public GameObject readyBtn;
    //Used to turn on the indicator to show that a player is ready
    public Image ReadyCircleObject;

    void Start()
    {
        //If this is not the Local Player
        if (PhotonNetwork.LocalPlayer.ActorNumber != ownerId) 
        {
            //Lose reference to the button and Ready Icon so it doesn't get activated
            readyBtn = null;
            return;
        }
        //Set the OnClick() event for the Ready Button
        readyBtn.GetComponent<Button>().onClick.AddListener(ReadyUp);

        //Initialise a Hashtable to use for the SetCustomProperties Function and set the player being ready to false
        UpdatePlayerReadyProp(false);

        //Debug.Log("Player: " + isPlayerReady);
    }
    public void Init(int playerId, string playerName)
    {
        ownerId = playerId;
        PlayerNameText.text = playerName;
        readyBtn = GameObject.Find("ReadyBtn");

    }

    public void ReadyUp() 
    {
        //Reverses what it currently is
        isPlayerReady = !isPlayerReady;

        //Update Hash Table
        UpdatePlayerReadyProp(isPlayerReady);

        //Depending on what the "isPlayerReady" variable is set the text
        readyBtn.transform.GetChild(0).GetComponent<TMP_Text>().text = isPlayerReady ? "Unready" : "Ready";

        Debug.Log("Player Ready = " + isPlayerReady);

    }

    //Boolean parameter so other players can go into custom properties and assign it
    public void SetReadyStatus(bool playerReady) 
    {
        if (ReadyCircleObject) 
        {
            //Make Ready Circle Available Or Unavailable
            ReadyCircleObject.enabled = playerReady;
        }
    }

    public static void UpdatePlayerReadyProp(bool playerReady) 
    {
        //Update Hash Table
        Hashtable properties = new Hashtable(){
            {"Player Ready", playerReady}
        };

        //Update the Hashtable that is being tracked by PUN
        PhotonNetwork.LocalPlayer.SetCustomProperties(properties);
    }
}
