using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TabGroup : MonoBehaviour
{
    public List<TabButton> tabButtons;
    public List<GameObject> pagesToSwap;

    private TabButton selectedTab;

    public void Subscribe(TabButton button)
    {
        if (tabButtons == null)
        {
            tabButtons = new List<TabButton>();
        }
        if (!tabButtons.Contains(button))
        {
            tabButtons.Add(button);
        }
    }

    public void OnTabSelected(TabButton button)
    {
        if (selectedTab != null)
        {
            selectedTab.SetActif(false);
        }

        selectedTab = button;
        selectedTab.SetActif(true);

        int index = tabButtons.IndexOf(button);
        if (index >= 0 && pagesToSwap != null)
        {
            for (int i = 0; i < pagesToSwap.Count; i++)
            {
                if (pagesToSwap[i] != null)
                {
                    pagesToSwap[i].SetActive(i == index);
                }
            }
        }
    }

    public void ResetTabs()
    {
        foreach (TabButton button in tabButtons)
        {
            button.SetActif(button == selectedTab);
        }
    }

    private void Start()
    {
        if (tabButtons != null && tabButtons.Count > 0)
        {
            OnTabSelected(tabButtons[0]);
        }
    }
}
