/// <summary>
/// Represente le resultat d'une commande metier susceptible d'echouer.
/// </summary>
/// <remarks>
/// Les services retournent ce type au lieu de modifier directement un texte
/// d'interface. <see cref="Montant"/> est exprime en centimes via
/// <see cref="argent"/> et peut valoir zero lorsqu'aucun montant n'est
/// pertinent.
/// </remarks>
public readonly struct ResultatOperation
{
    /// <summary>
    /// Initialise un resultat complet.
    /// </summary>
    /// <param name="succes">Indique si l'operation a ete appliquee.</param>
    /// <param name="message">Message destine au journal ou a l'interface.</param>
    /// <param name="code">
    /// Code stable permettant de distinguer les causes d'echec sans analyser
    /// le texte affiche.
    /// </param>
    /// <param name="montant">
    /// Montant effectivement transfere, debite ou credite, en centimes.
    /// </param>
    public ResultatOperation(
        bool succes,
        string message,
        string code = "",
        argent montant = default)
    {
        Succes = succes;
        Message = message ?? string.Empty;
        Code = code ?? string.Empty;
        Montant = montant;
    }

    /// <summary>
    /// Indique si la commande a modifie l'etat metier.
    /// </summary>
    public bool Succes { get; }

    /// <summary>
    /// Message lisible expliquant le resultat de la commande.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Code stable de succes ou d'erreur.
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Montant effectivement traite, exprime en centimes.
    /// </summary>
    public argent Montant { get; }

    /// <summary>
    /// Cree un resultat de succes.
    /// </summary>
    public static ResultatOperation Reussite(
        string message,
        argent montant = default,
        string code = "succes")
    {
        return new ResultatOperation(true, message, code, montant);
    }

    /// <summary>
    /// Cree un resultat d'echec sans modifier l'etat metier.
    /// </summary>
    public static ResultatOperation Echec(string message, string code)
    {
        return new ResultatOperation(false, message, code);
    }
}
