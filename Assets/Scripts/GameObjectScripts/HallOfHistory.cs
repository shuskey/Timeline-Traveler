using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HallOfHistory : MonoBehaviour
{
    public PersonNode focusPerson;
    public PersonNode previousFocusPerson;
    public GameObject topEventHallPanelPrefab;

    private IDictionary<int, GameObject> eventPanelDictionary = new Dictionary<int, GameObject>();
    private bool leaveMeAloneIAmBusy = false;

    // Start is called before the first frame update
    void Start()
    {
        leaveMeAloneIAmBusy = false;
    }

    public IEnumerator SetFocusPersonNode(PersonNode newfocusPerson)
    {       
        if (!leaveMeAloneIAmBusy && (previousFocusPerson == null || (newfocusPerson.dataBaseOwnerID != previousFocusPerson.dataBaseOwnerID)))
        {
            leaveMeAloneIAmBusy = true;

            foreach (var panelsToDisable in eventPanelDictionary)
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

                if (eventPanelDictionary.ContainsKey(year))
                {
                    eventPanelDictionary[year].SetActive(true);
                    eventPanelDictionary[year].transform.SetPositionAndRotation(new Vector3(x + 5.5f, y + 2f, (year) * 5 + 2.5f), Quaternion.Euler(90, -180, -90));
                }
                else
                {
                    GameObject newPanel = Instantiate(topEventHallPanelPrefab, new Vector3(x + 5.5f, y + 2f, (year) * 5 + 2.5f), Quaternion.Euler(90, -180, -90));

                    newPanel.transform.parent = transform;
                    newPanel.name = $"HistoryPanelfor{year}";

                   // var topEventHallPanelScript = newPanel.GetComponent<TopEventHallPanel>();
                   // topEventHallPanelScript.LoadTopEventsForYear_fromDataBase(year);

                    eventPanelDictionary.Add(year, newPanel);
                    yield return null;
                } 
            } 
        }
        leaveMeAloneIAmBusy = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
