using UnityEngine;
using System.Collections.Generic;

public class Tableau : MonoBehaviour
{
    public List<Ligne> tableau;    
    public string dataDefault = "";

    public void Start()
    {
       tableau = new List<Ligne>(GetComponentsInChildren<Ligne>());

        for (int i = 0; i < tableau.Count; i++)
        {
            for (int j = 0; j < tableau[i].ligne.Count; j++)
            {
                set(i, j, dataDefault);
            }
        }
    }

    public void set(int indiceLigne, int indiceColonne, string text)
    {
        tableau[indiceLigne].set(indiceColonne, text);
    }

    public void set(int indiceLigne, int indiceColonne, int data)
    {
        tableau[indiceLigne].set(indiceColonne, data);
    }
}
