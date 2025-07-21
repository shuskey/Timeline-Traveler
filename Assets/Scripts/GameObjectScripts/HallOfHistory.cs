using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.DataProviders;
using Assets.Scripts.ServiceProviders;
using System;
using Assets.Scripts.ContentProviders;
// 
// This class is used to display the hall of history for a person.
public class HallOfHistory : MonoBehaviour
{
    public PersonNode focusPerson;
    public PersonNode previousFocusPerson;
    public GameObject topEventHallPanelPrefab;

    private IDictionary<int, GameObject> eventPanelDictionary = new Dictionary<int, GameObject>();
    private bool leaveMeAloneIAmBusy = false;
    private ListOfTopEventsFromDataBase _eventsDataProvider;
    private FamilyHappeningsContent _familyHappeningsContent;

    // Start is called before the first frame update
    void Start()
    {
        leaveMeAloneIAmBusy = false;
        // Initialize the events data provider
        var dataPath = Application.streamingAssetsPath + "/";
        _eventsDataProvider = new ListOfTopEventsFromDataBase(dataPath + "TopEvents.db");
        // Initialize the family happenings content provider
        _familyHappeningsContent = new FamilyHappeningsContent();
        _familyHappeningsContent.Initialize();
    }
    
    private bool _dagConnected = false;
    
    // Update is called once per frame
    void Update()
    {
        // Connect DAG on first update to ensure Tribe has time to build it
        if (!_dagConnected)
        {
            ConnectDAGToFamilyHappenings();
            _dagConnected = true;
        }
    }
    
    /// <summary>
    /// Connect the DAG from Tribe to FamilyHappeningsContent for efficient relationship queries
    /// </summary>
    private void ConnectDAGToFamilyHappenings()
    {
        var tribe = FindFirstObjectByType<Tribe>();
        if (tribe != null)
        {
            var familyDAG = tribe.GetFamilyDAG();
            if (familyDAG != null)
            {
                _familyHappeningsContent.SetFamilyDAG(familyDAG);
                Debug.Log("[HallOfHistory] Connected DAG to FamilyHappeningsContent for efficient relationship queries");
            }
            else
            {
                Debug.LogWarning("[HallOfHistory] Tribe found but DAG is null - will retry on next update");
                _dagConnected = false; // Retry on next update
            }
        }
        else
        {
            Debug.LogWarning("[HallOfHistory] Tribe not found - will retry on next update");
            _dagConnected = false; // Retry on next update
        }
    }

    public IEnumerator SetFocusPersonNode(PersonNode newfocusPerson)
    {       
        if (!leaveMeAloneIAmBusy && (previousFocusPerson == null || (newfocusPerson.dataBaseOwnerID != previousFocusPerson.dataBaseOwnerID)))
        {
            leaveMeAloneIAmBusy = true;
            
            // Wait a frame to ensure the new focus person's transform is stable
            yield return null;
            
            // Establish physical parent-child relationship so hall of history moves with PersonNode
            this.transform.SetParent(newfocusPerson.transform, false);
            
            // Wait another frame to ensure the parent-child relationship is established
            yield return null;
            
            // Set the local position relative to the new parent
            this.transform.localPosition = new Vector3(0, 5f, 0); // Above the person
            this.transform.localRotation = Quaternion.identity;

            // Ensure proper positioning by forcing a position update
            yield return StartCoroutine(EnsureProperPositioning(newfocusPerson));

            // Hide all existing panels
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

            // Use <= to include the current year (lifeSpan represents completed years, but we want to include the current year)
            for (int age = 0; age <= Math.Max(0, lifeSpan); age++)
            {
                int year = birthDate + age;
                Vector3 position = new Vector3(x + 5.5f, y + 2f, (year) * 5 + 2.5f);
                Quaternion rotation = Quaternion.Euler(90, -180, -90);

                if (eventPanelDictionary.ContainsKey(year))
                {
                    // Reuse existing panel
                    var panel = eventPanelDictionary[year];
                    panel.SetActive(true);
                    panel.transform.SetPositionAndRotation(position, rotation);
                    var panelScript = panel.GetComponent<TopEventHallPanel>();
                    // Load the events for this year
                    panelScript.LoadTopEventsForYear_fromDataBase(year);
                    // Set the focus person and family happenings content
                    panelScript.SetFocusPerson(focusPerson);
                    panelScript.SetFamilyHappeningsContent(_familyHappeningsContent);
                }
                else
                {
                    // Create new panel
                    GameObject newPanel = Instantiate(topEventHallPanelPrefab, position, rotation);
                    newPanel.transform.parent = transform;
                    newPanel.name = $"HistoryPanelfor{year}";

                    var topEventHallPanelScript = newPanel.GetComponent<TopEventHallPanel>();
                    topEventHallPanelScript.LoadTopEventsForYear_fromDataBase(year);
                    // Set the focus person and family happenings content
                    topEventHallPanelScript.SetFocusPerson(focusPerson);
                    topEventHallPanelScript.SetFamilyHappeningsContent(_familyHappeningsContent);

                    eventPanelDictionary.Add(year, newPanel);
                }
                yield return null;
            } 
        }
        leaveMeAloneIAmBusy = false;
    }

    private IEnumerator EnsureProperPositioning(PersonNode focusPerson)
    {
        // Wait a few frames to ensure the parent transform is completely stable
        for (int i = 0; i < 3; i++)
        {
            yield return null;
        }
        
        // Force the position to be exactly where we want it
        this.transform.localPosition = new Vector3(0, 5f, 0);
        this.transform.localRotation = Quaternion.identity;
        
        // Log the positioning for debugging
        Debug.Log($"[HallOfHistory] Positioned at local position: {this.transform.localPosition}, world position: {this.transform.position}");
    }
}
