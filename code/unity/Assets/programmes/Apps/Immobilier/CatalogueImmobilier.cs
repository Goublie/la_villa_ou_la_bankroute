using System;
using System.Collections.Generic;

public static class CatalogueImmobilier
{
    private static List<DefinitionBienImmobilier> _biens;

    public static IReadOnlyList<DefinitionBienImmobilier> ObtenirBiens()
    {
        if (_biens == null)
        {
            _biens = ConstruireCatalogue();
        }
        return _biens;
    }

    /// <summary>
    /// Calcule le prix d'achat d'un bien en euros à un mois spécifique du jeu.
    /// </summary>
    public static float CalculerPrixAchatEuros(DefinitionBienImmobilier bien, int nombreMoisPasses)
    {
        if (bien == null) return 0f;

        float prixM2Ville = MarcheImmobilier.ObtenirPrixM2(bien.VilleId, nombreMoisPasses);
        return prixM2Ville * bien.SurfaceM2 * bien.FacteurQualite;
    }

    /// <summary>
    /// Calcule le prix d'achat d'un bien converti de manière sécurisée en structure 'argent' (centimes).
    /// </summary>
    public static argent CalculerPrixAchatArgent(DefinitionBienImmobilier bien, int nombreMoisPasses)
    {
        float prixEuros = CalculerPrixAchatEuros(bien, nombreMoisPasses);
        
        // Arrondi mathématique strict à l'unité supérieure pour les centimes (* 100)
        long centimes = (long)Math.Round(prixEuros * 100d);
        
        if (centimes > int.MaxValue) centimes = int.MaxValue;
        if (centimes < 0) centimes = 0;

        return new argent((int)centimes);
    }

    private static List<DefinitionBienImmobilier> ConstruireCatalogue()
    {
        var liste = new List<DefinitionBienImmobilier>();

        // Paris
        liste.Add(new DefinitionBienImmobilier(
            "paris_studio_premium", "Studio Lumineux - Marais", "paris", 
            TypeBienImmobilier.Studio, 20, "Idéal premier investissement, excellent secteur locatif.", 1.15f));
            
        liste.Add(new DefinitionBienImmobilier(
            "paris_appartement_familial", "Appartement 3 pièces - Bastille", "paris", 
            TypeBienImmobilier.Appartement, 65, "Bel appartement ancien avec travaux à prévoir.", 0.95f));

        // Bordeaux
        liste.Add(new DefinitionBienImmobilier(
            "bordeaux_studio_etudiant", "Studio Étudiant - Victoire", "bordeaux", 
            TypeBienImmobilier.Studio, 22, "Proche des commodités et des universités.", 1.0f));

        liste.Add(new DefinitionBienImmobilier(
            "bordeaux_immeuble_rapport", "Immeuble de Rapport - Centre", "bordeaux", 
            TypeBienImmobilier.ImmeubleRapport, 180, "4 appartements entièrement loués, forte rentabilité.", 1.05f));

        // Lyon
        liste.Add(new DefinitionBienImmobilier(
            "lyon_local_commercial", "Local Commercial - Presqu'île", "lyon", 
            TypeBienImmobilier.LocalCommercial, 45, "Boutique avec vitrine sur rue piétonne passante.", 1.20f));

        return liste;
    }
}