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
        eventDetailsHandlerScript.ClearEventDisplay();
    }

    public void DisplayHallPanelImageTexture()
    {
        if (numberOfEvents == 0)
        {
            setPanelTexture(noEventsThisYear_Texture);
            return;
        }
        var eventToShow = topEventsForYear[currentEventIndex];
        if (string.IsNullOrEmpty(eventToShow.picture))
        {
             setPanelTexture(noImageThisEvent_Texture);
            return;
        }
        // for now just log the URL
        Debug.Log("Downloading image from: " + eventToShow.picture + "?width=400px");
        /* FIX LATER TODO:
        StartCoroutine(DownloadImage(eventToShow.picture + "?width=400px"));
        //DownloadImage(eventToShow.picture + "?width=400px");
        */
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
        // ensure this is a secure URL
        string secureUrl = MediaUrl.Replace("http://", "https://");
        if (!MediaUrl.StartsWith("https://"))
            MediaUrl = "https://" + MediaUrl;
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(MediaUrl);
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.ProtocolError)
            Debug.LogError(request.error);
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
        this.eventImage_Texture = (Texture2D)textureToSet;
    }

    void setPanelTextureOld(Texture textureToSet)
    {
        this.gameObject.transform.Find("ImagePanel").GetComponent<Renderer>().material.mainTexture = textureToSet;
        eventImage_Texture = (Texture2D)textureToSet;
    }
}
