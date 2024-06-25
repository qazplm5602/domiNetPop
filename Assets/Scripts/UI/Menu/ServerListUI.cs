using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class ServerListUI : MonoBehaviour
{
    [SerializeField] GameObject roomBox;
    [SerializeField] Transform section;
    [SerializeField] Button refreshBtn;

    bool process = false;
    
    private void Awake() {
        refreshBtn.onClick.AddListener(Refresh);
    }

    private void OnEnable() {
        Refresh();
    }
        
    public async void Refresh() {
        if (process) return;

        // 다 지우기
        for (int i = 0; i < section.childCount; i++)
            Destroy(section.GetChild(i).gameObject);

        process = true;
        
        try {
            QueryLobbiesOptions options = new QueryLobbiesOptions();

            options.Count = 25;

            options.Filters = new List<QueryFilter>()
            {
                new QueryFilter(
                    QueryFilter.FieldOptions.AvailableSlots, 
                    "0",
                    QueryFilter.OpOptions.GT),
                new QueryFilter(
                    field: QueryFilter.FieldOptions.IsLocked,
                    value:"0",
                    op:QueryFilter.OpOptions.EQ)  //���� 0�� �ֵ�  
            };

            QueryResponse lobbies = await Lobbies.Instance.QueryLobbiesAsync(options);
            foreach (var item in lobbies.Results)
            {
                AddRoomBox(item);
            }
            
        } catch (LobbyServiceException ex) {
            Debug.LogError(ex);
        }

        process = false;
    }

    private void AddRoomBox(Lobby item)
    {
        var box = Instantiate(roomBox, section).transform;
        box.Find("Title").GetComponent<TextMeshProUGUI>().text =  item.Name;
        box.Find("Count").GetComponent<TextMeshProUGUI>().text =  $"{item.Players.Count}<size=30><color=#BDBDBD>/{item.MaxPlayers}</color></size>";
        box.Find("Button").GetComponent<Button>().onClick.AddListener(() => JoinRoom(item.Id));
    }

    private async void JoinRoom(string lobbyId) {
        Lobby lobby = await Lobbies.Instance.JoinLobbyByIdAsync(lobbyId);
        ClientSingleton.Instance.GameManager.StartClientWithJoinCode(lobby.Data["JoinCode"].Value);
    }
}
