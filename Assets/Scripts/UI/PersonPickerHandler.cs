using Assets.Scripts.DataObjects;
using Assets.Scripts.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Assets.Scripts.ServiceProviders.FamilyHistoryDataProvider;

public class PersonPickerHandler : MonoBehaviour
{
    public Text searchStatusText;
    public InputField lastNameFilterField;
    public Toggle ancestryToggle;
    public Toggle descendancyToggle;
    public Toggle rootPersonToggle;
    public Dropdown generationsDropdown;
    public Button quitButton;
    public Button nextButton;
    public int numberOfPeopleInTribe = 1000;

    private IFamilyHistoryDataProvider _dataProvider;
    private List<Person> _personsList;
    private Person selectedPerson;
    private int selectedPersonId;
    private string selectedPersonFullName;

    // Start is called before the first frame update
    void Start()
    {
        nextButton.transform.Find("LoadingCircle").gameObject.SetActive(false);
        nextButton.interactable = false;

        generationsDropdown.value = 0;

        quitButton.onClick.AddListener(delegate { quitClicked(); });

        nextButton.onClick.AddListener(delegate { NextClicked(); });

        lastNameFilterField.onEndEdit.AddListener(delegate { LastNameFilterFieldEndEdit(lastNameFilterField); });
        lastNameFilterField.onSubmit.AddListener(delegate { LastNameFilterFieldEndEdit(lastNameFilterField); });

        lastNameFilterField.onValueChanged.AddListener(delegate { LastNameFilterFieldChanging(lastNameFilterField); });

        ancestryToggle.onValueChanged.AddListener(delegate { ToggleControl(ancestryToggle); });
        descendancyToggle.onValueChanged.AddListener(delegate { ToggleControl(descendancyToggle); });
        rootPersonToggle.onValueChanged.AddListener(delegate { ToggleControl(rootPersonToggle); });

        transform.GetComponent<Dropdown>().onValueChanged.AddListener(delegate { DropDownItemSelected(transform.GetComponent<Dropdown>()); });
        ResetDropDown();

        // Initialize the data provider
        _dataProvider = new RootsMagicFamilyHistoryDataProvider();
        var config = new Dictionary<string, string>
        {
            { "RootsMagicDatabasePath", Assets.Scripts.CrossSceneInformation.rootsMagicDataFileNameWithFullPath }
        };
        
        // See if we have a previously selected Base PersonId
        if (CheckIfFileSelectedAndEnableUserInterface()) {
            _dataProvider.Initialize(config);
        }
    }

    public bool CheckIfFileSelectedAndEnableUserInterface()
    {
        if (PlayerPrefs.HasKey("LastUsedRootsMagicDataFilePath")) {
            lastNameFilterField.interactable = true;
            transform.GetComponent<Dropdown>().interactable = true;
            if (PlayerPrefs.HasKey("LastSelectedRootsMagicBasePersonId") && PlayerPrefs.HasKey("LastSelectedRootsMagicBasePersonFullName")) {
                selectedPersonId = PlayerPrefs.GetInt("LastSelectedRootsMagicBasePersonId");
                selectedPersonFullName = PlayerPrefs.GetString("LastSelectedRootsMagicBasePersonFullName");
                Debug.Log("Previously Selected Base PersonId identified");
                SetStatusTextEnableNext(selectedPersonId, selectedPersonFullName);
                return true;
            }
            else
                Debug.Log("There is no previously selected Base PerdonId save data!");
        } else {
            lastNameFilterField.interactable = false;
            transform.GetComponent<Dropdown>().interactable = false;

            Debug.Log("There is no RootsMagic DataFile Path save data!");
        }
        return false;
    }

    void quitClicked()
    {
        Application.Quit();
    }

    void NextClicked()
    {
        nextButton.transform.Find("LoadingCircle").gameObject.SetActive(true);
        SaveBasePersonIdToPlayerPrefs(selectedPersonId, selectedPersonFullName);
        Assets.Scripts.CrossSceneInformation.startingDataBaseId = selectedPersonId;
        Assets.Scripts.CrossSceneInformation.numberOfGenerations = Int32.Parse(generationsDropdown.options[generationsDropdown.value].text);
        Assets.Scripts.CrossSceneInformation.myTribeType = ancestryToggle.isOn ? TribeType.Ancestry
            : descendancyToggle.isOn ? TribeType.Descendancy
            : rootPersonToggle.isOn ? TribeType.Centered : TribeType.AllPersons;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    private void SaveBasePersonIdToPlayerPrefs(int basePersonId, string basePersonFullName)
    {
        Debug.Log("Base PersonId Chosen: " + basePersonFullName);
        PlayerPrefs.SetInt("LastSelectedRootsMagicBasePersonId", basePersonId);
        PlayerPrefs.SetString("LastSelectedRootsMagicBasePersonFullName", basePersonFullName);
        PlayerPrefs.Save();
        Debug.Log("Game data saved!");
    }

    void ToggleControl(Toggle toggleThatToggled)
    {
        if (toggleThatToggled.isOn)
        {
            if (toggleThatToggled.name.StartsWith("A"))
            {
                descendancyToggle.isOn = false;
                rootPersonToggle.isOn = false;
            }
            if (toggleThatToggled.name.StartsWith("D"))
            {
                ancestryToggle.isOn = false;
                rootPersonToggle.isOn = false;
            }
            if (toggleThatToggled.name.StartsWith("R"))
            {
                ancestryToggle.isOn = false;
                descendancyToggle.isOn = false;
            }
        }
    }

    void LastNameFilterFieldChanging(InputField input)
    {
        ResetDropDown();
    }

    void ResetDropDown()
    {
        var dropdown = transform.GetComponent<Dropdown>();
        dropdown.options.Clear();
        dropdown.value = 0;
        dropdown.RefreshShownValue();
        dropdown.options.Add(new Dropdown.OptionData() { text = $"Enter Last Name Filter Text, to populate this" });

        searchStatusText.text = "";

        nextButton.interactable = false;
    }

    void LastNameFilterFieldEndEdit(InputField input)
    {
        if (input.text.Length > 0)
        {
            Debug.Log($"Text: {input.text} has been entered");
            PopulateDropDownWithMyTribeSubSet(input.text);
            var dropdown = transform.GetComponent<Dropdown>();

            if (_personsList.Count > 0)
            {
                searchStatusText.text = $"{_personsList.Count} Search Results Available.";                
                dropdown.value = 0;
                DropDownItemSelected(dropdown);
                dropdown.Show();
            }
            else
                searchStatusText.text = $"No results available for that search string.";

        }
        else if (input.text.Length == 0)
        {
            Debug.Log("Main Input Empty");
            searchStatusText.text = $"No results available for that search string.";
        }
    }

    void DropDownItemSelected(Dropdown dropdown)
    {
        var index = dropdown.value;
        selectedPerson = _personsList[index];
        SetStatusTextEnableNext(selectedPerson.dataBaseOwnerId,
            $"{selectedPerson.surName}, {selectedPerson.givenName} b{selectedPerson.birthEventDate} id {selectedPerson.dataBaseOwnerId}");        
    }

    void SetStatusTextEnableNext(int basePersonId, string basePersonFullName)
    {   
        selectedPersonId = basePersonId;
        selectedPersonFullName = basePersonFullName;
        searchStatusText.text = $"Selected: {basePersonFullName}. Press Next, or Choose another.";
        nextButton.interactable = true;
    }

    void PopulateDropDownWithMyTribeSubSet(string filterText)
    {
        var dropdown = transform.GetComponent<Dropdown>();
        dropdown.options.Clear();
        dropdown.value = 0;        
        dropdown.RefreshShownValue();

        _personsList = _dataProvider.GetPersonListByLastName(filterText, numberOfPeopleInTribe);
        foreach (var person in _personsList)
        {
            dropdown.options.Add(new Dropdown.OptionData() { text = $"{person.surName}, {person.givenName} b{person.birthEventDate} id {person.dataBaseOwnerId}" });
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
