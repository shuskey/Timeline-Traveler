using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using SimpleFileBrowser;
using UnityEngine.UI;
using Assets.Scripts.ServiceProviders.FamilyHistoryPictureProvider;
using Assets.Scripts.ServiceProviders;
using Assets.Scripts;

public class DigiKamFileBrowserHandler : MonoBehaviour
{
	public Text fileSelectedText;
	public Text searchStatusText;
	public string initialPath = null;
	public string initialFilename = null;
    public GameObject imageTagDatabasePickerGameObject;

    private IFamilyHistoryPictureProvider _pictureProvider;
	// Warning: paths returned by FileBrowser dialogs do not contain a trailing '\' character
	// Warning: FileBrowser can only show 1 dialog at a time

	static string DigiKam_Base_DataBaseFileName = "digikam4.db";

	void Start()
	{
		// Set filters (optional)
		// It is sufficient to set the filters just once (instead of each time before showing the file browser dialog), 
		// if all the dialogs will be using the same filters
		var filenameWithOutExtension = Path.GetFileNameWithoutExtension(DigiKam_Base_DataBaseFileName);
		var extension = Path.GetExtension(DigiKam_Base_DataBaseFileName);
		FileBrowser.SetFilters(true, new FileBrowser.Filter(filenameWithOutExtension, extension));

		// Set default filter that is selected when the dialog is shown (optional)
		// Returns true if the default filter is set successfully
		// In this case, set Images filter as the default filter
		FileBrowser.SetDefaultFilter(".db");

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
		if (PlayerPrefs.HasKey(PlayerPrefsConstants.LAST_USED_DIGIKAM_DATA_FILE_PATH)) {
			Assets.Scripts.CrossSceneInformation.digiKamDataFileNameWithFullPath = PlayerPrefs.GetString(PlayerPrefsConstants.LAST_USED_DIGIKAM_DATA_FILE_PATH);
			initialFilename = Path.GetFileName(Assets.Scripts.CrossSceneInformation.digiKamDataFileNameWithFullPath);
			initialPath = Path.GetDirectoryName(Assets.Scripts.CrossSceneInformation.digiKamDataFileNameWithFullPath);
			fileSelectedText.text = initialFilename;
			imageTagDatabasePickerGameObject.GetComponent<ImageTagDatabasePickerHandler>().FileSelectedNowEnableUserInterface(true);

			searchStatusText.text = "DigiKam database location previously identified.  Press Start.";
		}
		else {
			searchStatusText.text = "Please identify the DigiKam database location.";
		}

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
		yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.FilesAndFolders,
                                             allowMultiSelection: false,
                                             initialPath: initialPath,
                                             initialFilename: initialFilename,
                                             $"Open {DigiKam_Base_DataBaseFileName} File",
                                             "Open");

		// Dialog is closed
		// Print whether the user has selected some files/folders or cancelled the operation (FileBrowser.Success)

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

				// Initialize the picture provider with the database path
				_pictureProvider = new DigiKamFamilyHistoryPictureProvider();
				var configuration = new Dictionary<string, string>
				{
					{ PlayerPrefsConstants.LAST_USED_DIGIKAM_DATA_FILE_PATH, result },
					{ PlayerPrefsConstants.LAST_USED_ROOTS_MAGIC_DATA_FILE_PATH, CrossSceneInformation.rootsMagicDataFileNameWithFullPath }
				};
				_pictureProvider.Initialize(configuration);

				// Verify that the database file is present
				if (!_pictureProvider.AreAllDatabaseFilesPresent())
                    throw new FileNotFoundException("Database file is missing.");	

				// Update the global path
				Assets.Scripts.CrossSceneInformation.digiKamDataFileNameWithFullPath = result;

				// Update UI
				fileSelectedText.text = Path.GetFileName(result);
                searchStatusText.text = "DigiKam database location identified.  Press Start.";

                imageTagDatabasePickerGameObject.GetComponent<ImageTagDatabasePickerHandler>().FileSelectedNowEnableUserInterface(true);

                // Save the path for next time
                PlayerPrefs.SetString(PlayerPrefsConstants.LAST_USED_DIGIKAM_DATA_FILE_PATH, result);
                PlayerPrefs.Save();
            }
            catch (System.Exception ex)
            {
				Debug.LogError($"{ex.Message} Exception thrown, database file is not valid.");
				Assets.Scripts.CrossSceneInformation.digiKamDataFileNameWithFullPath = null;
				fileSelectedText.text = "- File Failure -";
				searchStatusText.text = "Please try again.  Identify the DigiKam database location.";
				Debug.LogError("Bad DigiKam Data File Path Chosen: " + result);
			}
		}
	}
}
