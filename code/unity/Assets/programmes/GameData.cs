// Contient les données du jeu //
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "GameData", menuName = "Scriptable Objects/GameData")]
public class GameData : ScriptableObject
{
    public Dictionary<string, CompteBanquaire> comptes = new Dictionary<string, CompteBanquaire>(){{"courant",new CompteBanquaire()}};
    public int energie = 100; 
    public int santeMentale = 100; 
    public int moisPasse = 0; 

    public List<Investissement> investissements = new List<Investissement>(); // Liste des investissements du joueur

}