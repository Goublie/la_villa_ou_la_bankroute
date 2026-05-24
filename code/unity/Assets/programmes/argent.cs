using System;

[Serializable]
public struct argent
{
    public int centimes;

    public argent(int centimes)
    {
        this.centimes = centimes;
    }

    public override string ToString()
    {
        int euros = centimes / 100;
        int resteCentimes = centimes % 100;
        return euros + "," + (resteCentimes < 10 ? "0" : "") + resteCentimes + "€";
    }

    // Surcharge d'opérateurs pour pouvoir faire des maths directement avec ton type !
    public static argent operator +(argent a, argent b) => new argent(a.centimes + b.centimes);
    public static argent operator -(argent a, argent b) => new argent(a.centimes - b.centimes);
    public static argent operator *(argent a, float multiplicateur) => new argent((int)(a.centimes * multiplicateur));
}