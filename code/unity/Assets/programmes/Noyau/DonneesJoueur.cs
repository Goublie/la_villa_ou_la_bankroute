using System;
using System.Collections.Generic;

/// <summary>
/// Agregat racine de l'etat personnel, financier et professionnel du joueur.
/// </summary>
/// <remarks>
/// Cette classe ne contient aucune reference vers des composants UI Unity.
/// Chaque sous-systeme sauvegardable doit y exposer ses donnees et fournir une
/// copie profonde utilisable par les snapshots et le mode What If.
/// </remarks>
[Serializable]
public class DonneesJoueur
{
    /// <summary>
    /// Comptes bancaires indexes par identifiant fonctionnel.
    /// </summary>
    public Dictionary<string, CompteBanquaire> comptes;

    /// <summary>
    /// Liste des prets immobiliers actifs du joueur.
    /// </summary>
    public List<DonneesPret> pretsImmobiliers;

    /// <summary>
    /// Salaire mensuel, exprime en centimes.
    /// </summary>
    public argent salaire = new argent(0);

    /// <summary>
    /// Energie disponible, bornee entre 0 et 100.
    /// </summary>
    public int energie = 100;

    /// <summary>
    /// Sante mentale disponible, bornee entre 0 et 100.
    /// </summary>
    public int santeMentale = 100;

    /// <summary>
    /// Placements fixes autonomes, hors Livret A porte par les comptes.
    /// </summary>
    public List<Investissement> investissements;

    /// <summary>
    /// Etat persistant du portefeuille boursier.
    /// </summary>
    public DonneesBourse bourse;

    /// <summary>
    /// Etat persistant de l'entreprise et de son projet courant.
    /// </summary>
    public DonneesEntrepreneuriat entrepreneuriat;

    /// <summary>
    /// Etat persistant du parcours salarie.
    /// </summary>
    public DonneesSalariat salariat;

    /// <summary>
    /// Repartition du temps mensuel disponible par application.
    /// </summary>
    public DonneesRepartitionTemps tempsApplications;

    /// <summary>
    /// Choix de niveau de vie du joueur (Logement, Sport, Transport, Alimentation, Vie Sociale).
    /// </summary>
    public DonneesNiveauVie niveauVie;

    /// <summary>
    /// Cree un joueur avec son compte courant initial.
    /// </summary>
    public DonneesJoueur()
    {
        InitialiserSiNecessaire();
    }

    /// <summary>
    /// Repare les collections absentes apres une migration de donnees.
    /// </summary>
    public void InitialiserSiNecessaire()
    {
        if (comptes == null)
        {
            comptes = new Dictionary<string, CompteBanquaire>();
        }

        if (!comptes.ContainsKey(ServiceBanque.CompteCourantId) ||
            comptes[ServiceBanque.CompteCourantId] == null)
        {
            comptes[ServiceBanque.CompteCourantId] =
                new CompteBanquaire(
                    new argent(
                        ServiceBanque.SoldeInitialCourantCentimes));
        }

        // INITIALISATION DU SYSTÈME DE PRÊT IMMOBILIER
        if (pretsImmobiliers == null)
        {
            pretsImmobiliers = new List<DonneesPret>();
        }

        if (investissements == null)
        {
            investissements = new List<Investissement>();
        }

        if (bourse == null)
        {
            bourse = new DonneesBourse();
        }

        if (entrepreneuriat == null)
        {
            entrepreneuriat = new DonneesEntrepreneuriat();
        }
        entrepreneuriat.InitialiserSiNecessaire();

        if (salariat == null)
        {
            salariat = new DonneesSalariat();
        }
        salariat.InitialiserSiNecessaire();

        if (tempsApplications == null)
        {
            tempsApplications = new DonneesRepartitionTemps();
        }
        tempsApplications.InitialiserSiNecessaire();

        if (niveauVie == null)
        {
            niveauVie = new DonneesNiveauVie();
        }
    }

    /// <summary>
    /// Calcule le patrimoine total sans compter deux fois le Livret A.
    /// </summary>
    public argent CalculPatrimoineTotal()
    {
        return ServicePatrimoine.Calculer(this);
    }

    /// <summary>
    /// Produit une copie profonde de l'etat du joueur.
    /// </summary>
    /// <remarks>
    /// Les evenements C# ne sont volontairement pas copies. Les nouveaux
    /// objets reconstruisent leurs abonnements internes, tandis que les UI
    /// restent abonnees uniquement a l'etat reel.
    /// </remarks>
    public DonneesJoueur Copier()
    {
        DonneesJoueur copie = new DonneesJoueur
        {
            salaire = new argent(salaire.centimes),
            energie = energie,
            santeMentale = santeMentale,
            comptes = new Dictionary<string, CompteBanquaire>(),
            pretsImmobiliers = new List<DonneesPret>(), // Copie de la liste de prêts
            investissements = new List<Investissement>(),
            bourse = bourse != null ? bourse.Copier() : new DonneesBourse(),
            entrepreneuriat = entrepreneuriat != null
                ? entrepreneuriat.Copier()
                : new DonneesEntrepreneuriat(),
            salariat = salariat != null
                ? salariat.Copier()
                : new DonneesSalariat(),
            tempsApplications = tempsApplications != null
                ? tempsApplications.Copier()
                : new DonneesRepartitionTemps(),
            niveauVie = niveauVie != null
                ? niveauVie.Copier()
                : new DonneesNiveauVie()
        };

        if (comptes != null)
        {
            foreach (
                KeyValuePair<string, CompteBanquaire> entree in comptes)
            {
                if (entree.Value != null)
                {
                    copie.comptes[entree.Key] = entree.Value.Copier();
                }
            }
        }

        // COPIE PROFONDE DES PRÊTS IMMOBILIERS ACTIFS
        if (pretsImmobiliers != null)
        {
            foreach (DonneesPret pret in pretsImmobiliers)
            {
                if (pret != null)
                {
                    // On recrée une copie propre pour isoler le snapshot du mode What If
                    copie.pretsImmobiliers.Add(new DonneesPret(
                        pret.montantEmprunte, 
                        pret.dureeAns, 
                        pret.tauxAnnuel, 
                        pret.mensualite
                    ) {
                        moisRestants = pret.moisRestants,
                        capitalRestantDu = pret.capitalRestantDu
                    });
                }
            }
        }

        if (investissements != null)
        {
            foreach (Investissement investissement in investissements)
            {
                if (investissement != null &&
                    !EstMoteurInteretsLivretA(investissement))
                {
                    copie.investissements.Add(investissement.Copier());
                }
            }
        }

        copie.InitialiserSiNecessaire();
        return copie;
    }

    private bool EstMoteurInteretsLivretA(
        Investissement investissement)
    {
        if (comptes == null ||
            !comptes.TryGetValue(
                ServiceBanque.LivretAId,
                out CompteBanquaire compte) ||
            !(compte is Epargne epargne))
        {
            return false;
        }

        return ReferenceEquals(investissement, epargne.invest);
    }
}