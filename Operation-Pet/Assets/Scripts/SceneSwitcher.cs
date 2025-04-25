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

        object mapName;
        //Check the name of the map called in the Current Rooms Custom Property and put that output into variable named "mapName"
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(MapManager.mapKeyName, out mapName)) 
        {
            //Load that map
            PhotonNetwork.LoadLevel(mapName.ToString());
        }



    }
}
