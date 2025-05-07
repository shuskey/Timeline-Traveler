using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using SimpleFileBrowser;
using UnityEngine.UI;
using Assets.Scripts.ServiceProviders.FamilyHistoryDataProvider;
using Assets.Scripts.ServiceProviders;

public class RootsMagicFileBrowserHandler : MonoBehaviour
{
	public Text fileSelectedText;
	public string initialPath = null;
	public string initialFilename = null;
	public GameObject personPickerDropdownGameObject;

	private IFamilyHistoryDataProvider _dataProvider;
	// Warning: paths returned by FileBrowser dialogs do not contain a trailing '\' character
	// Warning: FileBrowser can only show 1 dialog at a time

	void Start()
	{
		// Set filters (optional)
		// It is sufficient to set the filters just once (instead of each time before showing the file browser dialog), 
		// if all the dialogs will be using the same filters
		FileBrowser.SetFilters(true, new FileBrowser.Filter("RootsMagic", ".rmgc", ".rmtree"));

		// Set default filter that is selected when the dialog is shown (optional)
		// Returns true if the default filter is set successfully
		// In this case, set Images filter as the default filter
		FileBrowser.SetDefaultFilter(".rmgc");

		// Set excluded file extensions (optional) (by default, .lnk and .tmp extensions are excluded)
		// Note that when you use this function, .lnk and .tmp extensions will no longer be
		// excluded unless you explicitly add them as parameters to the function
		FileBrowser.SetExcludedExtensions(".lnk", ".tmp", ".zip", ".rar", ".exe");

		// Add a new quick link to the browser (optional) (returns true if quick link is added successfully)
		// It is sufficient to add a quick link just once
		// Name: Users
		// Path: C:\Users
		// Icon: default (folder icon)
		FileBrowser.AddQuickLink("Users", "C:\\Users", null);
		if (PlayerPrefs.HasKey(PlayerPrefsConstants.LAST_USED_ROOTS_MAGIC_DATA_FILE_PATH))
		{
			Assets.Scripts.CrossSceneInformation.rootsMagicDataFileNameWithFullPath = PlayerPrefs.GetString(PlayerPrefsConstants.LAST_USED_ROOTS_MAGIC_DATA_FILE_PATH);
			initialFilename = Path.GetFileName(Assets.Scripts.CrossSceneInformation.rootsMagicDataFileNameWithFullPath);
			initialPath = Path.GetDirectoryName(Assets.Scripts.CrossSceneInformation.rootsMagicDataFileNameWithFullPath);
			fileSelectedText.text = initialFilename;
		} else Debug.LogError("There is no RootsMagic save data!");

		transform.GetComponent<Button>().onClick.AddListener(delegate { ShowLoadFileDialog(); });
	}

	void ShowLoadFileDialog()
	{
		StartCoroutine(ShowLoadDialogCoroutine());
	}

	IEnumerator ShowLoadDialogCoroutine()
	{
		// Show a load file dialog and wait for a response from user
		// Load file/folder: both, Allow multiple selection: true
		// Initial path: default (Documents), Initial filename: empty
		// Title: "Load File", Submit button text: "Load"
		yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.FilesAndFolders, allowMultiSelection: false, initialPath: initialPath, initialFilename: initialFilename, "Open RootMagic File", "Open");

		// Dialog is closed
		// Print whether the user has selected some files/folders or cancelled the operation (FileBrowser.Success)
		Debug.Log(FileBrowser.Success);

		if (FileBrowser.Success)
		{
			string result = null;
			for (int i = 0; i < FileBrowser.Result.Length; i++)
			{
				result = FileBrowser.Result[i];
			}
			try
			{
				if (!File.Exists(result))
					throw new FileNotFoundException($"{result} file was not found.");

				// Initialize the data provider with the selected database path
				_dataProvider = new RootsMagicFamilyHistoryDataProvider();
				var configuration = new Dictionary<string, string>
				{
					{ PlayerPrefsConstants.LAST_USED_ROOTS_MAGIC_DATA_FILE_PATH, result }
				};
				_dataProvider.Initialize(configuration);

				// Verify database integrity
				if (_dataProvider.ValidateDatabaseIntegrity())
				{
					string previousPath = Assets.Scripts.CrossSceneInformation.rootsMagicDataFileNameWithFullPath;
					Assets.Scripts.CrossSceneInformation.rootsMagicDataFileNameWithFullPath = result;
					fileSelectedText.text = Path.GetFileName(result);
					
					if (previousPath != result)
					{
						PlayerPrefs.SetString(PlayerPrefsConstants.LAST_USED_ROOTS_MAGIC_DATA_FILE_PATH, Assets.Scripts.CrossSceneInformation.rootsMagicDataFileNameWithFullPath);
						// Remove the old base person selection since it's from a different file
						PlayerPrefs.DeleteKey(PlayerPrefsConstants.LAST_SELECTED_ROOTS_MAGIC_BASE_PERSON_ID);
						PlayerPrefs.DeleteKey(PlayerPrefsConstants.LAST_SELECTED_ROOTS_MAGIC_BASE_PERSON_FULL_NAME);
						PlayerPrefs.Save();
					}
					
					personPickerDropdownGameObject.GetComponent<PersonPickerHandler>().CheckIfFileSelectedAndEnableUserInterface();
				}
				else
				{
					throw new System.Exception("Database integrity check failed");
				}
			}
			catch (System.Exception ex)
			{
				Debug.LogError($"{ex.Message} Exception thrown, database file is not valid.");
				Assets.Scripts.CrossSceneInformation.rootsMagicDataFileNameWithFullPath = null;
				fileSelectedText.text = "> File Failure <  Please try again.";
				Debug.LogError("Bad Data File Path Chosen: " + result);
				personPickerDropdownGameObject.GetComponent<PersonPickerHandler>().CheckIfFileSelectedAndEnableUserInterface();
			}

			
#if NOTNOW

			// Read the bytes of the first file via FileBrowserHelpers
			// Contrary to File.ReadAllBytes, this function works on Android 10+, as well
			byte[] bytes = FileBrowserHelpers.ReadBytesFromFile(FileBrowser.Result[0]);

			// Or, copy the first file to persistentDataPath
			string destinationPath = Path.Combine(Application.persistentDataPath, FileBrowserHelpers.GetFilename(FileBrowser.Result[0]));
			FileBrowserHelpers.CopyFile(FileBrowser.Result[0], destinationPath);
#endif
		}
	}
}
