using Assets.Scripts.DataObjects;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using Assets.Scripts.DataProviders;
using UnityEngine.UI;

public class TopEventHallPanel : MonoBehaviour, IInteractablePanel
{    
    public string topEventsDataBaseFileName;
    public Texture2D noEventsThisYear_Texture;
    public Texture2D noImageThisEvent_Texture;
    
    private ListOfTopEventsFromDataBase topEventsDataProvider;
    private List<TopEvent> topEventsForYear;
    private int year;
    private int currentEventIndex = 0;
    private int numberOfEvents = 0;
    private TextMeshPro dateTextFieldName;
    private TextMeshPro titleTextFieldName;
    private EventDetailsHandler eventDetailsHandlerScript;
    private Texture2D eventImage_Texture;
    private Dictionary<string, Texture2D> imageCache = new Dictionary<string, Texture2D>();
    
    // Focus tracking
    private bool hasFocus = false;

    // Awake is called when instantiated
    void Awake()
    {
        var textMeshProObjects = gameObject.GetComponentsInChildren<TextMeshPro>();

        dateTextFieldName = textMeshProObjects[0];
        titleTextFieldName = textMeshProObjects[1];
        eventImage_Texture = noImageThisEvent_Texture;
    }

    private void Start()
    {
        GameObject[] eventDetailsPanel = GameObject.FindGameObjectsWithTag("EventDetailsPanel");
        eventDetailsHandlerScript = eventDetailsPanel[0].transform.GetComponent<EventDetailsHandler>();     
    }

    public void LoadTopEventsForYear_fromDataBase(int year)
    {
        var dataPath = Application.streamingAssetsPath + "/";
        topEventsDataProvider = new ListOfTopEventsFromDataBase(dataPath + topEventsDataBaseFileName);

        this.year = year;
        topEventsDataProvider.GetListOfTopEventsFromDataBase(this.year);
        topEventsForYear = topEventsDataProvider.topEventsList;
        numberOfEvents = topEventsForYear.Count;
        DisplayHallPanelImageTexture();
        dateTextFieldName.text = year.ToString();
        titleTextFieldName.text = currentlySelectedEventTitle();
    }

    public void DisplayDetailsInEventDetailsPanel()
    {
        // Set focus when this panel is selected
        hasFocus = true;
        
        if (numberOfEvents != 0)
        {
            eventDetailsHandlerScript.DisplayThisEvent(topEventsForYear[currentEventIndex],
                                                       currentEventIndex,
                                                       numberOfEvents,
                                                       eventImage_Texture);
        }
    }

    public void ClearEventDetailsPanel()
    {
        // Clear focus when this panel is deselected
        hasFocus = false;
        
        eventDetailsHandlerScript.ClearEventDisplay();
    }

    public void DisplayHallPanelImageTexture()
    {
        if (numberOfEvents == 0)
        {
            setPanelTexture(noEventsThisYear_Texture);
            UpdateDetailsPanel();
            return;
        }
        var eventToShow = topEventsForYear[currentEventIndex];
        if (string.IsNullOrEmpty(eventToShow.picture))
        {
             setPanelTexture(noImageThisEvent_Texture);
            UpdateDetailsPanel();
            return;
        }
        
        string imageUrl = eventToShow.picture + "?width=400px";
        
        // Check if we already have this image cached
        if (imageCache.ContainsKey(imageUrl))
        {
            setPanelTexture(imageCache[imageUrl]);
            Debug.Log("Using cached image for: " + imageUrl);
            
            // Update details panel if this panel has focus
            UpdateDetailsPanel();
            return;
        }
        
        // Download the image for this event
        Debug.Log("Downloading image from: " + imageUrl);
        StartCoroutine(DownloadImage(imageUrl));
    }

    public string currentlySelectedEventTitle()
    {
        if (numberOfEvents == 0)
            return $"Year {year}: No events.";
        var stringToReturn = topEventsForYear[currentEventIndex].itemLabel;
        if (string.IsNullOrEmpty(stringToReturn))
            return "No title found for this event";
        return stringToReturn[0].ToString().ToUpper() + stringToReturn.Substring(1);
    }

    private void UpdateDetailsPanel()
    {
        if (hasFocus && numberOfEvents != 0 && eventDetailsHandlerScript != null)
        {
            eventDetailsHandlerScript.DisplayThisEvent(topEventsForYear[currentEventIndex],
                                                       currentEventIndex,
                                                       numberOfEvents,
                                                       eventImage_Texture);
        }
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
        if (numberOfEvents != 0)
            Application.OpenURL(topEventsForYear[currentEventIndex].wikiLink);
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

    IEnumerator DownloadImage(string MediaUrl)
    {
        // Ensure this is a secure URL
        string secureUrl = MediaUrl;
        
        // Convert HTTP to HTTPS
        if (secureUrl.StartsWith("http://"))
        {
            secureUrl = secureUrl.Replace("http://", "https://");
        }
        // Add HTTPS if no protocol is specified
        else if (!secureUrl.StartsWith("https://"))
        {
            secureUrl = "https://" + secureUrl;
        }
        
        Debug.Log($"Downloading image from: {secureUrl}");
        
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(secureUrl);
        yield return request.SendWebRequest();
        
        if (request.result == UnityWebRequest.Result.ConnectionError || 
            request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError($"Error downloading image: {request.error}");
            // Use fallback texture on error
            setPanelTexture(noImageThisEvent_Texture);
            
            // Update details panel even with fallback image
            UpdateDetailsPanel();
        }
        else
        {
            // Successfully downloaded image
            Texture2D downloadedTexture = ((DownloadHandlerTexture)request.downloadHandler).texture;
            
            // Cache the downloaded image
            if (!imageCache.ContainsKey(MediaUrl))
            {
                imageCache[MediaUrl] = downloadedTexture;
            }
            
            setPanelTexture(downloadedTexture);
            Debug.Log("Image downloaded and cached successfully");
            
            // Update details panel if this panel has focus
            UpdateDetailsPanel();
        }
        
        request.Dispose();
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
        this.eventImage_Texture = (Texture2D)textureToSet;
    }

    void setPanelTextureOld(Texture textureToSet)
    {
        this.gameObject.transform.Find("ImagePanel").GetComponent<Renderer>().material.mainTexture = textureToSet;
        eventImage_Texture = (Texture2D)textureToSet;
    }
}
