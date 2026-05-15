using UnityEngine;

public class OpenWindow : MonoBehaviour
{
    public GameObject window;

    public void Open()
    {
        window.SetActive(true);
    }

    public void Close()
    {
        window.SetActive(false);
    }
}