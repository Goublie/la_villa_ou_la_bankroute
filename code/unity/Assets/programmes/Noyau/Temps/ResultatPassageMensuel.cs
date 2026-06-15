/// <summary>
/// Decrit le resultat d'une cloture et de l'ouverture du mois suivant.
/// </summary>
public readonly struct ResultatPassageMensuel
{
    /// <summary>
    /// Initialise un resultat complet du passage du temps.
    /// </summary>
    /// <param name="succes">Indique si toutes les mutations ont ete faites.</param>
    /// <param name="message">Retour lisible ou cause de l'echec.</param>
    /// <param name="indexMoisCloture">Index absolu du mois cloture.</param>
    /// <param name="indexMoisOuverture">Index absolu du mois ouvert.</param>
    /// <param name="changementAnnee">
    /// Indique un passage de decembre a janvier.
    /// </param>
    public ResultatPassageMensuel(
        bool succes,
        string message,
        int indexMoisCloture,
        int indexMoisOuverture,
        bool changementAnnee)
    {
        Succes = succes;
        Message = message ?? string.Empty;
        IndexMoisCloture = indexMoisCloture;
        IndexMoisOuverture = indexMoisOuverture;
        ChangementAnnee = changementAnnee;
    }

    /// <summary>
    /// Indique si le calendrier et les agregats ont ete mis a jour.
    /// </summary>
    public bool Succes { get; }

    /// <summary>
    /// Retour lisible sur l'operation.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Index absolu du mois photographie avant l'ouverture suivante.
    /// </summary>
    public int IndexMoisCloture { get; }

    /// <summary>
    /// Index absolu du nouveau mois courant.
    /// </summary>
    public int IndexMoisOuverture { get; }

    /// <summary>
    /// Indique si une transition annuelle doit etre affichee.
    /// </summary>
    public bool ChangementAnnee { get; }

    /// <summary>
    /// Cree un resultat d'echec sans mutation attendue du calendrier.
    /// </summary>
    public static ResultatPassageMensuel Echec(
        string message,
        int indexMois)
    {
        return new ResultatPassageMensuel(
            false,
            message,
            indexMois,
            indexMois,
            false);
    }
}
