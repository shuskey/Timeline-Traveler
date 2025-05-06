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

public class FamilyPhotoHallPanel : MonoBehaviour
{    
    public Texture2D noPhotosThisYear_Texture;
    public Texture2D noImageThisEvent_Texture;
    
    private List<FamilyPhoto> familyPhotos = new List<FamilyPhoto>();
    private List<string> onlyThumbnails = new List<string>();
    private int year;
    private int currentEventIndex = 0;
    private int numberOfEvents = 0;
    private TextMeshPro dateTextFieldName;
    private TextMeshPro titleTextFieldName;
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
        familyPhotoImage_Texture = noImageThisEvent_Texture;
    }

    private void Start()
    {
      
    }

    public void LoadFamilyPhotosForYearAndPerson(int personOwnerID, int year, string fileNameString, ExifOrientation orientation = ExifOrientation.TopLeft)
    {
        this.year = year;
        
        var familyPhoto = new FamilyPhoto(
            year: this.year.ToString(), 
            itemLabel: $"Orientation: {orientation} Filename: {Path.GetFileName(fileNameString)}", 
            picturePathInArchive: fileNameString, 
            description: "temp Description", 
            locations: "temp Locations", 
            countries: "temp Countries", 
            pointInTime: "", 
            eventStartDate: "", 
            eventEndDate: "", 
            orientation: orientation);
        familyPhotos.Add(familyPhoto);
        numberOfEvents = 1;
        DisplayHallPanelImageTexture();
        dateTextFieldName.text = year.ToString();
        titleTextFieldName.text = currentlySelectedEventTitle();
    }

    // I need a call that will clear the familyPhotos list
    public void ClearFamilyPhotos()
    {
        familyPhotos.Clear();
        numberOfEvents = 0;
        DisplayHallPanelImageTexture();
    }

    public void DisplayHallPanelImageTexture()
    {
        if (numberOfEvents == 0)
        {
            setPanelTexture(noPhotosThisYear_Texture);
            return;
        }
        var familPhotoToShow = familyPhotos[currentEventIndex];
        if (string.IsNullOrEmpty(familPhotoToShow.picturePathInArchive))
        {
            setPanelTexture(noImageThisEvent_Texture);
            return;
        }
        StartCoroutine(GetPhotoFromPhotoArchive(familPhotoToShow.picturePathInArchive, familPhotoToShow.orientation));
    }

    public string currentlySelectedEventTitle()
    {
        if (numberOfEvents == 0)
            return $"Year {year}: No photos.";
        var stringToReturn = familyPhotos[currentEventIndex].itemLabel;
        if (string.IsNullOrEmpty(stringToReturn))
            return "No title found for this photo";
        return stringToReturn[0].ToString().ToUpper() + stringToReturn.Substring(1);
    }


    IEnumerator GetPhotoFromPhotoArchive(string fullPathtoPhotoInArchive, ExifOrientation orientation = ExifOrientation.TopLeft)
    {        
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(fullPathtoPhotoInArchive);
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log($"Error downloading photo:{fullPathtoPhotoInArchive} error: {request.error}");
            currentDownloadedTexture = noImageThisEvent_Texture;
        }
        else
        {
            // Clean up previous downloaded texture
            if (currentDownloadedTexture != null)
            {
                Destroy(currentDownloadedTexture);
                currentDownloadedTexture = null;
            }
            Texture2D downloaded = ((DownloadHandlerTexture)request.downloadHandler).texture as Texture2D;
            if (downloaded == null)
            {
                Debug.Log($"Error downloading photo:{fullPathtoPhotoInArchive} error: downloaded is null");
                Debug.Log("Will use placeholder texture");  
                downloaded = noImageThisEvent_Texture;
                orientation = ExifOrientation.TopLeft;
            }
            setPanelTexture(downloaded, orientation);
            currentDownloadedTexture = downloaded;
        }
    }

    private (Rect spriteRect, float rotation) GetSpriteRectAndRotationForOrientation(Texture2D texture, ExifOrientation orientation)
    {
        switch (orientation)
        {
            case ExifOrientation.TopLeft:     // Normal
                return (new Rect(0, 0, texture.width, texture.height), 0f);
            case ExifOrientation.TopRight:    // Mirrored
                return (new Rect(0, 0, -texture.width, texture.height), 0f);
            case ExifOrientation.BottomRight: // Rotated 180
                return (new Rect(0, 0, -texture.width, -texture.height), 0f);
            case ExifOrientation.BottomLeft:  // Mirrored and rotated 180
                return (new Rect(0, 0, texture.width, -texture.height), 0f);
            case ExifOrientation.LeftTop:     // Mirrored and rotated 270
                return (new Rect(0, 0, texture.height, texture.width), -270f);
            case ExifOrientation.RightTop:    // Rotated 90
                return (new Rect(0, 0, texture.height, texture.width), -90f);
            case ExifOrientation.RightBottom: // Mirrored and rotated 90
                return (new Rect(0, 0, -texture.height, texture.width), -90f);
            case ExifOrientation.LeftBottom:  // Rotated 270
                return (new Rect(0, 0, texture.height, texture.width), 270f);
            default:
                return (new Rect(0, 0, texture.width, texture.height), 0f);
        }
    }

    private Texture2D ResizeTexture(Texture2D source, int targetWidth)
    {
        // Calculate height maintaining aspect ratio
        float aspectRatio = (float)source.height / source.width;
        int targetHeight = Mathf.RoundToInt(targetWidth * aspectRatio);

        // Create a new texture with the target dimensions
        Texture2D resized = new Texture2D(targetWidth, targetHeight, source.format, false);
        
        // Create a temporary RenderTexture to do the resizing
        RenderTexture rt = new RenderTexture(targetWidth, targetHeight, 0, RenderTextureFormat.ARGB32);
        rt.Create();

        // Store the current render texture
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = rt;

        // Copy the source texture to the render texture
        Graphics.Blit(source, rt);

        // Read the pixels from the render texture
        resized.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), 0, 0);
        resized.Apply();

        // Restore the previous render texture
        RenderTexture.active = previous;
        rt.Release();

        return resized;
    }

    void setPanelTexture(Texture2D textureToSet, ExifOrientation orientation = ExifOrientation.TopLeft, bool crop = true)
    {
        try
        {
            // Resize the texture if it's too large (e.g., if width > 1024)
            const int MAX_TEXTURE_SIZE = 780;
            Texture2D finalTexture = textureToSet;
            
            if (textureToSet.width > MAX_TEXTURE_SIZE || textureToSet.height > MAX_TEXTURE_SIZE)
            {
                //Debug.Log($"Resizing texture from {textureToSet.width}x{textureToSet.height} to max dimension {MAX_TEXTURE_SIZE}");
                finalTexture = ResizeTexture(textureToSet, MAX_TEXTURE_SIZE);
            }

            if (crop)
            {
                int cropSize = Math.Min(finalTexture.width, finalTexture.height);
                int xStart = (finalTexture.width - cropSize) / 2;
                int yStart = (finalTexture.height - cropSize) / 2;

                Color[] pixels = finalTexture.GetPixels(xStart, yStart, cropSize, cropSize);
                finalTexture = new Texture2D(cropSize, cropSize, finalTexture.format, false);
                finalTexture.SetPixels(pixels);
                finalTexture.Apply();
            }
            // Get sprite rect and rotation based on orientation
            var (spriteRect, rotation) = GetSpriteRectAndRotationForOrientation(finalTexture, orientation);

            // Create sprite with the adjusted rect that may mirror the image based on orientation  
            Sprite sprite = Sprite.Create(finalTexture, spriteRect, new Vector2(0.5f, 0.5f), 100f);

            // Step 1: Find the Canvas child
            Transform canvasTransform = this.gameObject.transform.Find("Canvas");
            if (canvasTransform == null)
            {
                Debug.LogWarning("Canvas child not found!");
                return;
            }

            // Step 2: Find the ImagePanel child under Canvas
            Transform imagePanelTransform = canvasTransform.Find("ImagePanel");
            if (imagePanelTransform == null)
            {
                Debug.LogWarning("ImagePanel child not found under Canvas!");
                return;
            }


            // Get the Image component from the found transform
            UnityEngine.UI.Image image = imagePanelTransform.GetComponent<UnityEngine.UI.Image>();
            if (image == null)
            {
                Debug.LogWarning("Image component not found on ImagePanel!");
                return;
            }
            image.sprite = sprite; 
            // Apply the sprite and rotation
            image.transform.localRotation = Quaternion.Euler(0, 0, rotation);      
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Error in setPanelTexture: {ex.Message}, for Title: {titleTextFieldName.text}, and Date: {dateTextFieldName.text}");
        }
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
