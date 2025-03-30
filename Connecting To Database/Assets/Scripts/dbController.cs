using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.Networking;
using System.Xml;
using static UnityEngine.UIElements.UxmlAttributeDescription;
using UnityEditor.PackageManager.Requests;
using UnityEngine.UI;

public class dbController : MonoBehaviour
{
    public GameObject inputFieldName;
    public GameObject inputFieldScore;
    public TMP_Text webText;
    public TMP_Text testText;
    public string player_name;
    public int score;
    public string[] top10Scores;

    private string temperature;
    private string clouds;
    private string humidity;
    private string pressure;

    public void myTestExample() 
    {
        StartCoroutine(TestExample());
    }

    IEnumerator TestExample() 
    {
        string url = "api.openweathermap.org/data/2.5/weather?q=London,uk&mode=xml&appid=7489785e77f05f8b54bbcd62dc27a836";

        using (UnityWebRequest webRequest = UnityWebRequest.Get(url)) 
        {
            //Request and wait for the desired page (Use yield to wait for a response)
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError)
            {
                Debug.Log("Error: " + webRequest.error);
            }
            else 
            {
                Debug.Log("Received: " + webRequest.downloadHandler.text);
            }

            //Create an XML document and load it
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(webRequest.downloadHandler.text);

            //Store the contents of the XML document into this string
            string string0 = doc.OuterXml;

            //Strings to look for
            string toFind1 = "temperature value=\"";
            string toFind2 = "clouds value=\"";
            string toFind3 = "humidity value=\"";
            string toFind4 = "pressure value=\"";
            string toFindend = "\"";

            // Extract the strings in the XML Document
            #region Extracting Information
            int start1 = string0.IndexOf(toFind1) + toFind1.Length;
            Debug.Log(string0.IndexOf(toFind1));
            int end1 = string0.IndexOf(toFindend, start1);
            int start2 = string0.IndexOf(toFind2) + toFind2.Length;
            int end2 = string0.IndexOf(toFindend, start2);
            int start3 = string0.IndexOf(toFind3) + toFind3.Length;
            int end3 = string0.IndexOf(toFindend, start3);
            int start4 = string0.IndexOf(toFind4) + toFind4.Length;
            int end4 = string0.IndexOf(toFindend, start4);

            string string1 = string0.Substring(start1, end1 - start1);
            string string2 = string0.Substring(start2, end2 - start2);
            string string3 = string0.Substring(start3, end3 - start3);
            string string4 = string0.Substring(start4, end4 - start4);
            #endregion

            temperature = string1;
            clouds = string2;
            humidity = string3;
            pressure = string4;
        }

        //Create a form
        testText.text = "Temperature = " + temperature + "K\nClouds = " + clouds + "\nHuminity = " + humidity + "\nPressure";
        WWWForm form1 = new WWWForm();
        form1.AddField("temperature", temperature);
        form1.AddField("cloud_value", clouds);
        form1.AddField("humidity", humidity);

        //Post the form to our server
        using (UnityWebRequest www = UnityWebRequest.Post("http://localhost/leaderboard/api.php", form1)) 
        {
            //Request and wait for the desired page(Use yield to wait for a response)
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else 
            {
                Debug.Log("Form upload complete!");
            }
        }

    }

    public void mySaveScores() 
    {
        player_name = inputFieldName.GetComponent<TMP_InputField>().text;
        score = System.Convert.ToInt32(inputFieldScore.GetComponent<TMP_InputField>().text);
        print(player_name);
        print(score);

        //This will start our function, Coroutine means that the game doesn't need to wait for the function to be done
        StartCoroutine(SaveScores());
    }

    public void myLoadScores() 
    {
        StartCoroutine(LoadScores());
    }

    IEnumerator SaveScores() 
    {
        //Create form data
        WWWForm form1 = new WWWForm();
        form1.AddField("userName", player_name);
        form1.AddField("userScore", score);

        using (UnityWebRequest www = UnityWebRequest.Post("http://localhost/leaderboard/SaveScores.php", form1)) 
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else 
            {
                Debug.Log("Form upload complete!");
            }
        }
    }

    IEnumerator LoadScores() 
    {
        string url = "http://localhost/leaderboard/LoadScorse.php";

        using (UnityWebRequest webRequest = UnityWebRequest.Get(url)) 
        {
            //Request and wait for the desired page
            yield return webRequest.SendWebRequest();

            if (webRequest.isNetworkError)
            {
                Debug.Log("Error: " + webRequest.error);
            }
            else 
            {
                Debug.Log("Received: " + webRequest.downloadHandler.text);
                //Display the text from the Database
                webText.text = webRequest.downloadHandler.text;
            }
        }
    }
}
