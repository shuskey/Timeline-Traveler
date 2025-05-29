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

public class FamilyPhotoHallPanel : MonoBehaviour
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
        
        photoInfoList.Add(photoInfo);
        numberOfEvents = 1;
        DisplayHallPanelImageTexture();
        dateTextFieldName.text = year.ToString();
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
        var stringToReturn = photoInfoList[currentEventIndex].ItemLabel;
        if (string.IsNullOrEmpty(stringToReturn))
            return "No title found for this photo";
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
}
