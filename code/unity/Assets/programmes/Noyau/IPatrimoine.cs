/// <summary>
/// Interface unifiée pour tous les types d'actifs ou de placements
/// contribuant à la valeur globale du patrimoine du joueur.
/// </summary>
public interface IPatrimoine
{
    /// <summary>
    /// Calcule et renvoie la valeur actuelle de l'actif ou du compte.
    /// </summary>
    argent GetValeurPatrimoine();
}
