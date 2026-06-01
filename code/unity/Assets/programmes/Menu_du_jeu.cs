using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Menu_du_jeu : MonoBehaviour
{
    public Button playButton;
    public Button quitButton;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playButton.onClick.AddListener(StartGame);
        quitButton.onClick.AddListener(QuitGame);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartGame()
    {
        Debug.Log("Le jeu commence !");
        SceneManager.LoadScene("Jeu");
    }

    public void QuitGame()
    {
        // Code pour quitter le jeu
        Debug.Log("Le jeu se ferme !");
        Application.Quit();
    }   
}