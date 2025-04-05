using Assets.Scripts.DataObjects;
using Assets.Scripts.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FamilyPhotoDetailsHandler : MonoBehaviour
{
    public Sprite noImageForThisEvent;    
    public GameObject imageGameObject;
    public GameObject yearAndTitleGameObject;
    public GameObject descriptionGameObject;
    public GameObject dateRangeGameObject;
    public GameObject panelCountGameObject;
    public GameObject additionalInstructionsGameObject;

    private FamilyPhoto familyPhotoObject;
    private CanvasGroup canvasGroup;


    // Start is called before the first frame update
    void Start()
    {
        HideDetailsDialog();
    }

    public void ClearEventDisplay()
    {
        HideDetailsDialog();

        familyPhotoObject = null;
        yearAndTitleGameObject.GetComponent<Text>().text = null;
        descriptionGameObject.GetComponent<Text>().text = null;
        dateRangeGameObject.GetComponent<Text>().text = null;
        panelCountGameObject.GetComponent<Text>().text = null;
        additionalInstructionsGameObject.GetComponent<Text>().text = null;

        imageGameObject.GetComponent<Image>().sprite = noImageForThisEvent;
    }

    public void DisplayThisEvent(FamilyPhoto eventToDisplay, int currentEventIndex, int numberOfEvents, Texture2D familyPhotoImage_Texture)
    {
        //make the panel visible
        familyPhotoObject = eventToDisplay;

        yearAndTitleGameObject.GetComponent<Text>().text = familyPhotoObject.year + " " + familyPhotoObject.itemLabel;
        descriptionGameObject.GetComponent<Text>().text = familyPhotoObject.description;
        string dateRangeString = "";
        if (!String.IsNullOrEmpty(familyPhotoObject.eventStartDate)) 
            dateRangeString = JsonUtility.FromJson<DateTime>(("\"" + familyPhotoObject.eventStartDate + "\"")).ToString("dd MMM yyyy");
        if (!String.IsNullOrEmpty(familyPhotoObject.eventEndDate))
            dateRangeString = dateRangeString + " - " + JsonUtility.FromJson<DateTime>(("\"" + familyPhotoObject.eventEndDate + "\"")).ToString("dd MMM yyyy");
        dateRangeGameObject.GetComponent<Text>().text =  dateRangeString;
        var eventTally = numberOfEvents == 0 ? "0 / 0" : $"{currentEventIndex + 1} / {numberOfEvents}";
        panelCountGameObject.GetComponent<Text>().text =  $"Event: {eventTally}";

        additionalInstructionsGameObject.GetComponent<Text>().text = "            ^   or E to interact\n" + 
                                                                     " Previous < or > Next";

        var cropSize = Math.Min(familyPhotoImage_Texture.width, familyPhotoImage_Texture.height);
        var xStart = (familyPhotoImage_Texture.width - cropSize) / 2;
        var yStart = (familyPhotoImage_Texture.height - cropSize) / 2;

        imageGameObject.GetComponent<Image>().sprite = Sprite.Create(familyPhotoImage_Texture, new Rect(xStart, yStart, cropSize, cropSize), new Vector2(0.5f, 0.5f), 100f);
        ShowDetailsDialog();
    }

    // Update is called once per frame
    void Update()
    {

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
