using Assets.Scripts.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ImageTagDatabasePickerHandler : MonoBehaviour
{
    public Text searchStatusText;
    public Button quitButton;
    public Button startButton;

    // Start is called before the first frame update
    void Start()
    {
        startButton.transform.Find("LoadingCircle").gameObject.SetActive(false);
        startButton.interactable = false;
        FileSelectedNowEnableUserInterface(false);

        //if (PlayerPrefs.HasKey("LastUsedDigiKamDataFilePath")) {
        //    Assets.Scripts.CrossSceneInformation.digiKamDataFileNameWithFullPath = PlayerPrefs.GetString("LastUsedDigiKamDataFilePath");
        //    FileSelectedNowEnableUserInterface();
        //    Debug.Log("Game data from DigiKam loaded!");
        //    searchStatusText.text = "Previously selected DigiKam database will be used.";
        //}
        //else
        //    Debug.Log("There is no DigiKam save data!");

        //searchStatusText.text = "Please selected a DigiKam database file to use.";

        quitButton.onClick.AddListener(delegate { quitClicked(); });
        startButton.onClick.AddListener(delegate { NextClicked(); });
    }

    public void FileSelectedNowEnableUserInterface(bool enableFlag = true)
    {
        // Once the database file is selected, enable the user interface
        // This can be used to show the current mapping from RootMagic to DigiKam
        startButton.interactable = true;
    }

    void quitClicked()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
    }

    void NextClicked()
    {
        startButton.transform.Find("LoadingCircle").gameObject.SetActive(true);
         SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
