using Assets.Scripts.DataObjects;
//using Assets.Scripts.DataProviders;
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
    private FamilyPhotoDetailsHandler familyPhotoDetailsHandlerScript;
    private Texture2D familyPhotoImage_Texture;    

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
        GameObject[] familyPhotoDetailsPanel = GameObject.FindGameObjectsWithTag("FamilyPhotoDetailsPanel");
        familyPhotoDetailsHandlerScript = familyPhotoDetailsPanel[0].transform.GetComponent<FamilyPhotoDetailsHandler>();        
    }

    public void LoadFamilyPhotosForYearAndPerson(int personOwnerID, int year, string photoArchiveDrivePath, string thumbnailSubFolderName)
    {
        this.year = year;
        var filteredFiles = Directory.GetFiles(photoArchiveDrivePath, "*.*", SearchOption.AllDirectories)
                .Where(file => file.ToLower().EndsWith("jpg") || file.ToLower().EndsWith("gif")
                || file.ToLower().EndsWith("png") || file.ToLower().EndsWith("bmp")).ToList();

        onlyThumbnails = filteredFiles.Where(file => file.ToLower().Contains(thumbnailSubFolderName.ToLower())).ToList();

        var fileToUse = onlyThumbnails[Random.Range(0, onlyThumbnails.Count)];
        var fileNameString =  Path.GetFileName(fileToUse);
        var familyPhoto = new FamilyPhoto(this.year.ToString(), fileNameString, fileToUse, "temp Description", "temp Locations", "temp Countries", "", "", "");
        familyPhotos.Add(familyPhoto);
        numberOfEvents = 1;
        DisplayHallPanelImageTexture();
        dateTextFieldName.text = year.ToString();
        titleTextFieldName.text = currentlySelectedEventTitle();
    }

    public void DisplayDetailsInFamilyPhotoDetailsPanel()
    {
        familyPhotoDetailsHandlerScript.DisplayThisEvent(familyPhotos[currentEventIndex],
                                                   currentEventIndex,
                                                   numberOfEvents,
                                                   familyPhotoImage_Texture);
    }

    public void ClearEventDetailsPanel()
    {
        familyPhotoDetailsHandlerScript.ClearEventDisplay();
    }

    public void DisplayHallPanelImageTexture()
    {
        if (numberOfEvents == 0)
        {
            setPanelTexture(noPhotosThisYear_Texture);
            return;
        }
        var eventToShow = familyPhotos[currentEventIndex];
        if (string.IsNullOrEmpty(eventToShow.picturePathInArchive))
        {
            setPanelTexture(noImageThisEvent_Texture);
            return;
        }
        StartCoroutine(GetPhotoFromPhotoArchive(eventToShow.picturePathInArchive));
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

    public void NextEventInPanel()
    {
        currentEventIndex++;
        if (currentEventIndex >= numberOfEvents)
            currentEventIndex = 0;
        DisplayHallPanelImageTexture();
        titleTextFieldName.text = currentlySelectedEventTitle();
    }

    public void InteractWithPanel()
    {
        // What would this be? perhaps a full screen - zoomable/pannable image viewer ??
    }

    public void PreviousEventInPanel()
    {
        currentEventIndex--;
        if (currentEventIndex < 0)
            currentEventIndex = numberOfEvents - 1;
        DisplayHallPanelImageTexture();
        titleTextFieldName.text = currentlySelectedEventTitle();
    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator GetPhotoFromPhotoArchive(string fullPathtoPhotoInArchive)
    {        
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(fullPathtoPhotoInArchive);
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.ProtocolError)
            Debug.Log(request.error);
        else
            setPanelTexture(((DownloadHandlerTexture)request.downloadHandler).texture);
    }

    // an overload to GetPhotoFromPhotoArchive that takes a DigiKam thumbnailId
    IEnumerator GetPhotoFromPhotoArchive(int thumbnailId)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture($"http://localhost:8080/thumbnail/{thumbnailId}");
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.ProtocolError)
            Debug.Log(request.error);
        else
            setPanelTexture(((DownloadHandlerTexture)request.downloadHandler).texture);
    }

    void setPanelTexture(Texture textureToSet, bool crop = true)
    {
        RenderTexture tempTex;

        RenderTexture rTex = RenderTexture.GetTemporary(textureToSet.width, textureToSet.height, 24, RenderTextureFormat.Default);
        Graphics.Blit(textureToSet, rTex);
        if (crop)
        {
            var cropSize = Math.Min(textureToSet.width, textureToSet.height);
            var xStart = (textureToSet.width - cropSize) / 2;
            var yStart = (textureToSet.height - cropSize) / 2;

            tempTex = RenderTexture.GetTemporary(cropSize, cropSize, 24, RenderTextureFormat.Default);

            Graphics.CopyTexture(rTex, 0, 0, xStart, yStart, cropSize, cropSize, tempTex, 0, 0, 0, 0);

            RenderTexture.ReleaseTemporary(rTex);
            rTex = RenderTexture.GetTemporary(cropSize, cropSize, 24, RenderTextureFormat.Default);
            Graphics.Blit(tempTex, rTex);
            RenderTexture.ReleaseTemporary(tempTex);
        }

        this.gameObject.transform.Find("ImagePanel").GetComponent<Renderer>().material.mainTexture = rTex;
        this.familyPhotoImage_Texture = (Texture2D)textureToSet;
    }

    void setPanelTextureOld(Texture textureToSet)
    {
        this.gameObject.transform.Find("ImagePanel").GetComponent<Renderer>().material.mainTexture = textureToSet;
        familyPhotoImage_Texture = (Texture2D)textureToSet;
    }
}
