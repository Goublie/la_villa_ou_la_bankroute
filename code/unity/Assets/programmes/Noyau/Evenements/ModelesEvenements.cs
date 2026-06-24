using System;
using System.Collections.Generic;

/// <summary>
/// Noms canoniques des categories acceptees par le catalogue.
/// </summary>
public static class CategoriesEvenements
{
    public const string Boursiers = "Boursiers";
    public const string Personnels = "Personnels";
    public const string Professionnels = "Professionnels";

    /// <summary>
    /// Indique si une categorie appartient au vocabulaire actuel du jeu.
    /// </summary>
    public static bool EstValide(string categorie)
    {
        return categorie == Boursiers ||
            categorie == Personnels ||
            categorie == Professionnels;
    }
}

/// <summary>
/// Impact brut lu dans le catalogue. Il n'est pas applique par cette branche.
/// </summary>
[Serializable]
public class ImpactDefinitionEvenement
{
    public string actif;
    public float variation;

    /// <summary>
    /// Produit une copie independante pour les parties et snapshots.
    /// </summary>
    public ImpactDefinitionEvenement Copier()
    {
        return new ImpactDefinitionEvenement
        {
            actif = actif,
            variation = variation
        };
    }
}

/// <summary>
/// Definition immuable d'un evenement charge depuis le catalogue JSON.
/// </summary>
[Serializable]
public class DefinitionEvenement
{
    public string id;
    public string categorie;
    public string titre;
    public string importance;
    public string message;
    public List<ImpactDefinitionEvenement> impacts =
        new List<ImpactDefinitionEvenement>();
}

/// <summary>
/// Source susceptible de publier une rumeur dans ses domaines declares.
/// </summary>
[Serializable]
public class SourceActualite
{
    public string id;
    public string nom;
    public string type;
    public List<string> domaines = new List<string>();
    public float fiabilite;

    /// <summary>
    /// Verifie la compatibilite stricte avec une categorie de rumeur.
    /// </summary>
    public bool AccepteCategorie(string categorie)
    {
        return domaines != null && domaines.Contains(categorie);
    }
}

/// <summary>
/// Cycle de vie persistant d'une rumeur dans une partie.
/// </summary>
public enum EtatRumeur
{
    EnAttente,
    Confirmee,
    Invalidee
}

/// <summary>
/// Rumeur effectivement apparue dans une partie.
/// </summary>
[Serializable]
public class RumeurPartie
{
    public string id;
    public string evenementId;
    public string sourceId;
    public string categorie;
    public string titrePublic;
    public string textePublic;
    public int moisApparition;
    public int moisResolution;
    public float probabiliteConfirmation;
    public EtatRumeur etat = EtatRumeur.EnAttente;
    public bool tirageEffectue;
    public bool resultatConfirmation;

    /// <summary>
    /// Copie toutes les donnees, y compris le resultat du tirage resolu.
    /// </summary>
    public RumeurPartie Copier()
    {
        return new RumeurPartie
        {
            id = id,
            evenementId = evenementId,
            sourceId = sourceId,
            categorie = categorie,
            titrePublic = titrePublic,
            textePublic = textePublic,
            moisApparition = moisApparition,
            moisResolution = moisResolution,
            probabiliteConfirmation = probabiliteConfirmation,
            etat = etat,
            tirageEffectue = tirageEffectue,
            resultatConfirmation = resultatConfirmation
        };
    }
}

/// <summary>
/// Etat d'un evenement confirme. Seul l'etat Confirme est produit ici.
/// </summary>
public enum EtatEvenementPartie
{
    Confirme,
    Actif,
    Termine
}

/// <summary>
/// Etat persistant du traitement d'un evenement par les moteurs d'impacts.
/// </summary>
public enum EtatTraitementImpactsEvenement
{
    EnAttente,
    Applique,
    RejeteInvalide
}

/// <summary>
/// Evenement cree apres confirmation d'une rumeur.
/// </summary>
[Serializable]
public class EvenementConfirmePartie
{
    public string definitionId;
    public string rumeurId;
    public string sourceId;
    public string categorie;
    public string importance;
    public string titre;
    public string message;
    public int moisConfirmation;
    public EtatEvenementPartie etat = EtatEvenementPartie.Confirme;
    public List<ImpactDefinitionEvenement> impacts =
        new List<ImpactDefinitionEvenement>();

    /// <summary>
    /// Frontiere de consommation pour la future branche d'impacts.
    /// </summary>
    public bool consommeParMoteurImpacts;

    /// <summary>
    /// Etat persistant evitant le retraitement mensuel d'un evenement.
    /// </summary>
    public EtatTraitementImpactsEvenement etatTraitementImpacts =
        EtatTraitementImpactsEvenement.EnAttente;

    /// <summary>
    /// Diagnostic persistant du traitement des impacts.
    /// </summary>
    public string diagnosticTraitementImpacts = string.Empty;

    /// <summary>
    /// Produit une copie profonde, notamment de la liste d'impacts.
    /// </summary>
    public EvenementConfirmePartie Copier()
    {
        EvenementConfirmePartie copie = new EvenementConfirmePartie
        {
            definitionId = definitionId,
            rumeurId = rumeurId,
            sourceId = sourceId,
            categorie = categorie,
            importance = importance,
            titre = titre,
            message = message,
            moisConfirmation = moisConfirmation,
            etat = etat,
            consommeParMoteurImpacts = consommeParMoteurImpacts,
            etatTraitementImpacts = etatTraitementImpacts,
            diagnosticTraitementImpacts = diagnosticTraitementImpacts,
            impacts = new List<ImpactDefinitionEvenement>()
        };

        if (impacts != null)
        {
            foreach (ImpactDefinitionEvenement impact in impacts)
            {
                if (impact != null)
                {
                    copie.impacts.Add(impact.Copier());
                }
            }
        }

        return copie;
    }
}

/// <summary>
/// Nature d'une ligne persistante publiee dans Actualites.
/// </summary>
public enum TypePublicationActualite
{
    Rumeur,
    EvenementConfirme
}

/// <summary>
/// Projection publique persistante d'une rumeur ou d'une confirmation.
/// </summary>
[Serializable]
public class PublicationActualite
{
    public string id;
    public TypePublicationActualite type;
    public string objetId;
    public string sourceId;
    public string sourceNom;
    public string categorie;
    public string importance;
    public string titre;
    public string texte;
    public int moisPublication;
    public int ordrePublication;
    public EtatRumeur etatRumeur;

    /// <summary>
    /// Copie la publication sans conserver de reference vers la partie.
    /// </summary>
    public PublicationActualite Copier()
    {
        return new PublicationActualite
        {
            id = id,
            type = type,
            objetId = objetId,
            sourceId = sourceId,
            sourceNom = sourceNom,
            categorie = categorie,
            importance = importance,
            titre = titre,
            texte = texte,
            moisPublication = moisPublication,
            ordrePublication = ordrePublication,
            etatRumeur = etatRumeur
        };
    }
}

/// <summary>
/// Agregat persistant du moteur de rumeurs et d'evenements.
/// </summary>
/// <remarks>
/// Il ne contient ni MonoBehaviour ni reference UI. L'etat aleatoire est
/// conserve pour reproduire la suite apres snapshot ou sauvegarde.
/// </remarks>
[Serializable]
public class DonneesEvenements
{
    public List<RumeurPartie> rumeurs = new List<RumeurPartie>();
    public List<EvenementConfirmePartie> evenementsConfirmes =
        new List<EvenementConfirmePartie>();
    public List<PublicationActualite> publications =
        new List<PublicationActualite>();
    public int dernierMoisTraite = -1;
    public int prochaineSequenceRumeur;
    public int prochaineSequencePublication;
    public uint graineInitiale;
    public uint etatAleatoire;

    /// <summary>
    /// Repare les collections absentes apres une migration de donnees.
    /// </summary>
    public void InitialiserSiNecessaire()
    {
        if (rumeurs == null)
        {
            rumeurs = new List<RumeurPartie>();
        }

        if (evenementsConfirmes == null)
        {
            evenementsConfirmes = new List<EvenementConfirmePartie>();
        }

        if (publications == null)
        {
            publications = new List<PublicationActualite>();
        }
    }

    /// <summary>
    /// Retourne les rumeurs dans un etat donne sans exposer la liste interne.
    /// </summary>
    public List<RumeurPartie> ObtenirRumeurs(EtatRumeur etat)
    {
        InitialiserSiNecessaire();
        return rumeurs.FindAll(rumeur => rumeur != null && rumeur.etat == etat);
    }

    /// <summary>
    /// Retourne des copies des confirmations connues au mois demande.
    /// Cette API n'expose volontairement aucune rumeur au futur What If.
    /// </summary>
    public List<EvenementConfirmePartie> CopierConfirmationsJusqua(int mois)
    {
        InitialiserSiNecessaire();
        List<EvenementConfirmePartie> resultat =
            new List<EvenementConfirmePartie>();
        foreach (EvenementConfirmePartie evenement in evenementsConfirmes)
        {
            if (evenement != null && evenement.moisConfirmation <= mois)
            {
                resultat.Add(evenement.Copier());
            }
        }

        return resultat;
    }

    /// <summary>
    /// Retourne les confirmations que le futur moteur d'impacts peut consommer.
    /// </summary>
    public List<EvenementConfirmePartie> ObtenirConfirmationsAConsommer()
    {
        InitialiserSiNecessaire();
        return evenementsConfirmes.FindAll(
            evenement =>
                evenement != null &&
                !evenement.consommeParMoteurImpacts &&
                evenement.etatTraitementImpacts ==
                    EtatTraitementImpactsEvenement.EnAttente);
    }

    /// <summary>
    /// Retourne uniquement les confirmations relevant du moteur Bourse.
    /// Les evenements personnels et professionnels restent disponibles pour
    /// leurs futurs moteurs specialises.
    /// </summary>
    public List<EvenementConfirmePartie>
        ObtenirConfirmationsBoursieresAConsommer()
    {
        InitialiserSiNecessaire();
        return evenementsConfirmes.FindAll(
            evenement =>
                evenement != null &&
                evenement.categorie == CategoriesEvenements.Boursiers &&
                !evenement.consommeParMoteurImpacts &&
                evenement.etatTraitementImpacts ==
                    EtatTraitementImpactsEvenement.EnAttente);
    }

    /// <summary>
    /// Produit une copie profonde de tout l'historique et de l'etat aleatoire.
    /// </summary>
    public DonneesEvenements Copier()
    {
        InitialiserSiNecessaire();
        DonneesEvenements copie = new DonneesEvenements
        {
            dernierMoisTraite = dernierMoisTraite,
            prochaineSequenceRumeur = prochaineSequenceRumeur,
            prochaineSequencePublication = prochaineSequencePublication,
            graineInitiale = graineInitiale,
            etatAleatoire = etatAleatoire
        };

        foreach (RumeurPartie rumeur in rumeurs)
        {
            if (rumeur != null)
            {
                copie.rumeurs.Add(rumeur.Copier());
            }
        }

        foreach (EvenementConfirmePartie evenement in evenementsConfirmes)
        {
            if (evenement != null)
            {
                copie.evenementsConfirmes.Add(evenement.Copier());
            }
        }

        foreach (PublicationActualite publication in publications)
        {
            if (publication != null)
            {
                copie.publications.Add(publication.Copier());
            }
        }

        return copie;
    }
}
