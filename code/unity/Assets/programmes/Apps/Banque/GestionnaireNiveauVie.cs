/// <summary>
/// Calcule le coût mensuel total du niveau de vie du joueur.
/// Barème modifiable ici sans toucher au reste du code.
/// </summary>
public static class GestionnaireNiveauVie
{
    // ---------------------------------------------------------------
    // BARÈME (coût mensuel en euros pour chaque niveau 1→5)
    // ---------------------------------------------------------------

    private static readonly int[] CoutLogement     = { 0,  400,  600,  800, 1000, 1500 };
    private static readonly int[] CoutSport        = { 0,   20,   40,   80,  150,  250 };
    private static readonly int[] CoutTransport    = { 0,   50,  100,  200,  350,  600 };
    private static readonly int[] CoutAlimentation = { 0,  150,  250,  380,  550,  800 };
    private static readonly int[] CoutVieSociale   = { 0,   30,   80,  150,  280,  450 };

    // ---------------------------------------------------------------

    /// <summary>
    /// Calcule le coût mensuel total en centimes à partir des données du joueur.
    /// </summary>
    public static argent CalculerCoutMensuel(DonneesNiveauVie niveauVie)
    {
        if (niveauVie == null) return new argent(0);

        int totalEuros =
            CoutLogement    [DonneesNiveauVie.Borner(niveauVie.logement)]     +
            CoutSport       [DonneesNiveauVie.Borner(niveauVie.sport)]        +
            CoutTransport   [DonneesNiveauVie.Borner(niveauVie.transport)]    +
            CoutAlimentation[DonneesNiveauVie.Borner(niveauVie.alimentation)] +
            CoutVieSociale  [DonneesNiveauVie.Borner(niveauVie.vieSociale)];

        return new argent(totalEuros * 100); // conversion euros → centimes
    }

    /// <summary>
    /// Applique les effets mensuels du niveau de vie sur l'énergie et la santé mentale du joueur.
    /// Le niveau 2 est la référence neutre (0 effet). En dessous = malus, au-dessus = bonus.
    /// Résultat clampé entre 0 et 100.
    /// </summary>
    public static void AppliquerEffetsMensuels(DonneesNiveauVie niveauVie, DonneesJoueur joueur)
    {
        if (niveauVie == null || joueur == null) return;

        int deltaEnergie     = 0;
        int deltaSanteMentale = 0;

        // --- LOGEMENT (impact : santeMentale + énergie) ---
        // Minimum 1 car obligatoire, donc pas de case 0 nécessaire
        switch (DonneesNiveauVie.Borner(niveauVie.logement))
        {
            case 1: deltaEnergie -= 3; deltaSanteMentale -= 5; break;
            case 2: break; // neutre
            case 3: deltaSanteMentale += 3; break;
            case 4: deltaEnergie += 2; deltaSanteMentale += 6; break;
            case 5: deltaEnergie += 5; deltaSanteMentale += 10; break;
        }

        // --- SPORT (impact : énergie + santeMentale) ---
        switch (DonneesNiveauVie.Borner(niveauVie.sport))
        {
            case 0: deltaEnergie -= 8; deltaSanteMentale -= 2; break; // Aucun sport (pires stats)
            case 1: deltaEnergie -= 5; break;
            case 2: break; // neutre
            case 3: deltaEnergie += 5; break;
            case 4: deltaEnergie += 10; deltaSanteMentale += 3; break;
            case 5: deltaEnergie += 15; deltaSanteMentale += 5; break;
        }

        // --- TRANSPORT (impact : énergie) ---
        switch (DonneesNiveauVie.Borner(niveauVie.transport))
        {
            case 0: deltaEnergie -= 5; break; // Tout à pied (fatigue supplémentaire)
            case 1: deltaEnergie -= 3; break;
            case 2: break; // neutre
            case 3: deltaEnergie += 2; break;
            case 4: deltaEnergie += 4; break;
            case 5: deltaEnergie += 6; deltaSanteMentale += 3; break;
        }

        // --- ALIMENTATION (impact : énergie + santeMentale) ---
        // Minimum 1 car obligatoire, donc pas de case 0 nécessaire
        switch (DonneesNiveauVie.Borner(niveauVie.alimentation))
        {
            case 1: deltaEnergie -= 5; deltaSanteMentale -= 2; break;
            case 2: break; // neutre
            case 3: deltaEnergie += 5; break;
            case 4: deltaEnergie += 8; deltaSanteMentale += 3; break;
            case 5: deltaEnergie += 10; deltaSanteMentale += 6; break;
        }

        // --- VIE SOCIALE (impact : santeMentale) ---
        switch (DonneesNiveauVie.Borner(niveauVie.vieSociale))
        {
            case 0: deltaSanteMentale -= 12; break; // Solitude complète (fort malus)
            case 1: deltaSanteMentale -= 8; break;
            case 2: deltaSanteMentale -= 2; break;
            case 3: deltaSanteMentale += 5; break;
            case 4: deltaSanteMentale += 10; break;
            case 5: deltaEnergie += 3; deltaSanteMentale += 15; break;
        }

        // Application avec clamping 0–100
        joueur.energie      = System.Math.Clamp(joueur.energie      + deltaEnergie,      0, 100);
        joueur.santeMentale = System.Math.Clamp(joueur.santeMentale + deltaSanteMentale, 0, 100);
    }

    /// <summary>
    /// Retourne le coût mensuel d'une seule catégorie à un niveau donné.
    /// Utile pour l'affichage dans l'UI.
    /// </summary>
    public static argent CoutCategorie(CategorieNiveauVie categorie, int niveau)
    {
        int niveauBorne = DonneesNiveauVie.Borner(niveau);
        int euros = categorie switch
        {
            CategorieNiveauVie.Logement     => CoutLogement    [niveauBorne],
            CategorieNiveauVie.Sport        => CoutSport       [niveauBorne],
            CategorieNiveauVie.Transport    => CoutTransport   [niveauBorne],
            CategorieNiveauVie.Alimentation => CoutAlimentation[niveauBorne],
            CategorieNiveauVie.VieSociale   => CoutVieSociale  [niveauBorne],
            _ => 0
        };
        return new argent(euros * 100);
    }
}

/// <summary>Enum des 5 catégories de niveau de vie.</summary>
public enum CategorieNiveauVie
{
    Logement,
    Sport,
    Transport,
    Alimentation,
    VieSociale
}
