using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TabSwitcher : MonoBehaviour
{
    // We have two Tab Buttons and two Tab UI Contents
    // Click tabButtons[x] to show tabUIContents[x]
    // We want to hide all tabUIContents when we click a tab button
    // The tabButton that was clicked will get alpha 1 and the other tabButton will get alpha 0.5
    // This alpha must apply to the button and the text of the button
    [SerializeField] private Button[] tabsButtons;
    [SerializeField] private GameObject[] tabUIContents;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        tabsButtons[0].onClick.AddListener(() => SwitchTab(0));
        tabsButtons[1].onClick.AddListener(() => SwitchTab(1));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void SwitchTab(int tabIndex)
    {
        // When we click a tab button, we want to show the corresponding tab UI content
        tabUIContents[tabIndex].SetActive(true);
        tabUIContents[1 - tabIndex].SetActive(false);

        // Set Alpha to .5 only and not color
        var clickedTabButtonColor = tabsButtons[tabIndex].GetComponent<Image>().color;
        clickedTabButtonColor.a = 1.0f;
        tabsButtons[tabIndex].GetComponent<Image>().color = clickedTabButtonColor;

        var unclickedTabButtonColor = tabsButtons[1 - tabIndex].GetComponent<Image>().color;
        unclickedTabButtonColor.a = 0.5f;
        tabsButtons[1 - tabIndex].GetComponent<Image>().color = unclickedTabButtonColor;

        // Set the alpha of the text of the button
        var clickedTabButtonTextColor = tabsButtons[tabIndex].GetComponentInChildren<TMP_Text>().color; 
        clickedTabButtonTextColor.a = 1.0f;
        tabsButtons[tabIndex].GetComponentInChildren<TMP_Text>().color = clickedTabButtonTextColor;

        var unclickedTabButtonTextColor = tabsButtons[1 - tabIndex].GetComponentInChildren<TMP_Text>().color;
        unclickedTabButtonTextColor.a = 0.5f;
        tabsButtons[1 - tabIndex].GetComponentInChildren<TMP_Text>().color = unclickedTabButtonTextColor;
    }
}
