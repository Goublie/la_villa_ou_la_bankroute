using UnityEngine;

public class BoutonMenu : MonoBehaviour
{
    public void RevenirAuMenu()
    {
        ScenesManager.Instance.ChargerMenu();
    }

}
