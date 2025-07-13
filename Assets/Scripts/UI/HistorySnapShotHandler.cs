using UnityEngine;
using UnityEngine.UI;
using StarterAssets;
using System;

public class HistorySnapShotHandler : MonoBehaviour
{
    // Static event and property for global modal state
    public static event Action<bool> OnModalStateChanged;
    public static bool IsModalOpen { get; private set; } = false;

    public Button quitButton;
    public GameObject headerPanel;

    private CanvasGroup canvasGroup;
    private ThirdPersonController playerController;
    private bool wasCursorLocked;
    private StarterAssetsInputs starterAssetsInputs;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Initialize canvas group if not already set
        if (!canvasGroup) canvasGroup = GetComponent<CanvasGroup>();
        
        if (quitButton != null) quitButton.onClick.AddListener(OnQuitButtonClicked);
        
        HidePopup();
    }
    
    public void ShowHistorySnapShotDetails()
    {
        headerPanel.GetComponent<TabSwitcher>().SwitchTab(0);
        ShowPopup();
    }

    public void ShowHistorySnapShotDetails(Assets.Scripts.ContentProviders.FamilyHappeningsContent familyHappeningsContent, Assets.Scripts.DataObjects.Person focusPerson, int year)
    {
        headerPanel.GetComponent<TabSwitcher>().SwitchTab(0);
        
        // Generate family happenings content if provider is available
        if (familyHappeningsContent != null && focusPerson != null)
        {
            string familyHappeningsReport = familyHappeningsContent.GetFamilyHappeningsContent(focusPerson, year);
            Debug.Log($"Family Happenings Report for {focusPerson.givenName} {focusPerson.surName} in {year}:");
            Debug.Log(familyHappeningsReport);
            
            // TODO: Display this content in the UI - you can add UI elements to show the report
            // For now, we'll just log it to the console
        }
        
        ShowPopup();
    }

    private void OnQuitButtonClicked()
    {
        // Add quit button functionality here
        HidePopup();
    }

    private void ShowPopup()
    {
        if (!canvasGroup) canvasGroup = GetComponent<CanvasGroup>();
        
        // Store current cursor state
        wasCursorLocked = Cursor.lockState == CursorLockMode.Locked;
        
        // Unlock cursor and make it visible
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Find the player controller and input
        playerController = FindAnyObjectByType<ThirdPersonController>();
        if (playerController != null)
        {
            starterAssetsInputs = playerController.GetComponent<StarterAssetsInputs>();
        }
        
        // Disable player controller if found
        if (playerController != null)
        {
            playerController.enabled = false;
        }
        
        // Show the popup
        canvasGroup.alpha = 1;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        
        // Ensure this canvas is rendered on top
        Canvas canvas = GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.sortingOrder = 32767; // Maximum sorting order
        }
        
        // Notify other objects that modal is open
        IsModalOpen = true;
        OnModalStateChanged?.Invoke(true);
    }

    private void HidePopup()
    {
        if (!canvasGroup) canvasGroup = GetComponent<CanvasGroup>();
        
        // Restore cursor state
        if (wasCursorLocked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        
        // Re-enable player controller and flush input
        if (playerController != null)
        {
            // Flush input system to clear cached signals
            if (starterAssetsInputs != null)
            {
                // Reset all input values to clear any cached state
                starterAssetsInputs.move = Vector2.zero;
                starterAssetsInputs.look = Vector2.zero;
                starterAssetsInputs.jump = false;
                starterAssetsInputs.sprint = false;
                starterAssetsInputs.menu = false;
                starterAssetsInputs.start = false;
                starterAssetsInputs.previous = false;
                starterAssetsInputs.next = false;
                starterAssetsInputs.interact = false;
                starterAssetsInputs.debugNextPersonOfInterest = false;
                starterAssetsInputs.debugPreviousPersonOfInterest = false;
            }
            //playerController enable at the very end to make sure its inputs are flushed first
            playerController.enabled = true;
        }
        
        // Hide the popup
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        
        // Notify other objects that modal is closed
        IsModalOpen = false;
        OnModalStateChanged?.Invoke(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
