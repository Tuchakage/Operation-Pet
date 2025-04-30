using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;



    public GameObject loginScreen, registerScreen, statScreen, roomInfoScreen, lobbyScreen;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        //Make sure there isnt a duplicate of this instance
        if (Instance != null && Instance != this)
        {
            //Destroy the old Instance
            Destroy(Instance.gameObject);
        }
        Instance = this;

    }

    void Start()
    {
        //If Login Screen is set in Inspector but it is not currently active in the scene
        if (loginScreen != null && !loginScreen.activeInHierarchy) 
        {
            ShowLoginScreen();
        }
    }

    // Update is called once per frame
    void Update()
    {

    }


    public void ClearScreen()
    {
        //Find all game objects with tag and disable them
        foreach (GameObject screens in GameObject.FindGameObjectsWithTag("UIScreen"))
        {
            screens.SetActive(false);
        }
    }

    public void ShowLoginScreen() 
    {
        ClearScreen();
        loginScreen.SetActive(true);
    }

    public void ShowRegisterScreen()
    {
        ClearScreen();
        registerScreen.SetActive(true);
    }

    public void ShowStatScreen() 
    {
        ClearScreen();
        statScreen.SetActive(true);
    }

    public void ShowLobbyScreen() 
    {
        ClearScreen();
        lobbyScreen.SetActive(true);
    }

    public void ShowRoomInfoScreen() 
    {
        ClearScreen();
        roomInfoScreen.SetActive(true);
    }
}
