using Assets.Scripts.DataObjects;
//using Assets.Scripts.DataProviders;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using System.IO;
using Assets.Scripts.ServiceProviders.FamilyHistoryPictureProvider;
using Assets.Scripts.ServiceProviders;
using Assets.Scripts.Enums;
using Assets.Scripts.Utilities;
using System;

public class HallOfFamilyPhotos : MonoBehaviour
{
    public PersonNode focusPerson;
    public PersonNode previousFocusPerson;
    public GameObject familyPhotoPanelPrefab;
    private IDictionary<int, GameObject> familyPhotoPanelDictionary = new Dictionary<int, GameObject>();
    private GameObject undatedPhotosPanel;
    private bool leaveMeAloneIAmBusy = false;
    private DigiKamFamilyHistoryPictureProvider _pictureProvider;

    // Start is called before the first frame update
    void Start()
    {
        leaveMeAloneIAmBusy = false;
        _pictureProvider = new DigiKamFamilyHistoryPictureProvider();
        if (PlayerPrefs.HasKey(PlayerPrefsConstants.LAST_USED_ROOTS_MAGIC_DATA_FILE_PATH) && 
            PlayerPrefs.HasKey(PlayerPrefsConstants.LAST_USED_DIGIKAM_DATA_FILE_PATH))
        {
            _pictureProvider.Initialize(new Dictionary<string, string>
            {
                { PlayerPrefsConstants.LAST_USED_ROOTS_MAGIC_DATA_FILE_PATH, PlayerPrefs.GetString(PlayerPrefsConstants.LAST_USED_ROOTS_MAGIC_DATA_FILE_PATH) },
                { PlayerPrefsConstants.LAST_USED_DIGIKAM_DATA_FILE_PATH, PlayerPrefs.GetString(PlayerPrefsConstants.LAST_USED_DIGIKAM_DATA_FILE_PATH) }
            });
        }
    }

    public void UpdatePhotoInfoBackToDigiKam(int panelYearWithRequest, PhotoInfo modifiedPhotoInfo)
    {
        // WRITE IT BACK: Update the photo info in the DigiKamConnector
        _pictureProvider.UpdatePhotoInfo(modifiedPhotoInfo);

        // READ WHAT CHANGED:
        // Depending on what changed, some Photo panels should be refreshed
        // To keep it simple, we will update the panelYearWithRequest and the UnDated Panel
        // Then if the createdDate in the modifiedPhotoInfo has a different year than panelYearWithRequest
        // Then we will refresh that one as well
        if (panelYearWithRequest != -1)
        {
            var panel = familyPhotoPanelDictionary[panelYearWithRequest - focusPerson.birthDate];
            var panelScript = panel.GetComponent<FamilyPhotoHallPanel>();
            panelScript.ClearFamilyPhotos();
            var photosForYear = _pictureProvider.GetPhotoInfoListForPerson(focusPerson.dataBaseOwnerID, panelYearWithRequest);
            panelScript.LoadFamilyPhotosForYearAndPerson(focusPerson.dataBaseOwnerID, panelYearWithRequest, photosForYear);
        }
        if (modifiedPhotoInfo.CreationDate.HasValue && !modifiedPhotoInfo.IsNotDated)
        {
            var modifiedPhotoYear = modifiedPhotoInfo.CreationDate.Value.Year;
            if (modifiedPhotoYear != panelYearWithRequest)
            {
                var panel = familyPhotoPanelDictionary[modifiedPhotoYear - focusPerson.birthDate];
                var panelScript = panel.GetComponent<FamilyPhotoHallPanel>();
                panelScript.ClearFamilyPhotos();
                var photosForYear = _pictureProvider.GetPhotoInfoListForPerson(focusPerson.dataBaseOwnerID, modifiedPhotoYear);
                panelScript.LoadFamilyPhotosForYearAndPerson(focusPerson.dataBaseOwnerID, modifiedPhotoYear, photosForYear);
            }
        }
        // Refresh the undated photos panel
        var undatedPanelScript = undatedPhotosPanel.GetComponent<FamilyPhotoHallPanel>();
        undatedPanelScript.ClearFamilyPhotos();
        var undatedPhotos = _pictureProvider.GetPhotoInfoListForPerson(focusPerson.dataBaseOwnerID, -1); // -1 for undated photos
        undatedPanelScript.LoadFamilyPhotosForYearAndPerson(focusPerson.dataBaseOwnerID, -1, undatedPhotos);
    }

    private void UpdateUndatedPhotosPanel(PersonNode newfocusPerson, float x, float y, float z)
    {
        Vector3 undatedPosition = new Vector3(x, y, z); // Position in front of the person platform
        Quaternion undatedRotation = Quaternion.Euler(90, 0, 180); // Face the player

        if (undatedPhotosPanel == null)
        {
            undatedPhotosPanel = Instantiate(familyPhotoPanelPrefab, undatedPosition, undatedRotation);
            undatedPhotosPanel.transform.parent = transform;
            undatedPhotosPanel.name = "UndatedPhotosPanel";
        }
        else
        {
            undatedPhotosPanel.SetActive(true);
            undatedPhotosPanel.transform.SetPositionAndRotation(undatedPosition, undatedRotation);
        }

        var undatedPanelScript = undatedPhotosPanel.GetComponent<FamilyPhotoHallPanel>();
        undatedPanelScript.ClearFamilyPhotos();
        var undatedPhotos = _pictureProvider.GetPhotoInfoListForPerson(newfocusPerson.dataBaseOwnerID, -1); // -1 for undated photos
        undatedPanelScript.LoadFamilyPhotosForYearAndPerson(newfocusPerson.dataBaseOwnerID, -1, undatedPhotos);
        undatedPanelScript.SetPanelTitle("No Dates");
    }

    public IEnumerator SetFocusPersonNode(PersonNode newfocusPerson)
    {
        if (!leaveMeAloneIAmBusy && (previousFocusPerson == null || newfocusPerson.dataBaseOwnerID != previousFocusPerson.dataBaseOwnerID))
        {
            leaveMeAloneIAmBusy = true;
            // Establish physical parent-child relationship so hall of family photos moves with PersonNode
            this.transform.SetParent(newfocusPerson.transform, false);
            this.transform.localPosition = new Vector3(0, 5f, 0); // Above the person
            this.transform.localRotation = Quaternion.identity;

            // Hide all existing panels
            foreach (var panel in familyPhotoPanelDictionary.Values)
            {
                panel.SetActive(false);
            }
            if (undatedPhotosPanel != null)
            {
                undatedPhotosPanel.SetActive(false);
            }

            previousFocusPerson = newfocusPerson;
            focusPerson = newfocusPerson;

            var birthDate = focusPerson.birthDate;
            var lifeSpan = focusPerson.lifeSpan;
            var x = focusPerson.transform.position.x;
            var y = focusPerson.transform.position.y;

            // Update undated photos panel
            UpdateUndatedPhotosPanel(focusPerson, x, y + 2f, (birthDate) * 5 + 5f);

            // If the person has no life span, set the loop to at least add 1 photo panel
            // Use <= to include the current year (lifeSpan represents completed years, but we want to include the current year)
            for (int age = 0; age <= Math.Max(0, lifeSpan); age++)
            {
                int year = birthDate + age;
                Vector3 position = new Vector3(x - 5.5f, y + 2f, (year) * 5 + 2.5f);
                Quaternion rotation = Quaternion.Euler(90, 0, -90);

                if (familyPhotoPanelDictionary.ContainsKey(age))
                {
                    // Reuse existing panel
                    var panel = familyPhotoPanelDictionary[age];
                    panel.SetActive(true);
                    panel.transform.SetPositionAndRotation(position, rotation);
                    var panelScript = panel.GetComponent<FamilyPhotoHallPanel>();
                    panelScript.ClearFamilyPhotos();
                    // Get the photos for this yeay (birthDate + age)
                    var photosForYear = _pictureProvider.GetPhotoInfoListForPerson(newfocusPerson.dataBaseOwnerID, year);
                    panelScript.LoadFamilyPhotosForYearAndPerson(newfocusPerson.dataBaseOwnerID, year, photosForYear);
                }
                else
                {
                    // Create new panel
                    GameObject newPanel = Instantiate(familyPhotoPanelPrefab, position, rotation);
                    newPanel.transform.parent = transform;
                    newPanel.name = $"FamilyPhotoPanelforAge{age}";
                    var panelScript = newPanel.GetComponent<FamilyPhotoHallPanel>();
                    // Get the photos for this yeay (birthDate + age)
                    var photosForYear = _pictureProvider.GetPhotoInfoListForPerson(newfocusPerson.dataBaseOwnerID, year);
                    panelScript.LoadFamilyPhotosForYearAndPerson(newfocusPerson.dataBaseOwnerID, year, photosForYear);
                    familyPhotoPanelDictionary.Add(age, newPanel);
                }
                yield return null;
            }
        }
        leaveMeAloneIAmBusy = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
