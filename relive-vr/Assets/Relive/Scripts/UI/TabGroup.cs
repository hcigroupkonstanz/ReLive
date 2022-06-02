using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TabGroup : MonoBehaviour
{

    public List<TabButton> tabButtons;

    public TabButton selectedTab;

    public List<TabPage> pages;

    public Color32 highlightColor = new Color32(63, 164, 255, 50);
    public Color32 selectedColor = new Color32(63, 164, 255, 100);
    public Color32 defaultColor = new Color32(255, 255, 255, 100);

    public float selectedDistance = 0;
    public float defaultDistance = 0.008f;

    public void Subscribe(TabButton tabButton)
    {
        if (tabButtons == null)
        {
            tabButtons = new List<TabButton>();
        }
        tabButtons.Add(tabButton);
    }

    public void OnTabEnter(TabButton tabButton)
    {
        ResetTabs();
        if (selectedTab == null || tabButton != selectedTab)
        {
            tabButton.background.color = highlightColor;
        }
    }

    public void OnTabExit(TabButton tabButton)
    {
        ResetTabs();
    }

    public void OnTabSelected(TabButton tabButton)
    {
        selectedTab = tabButton;
        ResetTabs();
        tabButton.background.color = selectedColor;
        tabButton.transform.localPosition = new Vector3(tabButton.transform.localPosition.x, tabButton.transform.localPosition.y, selectedDistance);
        int index = tabButton.transform.GetSiblingIndex();
        for (int i = 0; i < pages.Count; i++)
        {
            if (i == index) {
                pages[i].gameObject.SetActive(true);
                pages[i].OnTabSelected();
            }
            else
            {
                pages[i].gameObject.SetActive(false);
            }
        }
    }

    public void ResetTabs()
    {
        foreach (TabButton tabButton in tabButtons)
        {
            if (selectedTab != null && tabButton == selectedTab) { continue; }
            tabButton.background.color = defaultColor;
            tabButton.transform.localPosition = new Vector3(tabButton.transform.localPosition.x, tabButton.transform.localPosition.y, defaultDistance);
        }
    }
}
