using UnityEngine;

[CreateAssetMenu(fileName = "TeamModelScriptableObject", menuName = "Scriptable Objects/TeamScriptableObject")]
public class TeamModelScriptableObject : ScriptableObject
{
    public GameObject[] playerModels;

    //Array that will contain all the different food models
    public Mesh[] foodModels;
}
