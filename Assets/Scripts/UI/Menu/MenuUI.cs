using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuUI : MonoBehaviour
{
    [SerializeField] Button hostBtn;
    [SerializeField] Button listBtn;
    [SerializeField] Button listCloseBtn;
    [SerializeField] TMP_InputField nameField;

    [SerializeField] GameObject menuList;
    [SerializeField] GameObject serverList;

    private void Awake() {
        hostBtn.onClick.AddListener(HandleStartHost);
        listBtn.onClick.AddListener(OpenServerList);
        listCloseBtn.onClick.AddListener(CloseServerList);
    
        nameField.onValueChanged.AddListener(HandleChangedName);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void HandleChangedName(string value) {
        ClientSingleton.Instance.GameManager.SetPlayerName(value);
    }

    void HandleStartHost() {
        HostSingleton.Instance.GameManager.StartHostAsync();
    }

    void OpenServerList() {
        serverList.SetActive(true);
        menuList.SetActive(false);
    }

    void CloseServerList() {
        serverList.SetActive(false);
        menuList.SetActive(true);
    }
}