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
using Assets.Scripts.Utilities;
using Assets.Scripts.DataProviders;

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
    public GameObject digiKamTagIdGameObject;
    public GameObject errorMessageTextField;

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
        
        // Clear error message when clearing person display
        if (errorMessageTextField != null)
        {
            errorMessageTextField.GetComponent<Text>().text = "";
        }
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
    
        Sprite fallbackSprite = (personObject == null) ? unknownGenderImage :
                personObject.gender == Assets.Scripts.Enums.PersonGenderType.Male ? maleImage :
                personObject.gender == Assets.Scripts.Enums.PersonGenderType.Female ? femaleImage : unknownGenderImage;

        if (_pictureProvider != null && personObject != null)
        {
            var photoInfo = _pictureProvider.GetThumbnailPhotoInfoForPerson(personObject.dataBaseOwnerId, 2024);
            if (photoInfo != null)
            {
                var destinationImagePanel = imageGameObject.GetComponent<Image>();
                var fallbackTexture = fallbackSprite.texture;
                // Thumbnails are meant to be cropped to the region of the person
                StartCoroutine(LoadImageWithErrorHandling(destinationImagePanel, photoInfo, fallbackTexture, true));
                
                // Display the DigiKam TagId if available
                if (digiKamTagIdGameObject != null)
                {
                    digiKamTagIdGameObject.GetComponent<Text>().text = photoInfo.TagId != -1 ? $"DigiKam Tag ID: {photoInfo.TagId}" : "DigiKam Tag ID: Not found";
                }
            }
            else
            {
                imageGameObject.GetComponent<Image>().sprite = fallbackSprite; 
                imageGameObject.GetComponent<Image>().transform.localRotation = Quaternion.Euler(0, 0, 0);
                
                // Clear DigiKam TagId when no photo info
                if (digiKamTagIdGameObject != null)
                {
                    digiKamTagIdGameObject.GetComponent<Text>().text = "DigiKam Tag ID: No photo found";
                }
                
                // Set error message for no photo available
                if (errorMessageTextField != null)
                {
                    string personName = $"{personObject.givenName} {personObject.surName}".Trim();
                    errorMessageTextField.GetComponent<Text>().text = $"No Image Available for {personName} (ID: {personObject.dataBaseOwnerId})";
                }
            }
        }
        else
        {
            imageGameObject.GetComponent<Image>().sprite = fallbackSprite; 
            imageGameObject.GetComponent<Image>().transform.localRotation = Quaternion.Euler(0, 0, 0);
            
            // Clear DigiKam TagId when no picture provider or no person
            if (digiKamTagIdGameObject != null)
            {
                digiKamTagIdGameObject.GetComponent<Text>().text = (personObject == null) ? "" : "DigiKam Tag ID: Provider not available";
            }
            
            // Set appropriate error message based on the condition
            if (errorMessageTextField != null)
            {
                if (personObject == null)
                {
                    errorMessageTextField.GetComponent<Text>().text = "";
                }
                else if (_pictureProvider == null)
                {
                    string personName = $"{personObject.givenName} {personObject.surName}".Trim();
                    errorMessageTextField.GetComponent<Text>().text = $"Image provider not configured for {personName} (ID: {personObject.dataBaseOwnerId})";
                }
                else
                {
                    string personName = $"{personObject.givenName} {personObject.surName}".Trim();
                    errorMessageTextField.GetComponent<Text>().text = $"Image provider unavailable for {personName} (ID: {personObject.dataBaseOwnerId})";
                }
            }
        }
    }

    private void resetSceneToThisRootPerson()
    {
        if (personObject == null)
            return;

        recordIdGameObject.GetComponent<Text>().text = (personObject == null) ? "" : $"RootsMagic DB ID: {personObject.dataBaseOwnerId}";
        CrossSceneInformation.startingDataBaseId = personObject.dataBaseOwnerId;
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
        resetSceneToThisRootPerson();
    }

    private IEnumerator LoadImageWithErrorHandling(Image destinationImagePanel, PhotoInfo photoInfo, Texture2D fallbackTexture, bool cropToFaceRegion)
    {
        // Start the image loading coroutine
        yield return StartCoroutine(ImageUtils.SetImagePanelTextureFromPhotoArchive(destinationImagePanel, photoInfo, fallbackTexture, cropToFaceRegion));
        
        // After image loading is complete, check for error messages and update the error field
        if (errorMessageTextField != null)
        {
            if (!string.IsNullOrEmpty(photoInfo.ErrorMessage))
            {
                errorMessageTextField.GetComponent<Text>().text = photoInfo.ErrorMessage;
            }
            else
            {
                errorMessageTextField.GetComponent<Text>().text = "";
            }
        }
    }
} 