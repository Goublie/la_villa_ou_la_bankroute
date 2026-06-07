using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenesManager : MonoBehaviour
{
    private static ScenesManager _instance;
    public static ScenesManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<ScenesManager>();

                if (_instance == null)
                {
                    GameObject go = new GameObject("ScenesManager (Auto-Generated)");
                    _instance = go.AddComponent<ScenesManager>();
                }
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ChargerJeu()
    {
        Debug.Log("ScenesManager : Chargement de 'Jeu'...");
        SceneManager.LoadScene("Jeu");
    }

    public void ChargerIntrospection()
    {
        Debug.Log("ScenesManager : Chargement de 'End'...");
        SceneManager.LoadScene("End");    
    }

    public void ChargerMenu()
    {
        SceneManager.LoadScene("Menu");
    }

    public void QuitterJeu()
    {
        Debug.Log("ScenesManager : Fermeture.");
        Application.Quit();
    }
}
