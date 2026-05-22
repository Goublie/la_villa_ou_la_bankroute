// Contient les données du jeu //
using UnityEngine;

[CreateAssetMenu(fileName = "GameData", menuName = "Scriptable Objects/GameData")]
public class GameData : ScriptableObject
{
    public int argent = 1000;
    public int energie = 100;
    public int santeMentale = 100;



    public void Start()
    {
        argent = 1000;
        energie = 100;
        santeMentale = 100;
    }

}