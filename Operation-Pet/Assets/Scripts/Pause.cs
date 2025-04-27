using ExitGames.Client.Photon;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Pun;
using static teamsEnum;

public class Pause : MonoBehaviour
{
    [SerializeField]
    private GameObject pauseMenuPanel;

    [SerializeField]
    private Button resumeBtn;

    [SerializeField]
    private Button mainMenuBtn;

    private bool isMenuActive;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        isMenuActive = false;

        //Find UI Elements
        pauseMenuPanel = GameObject.Find("Pause Menu");
        resumeBtn = GameObject.Find("ResumeBtn").GetComponent<Button>();
        mainMenuBtn = GameObject.Find("MainMenuBtn").GetComponent<Button>();

        if (pauseMenuPanel) 
        {
            pauseMenuPanel.SetActive(false);
        }

        if (resumeBtn) 
        {
            resumeBtn.onClick.AddListener(ResumeGame);
        }

        if (mainMenuBtn) 
        { 
            mainMenuBtn.onClick.AddListener(MainMenu); 
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) 
        {
            TogglePauseMenu();
        }
    }

    void TogglePauseMenu() 
    {
        //Reverse the value
        isMenuActive = !isMenuActive;
        pauseMenuPanel.SetActive(isMenuActive);
    }

    public void ResumeGame() 
    {
        isMenuActive = false;
        pauseMenuPanel.SetActive(isMenuActive);
    }

    public void MainMenu() 
    {
        //Call the static function to add player back to Unassigned
        TeamManager.AddPlayerToTeam(teams.Unassigned);
        //PhotonNetwork.AutomaticallySyncScene = false;
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene("Main Menu");
    }

}
