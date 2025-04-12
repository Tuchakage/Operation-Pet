using UnityEngine;
using Firebase;
using Firebase.Auth;
using TMPro;
using System.Collections;

public class AuthManager : MonoBehaviour
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

            //Make the Input Fields empty
            ResetInputFields();
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
                        //Make the Input Fields empty
                        ResetInputFields();
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
    }
}
