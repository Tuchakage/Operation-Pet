using UnityEngine;
using Firebase;
using Firebase.Auth;
using TMPro;
using System.Collections;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class FirebaseManager : MonoBehaviourPunCallbacks
{
    //Firebase variables
    [Header("Firebase")]
    public DependencyStatus dependencyStatus;
    public FirebaseAuth auth;
    public FirebaseUser User;

    [Header("Login")]
    public TMP_InputField emailLoginField;
    public TMP_InputField passwordLoginField;
    public TMP_Text warningLoginText;
    public TMP_Text confirmLoginText;

    [Header("Register")]
    public TMP_InputField usernameRegisterField;
    public TMP_InputField emailRegisterField;
    public TMP_InputField passwordRegisterField;
    public TMP_InputField passwordRegisterVerifyField;
    public TMP_Text warningRegisterText;

    
    #region String Variables
    string gameVersion = "0.9";
    #endregion

    //Variables to allow the enter key to be pressed when focused onto an input field
    bool allowEnterLogin;
    bool allowEnterRegister;
    void Awake()
    {

        //Check that all of the necesary dependenceies for Firebase are present on the system
        //"CheckAndFixDependenciesAsync" is a function that returns Task<DependencyStatus> and ".ContinueWith" is a continuation to that task
        //"task =>{} defines what is done after Firebase has checked/fixed the dependencies
        //Lamba expression (parameter) => lambda body in this case our parameter is the "task" variable returned from "CheckAndFixDependenciesASync"
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            dependencyStatus = task.Result;

            if (dependencyStatus == DependencyStatus.Available)
            {
                //If all the Dependencies are there, Initialise Firebase
                InitialiseFirebase();
            }
            else 
            {
                Debug.LogError("Could not resolve all Firebase dependences:" + dependencyStatus);
            }
        });
    }

    void Start()
    {
        //Makes sure we can use PhotonNetwork.LoadLevel() on the master client and all clients in the same rom sync their level automatically
        PhotonNetwork.AutomaticallySyncScene = true;

        if (!PhotonNetwork.IsConnected)
        {

            //Set the App version before connecting
            PhotonNetwork.PhotonServerSettings.AppSettings.AppVersion = gameVersion;
            //Connect to photon master-server. Uses the settings saved in PhotonServerSettings (An asset file in project)
            PhotonNetwork.ConnectUsingSettings();

        }
    }

    void Update()
    {
        //This is reversed because otherwise when you press the enter key whilst focused on an Input field it will not call the function, it will just unfocus from the inputfield
        if (allowEnterLogin && Input.GetKeyDown(KeyCode.Return))
        {
            LoginButton();
        }
        else 
        {
            //If pressing Enter doesn't work then we set the allowEnterLogin to whether we are focused on the login field or not, then Update() will check the next frame and
            //allowEnterLogin will still be true (If we focused on the input field) and the LoginButton Function will be called
            allowEnterLogin = emailLoginField.isFocused || passwordLoginField.isFocused;
        }

        if (allowEnterRegister && Input.GetKeyDown(KeyCode.Return))
        {
            RegisterButton();
        }
        else 
        {
            //If pressing Enter doesn't work then we set the allowEnterRegister to whether we are focused on the register field or not, then Update() will check the next frame and
            //allowEnterRegister will still be true (If we focused on the input field) and the RegisterButton Function will be called
            allowEnterRegister = usernameRegisterField.isFocused || emailRegisterField.isFocused || passwordRegisterField.isFocused || passwordRegisterVerifyField.isFocused;
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

        //if (regionTxt)
        //{
        //    regionTxt.text = "Region = " + PhotonNetwork.CloudRegion;
        //}

        //After we connect to Master server, join the lobby
        PhotonNetwork.JoinLobby(TypedLobby.Default);
    }

    #endregion

    #region Login & Register
    void InitialiseFirebase() 
    {
        Debug.Log("Setting up Firebase Auth");

        //Set the authentication instance object;
        auth = FirebaseAuth.DefaultInstance;
    }

    public void LoginButton() 
    {
        StartCoroutine(Login(emailLoginField.text, passwordLoginField.text));
    }

    public void RegisterButton() 
    {
        StartCoroutine(Register(emailRegisterField.text, passwordRegisterField.text, usernameRegisterField.text));
    }

    

    IEnumerator Login(string email, string password) 
    {
        //Call the Firebase auth signin function passing the email and password
        var LoginTask = auth.SignInWithEmailAndPasswordAsync(email, password);


        //Wait until the task completes (We use Lambda expression because WaitUntil expects a function that returns a boolean)
        yield return new WaitUntil(() => LoginTask.IsCompleted);

        //If there is any errors with this tasks
        if (LoginTask.Exception != null) 
        {
            //If there are errors handle them
            Debug.LogWarning($"Failed to register task with {LoginTask.Exception}");

            //Get the exception error
            FirebaseException firebaseEx = LoginTask.Exception.GetBaseException() as FirebaseException;

            //Get the error code from the error
            AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

            string message = "Login Failed!";

            switch (errorCode) 
            {
                case AuthError.MissingEmail:
                    message = "Missing Email";
                    break;

                case AuthError.MissingPassword:
                    message = "Missing Password";
                    break;

                case AuthError.WrongPassword:
                    message = "Wrong Password";
                    break;

                case AuthError.InvalidEmail:
                    message = "Invalid Email";
                    break;

                case AuthError.UserNotFound:
                    message = "Account does not exist";
                    break;
            }

            //Display Error on screen
            warningLoginText.text = message;
        }
        else //User is now logged in
        {
            //Store the result
            User = LoginTask.Result.User;
            Debug.LogFormat("User signed in successfully: {0} ({1})", User.DisplayName, User.Email);
            warningLoginText.text = "";
            confirmLoginText.text = "Logged In: "+User.DisplayName;
            //Set Photon Nickname to be the same as signed in Username
            PhotonNetwork.NickName = User.DisplayName;

            UIManager.Instance.ShowStatScreen();
            //Go to Main Menu
            //SceneManager.LoadScene("Main Menu");
        }
    }

    IEnumerator Register(string email, string password, string username) 
    {
        if (username == "") //If Username section is blank
        {
            warningRegisterText.text = "Missing Username";
        }
        else if (passwordRegisterField.text != passwordRegisterVerifyField.text) //If both password input fields dont match
        {
            warningRegisterText.text = "Password Does Not Match!";
        }
        else //Everything is there 
        {
            //Call the Firebase auth register function and pass in the email and password
            var RegisterTask = auth.CreateUserWithEmailAndPasswordAsync(email, password);

            //Wait until task is complete
            yield return new WaitUntil( () => RegisterTask.IsCompleted);

            //If there are any errors with this task
            if (RegisterTask.Exception != null)
            {
                //If there are errors handle them
                Debug.LogWarning($"Failed to register task with {RegisterTask.Exception}");

                //Get the exception error
                FirebaseException firebaseEx = RegisterTask.Exception.GetBaseException() as FirebaseException;

                //Get the error code from the error
                AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

                string message = "Register Failed!";

                switch (errorCode)
                {
                    case AuthError.MissingEmail:
                        message = "Missing Email";
                        break;
                    case AuthError.InvalidEmail:
                        message = "Invalid Email";
                        break;
                    case AuthError.MissingPassword:
                        message = "Missing Password";
                        break;
                    case AuthError.WeakPassword:
                        message = "Weak Password";
                        break;
                    case AuthError.EmailAlreadyInUse:
                        message = "Email Already In Use";
                        break;
                }

                warningRegisterText.text = message;
            }
            else //User account has been created 
            {
                //Store the Result
                User = RegisterTask.Result.User;

                if (User != null) 
                {
                    //Create a user profile and set the username
                    UserProfile profile = new UserProfile { DisplayName = username };

                    //Call the Firebase auth update user profile function passing the profile with the username
                    var ProfileTask = User.UpdateUserProfileAsync(profile);

                    //Wait until task is complete
                    yield return new WaitUntil(() => ProfileTask.IsCompleted);

                    if (ProfileTask.Exception != null)
                    {
                        //If there are errors handle them
                        Debug.LogWarning($"Failed to register task with {ProfileTask.Exception}");

                        //Get the exception error
                        FirebaseException firebaseEx = ProfileTask.Exception.GetBaseException() as FirebaseException;

                        //Get the error code from the error
                        AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

                        warningRegisterText.text = "Username Set Failed!";
                    }
                    else 
                    {
                        //Username is now set
                        warningRegisterText.text = "";

                        //Set Photon Nickname to be the same as signed in Username
                        //PhotonNetwork.NickName = User.DisplayName;

                        //Make the Input Fields empty
                        ResetInputFields();

                        UIManager.Instance.ShowLoginScreen();
                    }


                }
            }
        }
    }

    public void ResetInputFields() 
    {
        emailLoginField.text = "";
        passwordLoginField.text = "";
        usernameRegisterField.text = "";
        emailRegisterField.text = "";
        passwordRegisterField.text = "";
        passwordRegisterVerifyField.text = "";
        warningLoginText.text = "";
        warningRegisterText.text = "";
    }
    #endregion

}
