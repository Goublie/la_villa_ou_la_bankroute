using UnityEngine;
using UnityEngine.UI;

public class TableauScroll : Tableau
{
    [SerializeField] Ligne prefabLigne;
    
    [SerializeField] Transform conteneurLignes; 

    public override void AppliquerApparence()
    {
        base.AppliquerApparence();
        if (conteneurLignes != null)
        {
            Image imgConteneur = conteneurLignes.GetComponent<Image>();
            if (imgConteneur != null) imgConteneur.color = couleurLigne;
        }
    }

    public override bool Add(params object[] valeurs)
    {
        // On cherche une ligne vide existante
        foreach (Ligne l in tableau)
        {
            if (l.EstVide())
            {
                l.Set(valeurs);
                
                //On force cette ligne à remonter tout en haut visuellement
                l.transform.SetAsFirstSibling(); 
                
                return true;
            }
        }

        // Si toutes les lignes sont pleines, on instancie.
        if (prefabLigne != null && conteneurLignes != null)
        {
            Ligne nouvelleLigne = Instantiate(prefabLigne, conteneurLignes, false);
            
            // Appliquer l'apparence globale
            nouvelleLigne.SetApparence(couleurFondCases, couleurLigne, couleurTexte);
            
            // Appliquer le bon nombre de colonnes et la configuration
            nouvelleLigne.AjusterNombreColonnes(nombreColonnes);
            AppliquerConfigurationColonnes(nouvelleLigne);
            
            //On place la nouvelle ligne tout en haut visuellement 
            nouvelleLigne.transform.SetAsFirstSibling();
            
            nouvelleLigne.Set(valeurs);
            
            tableau.Add(nouvelleLigne); 
            
            return true;
        }
        else
        {
            Debug.LogError("Attention : Le préfabriqué ou le conteneur n'est pas assigné !");
            return false;
        }
    }

    public Ligne AjouterEtRetournerLigne(params object[] valeurs)
    {
        foreach (Ligne l in tableau)
        {
            if (l.EstVide())
            {
                l.Set(valeurs);
                l.transform.SetAsFirstSibling(); 
                return l;
            }
        }

        if (prefabLigne != null && conteneurLignes != null)
        {
            Ligne nouvelleLigne = Instantiate(prefabLigne, conteneurLignes, false);
            nouvelleLigne.SetApparence(couleurFondCases, couleurLigne, couleurTexte);
            nouvelleLigne.AjusterNombreColonnes(nombreColonnes);
            AppliquerConfigurationColonnes(nouvelleLigne);
            nouvelleLigne.transform.SetAsFirstSibling();
            nouvelleLigne.Set(valeurs);
            tableau.Add(nouvelleLigne); 
            return nouvelleLigne;
        }
        else
        {
            Debug.LogError("Attention : Le préfabriqué ou le conteneur n'est pas assigné !");
            return null;
        }
    }

    [Header("Affichage")]
    [Tooltip("Nombre de lignes vides minimum à afficher (pour un effet quadrillage)")]
    [Range(0, 50)] public int lignesMinimumVisualisees = 15;

    protected override void OnValidate()
    {
        base.OnValidate();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.delayCall += () =>
        {
            if (this == null) return;
            if (!this.gameObject.scene.IsValid()) return; 
            GarantirLignesMinimum();
        };
#endif
    }

    /// <summary>
    /// Instancie ou supprime des lignes vides supplémentaires selon le minimum requis.
    /// Cela permet de remplir visuellement l'espace vide avec un quadrillage et de le prévisualiser.
    /// </summary>
    public void GarantirLignesMinimum()
    {
        if (prefabLigne == null || conteneurLignes == null) return;
        
        Ligne[] lignesExistantes = conteneurLignes.GetComponentsInChildren<Ligne>(true);
        System.Collections.Generic.List<Ligne> listeLignes = new System.Collections.Generic.List<Ligne>(lignesExistantes);

        // Supprimer l'excédent s'il s'agit de lignes vides
        while (listeLignes.Count > lignesMinimumVisualisees)
        {
            Ligne ligneEnTrop = listeLignes[listeLignes.Count - 1];
            if (ligneEnTrop.EstVide())
            {
                listeLignes.RemoveAt(listeLignes.Count - 1);
                if (Application.isPlaying) Destroy(ligneEnTrop.gameObject);
                else DestroyImmediate(ligneEnTrop.gameObject);
            }
            else
            {
                break;
            }
        }

        // Ajouter les lignes manquantes
        while (listeLignes.Count < lignesMinimumVisualisees)
        {
            Ligne nouvelleLigne = Instantiate(prefabLigne, conteneurLignes, false);
            nouvelleLigne.SetApparence(couleurFondCases, couleurLigne, couleurTexte);
            nouvelleLigne.Vider();
            nouvelleLigne.transform.SetAsLastSibling();
            listeLignes.Add(nouvelleLigne); 
        }

        // Appliquer la configuration à TOUTES les lignes existantes pour que l'éditeur s'actualise
        foreach (Ligne l in listeLignes)
        {
            l.AjusterNombreColonnes(nombreColonnes);
            AppliquerConfigurationColonnes(l);
        }

        // Mettre à jour l'en-tête pour qu'il suive bien le tableau de largeurs
        if (ligneEnTete != null)
        {
            ligneEnTete.AjusterNombreColonnes(nombreColonnes);
            AppliquerConfigurationColonnes(ligneEnTete);
        }

        // Aligner PARFAITEMENT l'espacement et les marges internes de l'en-tête sur le modèle des lignes de données.
        // Maintenant que l'en-tête fait exactement la même largeur globale que les données (grâce au nouvel Espace_Scrollbar),
        // leurs marges internes (padding) doivent être rigoureusement identiques pour que les colonnes s'alignent au pixel près !
        Ligne modeleLigne = listeLignes.Count > 0 ? listeLignes[0] : null;
        if (ligneEnTete != null && modeleLigne != null)
        {
            UnityEngine.UI.HorizontalLayoutGroup hlgTete = ligneEnTete.GetComponent<UnityEngine.UI.HorizontalLayoutGroup>();
            UnityEngine.UI.HorizontalLayoutGroup hlgModele = modeleLigne.GetComponent<UnityEngine.UI.HorizontalLayoutGroup>();
            
            if (hlgTete != null && hlgModele != null)
            {
                hlgTete.spacing = hlgModele.spacing;
                // On copie exactement le padding (y compris les petites marges de 2px ou 4px)
                hlgTete.padding = new UnityEngine.RectOffset(hlgModele.padding.left, hlgModele.padding.right, hlgModele.padding.top, hlgModele.padding.bottom);
                hlgTete.childAlignment = hlgModele.childAlignment;
                hlgTete.childControlWidth = hlgModele.childControlWidth;
                hlgTete.childControlHeight = hlgModele.childControlHeight;
                hlgTete.childForceExpandWidth = hlgModele.childForceExpandWidth;
                hlgTete.childForceExpandHeight = hlgModele.childForceExpandHeight;
            }
        }

        if (Application.isPlaying)
        {
            tableau = listeLignes;
        }
    }
}