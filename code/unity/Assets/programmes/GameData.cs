// Contient les données du jeu //
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "GameData", menuName = "Scriptable Objects/GameData")]
public class GameData : ScriptableObject
{
    public int salaire = 0;
    public int energie = 100; 
    public int santeMentale = 100; 
    public int nombreMoisPasses = 0; 
    public List<Investissement> investissements = new List<Investissement>(); // Liste des investissements du joueur
    
    /// <summary>
    /// Réinitialise toutes les données de jeu à leur état par défaut.
    /// Appelé lors du démarrage d'une nouvelle partie.
    /// </summary>
    public void ResetData()
    {
        salaire = 0;
        energie = 100;
        santeMentale = 100;
        nombreMoisPasses = 0;
        investissements.Clear();
        
    }
}