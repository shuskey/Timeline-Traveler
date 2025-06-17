using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.Utilities;
using System;

public class FixOrFlagPhotoHandler : MonoBehaviour
{
    public Button quitButton;
    public Button cancelButton;
    public Button okButton;
    public Toggle originalContentDateToggle;
    public Toggle unknownDateToggle;
    public Toggle privateToggle;
    public Text originalContentDateText;
    public Text digiKamTodoText;
   
    private CanvasGroup canvasGroup;
    private PhotoInfo currentPhotoInfo;
    
    // Define the delegate type for the callback
    public delegate void PhotoActionCallback(PhotoInfo modifiedPhotoInfo);
    private PhotoActionCallback onPhotoActionComplete;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Initialize canvas group if not already set
        if (!canvasGroup) canvasGroup = GetComponent<CanvasGroup>();
        
        // Set up button listeners
        if (okButton != null)
        {
            okButton.onClick.AddListener(OnOKButtonClicked);
        }
        
        if (quitButton != null)
        {
            quitButton.onClick.AddListener(OnQuitButtonClicked);
        }
        
        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(OnCancelButtonClicked);
        }
        
        // Set up toggle listeners
        if (originalContentDateToggle != null)
        {
            originalContentDateToggle.onValueChanged.AddListener(OnOriginalContentDateToggleChanged);
        }
        
        if (unknownDateToggle != null)
        {
            unknownDateToggle.onValueChanged.AddListener(OnUnknownDateToggleChanged);
        }
        
        if (privateToggle != null)
        {
            privateToggle.onValueChanged.AddListener(OnPrivateToggleChanged);
        }
        
        HidePopup();
    }

    public void ShowFixOrFlagPhotoPopup(PhotoInfo photoInfo, PhotoActionCallback callback)
    {
        currentPhotoInfo = photoInfo;
        onPhotoActionComplete = callback;

        // Initialize UI elements based on PhotoInfo
        if (photoInfo.CreationDate.HasValue)
        {
            originalContentDateText.text = photoInfo.CreationDate.Value.ToString("yyyy-MM-dd HH:mm:ss");
        }
        else
        {
            originalContentDateText.text = string.Empty;
        }

        // Set toggle states
        if (photoInfo.IsUndated)
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
        privateToggle.isOn = photoInfo.IsPrivate;

        // Set DigiKam todo text if it exists
        if (!string.IsNullOrEmpty(photoInfo.DigiKamTodoText))
        {
            digiKamTodoText.text = photoInfo.DigiKamTodoText;
        }
        else
        {
            digiKamTodoText.text = string.Empty;
        }

        ShowPopup();
    }

    // This method should be called by the OK button in the UI
    public void OnOKButtonClicked()
    {
        // Here you would get the modified values from your UI elements
        // For now, we'll just pass back the original PhotoInfo
        // TODO: Modify this to get actual values from UI elements
        if (onPhotoActionComplete != null)
        {
            onPhotoActionComplete(currentPhotoInfo);
        }
        HidePopup();
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
                currentPhotoInfo.IsUndated = false;
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
                currentPhotoInfo.IsUndated = true;
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
        canvasGroup.alpha = 1;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    private void HidePopup()
    {
        if (!canvasGroup) canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }
}
