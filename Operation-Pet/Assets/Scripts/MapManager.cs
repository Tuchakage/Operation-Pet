using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapManager : MonoBehaviour
{
    public string[] mapsToLoad;
    public static string mapKeyName = "Map Name";

    public string SelectRandomMap() 
    {
        //Choose a Random Map from the array
        int randomIndex = UnityEngine.Random.Range(0, mapsToLoad.Length);

        Hashtable mapChosen = new Hashtable()
            {
                {mapKeyName, mapsToLoad[randomIndex] }
            };

        //Set the Custom Properties for the room so that it knows what Map to use 
        PhotonNetwork.CurrentRoom.SetCustomProperties(mapChosen);

        return mapsToLoad[randomIndex];
    }
}
