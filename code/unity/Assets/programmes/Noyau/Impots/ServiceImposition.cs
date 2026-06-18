public sealed class ServiceImposition
{
    // ── Barème progressif 2024 (ramené au mois, en centimes) ─────────────────
    private static readonly (long seuilMensuel, float taux)[] TRANCHES = {
        (1_129_400L / 12,  0.00f),  //     0 → 94 116 centimes
        (2_879_700L / 12,  0.11f),  // 94 116 → 239 975 centimes
        (8_234_100L / 12,  0.30f),  // 239 975 → 686 175 centimes
        (17_710_600L / 12, 0.41f),  // 686 175 → 1 475 883 centimes
        (long.MaxValue,    0.45f),  // au-delà
    };

    public ResultatPrelevement CalculerImpotMensuel(long salaireBrut)
    {
        if (salaireBrut <= 0L)
            return new ResultatPrelevement(salaireBrut, 0L);

        // Abattement 10% mensuel
        long baseImposable   = (long)(salaireBrut * 0.90f);

        long impot           = 0L;
        long seuilPrecedent  = 0L;

        foreach (var (seuilMensuel, taux) in TRANCHES)
        {
            if (baseImposable <= seuilPrecedent)
                break;

            long partImposable = Math.Min(baseImposable, seuilMensuel) - seuilPrecedent;
            impot             += (long)(partImposable * taux);
            seuilPrecedent     = seuilMensuel;
        }

        return new ResultatPrelevement(salaireBrut - impot, impot);
    }
}

public readonly struct ResultatPrelevement
{
    public readonly long salaireNet;
    public readonly long impotPreleve;

    public ResultatPrelevement(long salaireNet, long impotPreleve)
    {
        this.salaireNet   = salaireNet;
        this.impotPreleve = impotPreleve;
    }
}