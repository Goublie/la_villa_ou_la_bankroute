using UnityEngine;

public class CourantUI : MonoBehaviour
{
    public GameData GameData;    
    
    private void ActualiseAffichage()
    {
        
    }
    void OnDisable()
    {
        ActionPlay.moisPasse -= ActualiseAffichage;
    }
    void OnEnable()
    {
        ActionPlay.moisPasse += ActualiseAffichage;
    }
}
