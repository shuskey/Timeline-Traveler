using UnityEngine;
using UnityEngine.EventSystems;

public class FiveStarToggler : MonoBehaviour
{
    public void OnPointerEnter_Star1()
    {
        // Code to execute when the mouse enters the button
        Debug.Log("Mouse entered star 1!");
    }

    public void OnPointerExit_Star1()
    {
        // Code to execute when the mouse exits the button
        Debug.Log("Mouse exited star 1!");
    }
    public void OnPointerClick_Star1()
    {
        // Code to execute when the mouse clicks the button
        Debug.Log("Mouse clicked star 1!");
    }
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
