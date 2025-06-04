using Assets.Scripts.Utilities;
using Assets.Scripts.DataObjects;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

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
        HideDetailsDialog();
        // This will become visible when an interaction occurs
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
        Debug.Log($"[FamilyPhotoDetailsHandler] DisplayThisPhoto called for photo: {photoToDisplay?.FileName}, year: {year}");
        
        photoInfoObject = photoToDisplay;

        // Title and year
        string title = !string.IsNullOrEmpty(photoInfoObject.Description) ? photoInfoObject.Description : photoInfoObject.FileName;
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
        additionalInstructionsGameObject.GetComponent<Text>().text = " ^ or E to open photo. Previous < or > Next";

        // Set the image
        if (photoTexture != null)
        {
            // Use ImageUtils to create sprite with proper EXIF rotation handling
            var (sprite, rotation) = ImageUtils.CreateSpriteFromTexture(photoTexture, photoInfoObject, cropToFaceRegion: false, maxTextureSize: 780);
            
            var imageComponent = imageGameObject.GetComponent<Image>();
            imageComponent.sprite = sprite;
            
            // Apply the EXIF rotation to the image GameObject
            imageGameObject.transform.rotation = Quaternion.Euler(0, 0, rotation);
        }
        else
        {
            imageGameObject.GetComponent<Image>().sprite = noImageForThisPhoto;
            // Reset rotation when using fallback image
            imageGameObject.transform.rotation = Quaternion.identity;
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
        var gpsInfo = new System.Text.StringBuilder();
        
        // Add location information from tags if available
        string locationString = GetBranchString(photo.Tags, "Locations");
        if (!string.IsNullOrEmpty(locationString))
        {
            gpsInfo.AppendLine($"Location: {locationString}");
        } else if (photo.PositionLatitude != 0 || photo.PositionLongitude != 0)
        {
            gpsInfo.AppendLine($"Lat: {photo.PositionLatitude:F6}° Lng: {photo.PositionLongitude:F6}°");
            
            if (photo.PositionAltitude != 0)
            {
                gpsInfo.AppendLine($"Alt: {photo.PositionAltitude:F1}m");
            }
        }
        else if (string.IsNullOrEmpty(locationString))
        {
            gpsInfo.AppendLine("No GPS data");
        }
        
        return gpsInfo.ToString().Trim();
    }

    /// <summary>
    /// Builds a location string from photo tags by traversing up the tag tree from the tip to the branch root.
    /// </summary>
    /// <param name="tags">Dictionary of photo tags</param>
    /// <param name="branchName">The root branch name to search for (default: "Locations")</param>
    /// <returns>Concatenated string of tag names from tip to branch root, or empty string if no matching branch found</returns>
    private string GetBranchString(Dictionary<int, PhotoTag> tags, string branchName = "Locations")
    {
        if (tags == null || tags.Count == 0)
        {
            return string.Empty;
        }

        // Find the root tag with the specified branch name
        PhotoTag branchRoot = null;
        foreach (var tag in tags.Values)
        {
            if (string.Equals(tag.TagName, branchName, StringComparison.OrdinalIgnoreCase))
            {
                branchRoot = tag;
                break;
            }
        }

        if (branchRoot == null)
        {
            return string.Empty;
        }

        // Find all tags that belong to this branch (have the branch root as an ancestor)
        var branchTags = new List<PhotoTag>();
        foreach (var tag in tags.Values)
        {
            if (tag.TagId != branchRoot.TagId && IsDescendantOf(tag, branchRoot.TagId, tags))
            {
                branchTags.Add(tag);
            }
        }

        if (branchTags.Count == 0)
        {
            return string.Empty;
        }

        // Find the tip tags (tags with no children in this branch)
        var tipTags = branchTags.Where(tag => !branchTags.Any(otherTag => otherTag.ParentTagId == tag.TagId)).ToList();

        // For each tip tag, build the path up to (but not including) the branch root
        var locationStrings = new List<string>();
        foreach (var tipTag in tipTags)
        {
            var pathComponents = new List<string>();
            var currentTag = tipTag;

            // Walk up the tree until we reach the branch root
            while (currentTag != null && currentTag.TagId != branchRoot.TagId)
            {
                pathComponents.Add(currentTag.TagName);
                
                // Find parent tag
                currentTag = tags.Values.FirstOrDefault(t => t.TagId == currentTag.ParentTagId);
                
                // Safety check to prevent infinite loops
                if (currentTag != null && pathComponents.Count > 10)
                {
                    Debug.LogWarning($"Possible circular reference in tag hierarchy for tag: {tipTag.TagName}");
                    break;
                }
            }

            if (pathComponents.Count > 0)
            {
                // Join path components with " " separator (tip first, then parents)
                locationStrings.Add(string.Join(" ", pathComponents));
            }
        }

        return string.Join("; ", locationStrings);
    }

    /// <summary>
    /// Checks if a tag is a descendant of the specified ancestor tag ID.
    /// </summary>
    /// <param name="tag">The tag to check</param>
    /// <param name="ancestorTagId">The ancestor tag ID to search for</param>
    /// <param name="allTags">Dictionary of all available tags</param>
    /// <returns>True if the tag is a descendant of the ancestor</returns>
    private bool IsDescendantOf(PhotoTag tag, int ancestorTagId, Dictionary<int, PhotoTag> allTags)
    {
        var currentTag = tag;
        var visitedTags = new HashSet<int>(); // Prevent infinite loops

        while (currentTag != null && currentTag.ParentTagId != 0)
        {
            if (visitedTags.Contains(currentTag.TagId))
            {
                Debug.LogWarning($"Circular reference detected in tag hierarchy for tag: {currentTag.TagName}");
                break;
            }
            visitedTags.Add(currentTag.TagId);

            if (currentTag.ParentTagId == ancestorTagId)
            {
                return true;
            }

            // Move to parent tag
            allTags.TryGetValue(currentTag.ParentTagId, out currentTag);
        }

        return false;
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
