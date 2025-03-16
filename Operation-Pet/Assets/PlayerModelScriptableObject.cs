using UnityEngine;

[CreateAssetMenu(fileName = "PlayerModelScriptableObject", menuName = "Scriptable Objects/PlayerModelScriptableObject")]
public class PlayerModelScriptableObject : ScriptableObject
{
    public GameObject[] playerModels;
}
