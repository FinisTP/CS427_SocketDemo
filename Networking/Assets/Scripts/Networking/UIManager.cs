using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;
    public GameObject startMenu;
    public TMP_InputField usernameField;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else Destroy(this);
    }

    public void ConnectToServer()
    {
        startMenu.SetActive(false);
        usernameField.interactable = true;
        Client.instance.ConnectToServer();
    }
}
