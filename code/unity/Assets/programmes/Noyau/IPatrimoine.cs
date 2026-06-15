/// <summary>
/// Definit un element possede par le joueur qui contribue a son patrimoine.
/// </summary>
/// <remarks>
/// Une meme valeur economique ne doit etre exposee que par un seul agregat.
/// Par exemple, le Livret A est compte par son compte d'epargne et ne doit
/// pas etre additionne une seconde fois comme investissement autonome.
/// </remarks>
public interface IPatrimoine
{
    /// <summary>
    /// Calcule la valeur actuelle de l'element en centimes.
    /// </summary>
    /// <returns>
    /// Valeur patrimoniale a l'instant courant. Le calcul ne doit pas dependre
    /// de l'ouverture d'une fenetre Unity.
    /// </returns>
    argent GetValeurPatrimoine();
}
