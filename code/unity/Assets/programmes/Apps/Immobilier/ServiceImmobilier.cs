using System;
using UnityEngine;

/// <summary>
/// Structure éphémère représentant une opportunité sur le marché immobilier.
/// </summary>
public class AnnonceImmobiliere
{
    public DefinitionBienImmobilier Definition; // Lien direct vers le gabarit d'origine
    public Ville Ville;
    public TypeBien Type;
    public argent PrixVenteAffiche;
    public argent LoyerMensuelPropose;
    public float TauxRendementBrut;
}

public static class ServiceImmobilier
{
    /// <summary>
    /// Génère une annonce sur le marché avec les fluctuations de bonne/mauvaise affaire (Prix 0.95-1.05 et Renta 4%-7%).
    /// </summary>
    public static AnnonceImmobiliere GenererAnnonceSurLeMarche(DefinitionBienImmobilier definition, int nombreMoisPasses)
    {
        // 1. Détermination des Enums correspondants pour l'UI et le joueur
        Ville villeEnum = (Ville)Enum.Parse(typeof(Ville), definition.VilleId, true);
        TypeBien typeEnum = MaperTypeCatalogueVersJoueur(definition.Type);

        // 2. Calcul du vrai prix au m2 issu du marché mondial
        float prixM2Actuel = MarcheImmobilier.ObtenirPrixM2(definition.VilleId, nombreMoisPasses);
        float valeurTheoriqueEuros = prixM2Actuel * definition.SurfaceM2 * definition.FacteurQualite;

        // 3. Application de l'oscillation aléatoire de l'opportunité (0.95 à 1.05)
        float multiplicateurPrix = UnityEngine.Random.Range(0.95f, 1.05f);
        float prixAfficheEuros = valeurTheoriqueEuros * multiplicateurPrix;
        long prixCentimes = (long)Math.Round(prixAfficheEuros * 100d);
        argent prixVente = new argent((int)Math.Clamp(prixCentimes, 0, int.MaxValue));

        // 4. Calcul du loyer mystère (Taux de rendement aléatoire entre 4% et 7% par an)
        float tauxRendement = UnityEngine.Random.Range(0.04f, 0.07f);
        double loyerAnnuelEuros = prixAfficheEuros * tauxRendement;
        double loyerMensuelEuros = loyerAnnuelEuros / 12d;
        long loyerCentimes = (long)Math.Round(loyerMensuelEuros * 100d);
        argent loyerMensuel = new argent((int)Math.Clamp(loyerCentimes, 0, int.MaxValue));

        return new AnnonceImmobiliere
        {
            Definition = definition,
            Ville = villeEnum,
            Type = typeEnum,
            PrixVenteAffiche = prixVente,
            LoyerMensuelPropose = loyerMensuel,
            TauxRendementBrut = tauxRendement
        };
    }

    /// <summary>
    /// Traite l'achat cash brut. Si le joueur a assez, ça passe, sinon ça bloque.
    /// </summary>
    public static bool ExecuterAchatCash(DonneesJoueur joueur, AnnonceImmobiliere annonce, int nombreMoisPasses)
    {
        var compteCourant = joueur.comptes[ServiceBanque.CompteCourantId];

        // Règle 1 : Le joueur a-t-il assez d'argent cash ?
        if (compteCourant.Solde.centimes < annonce.PrixVenteAffiche.centimes)
        {
            Debug.LogWarning("[ServiceImmobilier] Achat échoué : Fonds insuffisants sur le compte courant.");
            return false;
        }

        // Règle 2 : Débit immédiat du compte courant
        compteCourant.Retirer(annonce.PrixVenteAffiche);

        // Règle 3 : Création du bien immobilier dans le patrimoine du joueur
        BienImmobilier nouveauBien = new BienImmobilier
        {
            ville = annonce.Ville,
            type = annonce.Type,
            prixAchat = annonce.PrixVenteAffiche,
            valeurActuelle = annonce.PrixVenteAffiche, // Égal au prix d'achat au jour J
            estLoue = true, // Loué directement en v1
            tauxRendementInitial = annonce.TauxRendementBrut,
            loyerInitial = annonce.LoyerMensuelPropose,
            loyerMensuel = annonce.LoyerMensuelPropose
        };

        joueur.immobilier.biensPossedes.Add(nouveauBien);
        
        // Forcer le recalcul immédiat du patrimoine total du joueur
        joueur.valeurActuelle = nouveauBien.valeurActuelle; 

        Debug.Log($"[ServiceImmobilier] Succès ! Achat de {annonce.Type} à {annonce.Ville} pour {annonce.PrixVenteAffiche.centimes / 100f} €");
        return true;
    }

    /// <summary>
    /// Calcule l'estimation actuelle du bien basée sur le vrai prix du marché (m2 * surface * qualité).
    /// </summary>
    public static argent CalculerValeurActuelle(BienImmobilier bien, int nombreMoisPasses)
    {
        DefinitionBienImmobilier def = TrouverDefinitionAssociee(bien.ville, bien.type);
        if (def == null) return bien.prixAchat;

        float prixM2Actuel = MarcheImmobilier.ObtenirPrixM2(def.VilleId, nombreMoisPasses);
        float valeurEuros = prixM2Actuel * def.SurfaceM2 * def.FacteurQualite;

        long centimes = (long)Math.Round(valeurEuros * 100d);
        return new argent((int)Math.Clamp(centimes, 0, int.MaxValue));
    }

    /// <summary>
    /// Recalcule et indexe les loyers annuellement (+/- x% d'évolution de la valeur).
    /// À appeler tous les 12 mois.
    /// </summary>
    public static void ActualiserLoyersAnnuels(DonneesJoueur joueur, int nombreMoisPasses)
    {
        foreach (var bien in joueur.immobilier.biensPossedes)
        {
            if (bien.prixAchat.centimes == 0) continue;

            // 1. On met à jour l'estimation marchande du bâtiment
            bien.valeurActuelle = CalculerValeurActuelle(bien, nombreMoisPasses);

            // 2. Calcul du coefficient d'évolution relative (x) par rapport au prix d'achat de base
            double evolution = (double)(bien.valeurActuelle.centimes - bien.prixAchat.centimes) / bien.prixAchat.centimes;

            // 3. Le loyer suit exactement la même courbe : Nouveau Loyer = LoyerInitial * (1 + x)
            double nouveauLoyerCentimes = bien.loyerInitial.centimes * (1d + evolution);
            
            bien.loyerMensuel = new argent((int)Math.Clamp((long)Math.Round(nouveauLoyerCentimes), 0, int.MaxValue));
        }
        Debug.Log("[ServiceImmobilier] Indexation annuelle des loyers et mise à jour des estimations immobilières terminées.");
    }

    // --- UTILS DE CORRESPONDANCE LOGIQUE ---

    private static DefinitionBienImmobilier TrouverDefinitionAssociee(Ville ville, TypeBien type)
    {
        string villeCle = ville.ToString().ToLower();
        var catalogue = CatalogueImmobilier.ObtenirBiens();

        foreach (var def in catalogue)
        {
            if (def.VilleId.ToLower() == villeCle && MaperTypeCatalogueVersJoueur(def.Type) == type)
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