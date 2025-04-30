using Assets.Scripts.DataObjects;
using Assets.Scripts.Enums;
using StarterAssets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Assets.Scripts.ServiceProviders.FamilyHistoryPictureProvider;
using Assets.Scripts.ServiceProviders;

public class PersonDetailsHandler : MonoBehaviour
{
    public Sprite femaleImage;
    public Sprite maleImage;
    public Sprite unknownGenderImage;
    public Person personObject;
    public GameObject imageGameObject;
    public GameObject nameGameObject;
    public GameObject birthGameObject;
    public GameObject deathGameObject;
    public GameObject generationGameObject;
    public GameObject currentDateObject;
    public GameObject currentAgeObject;
    public GameObject dateQualityInformationGameObject;
    public GameObject recordIdGameObject;

    private StarterAssetsInputs _input;
    private ThirdPersonController thirdPersonController;
    private bool controllerSubscribed = false;
    private IFamilyHistoryPictureProvider _pictureProvider;

    void Start()
    {
        _input = GetComponent<StarterAssetsInputs>();
        InitializePictureProvider();
    }

    private void InitializePictureProvider()
    {
        if (PlayerPrefs.HasKey(PlayerPrefsConstants.LAST_USED_ROOTS_MAGIC_DATA_FILE_PATH) && 
            PlayerPrefs.HasKey(PlayerPrefsConstants.LAST_USED_DIGIKAM_DATA_FILE_PATH))
        {
            _pictureProvider = new DigiKamFamilyHistoryPictureProvider();
            _pictureProvider.Initialize(new Dictionary<string, string>
            {
                { PlayerPrefsConstants.LAST_USED_ROOTS_MAGIC_DATA_FILE_PATH, PlayerPrefs.GetString(PlayerPrefsConstants.LAST_USED_ROOTS_MAGIC_DATA_FILE_PATH) },
                { PlayerPrefsConstants.LAST_USED_DIGIKAM_DATA_FILE_PATH, PlayerPrefs.GetString(PlayerPrefsConstants.LAST_USED_DIGIKAM_DATA_FILE_PATH) }
            });
        }
    }

    void Update()
    {
        if (!controllerSubscribed)
        {
            thirdPersonController = FindFirstObjectByType<ThirdPersonController>();
            if (thirdPersonController != null)
            {
                SubscribeToControllerEvents();
            }
        }
    }

    private void SubscribeToControllerEvents()
    {
        if (thirdPersonController != null && !controllerSubscribed)
        {
            thirdPersonController.onZCoordinateChanged.AddListener(UpdateCurrentDate);
            controllerSubscribed = true;
        }
    }

    void OnDestroy()
    {
        if (thirdPersonController != null)
        {
            thirdPersonController.onZCoordinateChanged.RemoveListener(UpdateCurrentDate);
        }
    }

    public void ClearPersonDisplay()
    {
        personObject = null;
        DisplayThisPerson(personObject);
    }

    public void DisplayThisPerson(Person personToDisplay, int currentDate = 0)
    {
        personObject = personToDisplay;
        var tempBirthDate = (personObject == null) ? "" : $"{personObject.originalBirthEventDateMonth}/{personObject.originalBirthEventDateDay}/{personObject.originalBirthEventDateYear}";
        var tempDeathDate = (personObject == null) ? "" : $"{personObject.originalDeathEventDateMonth}/{personObject.originalDeathEventDateDay}/{personObject.originalDeathEventDateYear}";

        nameGameObject.GetComponent<Text>().text = (personObject == null) ? "" : personObject.givenName + " " + personObject.surName;
        birthGameObject.GetComponent<Text>().text = (personObject == null) ? "" : $"Birth: {personObject.birthEventDate}, orig: {tempBirthDate}";
        deathGameObject.GetComponent<Text>().text = (personObject == null) ? "" : personObject.isLiving ? "Living" : $"Death: {personObject.deathEventDate}, orig: {tempDeathDate}";
        generationGameObject.GetComponent<Text>().text = (personObject == null) ? "" : $"Generation: {personObject.generation}";
        UpdateCurrentDate(currentDate);
        dateQualityInformationGameObject.GetComponent<Text>().text = (personObject == null) ? "" : personObject.dateQualityInformationString;
        recordIdGameObject.GetComponent<Text>().text = (personObject == null) ? "" : $"RootsMagic DB ID: {personObject.dataBaseOwnerId}";
        var myProfileImage = personObject?.personNodeGameObject.GetComponent<PersonNode>().GetPrimaryPhoto(_pictureProvider);
        if (myProfileImage != null)
        {
            Debug.Log("We got an image!");

            Texture2D texture = new Texture2D(2, 2);  // Size does not matter - will be replaced upon load
            texture.LoadImage(myProfileImage);

            var cropSize = Math.Min(texture.width, texture.height);
            var xStart = (texture.width - cropSize) / 2;
            var yStart = (texture.height - cropSize) / 2;

            imageGameObject.GetComponent<Image>().sprite = Sprite.Create(texture, new Rect(xStart, yStart, cropSize, cropSize), new Vector2(0.5f, 0.5f), 100f);
        }
        else
        {
            imageGameObject.GetComponent<Image>().sprite = (personObject == null) ? unknownGenderImage :
                personObject.gender == Assets.Scripts.Enums.PersonGenderType.Male ? maleImage :
                personObject.gender == Assets.Scripts.Enums.PersonGenderType.Female ? femaleImage : unknownGenderImage;
        }
    }

    private void resetSceneToThisRootPerson()
    {
        if (personObject == null)
            return;

        recordIdGameObject.GetComponent<Text>().text = (personObject == null) ? "" : $"RootsMagic DB ID: {personObject.dataBaseOwnerId}";
        Assets.Scripts.CrossSceneInformation.startingDataBaseId = personObject.dataBaseOwnerId;
        Assets.Scripts.CrossSceneInformation.myTribeType = TribeType.Centered;
        SceneManager.LoadScene("MyTribeScene");
    }

    public void UpdateCurrentDate(int currentDate)
    {
        currentDate = currentDate / 5;
        currentDateObject.GetComponent<Text>().text = (personObject == null) ? "" : $"Current Date: {currentDate}";
        currentAgeObject.GetComponent<Text>().text = (personObject == null) ? "" : $"Current Age: {Mathf.Max(0f, (currentDate - personObject.birthEventDate))}";
    }

    public void OnStartInputAction()
    {
        Debug.Log("Got to OnStartInputAction inside PersonDetailsHandler.");
        resetSceneToThisRootPerson();
    }
} 