using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class SceneSwitcher : MonoBehaviour
{

    private void Awake()
    {

    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
 
        StartCoroutine(StartGame());



    }

    // Update is called once per frame
    void Update()
    {
        
    }

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

        //Go To The Actual Game
        PhotonNetwork.LoadLevel("Testing Lobby");

    }
}
