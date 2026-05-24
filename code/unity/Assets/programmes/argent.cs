using System;
using UnityEngine;

[Serializable]
public struct argent
{
    public int centimes;

    public argent(int centimes)
    {
        this.centimes = centimes;
    }

    public argent(float somme) : this(Mathf.RoundToInt(somme * 100f))
    {
        
    }

    public override string ToString()
    {
        //Conversion en décimal pour la précision
        decimal montant = centimes / 100m;
        return montant.ToString("F2") + " €";
    }

    // Surcharge d'opérateurs pour pouvoir faire des maths directement avec ton type !
    public static argent operator +(argent a, argent b) => new argent(a.centimes + b.centimes);
    public static argent operator -(argent a, argent b) => new argent(a.centimes - b.centimes);
    public static argent operator -(argent a) => new argent(-a.centimes);
    public static argent operator *(argent a, float multiplicateur) => new argent((int)(a.centimes * multiplicateur));
    public static bool operator >(argent a, argent b) => a.centimes > b.centimes;
    public static bool operator <(argent a, argent b) => a.centimes < b.centimes;
    public static bool operator >=(argent a, argent b) => a.centimes >= b.centimes;
    public static bool operator <=(argent a, argent b) => a.centimes <= b.centimes;
}