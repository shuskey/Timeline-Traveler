using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Utilities;
using System;
using StarterAssets;

public class FixOrFlagPhotoHandler : MonoBehaviour
{
    // Static event and property for global modal state
    public static event Action<bool> OnModalStateChanged;
    public static bool IsModalOpen { get; private set; } = false;
    
    public Button quitButton;
    public Button cancelButton;
    public Button okButton;
    public Toggle originalContentDateToggle;
    public Toggle unknownDateToggle;
    public Toggle privateToggle;
    public InputField originalContentDateInputField;
    public InputField digiKamTodoInputField;
   
    private CanvasGroup canvasGroup;
    private PhotoInfo currentPhotoInfo;
    private PhotoInfo originalPhotoInfo;
    private ThirdPersonController playerController;
    private bool wasCursorLocked;
    private StarterAssetsInputs starterAssetsInputs;
    
    // Define the delegate type for the callback
    public delegate void PhotoActionCallback(PhotoInfo modifiedPhotoInfo);
    private PhotoActionCallback onPhotoActionComplete;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Initialize canvas group if not already set
        if (!canvasGroup) canvasGroup = GetComponent<CanvasGroup>();
        
        // Set up UI component listeners
        SetupUIComponentListeners();
        
        HidePopup();
    }

    private void SetupUIComponentListeners()
    {
        // Set up button listeners
        if (okButton != null) okButton.onClick.AddListener(OnOKButtonClicked);
        
        if (quitButton != null) quitButton.onClick.AddListener(OnQuitButtonClicked);
        
        if (cancelButton != null) cancelButton.onClick.AddListener(OnCancelButtonClicked);
        
        // Set up toggle listeners
        if (originalContentDateToggle != null) originalContentDateToggle.onValueChanged.AddListener(OnOriginalContentDateToggleChanged);
        
        if (unknownDateToggle != null) unknownDateToggle.onValueChanged.AddListener(OnUnknownDateToggleChanged);
        
        if (privateToggle != null) privateToggle.onValueChanged.AddListener(OnPrivateToggleChanged);
    }

    public void ShowFixOrFlagPhotoPopup(PhotoInfo photoInfo, PhotoActionCallback callback)
    {
        currentPhotoInfo = photoInfo;
        onPhotoActionComplete = callback;
        originalPhotoInfo = photoInfo;
        InitializeUIComponentValues();
        
        ShowPopup();
    }
    
    public void HideFixOrFlagPhotoPopup()
    {
        HidePopup();
    }

    private void InitializeUIComponentValues()
    {
        if (currentPhotoInfo.CreationDate.HasValue)
        {
            var dateTime = currentPhotoInfo.CreationDate.Value;

            string format1 = dateTime.ToString("G");
            
            originalContentDateInputField.text = format1;
        }
        else
        {
            originalContentDateInputField.text = string.Empty;
        }

        // Set toggle states
        if (currentPhotoInfo.IsNotDated)
        {
            unknownDateToggle.isOn = true;
            originalContentDateToggle.isOn = false;
        }
        else
        {
            unknownDateToggle.isOn = false;
            originalContentDateToggle.isOn = true;
        }

        // Set private toggle
        privateToggle.isOn = currentPhotoInfo.IsPrivate;

        // Set DigiKam todo text if it exists
        if (!string.IsNullOrEmpty(currentPhotoInfo.TodoCaptionText))
        {
            digiKamTodoInputField.text = currentPhotoInfo.TodoCaptionText;
        }
        else
        {
            digiKamTodoInputField.text = string.Empty;
        }

        currentPhotoInfo.HasTodoCaption = !string.IsNullOrEmpty(currentPhotoInfo.TodoCaptionText);
    }

    // This method should be called by the OK button in the UI
    public void OnOKButtonClicked()
    {
        // Collect the modified values from UI elements
        CollectUIValuesAndUpdatePhotoInfo();
        
        if (onPhotoActionComplete != null)
        {
            onPhotoActionComplete(currentPhotoInfo);
        }
       
    }

    private void CollectUIValuesAndUpdatePhotoInfo()
    {
        if (currentPhotoInfo == null) return;
        
        // Update creation date from input field
        if (!string.IsNullOrEmpty(originalContentDateInputField.text))
        {
            if (DateTime.TryParse(originalContentDateInputField.text, out DateTime parsedDate))
            {
                currentPhotoInfo.CreationDate = parsedDate;
            } // if this parse fails then fallback to original creation date
            else
            {
                // log the error
                var originalOriginalCreationDate =  currentPhotoInfo.CreationDate.Value.ToString("G");
                Debug.LogError($"Failed to parse updated Original Content creation date {originalContentDateInputField.text}, we will fall back to the incoming value: {originalOriginalCreationDate}");
                currentPhotoInfo.CreationDate = originalPhotoInfo.CreationDate;
            }
        }
        else
        {
            currentPhotoInfo.CreationDate = null;
        }
        
        // Update undated status based on toggle states
        currentPhotoInfo.IsNotDated = unknownDateToggle.isOn;
        
        // Update private status
        currentPhotoInfo.IsPrivate = privateToggle.isOn;
        
        // Update DigiKam todo text
        currentPhotoInfo.TodoCaptionText = digiKamTodoInputField.text;
        currentPhotoInfo.HasTodoCaption = !string.IsNullOrEmpty(currentPhotoInfo.TodoCaptionText);
    }

    private void OnQuitButtonClicked()
    {
        // Close the popup without saving changes
        HidePopup();
    }

    private void OnCancelButtonClicked()
    {
        // Close the popup without saving changes
        HidePopup();
    }

    private void OnOriginalContentDateToggleChanged(bool isOn)
    {
        if (isOn)
        {
            // If original content date is checked, uncheck unknown date
            unknownDateToggle.isOn = false;
            
            // Update the photo info
            if (currentPhotoInfo != null)
            {
                currentPhotoInfo.IsNotDated = false;
            }
        }
        else if (!unknownDateToggle.isOn)
        {
            // If both are unchecked, force original content date to be checked
            originalContentDateToggle.isOn = true;
        }
    }

    private void OnUnknownDateToggleChanged(bool isOn)
    {
        if (isOn)
        {
            // If unknown date is checked, uncheck original content date
            originalContentDateToggle.isOn = false;
            
            // Update the photo info
            if (currentPhotoInfo != null)
            {
                currentPhotoInfo.IsNotDated = true;
            }
        }
        else if (!originalContentDateToggle.isOn)
        {
            // If both are unchecked, force unknown date to be checked
            unknownDateToggle.isOn = true;
        }
    }

    private void OnPrivateToggleChanged(bool isOn)
    {
        if (currentPhotoInfo != null)
        {
            // Update the photo info with the private status
            // TODO: Implement the actual logic to update the photo info
        }
    }

    // Update is called once per frame
    void Update()
    {
        
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
}
