using Assets.Scripts.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FamilyPhotoDetailsHandler : MonoBehaviour
{
    public Sprite noImageForThisPhoto;    
    public GameObject imageGameObject;
    public GameObject yearAndTitleGameObject;
    public GameObject descriptionGameObject;
    public GameObject cameraInfoGameObject;
    public GameObject gpsInfoGameObject;
    public GameObject panelCountGameObject;
    public GameObject additionalInstructionsGameObject;

    private PhotoInfo photoInfoObject;
    private CanvasGroup canvasGroup;

    // Start is called before the first frame update
    void Start()
    {
        // Removed HideDetailsDialog() to make it behave like PersonDetailsHandler
        // The panel will now be visible by default until explicitly hidden
    }

    public void ClearPhotoDisplay()
    {
        HideDetailsDialog();

        photoInfoObject = null;
        yearAndTitleGameObject.GetComponent<Text>().text = null;
        descriptionGameObject.GetComponent<Text>().text = null;
        cameraInfoGameObject.GetComponent<Text>().text = null;
        gpsInfoGameObject.GetComponent<Text>().text = null;
        panelCountGameObject.GetComponent<Text>().text = null;
        additionalInstructionsGameObject.GetComponent<Text>().text = null;

        imageGameObject.GetComponent<Image>().sprite = noImageForThisPhoto;
    }

    public void DisplayThisPhoto(PhotoInfo photoToDisplay, int currentPhotoIndex, int numberOfPhotos, Texture2D photoTexture, int year)
    {
        Debug.Log($"[FamilyPhotoDetailsHandler] DisplayThisPhoto called for photo: {photoToDisplay?.ItemLabel}, year: {year}");
        
        photoInfoObject = photoToDisplay;

        // Title and year
        string title = !string.IsNullOrEmpty(photoInfoObject.ItemLabel) ? photoInfoObject.ItemLabel : "Unknown Photo";
        yearAndTitleGameObject.GetComponent<Text>().text = $"{year} - {title}";

        // Photo metadata description
        string description = BuildPhotoDescription(photoInfoObject);
        descriptionGameObject.GetComponent<Text>().text = description;

        // Camera information
        string cameraInfo = BuildCameraInfo(photoInfoObject);
        cameraInfoGameObject.GetComponent<Text>().text = cameraInfo;

        // GPS information
        string gpsInfo = BuildGPSInfo(photoInfoObject);
        gpsInfoGameObject.GetComponent<Text>().text = gpsInfo;

        // Photo count
        var photoTally = numberOfPhotos == 0 ? "0 / 0" : $"{currentPhotoIndex + 1} / {numberOfPhotos}";
        panelCountGameObject.GetComponent<Text>().text = $"Photo: {photoTally}";

        // Instructions
        additionalInstructionsGameObject.GetComponent<Text>().text = "            ^   or E to open photo\n" + 
                                                                     " Previous < or > Next";

        // Set the image
        if (photoTexture != null)
        {
            var cropSize = Math.Min(photoTexture.width, photoTexture.height);
            var xStart = (photoTexture.width - cropSize) / 2;
            var yStart = (photoTexture.height - cropSize) / 2;
            imageGameObject.GetComponent<Image>().sprite = Sprite.Create(photoTexture, new Rect(xStart, yStart, cropSize, cropSize), new Vector2(0.5f, 0.5f), 100f);
        }
        else
        {
            imageGameObject.GetComponent<Image>().sprite = noImageForThisPhoto;
        }

        Debug.Log($"[FamilyPhotoDetailsHandler] About to call ShowDetailsDialog()");
        ShowDetailsDialog();
        Debug.Log($"[FamilyPhotoDetailsHandler] ShowDetailsDialog() completed");
    }

    private string BuildPhotoDescription(PhotoInfo photo)
    {
        var description = new System.Text.StringBuilder();
        
        if (photo.ImageRating > 0)
        {
            description.AppendLine($"Rating: {photo.ImageRating}/5 stars");
        }

        if (photo.CreationDate.HasValue)
        {
            description.AppendLine($"Taken: {photo.CreationDate.Value:dd MMM yyyy HH:mm}");
        }

        if (photo.DigitizationDate.HasValue && photo.DigitizationDate != photo.CreationDate)
        {
            description.AppendLine($"Digitized: {photo.DigitizationDate.Value:dd MMM yyyy}");
        }

        if (photo.ImageId > 0)
        {
            description.AppendLine($"Image ID: {photo.ImageId}");
        }

        return description.ToString().Trim();
    }

    private string BuildCameraInfo(PhotoInfo photo)
    {
        var cameraInfo = new System.Text.StringBuilder();
        
        if (!string.IsNullOrEmpty(photo.CameraMake))
        {
            cameraInfo.AppendLine($"Make: {photo.CameraMake}");
        }

        if (!string.IsNullOrEmpty(photo.CameraModel))
        {
            cameraInfo.AppendLine($"Model: {photo.CameraModel}");
        }

        if (!string.IsNullOrEmpty(photo.CameraLens))
        {
            cameraInfo.AppendLine($"Lens: {photo.CameraLens}");
        }

        return cameraInfo.ToString().Trim();
    }

    private string BuildGPSInfo(PhotoInfo photo)
    {
        if (photo.PositionLatitude != 0 || photo.PositionLongitude != 0)
        {
            var gpsInfo = new System.Text.StringBuilder();
            gpsInfo.AppendLine($"Lat: {photo.PositionLatitude:F6}°");
            gpsInfo.AppendLine($"Lng: {photo.PositionLongitude:F6}°");
            
            if (photo.PositionAltitude != 0)
            {
                gpsInfo.AppendLine($"Alt: {photo.PositionAltitude:F1}m");
            }
            
            return gpsInfo.ToString().Trim();
        }
        
        return "No GPS data";
    }

    private void ShowDetailsDialog()
    {
        if (!canvasGroup) canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 1;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    private void HideDetailsDialog()
    {
        if (!canvasGroup) canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }
}
