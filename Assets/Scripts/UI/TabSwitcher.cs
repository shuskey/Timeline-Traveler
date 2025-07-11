using UnityEngine;
using UnityEngine.UI;

public class TabSwitcher : MonoBehaviour
{
    // We have an array of Tab Buttons and an equal number of Tab UI Contents
    // Click tabButtons[x] to show tabUIContents[x]
    // We want to hide all tabUIContents when we click a tab button
    // The tabButton that was clicked will get alpha 1 and the other tabButton will get alpha 0.5
    // This alpha must apply to the button and the text of the button
    [SerializeField] private Button[] tabButtons;
    [SerializeField] private GameObject[] tabUIContents;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private int previousTabIndex = 0;
    void Start()
    {
    
        for (int i = 0; i < tabButtons.Length; i++)
        {
            int tabIndex = i; // Capture the loop variable in a local variable
            tabButtons[i].onClick.AddListener(() => SwitchTab(tabIndex));
        }
        ClearAllTabs();
        SwitchTab(0);  // Set the first tab as active by default
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SwitchTab(int tabIndex)
    {
        // Clear the previous tab and set the new tab
        ClearTab(previousTabIndex);
        SetTab(tabIndex);
        
        previousTabIndex = tabIndex;
    }

    private void ClearTab(int tabIndex)
    {
        // Hide the tab content
        tabUIContents[tabIndex].SetActive(false);
        
        // Set button alpha to 0.5f
        var buttonColor = tabButtons[tabIndex].GetComponent<Image>().color;
        buttonColor.a = 0.5f;
        tabButtons[tabIndex].GetComponent<Image>().color = buttonColor;
        
        // Set text alpha to 0.5f
        SetTextAlpha(tabButtons[tabIndex], 0.5f);
    }

    private void SetTab(int tabIndex)
    {
        // Show the tab content
        tabUIContents[tabIndex].SetActive(true);
        
        // Set button alpha to 1.0f
        var buttonColor = tabButtons[tabIndex].GetComponent<Image>().color;
        buttonColor.a = 1.0f;
        tabButtons[tabIndex].GetComponent<Image>().color = buttonColor;
        
        // Set text alpha to 1.0f
        SetTextAlpha(tabButtons[tabIndex], 1.0f);
    }

    private void ClearAllTabs()
    {
        // Clear all tabs by calling ClearTab for each tab
        for (int i = 0; i < tabButtons.Length; i++)
        {
            ClearTab(i);
        }
    }

    private void SetTextAlpha(Button button, float alpha)
    {
        // Use GetComponentInChildren<Graphic> to find any text component
        // Both Text and TMP_Text inherit from Graphic
        var graphics = button.GetComponentsInChildren<Graphic>();
        
        foreach (var graphic in graphics)
        {
            // Skip the button's own Image component
            if (graphic == button.GetComponent<Image>()) continue;
            
            // Check if it's a text component by checking the type name
            var typeName = graphic.GetType().Name;
            if (typeName == "Text" || typeName.Contains("TMP") || typeName.Contains("TextMeshPro"))
            {
                var color = graphic.color;
                color.a = alpha;
                graphic.color = color;
                break; // Only set the first text component found
            }
        }
    }


}
