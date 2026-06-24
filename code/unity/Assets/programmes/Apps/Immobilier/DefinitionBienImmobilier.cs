using System;

/// <summary>
/// Types de biens immobiliers disponibles à l'achat.
/// </summary>
public enum TypeBienImmobilier
  {
    Studio,
    Appartement,
    ImmeubleRapport,
    LocalCommercial
  }

/// <summary>
/// Définition immuable d'un bien immobilier disponible dans le catalogue.
/// </summary>
public sealed class DefinitionBienImmobilier
{
    public string Id { get; }
    public string Nom { get; }
    public string VilleId { get; } // Exemple: "paris", "bordeaux"
    public TypeBienImmobilier TypeBien { get; }
    public int SurfaceM2 { get; }
    public string Description { get; }
    
    /// <summary>
    /// Multiplicateur appliqué au prix moyen au m2 de la ville pour ce bien spécifique 
    /// (ex: 1.2f pour un bien premium, 0.8f pour un bien à rénover).
    /// </summary>
    public float FacteurQualite { get; }

    public DefinitionBienImmobilier(
        string id, 
        string nom, 
        string villeId, 
        TypeBienImmobilier typeBien, 
        int surfaceM2, 
        string description, 
        float facteurQualite = 1.0f)
    {
        Id = id;
        Nom = nom;
        VilleId = villeId?.ToLower() ?? throw new ArgumentNullException(nameof(villeId));
        TypeBien = typeBien;
        SurfaceM2 = surfaceM2;
        Description = description;
        FacteurQualite = facteurQualite;
    }
}