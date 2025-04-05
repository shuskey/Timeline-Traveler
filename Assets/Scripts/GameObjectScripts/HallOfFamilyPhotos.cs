//using Assets.Scripts.DataObjects;
//using Assets.Scripts.DataProviders;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using System.IO;

public class HallOfFamilyPhotos : MonoBehaviour
{
    public PersonNode focusPerson;
    public PersonNode previousFocusPerson;
    public GameObject familyPhotoPanelPrefab;
    public string photoArchiveDrivePath = "C:\\Users\\Scott\\OneDrive\\Pictures\\Camera Roll\\2018\\02";
    public string thumbnailSubFolderName = "Thumb200";

    private IDictionary<int, GameObject> familyPhotoPanelDictionary = new Dictionary<int, GameObject>();
    private bool leaveMeAloneIAmBusy = false;

    // Start is called before the first frame update
    void Start()
    {
        leaveMeAloneIAmBusy = false;
    }

    public IEnumerator SetFocusPersonNode(PersonNode newfocusPerson)
    {
        if (!leaveMeAloneIAmBusy && (previousFocusPerson == null || newfocusPerson.dataBaseOwnerID != previousFocusPerson.dataBaseOwnerID))
        {
            leaveMeAloneIAmBusy = true;
            foreach (var panelsToDisable in familyPhotoPanelDictionary)
            {
                panelsToDisable.Value.SetActive(false);
            }

            previousFocusPerson = newfocusPerson;
            focusPerson = newfocusPerson;

            var birthDate = focusPerson.birthDate;
            var lifeSpan = focusPerson.lifeSpan;
            var x = focusPerson.transform.position.x;
            var y = focusPerson.transform.position.y;

            for (int age = 0; age < lifeSpan; age++)
            {
                int year = birthDate + age;

                if (familyPhotoPanelDictionary.ContainsKey(year))
                {
                    familyPhotoPanelDictionary[year].SetActive(true);
                    familyPhotoPanelDictionary[year].transform.SetPositionAndRotation(new Vector3(x - 5.5f, y + 2f, (year) * 5 + 2.5f), Quaternion.Euler(90, 0, -90));
                }
                else
                {
                    GameObject newPanel = Instantiate(familyPhotoPanelPrefab, new Vector3(x - 5.5f, y + 2f, (year) * 5 + 2.5f), Quaternion.Euler(90, 0, -90));

                    newPanel.transform.parent = transform;
                    newPanel.name = $"FamilyPhotoPanelfor{year}";

                    var familyPhotoHallPanelScript = newPanel.GetComponent<FamilyPhotoHallPanel>();
                    familyPhotoHallPanelScript.LoadFamilyPhotosForYearAndPerson(newfocusPerson.dataBaseOwnerID, year, photoArchiveDrivePath, thumbnailSubFolderName);

                    familyPhotoPanelDictionary.Add(year, newPanel);
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
