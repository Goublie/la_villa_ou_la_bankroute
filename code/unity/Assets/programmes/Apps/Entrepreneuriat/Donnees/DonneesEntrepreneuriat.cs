using System;

/// <summary>
/// Agregat persistant du systeme Entrepreneuriat.
/// </summary>
[Serializable]
public class DonneesEntrepreneuriat : IPatrimoine
{
    /// <summary>
    /// Projet courant du joueur.
    /// </summary>
    public ProjetEntrepreneurial projet = new ProjetEntrepreneurial();

    /// <summary>
    /// Dernier retour metier affiche dans le journal.
    /// </summary>
    public string dernierMessage =
        "Choisissez un secteur, un public et une technologie.";

    /// <summary>
    /// Dernier mois traite par le service.
    /// </summary>
    public int dernierMoisObserve = -1;

    /// <summary>
    /// Etat de la migration des anciennes valeurs stockees dans le prefab.
    /// </summary>
    public bool migrationPrefabEffectuee;

    /// <summary>
    /// Graine persistante du generateur deterministe des pitchs.
    /// </summary>
    public uint etatAleatoire = 2463534242u;

    /// <summary>
    /// Repare les anciennes donnees auxquelles il manque un projet.
    /// </summary>
    public void InitialiserSiNecessaire()
    {
        if (projet == null)
        {
            projet = new ProjetEntrepreneurial();
        }

        if (string.IsNullOrEmpty(dernierMessage))
        {
            dernierMessage =
                "Choisissez un secteur, un public et une technologie.";
        }

        if (etatAleatoire == 0u)
        {
            etatAleatoire = 2463534242u;
        }
    }

    /// <inheritdoc />
    public argent GetValeurPatrimoine()
    {
        InitialiserSiNecessaire();
        return projet.GetValeurPatrimoine();
    }

    /// <summary>
    /// Produit une copie profonde incluant l'etat aleatoire.
    /// </summary>
    /// <remarks>
    /// Copier la graine garantit qu'une simulation What If rejouee depuis le
    /// meme snapshot produit les memes succes et echecs de pitch.
    /// </remarks>
    public DonneesEntrepreneuriat Copier()
    {
        InitialiserSiNecessaire();
        return new DonneesEntrepreneuriat
        {
            projet = projet.Copier(),
            dernierMessage = dernierMessage,
            dernierMoisObserve = dernierMoisObserve,
            migrationPrefabEffectuee = migrationPrefabEffectuee,
            etatAleatoire = etatAleatoire
        };
    }
}
