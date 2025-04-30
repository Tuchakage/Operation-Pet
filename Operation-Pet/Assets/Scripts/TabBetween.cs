using UnityEngine;
using TMPro;
public class TabBetween : MonoBehaviour
{
    //The next Input field the script will focus on
    public TMP_InputField nextField;
    TMP_InputField myField;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (nextField == null) 
        {
            Destroy(this); //Destroy this component
            return;
        }
        myField = GetComponent<TMP_InputField>();
    }

    // Update is called once per frame
    void Update()
    {
        if (myField.isFocused && Input.GetKeyDown(KeyCode.Tab)) 
        {
            nextField.ActivateInputField();
        }
    }
}
