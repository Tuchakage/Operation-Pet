using Photon.Realtime;
using TMPro;
using UnityEngine;

public class joinRoom : MonoBehaviour
{
    public void OnButtonPressed() 
    {
        string roomName = this.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text;

        MainMenuManager.menuInstance.JoinRoom(roomName);
    }
}
