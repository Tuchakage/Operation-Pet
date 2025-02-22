using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class inEditor : MonoBehaviour
{
    public GameObject top; // The tabletop object
    public Transform[] legs;    // The legs

    public float legHeight = 2f;

    void Update()
    {
        if (top != null && legs.Length > 0)
        {
            // Get the current scale of the tabletop
            Vector3 tableSize = top.transform.localScale;

            // Set the positions of the legs based on the tabletop's size
            if (legs.Length >= 4)
            {
                // Leg 1
                legs[0].localPosition = new Vector3(tableSize.x / 2, legHeight, tableSize.z / 2);
                // Leg 2
                legs[1].localPosition = new Vector3(-tableSize.x / 2, legHeight, tableSize.z / 2);
                // Leg 3
                legs[2].localPosition = new Vector3(tableSize.x / 2, legHeight, -tableSize.z / 2);
                // Leg 4
                legs[3].localPosition = new Vector3(-tableSize.x / 2, legHeight, -tableSize.z / 2);
            }
        }
    }
}