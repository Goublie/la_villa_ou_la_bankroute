/// <summary>
/// Modificateurs metier associes a un secteur, un public ou une technologie.
/// </summary>
public readonly struct DefinitionChoixEntrepreneurial
{
    /// <summary>
    /// Cree les modificateurs d'un choix de creation.
    /// </summary>
    /// <param name="nom">Libelle affiche.</param>
    /// <param name="difficulte">Variation de difficulte en points.</param>
    /// <param name="potentiel">Variation de potentiel en points.</param>
    /// <param name="concurrence">Variation de concurrence en points.</param>
    /// <param name="coutEuros">Variation du cout de lancement en euros.</param>
    /// <param name="probabilite">
    /// Variation de probabilite de succes en points de pourcentage.
    /// </param>
    public DefinitionChoixEntrepreneurial(
        string nom,
        int difficulte,
        int potentiel,
        int concurrence,
        int coutEuros,
        int probabilite)
    {
        Nom = nom;
        Difficulte = difficulte;
        Potentiel = potentiel;
        Concurrence = concurrence;
        CoutEuros = coutEuros;
        Probabilite = probabilite;
    }

    /// <summary>Libelle affiche.</summary>
    public string Nom { get; }

    /// <summary>Variation de difficulte en points.</summary>
    public int Difficulte { get; }

    /// <summary>Variation de potentiel de marche en points.</summary>
    public int Potentiel { get; }

    /// <summary>Variation de concurrence en points.</summary>
    public int Concurrence { get; }

    /// <summary>Variation du cout de lancement en euros.</summary>
    public int CoutEuros { get; }

    /// <summary>Variation de probabilite en points de pourcentage.</summary>
    public int Probabilite { get; }
}

/// <summary>
/// Catalogue centralise des choix disponibles pour creer une entreprise.
/// </summary>
public static class CatalogueEntrepreneuriat
{
    private static readonly DefinitionChoixEntrepreneurial[] Secteurs =
    {
        new DefinitionChoixEntrepreneurial("Finance", 12, 14, 12, 2500, -3),
        new DefinitionChoixEntrepreneurial("Sante", 18, 18, 5, 4500, -6),
        new DefinitionChoixEntrepreneurial("Education", -6, 2, -4, -1000, 5),
        new DefinitionChoixEntrepreneurial("Immobilier", 7, 8, 4, 1800, -1),
        new DefinitionChoixEntrepreneurial("Transport", 14, 10, 5, 3500, -4),
        new DefinitionChoixEntrepreneurial("Commerce", 0, 6, 12, 0, 0),
        new DefinitionChoixEntrepreneurial("Divertissement", 2, 8, 10, 500, 0),
        new DefinitionChoixEntrepreneurial("Cybersecurite", 17, 16, 3, 3200, -4),
        new DefinitionChoixEntrepreneurial("Energie", 20, 20, 3, 5500, -8),
        new DefinitionChoixEntrepreneurial("Reseaux sociaux", 4, 15, 24, 1000, -4)
    };

    private static readonly DefinitionChoixEntrepreneurial[] Publics =
    {
        new DefinitionChoixEntrepreneurial("Etudiants", -5, 0, 0, -700, 4),
        new DefinitionChoixEntrepreneurial("Jeunes actifs", 0, 8, 7, 0, 1),
        new DefinitionChoixEntrepreneurial("Familles", 3, 10, 5, 500, -1),
        new DefinitionChoixEntrepreneurial("Seniors", 5, 4, -2, 500, -1),
        new DefinitionChoixEntrepreneurial("Entreprises", 8, 13, -5, 1500, 2),
        new DefinitionChoixEntrepreneurial("Independants", 0, 2, -2, 0, 2),
        new DefinitionChoixEntrepreneurial("Investisseurs", 10, 11, 5, 1000, -3),
        new DefinitionChoixEntrepreneurial("Sportifs", 3, 5, 0, 400, 0),
        new DefinitionChoixEntrepreneurial("Createurs de contenu", 2, 9, 8, 0, 0),
        new DefinitionChoixEntrepreneurial("Grand public", 8, 20, 18, 2200, -5)
    };

    private static readonly DefinitionChoixEntrepreneurial[] Technologies =
    {
        new DefinitionChoixEntrepreneurial("Application mobile", -4, 5, 8, -500, 4),
        new DefinitionChoixEntrepreneurial("Plateforme web", -2, 4, 5, -300, 3),
        new DefinitionChoixEntrepreneurial("Intelligence artificielle", 18, 18, 5, 4000, -7),
        new DefinitionChoixEntrepreneurial("Blockchain", 20, 15, 10, 4500, -9),
        new DefinitionChoixEntrepreneurial("Objets connectes", 16, 12, 3, 3800, -6),
        new DefinitionChoixEntrepreneurial("Marketplace", 7, 12, 18, 1500, -2),
        new DefinitionChoixEntrepreneurial("SaaS", 7, 13, 5, 1200, 1),
        new DefinitionChoixEntrepreneurial("Data analyse", 10, 12, 3, 2000, -2),
        new DefinitionChoixEntrepreneurial("Automatisation", 8, 10, 2, 1500, 0),
        new DefinitionChoixEntrepreneurial("Jeu video / simulation", 10, 14, 14, 2500, -3)
    };

    /// <summary>Nombre de secteurs disponibles.</summary>
    public static int NombreSecteurs => Secteurs.Length;

    /// <summary>Nombre de publics disponibles.</summary>
    public static int NombrePublics => Publics.Length;

    /// <summary>Nombre de technologies disponibles.</summary>
    public static int NombreTechnologies => Technologies.Length;

    /// <summary>Retourne les modificateurs du secteur indique.</summary>
    public static DefinitionChoixEntrepreneurial ObtenirSecteur(
        SecteurEntrepreneurial secteur)
    {
        return Secteurs[(int)secteur];
    }

    /// <summary>Retourne les modificateurs du public indique.</summary>
    public static DefinitionChoixEntrepreneurial ObtenirPublic(
        PublicEntrepreneurial publicCible)
    {
        return Publics[(int)publicCible];
    }

    /// <summary>Retourne les modificateurs de la technologie indiquee.</summary>
    public static DefinitionChoixEntrepreneurial ObtenirTechnologie(
        TechnologieEntrepreneuriale technologie)
    {
        return Technologies[(int)technologie];
    }
}
