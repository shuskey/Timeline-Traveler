//using Assets.Scripts.DataObjects;
//using Assets.Scripts.DataProviders;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using System.IO;
using Assets.Scripts.ServiceProviders.FamilyHistoryPictureProvider;
using Assets.Scripts.ServiceProviders;

public class HallOfFamilyPhotos : MonoBehaviour
{
    public PersonNode focusPerson;
    public PersonNode previousFocusPerson;
    public GameObject familyPhotoPanelPrefab;
    private IDictionary<int, GameObject> familyPhotoPanelDictionary = new Dictionary<int, GameObject>();
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

    public IEnumerator SetFocusPersonNode(PersonNode newfocusPerson)
    {
        if (!leaveMeAloneIAmBusy && (previousFocusPerson == null || newfocusPerson.dataBaseOwnerID != previousFocusPerson.dataBaseOwnerID))
        {
            leaveMeAloneIAmBusy = true;
            
            // Hide all existing panels
            foreach (var panel in familyPhotoPanelDictionary.Values)
            {
                panel.SetActive(false);
            }

            previousFocusPerson = newfocusPerson;
            focusPerson = newfocusPerson;

            var birthDate = focusPerson.birthDate;
            var lifeSpan = focusPerson.lifeSpan;
            var x = focusPerson.transform.position.x;
            var y = focusPerson.transform.position.y;

            // Get all photos for this person
            var allPhotos = _pictureProvider.GetPhotoListForPerson(newfocusPerson.dataBaseOwnerID, birthDate);
            var photoCount = allPhotos.Count;

            for (int age = 0; age < lifeSpan; age++)
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
                    // Update the photo if we have any
                    if (photoCount > 0)
                    {
                        var photo = allPhotos[age % photoCount];
                        panelScript.LoadFamilyPhotosForYearAndPerson(newfocusPerson.dataBaseOwnerID, year, photo.FullPathToFileName);
                    }
                }
                else
                {
                    // Create new panel
                    GameObject newPanel = Instantiate(familyPhotoPanelPrefab, position, rotation);
                    newPanel.transform.parent = transform;
                    newPanel.name = $"FamilyPhotoPanelforAge{age}";

                    var panelScript = newPanel.GetComponent<FamilyPhotoHallPanel>();
                    if (photoCount > 0)
                    {
                        var photo = allPhotos[age % photoCount];
                        panelScript.LoadFamilyPhotosForYearAndPerson(newfocusPerson.dataBaseOwnerID, year, photo.FullPathToFileName);
                    }

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
