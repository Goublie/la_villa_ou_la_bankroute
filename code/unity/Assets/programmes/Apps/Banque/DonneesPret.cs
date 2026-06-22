using System;
using UnityEngine;

/// <summary>
/// Etat persistant d'un pret immobilier contracte par le joueur.
/// </summary>
[Serializable]
public class DonneesPret
{
    public argent montantEmprunte;
    public int dureeAns;
    public float tauxAnnuel; // Stocké en pourcentage (ex: 3.5f pour 3.5%)
    public argent mensualite;
    public int moisRestants;
    public argent capitalRestantDu;

    public DonneesPret() { }

    public DonneesPret(argent montant, int duree, float taux, argent mensualite)
    {
        this.montantEmprunte = montant;
        this.dureeAns = duree;
        this.tauxAnnuel = taux;
        this.mensualite = mensualite;
        this.moisRestants = duree * 12;
        this.capitalRestantDu = montant;
    }
}