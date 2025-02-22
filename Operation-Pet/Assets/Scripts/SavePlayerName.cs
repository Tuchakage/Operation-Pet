using UnityEngine;
using UnityEngine.UI;

using Photon.Pun;
using Photon.Realtime;

using System.Collections;
using TMPro;

[RequireComponent(typeof(InputField))]
public class SavePlayerName : MonoBehaviour
{

    // Store the PlayerPref Key to avoid typos
    const string playerNamePrefKey = "PlayerName";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Variable that will hold the name in the player prefs
        string defaultName = string.Empty;

        TMP_InputField inputField = GetComponent<TMP_InputField>();

        if (inputField != null) 
        {
            //Check if Player Prefs has he key called "PlayerName"
            if (PlayerPrefs.HasKey(playerNamePrefKey)) 
            {
                //Set the name to what is stored in the player pref
                defaultName = PlayerPrefs.GetString(playerNamePrefKey);

                //Set the input field to the default name
                inputField.text = defaultName;
            }
        }

        PhotonNetwork.NickName = defaultName;
    }

    public void SetPlayerName(string value) 
    {

        //Check if the value is empty
        if (string.IsNullOrEmpty(value))
        {
            Debug.LogError("Player Name is null or empty");
            return;
        }

        PhotonNetwork.NickName = value;

        //Save the name in Player Prefs
        PlayerPrefs.SetString(playerNamePrefKey, value);
    }
}
