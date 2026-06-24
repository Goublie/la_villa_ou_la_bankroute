using UnityEngine;
using XCharts.Runtime;

public sealed class GraphiqueCamembertBourse
{
    private readonly RectTransform racine;
    private PieChart graphique;
    private bool initialise;

    public GraphiqueCamembertBourse(RectTransform racine)
    {
        this.racine = racine;
    }

    public void Afficher(DonneesBourse donnees)
    {
        if (donnees == null || !Initialiser()) return;

        graphique.RemoveData();
        Pie serie = graphique.GetSerie(0) as Pie;
        if (serie == null)
        {
            serie = graphique.AddSerie<Pie>("Portefeuille");
        }

        serie.show = true;

        bool hasData = false;
        if (donnees.positions != null)
        {
            foreach (var position in donnees.positions)
            {
                if (position.quantite > 0)
                {
                    var actif = CatalogueActifs.Trouver(position.actifId);
                    string nom = actif != null ? actif.Nom : position.actifId;
                    graphique.AddData(0, position.quantite, nom);
                    hasData = true;
                }
            }
        }

        if (!hasData)
        {
            graphique.AddData(0, 1, "Vide");
        }

        graphique.SetAllDirty();
        graphique.RefreshChart();
    }

    private bool Initialiser()
    {
        if (racine == null) return false;

        if (graphique == null) graphique = racine.GetComponent<PieChart>();
        if (graphique == null) graphique = racine.gameObject.AddComponent<PieChart>();
        if (graphique == null) return false;

        if (initialise) return true;

        graphique.Init();
        graphique.theme.transparentBackground = true;
        
        var title = graphique.EnsureChartComponent<Title>();
        title.show = true;
        title.text = "Répartition";
        
        var legend = graphique.EnsureChartComponent<Legend>();
        legend.show = true;

        var tooltip = graphique.EnsureChartComponent<Tooltip>();
        tooltip.show = true;

        initialise = true;
        return true;
    }
}
