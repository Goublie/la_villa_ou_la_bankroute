using System;
using System.Collections.Generic;

public enum Ville
{
    Bordeaux,
    Lyon,
    Marseille,
    Nantes,
    Paris,
    Toulouse
}

public enum TypeBien
{
    Studio,
    AppartementT2,
    AppartementT4,
    ImmeubleRapport,
    LocalCommercial
}

/// <summary>
/// Représente un bien immobilier possédé par le joueur ou disponible sur le marché.
/// </summary>
[Serializable]
public class BienImmobilier : IPatrimoine
{
    public string idUnique;
    public Ville ville;
    public TypeBien type;
    public int surfaceM2;   // Surface réelle du bien (aléatoire à la génération du marché)
    public bool estMeuble;  // Bien proposé meublé ou non (aléatoire à la génération)
    public int moisAchat = -1;
    public float indicePrixReferenceAchat;
    public int valeurReferenceAchatCentimes;

    // Bases financières
    public argent prixAchat;
    public argent valeurActuelle;
    public bool estLoue;

    // Renta de base et indexation (Calculs internes)
    public float tauxRendementInitial;
    public argent loyerInitial;
    public argent loyerMensuel; // Loyer réel perçu (évolue chaque année)

    public BienImmobilier()
    {
        idUnique = Guid.NewGuid().ToString();
        prixAchat = new argent(0);
        valeurActuelle = new argent(0);
        loyerInitial = new argent(0);
        loyerMensuel = new argent(0);
        estLoue = true; // Loué automatiquement dès l'achat
    }

    /// <summary>
    /// Liaison avec ServicePatrimoine pour le calcul de la richesse totale.
    /// </summary>
    public argent GetValeurPatrimoine()
    {
        return valeurActuelle;
    }

    /// <summary>
    /// Copie profonde pour les snapshots de sauvegarde et le mode What If.
    /// </summary>
    public BienImmobilier Copier()
    {
        return new BienImmobilier
        {
            idUnique = this.idUnique,
            ville = this.ville,
            type = this.type,
            surfaceM2 = this.surfaceM2,
            estMeuble = this.estMeuble,
            moisAchat = this.moisAchat,
            indicePrixReferenceAchat = this.indicePrixReferenceAchat,
            valeurReferenceAchatCentimes =
                this.valeurReferenceAchatCentimes,
            prixAchat = new argent(this.prixAchat.centimes),
            valeurActuelle = new argent(this.valeurActuelle.centimes),
            estLoue = this.estLoue,

            tauxRendementInitial = this.tauxRendementInitial,
            loyerInitial = new argent(this.loyerInitial.centimes),
            loyerMensuel = new argent(this.loyerMensuel.centimes)
        };
    }
}

/// <summary>
/// Conteneur du portefeuille immobilier intégré dans DonneesJoueur.
/// </summary>
[Serializable]
public class DonneesImmobilier
{
    public List<BienImmobilier> biensPossedes;
    public List<AnnonceImmobiliere> annoncesActuelles; // Stockage persistant du marché
    public List<ImpactEvenementImmobilier> impactsActifs;

    public DonneesImmobilier()
    {
        biensPossedes = new List<BienImmobilier>();
        annoncesActuelles = new List<AnnonceImmobiliere>();
        impactsActifs = new List<ImpactEvenementImmobilier>();
    }


    public void InitialiserSiNecessaire()
    {
        if (biensPossedes == null)
        {
            biensPossedes = new List<BienImmobilier>();
        }

        if (annoncesActuelles == null)
        {
            annoncesActuelles = new List<AnnonceImmobiliere>();
        }

        if (impactsActifs == null)
        {
            impactsActifs = new List<ImpactEvenementImmobilier>();
        }
    }

    public DonneesImmobilier Copier()
    {
        InitialiserSiNecessaire();
        DonneesImmobilier copie = new DonneesImmobilier();

        if (this.biensPossedes != null)
        {
            foreach (var bien in this.biensPossedes)
            {
                copie.biensPossedes.Add(bien.Copier());
            }
        }

        if (this.annoncesActuelles != null)
        {
            foreach (var annonce in this.annoncesActuelles)
            {
                copie.annoncesActuelles.Add(annonce.Copier());
            }
        }


        if (impactsActifs != null)
        {
            foreach (ImpactEvenementImmobilier impact in impactsActifs)
            {
                if (impact != null)
                {
                    copie.impactsActifs.Add(impact.Copier());
                }
            }
        }

        return copie;
    }
}
