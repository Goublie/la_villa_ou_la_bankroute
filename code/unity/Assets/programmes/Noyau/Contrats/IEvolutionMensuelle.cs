/// <summary>
/// Definit le contrat des systemes metier qui evoluent lors du passage
/// au mois suivant.
/// </summary>
/// <remarks>
/// L'orchestrateur mensuel appelle ce contrat sans dependre des interfaces
/// Unity. Les implementations doivent rester deterministes pour un etat et
/// un index de mois identiques afin de garantir la coherence des snapshots
/// et du mode What If.
/// </remarks>
public interface IEvolutionMensuelle
{
    /// <summary>
    /// Applique les regles metier du mois indique a l'etat porte par
    /// l'implementation.
    /// </summary>
    /// <param name="mois">
    /// Index absolu du mois de jeu, exprime en mois ecoules depuis le debut
    /// de la partie.
    /// </param>
    void AppliquerEvolutionMensuelle(int mois);
}
