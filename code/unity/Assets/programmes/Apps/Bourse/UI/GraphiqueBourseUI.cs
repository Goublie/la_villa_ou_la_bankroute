using System.Text;
using UnityEngine;
using XCharts.Runtime;

/// <summary>
/// Adapte XCharts aux series historiques de l'application Bourse.
/// </summary>
/// <remarks>
/// Cette classe est volontairement specifique a la presentation. Elle ne
/// modifie ni le portefeuille, ni les ordres, ni la valeur patrimoniale.
/// </remarks>
public sealed class GraphiqueBourseUI
{
    private const int NombrePoints = 6;

    private readonly RectTransform racine;
    private LineChart graphique;
    private bool initialise;

    /// <summary>
    /// Cree l'adaptateur graphique dans la zone RectTransform indiquee.
    /// </summary>
    public GraphiqueBourseUI(RectTransform racine)
    {
        this.racine = racine;
    }

    /// <summary>
    /// Affiche la courbe historique jusqu'au mois courant inclus.
    /// </summary>
    /// <returns>
    /// True si XCharts a affiche la serie, false si le texte de secours doit
    /// etre utilise.
    /// </returns>
    public bool Afficher(
        DefinitionActifFinancier actif,
        ServiceBourse service,
        int moisActuel)
    {
        if (actif == null ||
            service == null ||
            !Initialiser())
        {
            return false;
        }

        graphique.RemoveData();
        Line serie = graphique.AddSerie<Line>(actif.Nom);
        if (serie == null)
        {
            return false;
        }

        serie.show = true;
        serie.lineStyle.width = 4f;
        serie.lineStyle.color = new Color32(34, 92, 210, 255);
        serie.symbol.show = true;
        serie.symbol.size = 8f;

        Title title = graphique.EnsureChartComponent<Title>();
        title.show = true;
        title.text = actif.Nom;

        ObtenirPeriode(
            actif,
            moisActuel,
            out int premierMois,
            out int dernierMois);
        for (int mois = premierMois; mois <= dernierMois; mois++)
        {
            string etiquetteMois = ObtenirNomMois(mois);
            graphique.AddXAxisData(etiquetteMois);
            graphique.AddData(0, service.ObtenirPrix(actif, mois));
        }

        graphique.SetAllDirty();
        graphique.RefreshChart();
        return true;
    }

    /// <summary>
    /// Construit une representation textuelle des memes douze points.
    /// </summary>
    public string ConstruireTexteSecours(
        DefinitionActifFinancier actif,
        ServiceBourse service,
        int moisActuel)
    {
        if (actif == null || service == null)
        {
            return string.Empty;
        }

        ObtenirPeriode(
            actif,
            moisActuel,
            out int premierMois,
            out int dernierMois);
        StringBuilder resultat =
            new StringBuilder("<b>Courbe 12 mois</b>\n");
        for (int mois = premierMois; mois <= dernierMois; mois++)
        {
            if (mois > premierMois)
            {
                resultat.Append("  |  ");
            }

            resultat.Append("M")
                .Append(mois)
                .Append(" ")
                .Append(
                    service.ObtenirPrix(actif, mois)
                        .ToString("N2"))
                .Append(" EUR");
        }

        return resultat.ToString();
    }

    private bool Initialiser()
    {
        if (racine == null)
        {
            return false;
        }

        if (graphique == null)
        {
            graphique = racine.GetComponent<LineChart>();
        }

        if (graphique == null)
        {
            graphique = racine.gameObject.AddComponent<LineChart>();
        }

        if (graphique == null)
        {
            return false;
        }

        if (initialise)
        {
            return true;
        }

        graphique.Init();
        graphique.theme.transparentBackground = true;
        graphique.EnsureChartComponent<Title>().show = true;
        graphique.EnsureChartComponent<Legend>().show = false;
        graphique.EnsureChartComponent<Tooltip>().show = true;

        Color32 couleurAxes = new Color32(55, 65, 82, 255);
        Color32 couleurGrille = new Color32(160, 170, 185, 100);
        GridCoord grille = graphique.EnsureChartComponent<GridCoord>();
        grille.show = true;
        grille.showBorder = true;
        grille.borderWidth = 1f;
        grille.borderColor = couleurAxes;
        grille.left = 70f;
        grille.right = 25f;
        grille.top = 20f;
        grille.bottom = 45f;

        XAxis axeX = graphique.EnsureChartComponent<XAxis>();
        axeX.show = true;
        axeX.type = Axis.AxisType.Category;
        axeX.boundaryGap = false;
        axeX.splitNumber = 6;
        axeX.axisLine.lineStyle.color = couleurAxes;
        axeX.axisLabel.textStyle.color = couleurAxes;
        axeX.axisName.show = true;
        axeX.axisName.name = "Mois";

        YAxis axeY = graphique.EnsureChartComponent<YAxis>();
        axeY.show = true;
        axeY.type = Axis.AxisType.Value;
        axeY.splitNumber = 4;
        axeY.axisLine.lineStyle.color = couleurAxes;
        axeY.axisLabel.textStyle.color = couleurAxes;
        axeY.axisLabel.formatter = "{value} €";
        axeY.splitLine.show = true;
        axeY.splitLine.lineStyle.color = couleurGrille;

        initialise = true;
        return true;
    }

    private static void ObtenirPeriode(
        DefinitionActifFinancier actif,
        int moisActuel,
        out int premierMois,
        out int dernierMois)
    {
        int dernierIndex = Mathf.Max(0, actif.Prix.Count - 1);
        dernierMois = Mathf.Clamp(moisActuel, 0, dernierIndex);
        premierMois = Mathf.Max(
            0,
            dernierMois - NombrePoints + 1);
    }

    private string ObtenirNomMois(int moisAbsolu)
    {
        // Le jeu commence au mois 0, qui correspond à Juillet (index 6)
        int indexMois = (6 + moisAbsolu) % 12;
        switch (indexMois)
        {
            case 0: return "Janvier";
            case 1: return "Février";
            case 2: return "Mars";
            case 3: return "Avril";
            case 4: return "Mai";
            case 5: return "Juin";
            case 6: return "Juillet";
            case 7: return "Août";
            case 8: return "Septembre";
            case 9: return "Octobre";
            case 10: return "Novembre";
            case 11: return "Décembre";
            default: return "";
        }
    }
}
