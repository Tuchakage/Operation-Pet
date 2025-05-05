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

    //Variables that store the ready buttons images
    [SerializeField] private GameObject ReadyImageBtn;
    [SerializeField] private GameObject UnreadyImageBtn;



    void Start()
    {
        //If this is not the Local Player
        if (PhotonNetwork.LocalPlayer.ActorNumber != ownerId) 
        {
            //Lose reference to the button and Ready Icon so it doesn't get activated
            ReadyImageBtn.GetComponent<Button>().enabled = false;
            UnreadyImageBtn.GetComponent<Button>().enabled = false;

            //Disable the button Image
            ReadyImageBtn.transform.GetChild(0).GetComponent<Image>().enabled = false;
            UnreadyImageBtn.transform.GetChild(0).GetComponent<Image>().enabled = false;
            return;
        }

        ReadyImageBtn.SetActive(false);
        UnreadyImageBtn.SetActive(true);
        //Set the OnClick() event for the Ready Button
        ReadyImageBtn.GetComponent<Button>().onClick.AddListener(ReadyUp);
        UnreadyImageBtn.GetComponent<Button>().onClick.AddListener(ReadyUp);


        //Initialise a Hashtable to use for the SetCustomProperties Function and set the player being ready to false
        UpdatePlayerReadyProp(false);

        //Debug.Log("Player: " + isPlayerReady);
    }
    public void Init(int playerId, string playerName)
    {
        ownerId = playerId;
        PlayerNameText.text = playerName;

    }

    public void ReadyUp() 
    {
        //Reverses what it currently is
        isPlayerReady = !isPlayerReady;

        SetReadyStatus(isPlayerReady);


        //Update Hash Table
        UpdatePlayerReadyProp(isPlayerReady);


        Debug.Log("Player Ready = " + isPlayerReady);

    }

    //Boolean parameter so other players can go into custom properties and assign it
    public void SetReadyStatus(bool playerReady)
    {
        if (ReadyImageBtn) 
        {
            ReadyImageBtn.SetActive(playerReady);
        }

        if (UnreadyImageBtn) 
        {
            UnreadyImageBtn.SetActive(!playerReady);
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
