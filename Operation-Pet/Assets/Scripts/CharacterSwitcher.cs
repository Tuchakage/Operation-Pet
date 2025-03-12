using UnityEngine;

public class CharacterSwitcher : MonoBehaviour
{
    public GameObject[] characters; // Array to hold the characters
    private int currentIndex = 0; // Keeps track of the active character
    private int pressCount = 0; // Tracks how many times the "5" key has been pressed

    void Start()
    {
        // Deactivate all characters except the first one
        for (int i = 0; i < characters.Length; i++)
        {
            characters[i].SetActive(i == currentIndex);
        }
    }

    void Update()
    {
        // Check if the "5" key is pressed
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            pressCount++;

            // Switch character after the key is pressed 4 times
            if (pressCount >= 4)
            {
                pressCount = 0; // Reset the press count
                SwitchCharacter();
            }
        }
    }

    private void SwitchCharacter()
    {
        // Deactivate the current character
        characters[currentIndex].SetActive(false);

        // Move to the next character
        currentIndex = (currentIndex + 1) % characters.Length;

        // Activate the new character
        characters[currentIndex].SetActive(true);
    }
}
