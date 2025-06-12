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
using Assets.Scripts.ServiceProviders;

public class PersonPickerHandler : MonoBehaviour
{
    public Text searchStatusText;
    public InputField fullNameFilterField;
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

        fullNameFilterField.onEndEdit.AddListener(delegate { FilterFieldEndEdit(); });
        fullNameFilterField.onSubmit.AddListener(delegate { FilterFieldEndEdit(); });
        fullNameFilterField.onValueChanged.AddListener(delegate { FilterFieldChanging(); });

        ancestryToggle.onValueChanged.AddListener(delegate { ToggleControl(ancestryToggle); });
        descendancyToggle.onValueChanged.AddListener(delegate { ToggleControl(descendancyToggle); });
        rootPersonToggle.onValueChanged.AddListener(delegate { ToggleControl(rootPersonToggle); });

        transform.GetComponent<Dropdown>().onValueChanged.AddListener(delegate { DropDownItemSelected(transform.GetComponent<Dropdown>()); });
        ResetDropDown();

        // Initialize the data provider
        _dataProvider = new RootsMagicFamilyHistoryDataProvider();
        var config = new Dictionary<string, string>
        {
            { PlayerPrefsConstants.LAST_USED_ROOTS_MAGIC_DATA_FILE_PATH, Assets.Scripts.CrossSceneInformation.rootsMagicDataFileNameWithFullPath }
        };

         _dataProvider.Initialize(config);
        
        // Load DigiKam path from config file first, then fall back to PlayerPrefs
        LoadDigiKamPathFromConfigOrPlayerPrefs();
        
        // See if we have a previously selected Base PersonId
        CheckIfFileSelectedAndEnableUserInterface();
    }

    public bool CheckIfFileSelectedAndEnableUserInterface()
    {
        if (PlayerPrefs.HasKey(PlayerPrefsConstants.LAST_USED_ROOTS_MAGIC_DATA_FILE_PATH)) {
            // Reinitialize the data provider with the latest path
            _dataProvider = new RootsMagicFamilyHistoryDataProvider();
            var config = new Dictionary<string, string>
            {
                { PlayerPrefsConstants.LAST_USED_ROOTS_MAGIC_DATA_FILE_PATH, PlayerPrefs.GetString(PlayerPrefsConstants.LAST_USED_ROOTS_MAGIC_DATA_FILE_PATH) }
            };
            _dataProvider.Initialize(config);

            // Load DigiKam path from config file first, then fall back to PlayerPrefs
            LoadDigiKamPathFromConfigOrPlayerPrefs();
            
            fullNameFilterField.interactable = true;
            transform.GetComponent<Dropdown>().interactable = true;
            if (PlayerPrefs.HasKey(PlayerPrefsConstants.LAST_SELECTED_ROOTS_MAGIC_BASE_PERSON_ID) && 
                PlayerPrefs.HasKey(PlayerPrefsConstants.LAST_SELECTED_ROOTS_MAGIC_BASE_PERSON_FULL_NAME)) {
                selectedPersonId = PlayerPrefs.GetInt(PlayerPrefsConstants.LAST_SELECTED_ROOTS_MAGIC_BASE_PERSON_ID);
                selectedPersonFullName = PlayerPrefs.GetString(PlayerPrefsConstants.LAST_SELECTED_ROOTS_MAGIC_BASE_PERSON_FULL_NAME);
                SetStatusTextEnableNext(selectedPersonId, selectedPersonFullName);
                return true;
            }
            else
            {
                ClearStatusTextAndDisableNext();
            }
        } else {
            fullNameFilterField.interactable = false;
            transform.GetComponent<Dropdown>().interactable = false;
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
        PlayerPrefs.SetInt(PlayerPrefsConstants.LAST_SELECTED_ROOTS_MAGIC_BASE_PERSON_ID, basePersonId);
        PlayerPrefs.SetString(PlayerPrefsConstants.LAST_SELECTED_ROOTS_MAGIC_BASE_PERSON_FULL_NAME, basePersonFullName);
        PlayerPrefs.Save();
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

    void FilterFieldChanging()
    {
        ResetDropDown();
    }

    void ResetDropDown()
    {
        var dropdown = transform.GetComponent<Dropdown>();
        dropdown.options.Clear();
        dropdown.value = 0;
        dropdown.RefreshShownValue();
        dropdown.options.Add(new Dropdown.OptionData() { text = $"Enter Name Filter Text, to populate this" });

        searchStatusText.text = "";

        nextButton.interactable = false;
    }

    void FilterFieldEndEdit()
    {
        // Parse the full name filter field for last name and first name parts
        string fullFilterText = fullNameFilterField.text;
        string lastNameFilter = "";
        string firstNameFilter = "";
        
        if (!string.IsNullOrEmpty(fullFilterText))
        {
            if (fullFilterText.Contains(","))
            {
                // Split on comma
                string[] parts = fullFilterText.Split(',');
                lastNameFilter = parts[0].Trim();
                if (parts.Length > 1)
                {
                    firstNameFilter = parts[1].Trim();
                }
            }
            else
            {
                // No comma, treat entire string as last name filter
                lastNameFilter = fullFilterText.Trim();
            }
            
            PopulateDropDownWithMyTribeSubSet(lastNameFilter, firstNameFilter);
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
        else
        {
            Debug.Log("Filter input is empty");
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

    // FUnction to clear the Status Text and disable the Next Button
    void ClearStatusTextAndDisableNext()
    {
        searchStatusText.text = "";
        nextButton.interactable = false;
    }

    void PopulateDropDownWithMyTribeSubSet(string lastNameFilter, string firstNameFilter)
    {
        var dropdown = transform.GetComponent<Dropdown>();
        dropdown.options.Clear();
        dropdown.value = 0;        
        dropdown.RefreshShownValue();

        // First pass: Filter by last name (or get all if no last name filter)
        if (!string.IsNullOrEmpty(lastNameFilter))
        {
            _personsList = _dataProvider.GetPersonListByLastName(lastNameFilter, numberOfPeopleInTribe);
        }
        else
        {
            // If no last name filter, we still need some way to get people. 
            // For now, we'll use an empty string which might return all or none depending on the data provider implementation
            _personsList = _dataProvider.GetPersonListByLastName("", numberOfPeopleInTribe);
        }

        // Second pass: Filter by first name if provided
        if (!string.IsNullOrEmpty(firstNameFilter))
        {
            _personsList = _personsList.Where(person => 
            {
                return person.givenName.Contains(firstNameFilter, StringComparison.OrdinalIgnoreCase);
            }).ToList();
        }

        // Populate dropdown with filtered results
        foreach (var person in _personsList)
        {
            dropdown.options.Add(new Dropdown.OptionData() { text = $"{person.surName}, {person.givenName} b{person.birthEventDate} id {person.dataBaseOwnerId}" });
        }
    }

    /// <summary>
    /// Creates or updates the digikam.config file in the RootsMagic database directory
    /// </summary>
    /// <param name="digiKamRootFolder">The DigiKam root folder path to associate with the current RootsMagic database</param>
    public void CreateOrUpdateDigiKamConfig(string digiKamRootFolder)
    {
        DigiKamConfigManager.CreateOrUpdateDigiKamConfig(
            Assets.Scripts.CrossSceneInformation.rootsMagicDataFileNameWithFullPath, 
            digiKamRootFolder);
    }

    /// <summary>
    /// Convenience method to create or update the config with the DigiKam database directory
    /// </summary>
    public void CreateOrUpdateDigiKamConfigWithCurrentPaths()
    {
        DigiKamConfigManager.CreateOrUpdateDigiKamConfigFromDatabasePath(
            Assets.Scripts.CrossSceneInformation.rootsMagicDataFileNameWithFullPath,
            Assets.Scripts.CrossSceneInformation.digiKamDataFileNameWithFullPath);
    }

    /// <summary>
    /// Reads the DigiKam root folder from the config file for the current RootsMagic database
    /// </summary>
    /// <returns>The DigiKam root folder path if found, null otherwise</returns>
    public string GetDigiKamRootFolderFromConfig()
    {
        return DigiKamConfigManager.GetDigiKamRootFolderFromConfig(
            Assets.Scripts.CrossSceneInformation.rootsMagicDataFileNameWithFullPath);
    }

    /// <summary>
    /// Loads DigiKam path information using config file first, then falling back to PlayerPrefs
    /// </summary>
    private void LoadDigiKamPathFromConfigOrPlayerPrefs()
    {
        string digiKamDatabasePath = DigiKamConfigManager.GetDigiKamDatabasePath(
            Assets.Scripts.CrossSceneInformation.rootsMagicDataFileNameWithFullPath,
            PlayerPrefsConstants.LAST_USED_DIGIKAM_DATA_FILE_PATH);
        
        if (!string.IsNullOrEmpty(digiKamDatabasePath))
        {
            Assets.Scripts.CrossSceneInformation.digiKamDataFileNameWithFullPath = digiKamDatabasePath;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
