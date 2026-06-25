using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Structure éphémère représentant une opportunité sur le marché immobilier.
/// </summary>
[Serializable]
public class AnnonceImmobiliere
{
    public DefinitionBienImmobilier Definition; // Lien direct vers le gabarit d'origine
    public Ville Ville;
    public TypeBien Type;
    public argent PrixVenteAffiche;
    public argent LoyerMensuelPropose;
    public float TauxRendementBrut;

    /// <summary>Surface aléatoire générée à la création de l'annonce (25-150 m²).</summary>
    public int SurfaceM2;
    /// <summary>Indique si le bien est proposé meublé (tiré aléatoirement à la génération).</summary>
    public bool EstMeuble;

    public AnnonceImmobiliere Copier()
    {
        return new AnnonceImmobiliere
        {
            Definition = this.Definition,
            Ville = this.Ville,
            Type = this.Type,
            PrixVenteAffiche = new argent(this.PrixVenteAffiche.centimes),
            LoyerMensuelPropose = new argent(this.LoyerMensuelPropose.centimes),
            TauxRendementBrut = this.TauxRendementBrut,
            SurfaceM2 = this.SurfaceM2,
            EstMeuble = this.EstMeuble
        };
    }
}

public static class ServiceImmobilier
{
    // Types de biens disponibles sur le marché libre
    private static readonly TypeBien[] TypesBienMarche = new[]
    {
        TypeBien.Studio,
        TypeBien.AppartementT2,
        TypeBien.AppartementT4,
        TypeBien.ImmeubleRapport,
        TypeBien.LocalCommercial
    };

    /// <summary>
    /// Génère une annonce sur le marché à partir d'une ville, d'un type de bien,
    /// d'une surface et d'un statut meublé tirés aléatoirement.
    /// </summary>
    public static AnnonceImmobiliere GenererAnnonceSurLeMarche(
        Ville ville,
        TypeBien type,
        int surfaceM2,
        bool estMeuble,
        int nombreMoisPasses,
        DonneesImmobilier donneesImmobilier = null)
    {
        string villeId = ville.ToString().ToLower();

        // 1. Prix au m2 issu du marché mondial pour cette ville
        float prixM2Actuel =
            MarcheImmobilier.ObtenirPrixM2(villeId, nombreMoisPasses) *
            ServiceImpactsImmobiliers.CalculerCoefficientPrix(
                donneesImmobilier,
                ville,
                type,
                nombreMoisPasses);

        // 2. Facteur qualité : légèrement aléatoire (0.85 à 1.20)
        float facteurQualite = UnityEngine.Random.Range(0.85f, 1.20f);
        float valeurTheoriqueEuros = prixM2Actuel * surfaceM2 * facteurQualite;

        // 3. Oscillation aléatoire de l'opportunité (0.95 à 1.05)
        float multiplicateurPrix = UnityEngine.Random.Range(0.95f, 1.05f);
        float prixAfficheEuros = valeurTheoriqueEuros * multiplicateurPrix;
        long prixCentimes = (long)Math.Round(prixAfficheEuros * 100d);
        argent prixVente = new argent((int)Math.Clamp(prixCentimes, 0, int.MaxValue));

        // 4. Loyer : rendement brut aléatoire entre 4% et 7% par an
        float tauxRendement = UnityEngine.Random.Range(0.04f, 0.07f);
        double loyerAnnuelEuros = prixAfficheEuros * tauxRendement;
        double loyerMensuelEuros =
            (loyerAnnuelEuros / 12d) *
            ServiceImpactsImmobiliers.CalculerCoefficientLoyer(
                donneesImmobilier,
                ville,
                type,
                nombreMoisPasses);
        long loyerCentimes = (long)Math.Round(loyerMensuelEuros * 100d);
        argent loyerMensuel = new argent((int)Math.Clamp(loyerCentimes, 0, int.MaxValue));

        return new AnnonceImmobiliere
        {
            Ville = ville,
            Type = type,
            SurfaceM2 = surfaceM2,
            EstMeuble = estMeuble,
            PrixVenteAffiche = prixVente,
            LoyerMensuelPropose = loyerMensuel,
            TauxRendementBrut = tauxRendement
        };
    }

    /// <summary>
    /// Vide le marché et génère exactement 2 biens par ville avec surface (25-150 m²)
    /// et statut meublé aléatoires. Appelé tous les 6 mois.
    /// </summary>
    public static void RafraichirMarche(DonneesJoueur joueur, int nombreMoisPasses, int nombreAnnoncesSouhaitees = 3)
    {
        if (joueur.immobilier == null) return;

        joueur.immobilier.annoncesActuelles.Clear();

        Ville[] toutesLesVilles = (Ville[])Enum.GetValues(typeof(Ville));
        int totalGenere = 0;

        foreach (Ville ville in toutesLesVilles)
        {
            // 2 biens par ville
            for (int i = 0; i < 2; i++)
            {
                // Type aléatoire parmi les types disponibles
                TypeBien type = TypesBienMarche[UnityEngine.Random.Range(0, TypesBienMarche.Length)];

                // Surface aléatoire entre 25 et 150 m²
                int surface = UnityEngine.Random.Range(25, 151);

                // Meublé aléatoire (50/50)
                bool meuble = UnityEngine.Random.value > 0.5f;

                AnnonceImmobiliere annonce = GenererAnnonceSurLeMarche(
                    ville,
                    type,
                    surface,
                    meuble,
                    nombreMoisPasses,
                    joueur.immobilier);
                joueur.immobilier.annoncesActuelles.Add(annonce);
                totalGenere++;
            }
        }

        Debug.Log($"[ServiceImmobilier] Marché renouvelé : {totalGenere} annonces ({toutesLesVilles.Length} villes × 2) au mois {nombreMoisPasses}.");
    }

    /// <summary>
    /// Traite l'achat cash brut. Si le joueur a assez, ça passe, sinon ça bloque.
    /// </summary>
    public static bool ExecuterAchatCash(DonneesJoueur joueur, AnnonceImmobiliere annonce, int nombreMoisPasses)
    {
        var compteCourant = joueur.comptes[ServiceBanque.CompteCourantId];

        // Règle 1 : Le joueur a-t-il assez d'argent cash ?
        if (compteCourant.GetSolde().centimes < annonce.PrixVenteAffiche.centimes)
        {
            Debug.LogWarning("[ServiceImmobilier] Achat échoué : Fonds insuffisants sur le compte courant.");
            return false;
        }

        // Règle 2 : Débit immédiat du compte courant
        compteCourant.AjoutHistorique("Achat immobilier", -annonce.PrixVenteAffiche);

        // Règle 3 : Création du bien immobilier dans le patrimoine du joueur
        BienImmobilier nouveauBien = new BienImmobilier
        {
            ville = annonce.Ville,
            type = annonce.Type,
            surfaceM2 = annonce.SurfaceM2,
            estMeuble = annonce.EstMeuble,
            moisAchat = Math.Max(0, nombreMoisPasses),
            indicePrixReferenceAchat = CalculerIndicePrixReference(
                annonce.Ville,
                annonce.Type,
                nombreMoisPasses,
                joueur.immobilier),
            valeurReferenceAchatCentimes =
                annonce.PrixVenteAffiche.centimes,            prixAchat = annonce.PrixVenteAffiche,
            valeurActuelle = annonce.PrixVenteAffiche, // Égal au prix d'achat au jour J
            estLoue = true, // Loué directement en v1
            tauxRendementInitial = annonce.TauxRendementBrut,
            loyerInitial = annonce.LoyerMensuelPropose,
            loyerMensuel = annonce.LoyerMensuelPropose
        };

        joueur.immobilier.biensPossedes.Add(nouveauBien);
        
        // Retirer l'annonce du marché pour qu'elle ne soit plus achetable deux fois
        joueur.immobilier.annoncesActuelles.Remove(annonce);
        
        // Forcer le recalcul immédiat du patrimoine total du joueur
        // La valeurActuelle est portée par BienImmobilier directement, pas par DonneesJoueur.

        Debug.Log($"[ServiceImmobilier] Succès ! Achat de {annonce.Type} à {annonce.Ville} pour {annonce.PrixVenteAffiche.centimes / 100f} €");
        return true;
    }

    /// <summary>
    /// Calcule l'estimation actuelle du bien basée sur le vrai prix du marché (m2 * surface * qualité).
    /// </summary>
    public static float CalculerIndicePrixReference(
        Ville ville,
        TypeBien type,
        int nombreMoisPasses,
        DonneesImmobilier donneesImmobilier = null)
    {
        string villeId = ville.ToString().ToLowerInvariant();
        float prixM2 =
            MarcheImmobilier.ObtenirPrixM2(
                villeId,
                Math.Max(0, nombreMoisPasses));
        float coefficient =
            ServiceImpactsImmobiliers.CalculerCoefficientPrix(
                donneesImmobilier,
                ville,
                type,
                Math.Max(0, nombreMoisPasses));

        return Math.Max(0f, prixM2 * coefficient);
    }

    public static argent CalculerValeurActuelle(
        BienImmobilier bien,
        int nombreMoisPasses,
        DonneesImmobilier donneesImmobilier = null)
    {
        if (bien == null)
        {
            return new argent(0);
        }

        int valeurExistante =
            Math.Max(0, bien.valeurActuelle.centimes);
        int prixAchatCentimes =
            Math.Max(0, bien.prixAchat.centimes);
        float indiceActuel =
            CalculerIndicePrixReference(
                bien.ville,
                bien.type,
                nombreMoisPasses,
                donneesImmobilier);

        if (prixAchatCentimes > 0 && indiceActuel > 0f)
        {
            if (bien.indicePrixReferenceAchat <= 0f ||
                bien.valeurReferenceAchatCentimes <= 0)
            {
                bien.moisAchat = bien.moisAchat >= 0
                    ? bien.moisAchat
                    : Math.Max(0, nombreMoisPasses);
                bien.indicePrixReferenceAchat = indiceActuel;
                bien.valeurReferenceAchatCentimes =
                    valeurExistante > 0
                        ? valeurExistante
                        : prixAchatCentimes;
            }

            double ratio =
                indiceActuel / bien.indicePrixReferenceAchat;
            long valeurCentimes =
                (long)Math.Round(
                    bien.valeurReferenceAchatCentimes * ratio);

            return new argent(
                (int)Math.Clamp(
                    valeurCentimes,
                    0,
                    int.MaxValue));
        }

        if (valeurExistante > 0)
        {
            return new argent(valeurExistante);
        }

        return new argent(prixAchatCentimes);
    }

    /// <summary>
    /// Calcule le loyer courant en séparant la tendance historique du prix
    /// et le coefficient temporaire propre aux loyers.
    /// </summary>
    public static argent CalculerLoyerMensuelActuel(
        BienImmobilier bien,
        int nombreMoisPasses,
        DonneesImmobilier donneesImmobilier = null)
    {
        if (bien == null || bien.prixAchat.centimes <= 0)
        {
            return bien?.loyerMensuel ?? new argent(0);
        }

        DefinitionBienImmobilier def =
            TrouverDefinitionAssociee(bien.ville, bien.type);
        if (def == null)
        {
            return bien.loyerMensuel;
        }

        float prixM2Base =
            MarcheImmobilier.ObtenirPrixM2(
                def.VilleId,
                nombreMoisPasses);
        double valeurBaseCentimes =
            prixM2Base *
            def.SurfaceM2 *
            def.FacteurQualite *
            100d;
        double evolution =
            (valeurBaseCentimes - bien.prixAchat.centimes) /
            bien.prixAchat.centimes;

        double coefficientLoyer =
            ServiceImpactsImmobiliers.CalculerCoefficientLoyer(
                donneesImmobilier,
                bien.ville,
                bien.type,
                nombreMoisPasses);
        double centimes =
            bien.loyerInitial.centimes *
            Math.Max(0d, 1d + evolution) *
            coefficientLoyer;

        return new argent(
            (int)Math.Clamp(
                (long)Math.Round(centimes),
                0,
                int.MaxValue));
    }

    /// <summary>
    /// Recalcule et indexe les loyers annuellement (+/- x% d'évolution de la valeur).
    /// À appeler tous les 12 mois.
    /// </summary>

    public static void ActualiserLoyersAnnuels(
        DonneesJoueur joueur,
        int nombreMoisPasses)
    {
        if (joueur?.immobilier?.biensPossedes == null)
        {
            return;
        }

        foreach (BienImmobilier bien in joueur.immobilier.biensPossedes)
        {
            if (bien == null)
            {
                continue;
            }

            bien.valeurActuelle = CalculerValeurActuelle(
                bien,
                nombreMoisPasses,
                joueur.immobilier);
            bien.loyerMensuel = CalculerLoyerMensuelActuel(
                bien,
                nombreMoisPasses,
                joueur.immobilier);
        }

        Debug.Log(
            "[ServiceImmobilier] Indexation annuelle et impacts immobiliers actualisés.");
    }

    // --- UTILS DE CORRESPONDANCE LOGIQUE ---

    public static DefinitionBienImmobilier TrouverDefinitionAssociee(Ville ville, TypeBien type)
    {
        string villeCle = ville.ToString().ToLower();
        var catalogue = CatalogueImmobilier.ObtenirBiens();

        foreach (var def in catalogue)
        {
            if (def.VilleId.ToLower() == villeCle && MaperTypeCatalogueVersJoueur(def.TypeBien) == type)
            {
                return def;
            }
        }
        return null;
    }

    private static TypeBien MaperTypeCatalogueVersJoueur(TypeBienImmobilier typeCatalogue)
    {
        return typeCatalogue switch
        {
            TypeBienImmobilier.Studio => TypeBien.Studio,
            TypeBienImmobilier.Appartement => TypeBien.AppartementT2, // Redirection automatique par défaut
            TypeBienImmobilier.ImmeubleRapport => TypeBien.ImmeubleRapport,
            TypeBienImmobilier.LocalCommercial => TypeBien.LocalCommercial,
            _ => TypeBien.Studio
        };
    }
}
