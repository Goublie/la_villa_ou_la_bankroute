using System;

/// <summary>
/// Nature d'une operation reellement executee par le portefeuille What If.
/// </summary>
public enum TypeOrdreHistoriqueWhatIf
{
    Achat,
    Vente,
    VenteForcee
}

/// <summary>
/// Trace persistante et explicable d'un achat ou d'une vente du modele.
/// </summary>
[Serializable]
public sealed class OrdreHistoriqueWhatIf
{
    public int indexMois;
    public TypeOrdreHistoriqueWhatIf type;
    public string actifId;
    public float quantite;
    public int prixUnitaireCentimes;
    public int montantCentimes;
    public int coutTransactionCentimes;
    public string raison;

    public OrdreHistoriqueWhatIf Copier()
    {
        return new OrdreHistoriqueWhatIf
        {
            indexMois = indexMois,
            type = type,
            actifId = actifId,
            quantite = quantite,
            prixUnitaireCentimes = prixUnitaireCentimes,
            montantCentimes = montantCentimes,
            coutTransactionCentimes = coutTransactionCentimes,
            raison = raison
        };
    }
}