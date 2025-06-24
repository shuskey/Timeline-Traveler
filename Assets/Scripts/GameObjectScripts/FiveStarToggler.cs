using UnityEngine;
using UnityEngine.EventSystems;

public class FiveStarToggler : MonoBehaviour
{
    private int starCount = 0;
    public GameObject[] starArray;

    public void FiveStarComponentInit(int startCount)
    {
        starCount = startCount;
        UpdateStarDisplay();
    }

    // Helper method to update star visibility based on current starCount
    private void UpdateStarDisplay()
    {
        for (int i = 0; i < starArray.Length - 1; i++) // Skip the zero button (index 0)
        {
            if (starArray[i + 1] != null) // starArray[0] is the zero button, so we use i+1 for actual stars
            {
                Transform starOnChild = starArray[i + 1].transform.Find("Star On");
                if (starOnChild != null)
                {
                    starOnChild.gameObject.SetActive(i < starCount);
                } else {
                    Debug.LogError("'Star On' GameObject child not found for star at index " + (i + 1));
                }
            } else {
                Debug.LogError("Star GameObject at index " + (i + 1) + " is null");
            }
        }
    }

    // Helper method to set temporary hover state
    private void SetHoverState(int hoverStarIndex)
    {
        
        for (int i = 0; i < starArray.Length - 1; i++) // Skip the zero button
        {
            if (starArray[i + 1] != null)
            {
                Transform starOnChild = starArray[i + 1].transform.Find("Star On");
                if (starOnChild != null)
                {
                    // Turn on stars up to hoverStarIndex, turn off the rest
                    starOnChild.gameObject.SetActive(i < hoverStarIndex);
                } else {
                    Debug.LogError("'Star On' GameObject child not found for star at index " + (i + 1));
                }
            } else {
                Debug.LogError("Star GameObject at index " + (i + 1) + " is null");
            }
        }
    }

    // Helper method to restore the actual star count display
    private void RestoreActualState()
    {
        UpdateStarDisplay();
    }

    public void OnPointerEnter_Star0()
    {
        // Turn off all stars when hovering over the zero button
        SetHoverState(0);
    }

    public void OnPointerExit_Star0()
    {
        // Restore actual state when leaving the zero button
        RestoreActualState();
    }

    public void OnPointerClick_Star0()
    {
        // Set rating to 0
        starCount = 0;
        UpdateStarDisplay();
        Debug.Log("Rating set to 0 stars!");
    }
    
    public void OnPointerEnter_Star1()
    {
        // Show 1 star on hover
        SetHoverState(1);
    }

    public void OnPointerExit_Star1()
    {
        // Restore actual state
        RestoreActualState();
    }

    public void OnPointerClick_Star1()
    {
        // Set rating to 1
        starCount = 1;
        UpdateStarDisplay();
        Debug.Log("Rating set to 1 star!");
    }

    public void OnPointerEnter_Star2()
    {
        // Show 2 stars on hover
        SetHoverState(2);
    }

    public void OnPointerExit_Star2()
    {
        // Restore actual state
        RestoreActualState();
    }

    public void OnPointerClick_Star2()
    {
        // Set rating to 2
        starCount = 2;
        UpdateStarDisplay();
        Debug.Log("Rating set to 2 stars!");
    }

    public void OnPointerEnter_Star3()
    {
        // Show 3 stars on hover
        SetHoverState(3);
    }

    public void OnPointerExit_Star3()
    {
        // Restore actual state
        RestoreActualState();
    }

    public void OnPointerClick_Star3()
    {
        // Set rating to 3
        starCount = 3;
        UpdateStarDisplay();
        Debug.Log("Rating set to 3 stars!");
    }

    public void OnPointerEnter_Star4()
    {
        // Show 4 stars on hover
        SetHoverState(4);
    }

    public void OnPointerExit_Star4()
    {
        // Restore actual state
        RestoreActualState();
    }

    public void OnPointerClick_Star4()
    {
        // Set rating to 4
        starCount = 4;
        UpdateStarDisplay();
        Debug.Log("Rating set to 4 stars!");
    }

    public void OnPointerEnter_Star5()
    {
        // Show 5 stars on hover
        SetHoverState(5);
    }

    public void OnPointerExit_Star5()
    {
        // Restore actual state
        RestoreActualState();
    }

    public void OnPointerClick_Star5()
    {
        // Set rating to 5
        starCount = 5;
        UpdateStarDisplay();
        Debug.Log("Rating set to 5 stars!");
    }

    // Getter method to retrieve the current star count
    public int GetStarCount()
    {
        return starCount;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Initialize the display based on the current starCount
        UpdateStarDisplay();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
