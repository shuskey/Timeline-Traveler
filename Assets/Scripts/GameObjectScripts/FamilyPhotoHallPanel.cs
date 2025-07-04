using Assets.Scripts.DataObjects;
using Assets.Scripts.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using Assets.Scripts.Utilities;
using Assets.Scripts.ServiceProviders.FamilyHistoryPictureProvider;
using Assets.Scripts.ServiceProviders;

public class FamilyPhotoHallPanel : MonoBehaviour, IInteractablePanel
{    
    public Sprite noPhotosThisYear_Sprite;
    public Sprite noImageThisEvent_Sprite;
    private List<PhotoInfo> photoInfoList = new List<PhotoInfo>();
    private List<string> onlyThumbnails = new List<string>();
    private int year;
    private int currentEventIndex = 0;
    private int numberOfEvents = 0;
    private TextMeshPro dateTextFieldName;
    private TextMeshPro titleTextFieldName;
    private TextMeshPro errorMessageTextFieldName;
    private Texture2D familyPhotoImage_Texture;    
    // --- ADDED FIELDS FOR CLEANUP ---
    private RenderTexture currentRenderTexture; // Track the current RenderTexture
    private Texture2D currentDownloadedTexture; // Track the current downloaded Texture2D
    
    // Focus tracking
    private bool hasFocus = false;
    private bool autoRotateToFacePlayer = false;
    
    // Details handler reference
    private FamilyPhotoDetailsHandler familyPhotoDetailsHandlerScript;
    private FixOrFlagPhotoHandler fixOrFlagPhotoHandlerScript;

    // Awake is called when instantiated
    void Awake()
    {
        var textMeshProObjects = gameObject.GetComponentsInChildren<TextMeshPro>();
        
        dateTextFieldName = textMeshProObjects[0];
        titleTextFieldName = textMeshProObjects[1];
        errorMessageTextFieldName = textMeshProObjects[2];
        familyPhotoImage_Texture = noImageThisEvent_Sprite.texture;
    }

    private void Start()
    {
        // Initially hide the error message
        HideErrorMessage();
        
        // Find the photo details panel
        GameObject[] familyPhotoDetailsPanel = GameObject.FindGameObjectsWithTag("FamilyPhotoDetailsPanel");
        GameObject[] fixOrFlagPhotoPanel = GameObject.FindGameObjectsWithTag("FixOrFlagPhotoPanel");
        if (familyPhotoDetailsPanel.Length > 0)
        {
            familyPhotoDetailsHandlerScript = familyPhotoDetailsPanel[0].transform.GetComponent<FamilyPhotoDetailsHandler>();
        }
        else
        {
            Debug.LogWarning("FamilyPhotoDetailsPanel not found! Make sure there's a GameObject with tag 'FamilyPhotoDetailsPanel'");
        }
        if (fixOrFlagPhotoPanel.Length > 0)
        {
            fixOrFlagPhotoHandlerScript = fixOrFlagPhotoPanel[0].transform.GetComponent<FixOrFlagPhotoHandler>();
        }
        else
        {
            Debug.LogWarning("FixOrFlagPhotoPanel not found! Make sure there's a GameObject with tag 'FixOrFlagPhotoPanel'");
        }
    }

    private void ShowErrorMessage(string message)
    {
        if (errorMessageTextFieldName != null)
        {
            errorMessageTextFieldName.text = message;
            // Show the parent object to make the error message visible
            if (errorMessageTextFieldName.transform.parent != null)
            {
                errorMessageTextFieldName.transform.parent.gameObject.SetActive(true);
            }
        }
    }

    private void HideErrorMessage()
    {
        if (errorMessageTextFieldName != null)
        {
            errorMessageTextFieldName.text = "";
            // Hide the parent object to make the error message invisible
            if (errorMessageTextFieldName.transform.parent != null)
            {
                errorMessageTextFieldName.transform.parent.gameObject.SetActive(false);
            }
        }
    }

    private IEnumerator LoadImageWithErrorHandling(Image destinationImagePanel, PhotoInfo photoInfo, Texture2D fallbackTexture)
    {
        // Start the image loading coroutine
        yield return StartCoroutine(ImageUtils.SetImagePanelTextureFromPhotoArchive(destinationImagePanel, photoInfo, fallbackTexture));
        
        // Store the current texture for the details panel
        if (destinationImagePanel.sprite != null && destinationImagePanel.sprite.texture != null)
        {
            familyPhotoImage_Texture = destinationImagePanel.sprite.texture as Texture2D;
        }
        else
        {
            familyPhotoImage_Texture = fallbackTexture;
        }
        
        // Update the details panel NOW that the image has finished loading and familyPhotoImage_Texture is updated
        if (familyPhotoDetailsHandlerScript != null && hasFocus)
        {
            // I have been getting an index out of bounds error here, so I am adding a check and a log
            if (currentEventIndex >= photoInfoList.Count)
            {
                Debug.LogError($"[FamilyPhotoHallPanel] For year {year}, Index out of bounds error: currentEventIndex={currentEventIndex}, photoInfoList.Count={photoInfoList.Count}");
                Debug.LogError($"ImageId: {photoInfo.ImageId}, FileName: {photoInfo.FileName}, Description: {photoInfo.Description}");
            }
            var currentPhoto = photoInfoList[currentEventIndex];
            Debug.Log($"[FamilyPhotoHallPanel] Updating details handler after image load: Photo ID={currentPhoto.ImageId}");
            familyPhotoDetailsHandlerScript.DisplayThisPhoto(currentPhoto,
                                                     currentEventIndex,
                                                     numberOfEvents,
                                                     familyPhotoImage_Texture,
                                                     year);
        }
        
        // After image loading is complete, check for error messages and update the error field
        if (!string.IsNullOrEmpty(photoInfo.ErrorMessage))
        {
            ShowErrorMessage(photoInfo.ErrorMessage);
        }
        else
        {
            HideErrorMessage();
        }
    }

    public void LoadFamilyPhotosForYearAndPerson(int personOwnerID, int year, PhotoInfo photoInfo)
    {
        this.year = year;
        
        photoInfoList.Clear();
        photoInfoList.Add(photoInfo);
        numberOfEvents = photoInfoList.Count;
        currentEventIndex = 0;
        DisplayHallPanelImageTexture();
        dateTextFieldName.text = year.ToString();
        titleTextFieldName.text = currentlySelectedEventTitle();
    }

    public void LoadFamilyPhotosForYearAndPerson(int personOwnerID, int year, List<PhotoInfo> photoInfos)
    {
        this.year = year;
        
        photoInfoList.Clear();
        if (photoInfos != null && photoInfos.Count > 0)
        {
            photoInfoList.AddRange(photoInfos);
        }
        numberOfEvents = photoInfoList.Count;
        currentEventIndex = 0;
        DisplayHallPanelImageTexture();
        
        // Handle special year values with descriptive text
        if (year == -1)
        {
            dateTextFieldName.text = "Needs Review";
        }
        else
        {
            dateTextFieldName.text = year.ToString();
        }
        
        titleTextFieldName.text = currentlySelectedEventTitle();
    }

    // I need a call that will clear the familyPhotos list
    public void ClearFamilyPhotos()
    {
        photoInfoList.Clear();
        numberOfEvents = 0;
        currentEventIndex = 0; // Reset the event index as well
        HideErrorMessage();
        DisplayHallPanelImageTexture();
        // Update the title to reflect that there are no photos
        titleTextFieldName.text = currentlySelectedEventTitle();
    }

    public void SetPanelTitleAndAutoRotate(string title, bool autoRotate = false)
    {
        titleTextFieldName.text = title;
        autoRotateToFacePlayer = autoRotate;
        Debug.Log($"[FamilyPhotoHallPanel] Auto-rotation set to: {autoRotate}");
    }

    public void DisplayHallPanelImageTexture()
    {
        var destinationImagePanel = GetUICanvasImagePanel();
        if (destinationImagePanel == null)
        {
            ShowErrorMessage("Image panel not found - UI setup error");
            return;
        }
        
        if (numberOfEvents == 0)
        {
            ImageUtils.SetImagePanelTexture(destinationImagePanel, noPhotosThisYear_Sprite.texture);
            ShowErrorMessage($"No photos found for year {year}");
            return;
        }
        var familPhotoToShow = photoInfoList[currentEventIndex];
        if (string.IsNullOrEmpty(familPhotoToShow.PicturePathInArchive))
        {
            ImageUtils.SetImagePanelTexture(destinationImagePanel, noImageThisEvent_Sprite.texture);
            string fileName = !string.IsNullOrEmpty(familPhotoToShow.FullPathToFileName) ? familPhotoToShow.FullPathToFileName : "Unknown file";
            ShowErrorMessage($"No image path available for photo '{fileName}'");
            return;
        }
        StartCoroutine(LoadImageWithErrorHandling(destinationImagePanel, familPhotoToShow, noImageThisEvent_Sprite.texture));
    }

    public string currentlySelectedEventTitle()
    {
        if (numberOfEvents == 0)
            return $"Year {year}: No photos.";
        
        var currentPhoto = photoInfoList[currentEventIndex];
        
        // Prioritize description over filename
        string stringToReturn = null;
        if (!string.IsNullOrEmpty(currentPhoto.Description))
        {
            stringToReturn = currentPhoto.Description;
        }
        else if (!string.IsNullOrEmpty(currentPhoto.FileName))
        {
            stringToReturn = currentPhoto.FileName;
        }
        else
        {
            return "No title found for this photo";
        }
        
        return stringToReturn[0].ToString().ToUpper() + stringToReturn.Substring(1);
    }

  
    Image GetUICanvasImagePanel()
    {
        // Step 1: Find the Canvas child
        Transform canvasTransform = this.gameObject.transform.Find("Canvas");
        if (canvasTransform == null)
        {
            Debug.LogWarning("Canvas child not found!");
            return null;
        }

        // Step 2: Find the ImagePanel child under Canvas
        Transform imagePanelTransform = canvasTransform.Find("ImagePanel");
        if (imagePanelTransform == null)
        {
            Debug.LogWarning("ImagePanel child not found under Canvas!");
            return null;
        }

        // Get the Image component from the found transform
        UnityEngine.UI.Image image = imagePanelTransform.GetComponent<UnityEngine.UI.Image>();
        if (image == null)
        {
            Debug.LogWarning("Image component not found on ImagePanel!");
            return null;
        }
        return image;
    }

    // IInteractablePanel implementation
    public void DisplayDetailsInEventDetailsPanel()
    {
        Debug.Log($"[FamilyPhotoHallPanel] DisplayDetailsInEventDetailsPanel called. numberOfEvents: {numberOfEvents}, familyPhotoDetailsHandlerScript null: {familyPhotoDetailsHandlerScript == null}");
        
        // Set focus when this panel is selected
        hasFocus = true;
        
        if (numberOfEvents != 0 && familyPhotoDetailsHandlerScript != null)
        {
            var currentPhoto = photoInfoList[currentEventIndex];
            Debug.Log($"[FamilyPhotoHallPanel] Displaying photo: {currentPhoto?.FileName} for year {year}");
            
            familyPhotoDetailsHandlerScript.DisplayThisPhoto(currentPhoto,
                                                     currentEventIndex,
                                                     numberOfEvents,
                                                     familyPhotoImage_Texture,
                                                     year);
        }
        else
        {
            Debug.LogWarning($"[FamilyPhotoHallPanel] Cannot display details - numberOfEvents: {numberOfEvents}, handler is null: {familyPhotoDetailsHandlerScript == null}");
        }
    }

    public void ClearEventDetailsPanel()
    {
        // Clear focus when this panel is deselected
        hasFocus = false;
        
        if (familyPhotoDetailsHandlerScript != null)
        {
            familyPhotoDetailsHandlerScript.ClearPhotoDisplay();
        }
    }

    public void NextEventInPanel()
    {
        if (numberOfEvents > 1)
        {
            currentEventIndex++;
            if (currentEventIndex >= numberOfEvents)
                currentEventIndex = 0;
            DisplayHallPanelImageTexture();
            titleTextFieldName.text = currentlySelectedEventTitle();
            
            // Details handler is now updated inside LoadImageWithErrorHandling after the image loads
        }
    }

    public void PreviousEventInPanel()
    {
        if (numberOfEvents > 1)
        {
            currentEventIndex--;
            if (currentEventIndex < 0)
                currentEventIndex = numberOfEvents - 1;
            DisplayHallPanelImageTexture();
            titleTextFieldName.text = currentlySelectedEventTitle();
            
            // Details handler is now updated inside LoadImageWithErrorHandling after the image loads
        }
    }

    public void InteractWithPanel()
    {
        if (numberOfEvents != 0)
        {
            // we will launch the fix or flag photo popup with a callback
            fixOrFlagPhotoHandlerScript.ShowFixOrFlagPhotoPopup(photoInfoList[currentEventIndex], OnPhotoActionComplete);
            return; 
        }
    }

    private void OnPhotoActionComplete(PhotoInfo modifiedPhotoInfo)
    {
        // Update the photo info in our list with the modified version
        if (modifiedPhotoInfo != null)
        {
            photoInfoList[currentEventIndex] = modifiedPhotoInfo;
            // Refresh the display to show any changes
            DisplayHallPanelImageTexture();
            titleTextFieldName.text = currentlySelectedEventTitle();

            // We need a call back to our parent, the HallOfFamilyPhotos, to update the photo info in the DigiKamConnector
            // And do what every panel refreshes may be needed to do
            HallOfFamilyPhotos hallOfFamilyPhotos = FindFirstObjectByType<HallOfFamilyPhotos>();
            hallOfFamilyPhotos.UpdatePhotoInfoBackToDigiKam(year, modifiedPhotoInfo);
        }
        
        fixOrFlagPhotoHandlerScript.HideFixOrFlagPhotoPopup();
    }

    void OnDestroy()
    {
        // Cleanup RenderTexture
        if (currentRenderTexture != null)
        {
            RenderTexture.ReleaseTemporary(currentRenderTexture);
            currentRenderTexture = null;
        }
        // Cleanup downloaded Texture2D
        if (currentDownloadedTexture != null)
        {
            Destroy(currentDownloadedTexture);
            currentDownloadedTexture = null;
        }
    }

    private void Update()
    {
        if (autoRotateToFacePlayer)
        {
            RotateToFacePlayer();
        }
    }

    private void RotateToFacePlayer()
    {
        // Find the third person player (assuming it has a specific tag or component)
        GameObject player = FindThirdPersonPlayer();
        if (player == null)
        {
            Debug.LogWarning("[FamilyPhotoHallPanel] No player found for auto-rotation");
            return;
        }

        // Get the direction from this panel to the player
        Vector3 directionToPlayer = player.transform.position - transform.position;
        
        // Project the direction onto the XZ plane (ignore Y component)
        directionToPlayer.y = 0f;
        
        // Only rotate if there's a meaningful direction
        if (directionToPlayer.magnitude > 0.1f)
        {
            // Calculate the target rotation to face the player
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer, Vector3.up);
            
            // Get current rotation
            Vector3 currentEulerAngles = transform.rotation.eulerAngles;
            
            // Create new rotation - only change Y rotation to face player
            Vector3 newEulerAngles = new Vector3(
                // X axis is the tilt of the panel and needs a 90 degree offset
                targetRotation.eulerAngles.x + 90f,  // Update current X rotation
                targetRotation.eulerAngles.y,   // Update Y rotation to face player
                targetRotation.eulerAngles.z   // Keep current Z rotation
            );
            
            // Apply the rotation smoothly
            Quaternion newRotation = Quaternion.Euler(newEulerAngles);
            transform.rotation = Quaternion.Slerp(transform.rotation, newRotation, Time.deltaTime * 3f);
            
            /* Debug logging
            Debug.Log($"[FamilyPhotoHallPanel] Rotating to face player. Current Y: {currentEulerAngles.y:F1}, Target Y: {targetRotation.eulerAngles.y:F1}, New Y: {newEulerAngles.y:F1}");
            */
        }
    }

    private GameObject FindThirdPersonPlayer()
    {
    
        // Try to find the player by tag first
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            return player;
        }
        
        // Try to find any object with "Player" in the name
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.ToLower().Contains("player") || obj.name.ToLower().Contains("character"))
            {
                return obj;
            }
        }
    
        // If still not found, try to find the main camera's parent (common in third person setups)
        Camera mainCamera = Camera.main;
        if (mainCamera != null && mainCamera.transform.parent != null)
        {
            return mainCamera.transform.parent.gameObject;
        }
        
        return null;
    }
}
