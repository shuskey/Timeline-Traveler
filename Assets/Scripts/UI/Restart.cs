using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Restart : MonoBehaviour
{
    private void Start()
    {
        transform.GetComponent<Button>().onClick.AddListener(delegate { RestartClicked(); });
        transform.GetComponent<Button>().onClick.AddListener(delegate { RestartClicked(); });


    }
    void RestartClicked()
    {
        SceneManager.LoadScene("aaStart RootsMagicNamePicker");
    }

}
