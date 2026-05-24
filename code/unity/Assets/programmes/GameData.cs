// Contient les données du jeu //
using UnityEngine;

[CreateAssetMenu(fileName = "GameData", menuName = "Scriptable Objects/GameData")]
public class GameData : ScriptableObject
{
    public argent argent = new argent(1000);
    public int energie = 100;
    public int santeMentale = 100;

    public int moisPasse = 0;

    public void Start()
    {
        moisPasse = 0;
        argent = new argent(1000);
        energie = 100;
        santeMentale = 100;
    }

}