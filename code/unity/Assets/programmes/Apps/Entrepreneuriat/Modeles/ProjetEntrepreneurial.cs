using System;

public enum SecteurEntrepreneurial
{
    Finance,
    Sante,
    Education,
    Immobilier,
    Transport,
    Commerce,
    Divertissement,
    Cybersecurite,
    Energie,
    ReseauxSociaux
}

public enum PublicEntrepreneurial
{
    Etudiants,
    JeunesActifs,
    Familles,
    Seniors,
    Entreprises,
    Independants,
    Investisseurs,
    Sportifs,
    CreateursContenu,
    GrandPublic
}

public enum TechnologieEntrepreneuriale
{
    ApplicationMobile,
    PlateformeWeb,
    IntelligenceArtificielle,
    Blockchain,
    ObjetsConnectes,
    Marketplace,
    Saas,
    DataAnalyse,
    Automatisation,
    JeuSimulation
}

/// <summary>
/// Etat sauvegardable du projet entrepreneurial du joueur.
/// </summary>
[Serializable]
public class ProjetEntrepreneurial : IPatrimoine
{
    public SecteurEntrepreneurial secteur;
    public PublicEntrepreneurial publicCible;
    public TechnologieEntrepreneuriale technologie;
    public bool estCree;
    public int progressionProduit;
    public int tractionMarche;
    public int reputation = 5;
    public int connaissanceMarche;
    public int tresorerieCentimes;
    public int nombrePivots;
    public int bonusCompatibilite;
    public int valorisationCentimes;

    /// <summary>
    /// Memorise la valorisation estimee, exprimee en centimes.
    /// </summary>
    public void DefinirValorisation(int valeurCentimes)
    {
        valorisationCentimes = Math.Max(0, valeurCentimes);
    }

    /// <inheritdoc />
    public argent GetValeurPatrimoine()
    {
        return new argent(estCree ? valorisationCentimes : 0);
    }

    /// <summary>
    /// Produit une copie profonde du projet pour les snapshots et simulations.
    /// </summary>
    public ProjetEntrepreneurial Copier()
    {
        return new ProjetEntrepreneurial
        {
            secteur = secteur,
            publicCible = publicCible,
            technologie = technologie,
            estCree = estCree,
            progressionProduit = progressionProduit,
            tractionMarche = tractionMarche,
            reputation = reputation,
            connaissanceMarche = connaissanceMarche,
            tresorerieCentimes = tresorerieCentimes,
            nombrePivots = nombrePivots,
            bonusCompatibilite = bonusCompatibilite,
            valorisationCentimes = valorisationCentimes
        };
    }
}

/// <summary>
/// Profil derive des choix stratégiques et utilise par les regles du service.
/// </summary>
public readonly struct ProfilProjetEntrepreneurial
{
    public ProfilProjetEntrepreneurial(
        int difficulte,
        int potentielMarche,
        int risqueConcurrentiel,
        int compatibilite,
        int coutLancementCentimes,
        int probabiliteBase,
        int valorisationBaseCentimes)
    {
        Difficulte = difficulte;
        PotentielMarche = potentielMarche;
        RisqueConcurrentiel = risqueConcurrentiel;
        Compatibilite = compatibilite;
        CoutLancementCentimes = coutLancementCentimes;
        ProbabiliteBase = probabiliteBase;
        ValorisationBaseCentimes = valorisationBaseCentimes;
    }

    public int Difficulte { get; }
    public int PotentielMarche { get; }
    public int RisqueConcurrentiel { get; }
    public int Compatibilite { get; }
    public int CoutLancementCentimes { get; }
    public int ProbabiliteBase { get; }
    public int ValorisationBaseCentimes { get; }
}
