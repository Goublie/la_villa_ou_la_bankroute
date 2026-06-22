using System;
using UnityEngine;

/// <summary>
/// Etat persistant d'un pret immobilier contracte par le joueur.
/// </summary>
[Serializable]
public class DonneesPret : IPatrimoine
{
    public argent montantEmprunte;
    public int dureeAns;
    public float tauxAnnuel;
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

    /// <summary>
    /// Implémentation de IPatrimoine. Un prêt est un passif (une dette), 
    /// sa valeur réduit donc le patrimoine global.
    /// </summary>
    public argent GetValeurPatrimoine()
    {
        return -capitalRestantDu;
    }
}