using UnityEngine;

[CreateAssetMenu(fileName = "NewActualite", menuName = "Actualites/Actualite", order = 1)]
public class ActualiteSO : ScriptableObject
{
    public string titre;
    [TextArea]
    public string description;
    public Sprite icone;
    public System.DateTime dateDebut;
    public System.DateTime dateFin;
    public TypeActualite type;
    public int impactFinancier;
    public bool actif = true;

    public enum TypeActualite { Private, Public }
}
