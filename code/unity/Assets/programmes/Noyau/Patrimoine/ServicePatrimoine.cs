using System.Collections.Generic;

/// <summary>
/// Calcule la valeur patrimoniale du joueur depuis ses agregats metier.
/// </summary>
public static class ServicePatrimoine
{
    /// <summary>
    /// Additionne les comptes, placements autonomes et portefeuilles.
    /// </summary>
    /// <remarks>
    /// Le moteur d'interets d'un Livret A est explicitement ignore dans la
    /// liste des investissements, car son solde est deja porte par le compte.
    /// </remarks>
    public static argent Calculer(DonneesJoueur joueur)
    {
        if (joueur == null)
        {
            return new argent(0);
        }

        long totalCentimes = 0;
        HashSet<Investissement> placementsDejaComptes =
            new HashSet<Investissement>();

        if (joueur.comptes != null)
        {
            foreach (CompteBanquaire compte in joueur.comptes.Values)
            {
                if (compte == null)
                {
                    continue;
                }

                totalCentimes += compte.GetValeurPatrimoine().centimes;
                if (compte is Epargne epargne && epargne.invest != null)
                {
                    placementsDejaComptes.Add(epargne.invest);
                }
            }
        }

        // Les prêts sont des passifs : DonneesPret retourne déjà
        // une valeur patrimoniale négative correspondant au capital restant dû.
        if (joueur.pretsImmobiliers != null)
        {
            foreach (DonneesPret pret in joueur.pretsImmobiliers)
            {
                if (pret != null)
                {
                    totalCentimes += pret.GetValeurPatrimoine().centimes;
                }
            }
        }

        // Les biens immobiliers possédés sont des actifs patrimoniaux.
        if (joueur.immobilier?.biensPossedes != null)
        {
            foreach (BienImmobilier bien in joueur.immobilier.biensPossedes)
            {
                if (bien != null)
                {
                    totalCentimes += bien.GetValeurPatrimoine().centimes;
                }
            }
        }
        if (joueur.investissements != null)
        {
            foreach (Investissement investissement in joueur.investissements)
            {
                if (investissement != null &&
                    !placementsDejaComptes.Contains(investissement))
                {
                    totalCentimes +=
                        investissement.GetValeurPatrimoine().centimes;
                }
            }
        }

        if (joueur.bourse != null)
        {
            totalCentimes += joueur.bourse.GetValeurPatrimoine().centimes;
        }

        if (joueur.entrepreneuriat != null)
        {
            totalCentimes +=
                joueur.entrepreneuriat.GetValeurPatrimoine().centimes;
        }

        if (totalCentimes > int.MaxValue)
        {
            totalCentimes = int.MaxValue;
        }
        else if (totalCentimes < int.MinValue)
        {
            totalCentimes = int.MinValue;
        }

        return new argent((int)totalCentimes);
    }
}
