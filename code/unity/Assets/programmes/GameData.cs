// Contient les données du jeu //
using UnityEngine;
using System.Collections.Generic;

 public enum Mois
{
    Janvier, Fevrier, Mars, Avril, Mai, Juin,
    Juillet, Aout, Septembre, Octobre, Novembre, Decembre
}

[CreateAssetMenu(fileName = "GameData", menuName = "Scriptable Objects/GameData")]
public class GameData : ScriptableObject
{
    public Dictionary<string, CompteBanquaire> comptes = new Dictionary<string, CompteBanquaire>(){{"courant",new CompteBanquaire()}};

    public argent salaire = new argent(0);
    public int energie = 100; 
    public int santeMentale = 100; 
    public int nombreMoisPasses = 0; 
    public Mois moisActuel = Mois.Juillet; // Le jeu commence en Juillet

    public List<Investissement> investissements = new List<Investissement>(); // Liste des investissements du joueur

}