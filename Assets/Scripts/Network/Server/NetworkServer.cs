using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkServer : IDisposable
{
    public NetworkManager _networkManager;

    public Action<string> OnClientLeft; // AuthID

    //Ŭ���̾�Ʈ ���̵�� Auth ���̵� �˾Ƴ��� ��
    private Dictionary<ulong, string> _clientToAuthDictionary = new Dictionary<ulong, string>();
    private Dictionary<string, UserData> _authToUserDictionary = new Dictionary<string, UserData>();

    public NetworkServer(NetworkManager manager)
    {
        _networkManager = manager;
        _networkManager.ConnectionApprovalCallback += HandleApprovalCheck;

        _networkManager.OnServerStarted += HandleServerStart;
    }

    private void HandleServerStart()
    {
        _networkManager.OnClientDisconnectCallback += HandleClientDisconnect;
        _networkManager.SceneManager.OnLoadComplete += HandleSceneLoadComplete;
    }

    //���� ������ 2���� ��ųʸ� ��� �����Ѵ�.
    private void HandleClientDisconnect(ulong clientID)
    {
        if(_clientToAuthDictionary.TryGetValue(clientID, out string authID))
        {
            _clientToAuthDictionary.Remove(clientID);
            _authToUserDictionary.Remove(authID);

            OnClientLeft?.Invoke(authID);
            IngameManager.Instance.PlayerLeft(clientID);
        }
    }

    private void HandleApprovalCheck(
        NetworkManager.ConnectionApprovalRequest request,
        NetworkManager.ConnectionApprovalResponse response)
    {
        string json = Encoding.UTF8.GetString( request.Payload);
        UserData data = JsonUtility.FromJson<UserData>(json);

        _clientToAuthDictionary[request.ClientNetworkId] = data.userAuthID;
        _authToUserDictionary[data.userAuthID] = data;

        response.CreatePlayerObject = false;
        response.Approved = true;
    }

    private void HandleSceneLoadComplete(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
        if (sceneName == SceneNames.GameScene)
            IngameManager.Instance.PlayerAdd(clientId);
    }

    public void Dispose()
    {
        if (_networkManager == null) return;

        _networkManager.ConnectionApprovalCallback -= HandleApprovalCheck;
        _networkManager.OnServerStarted -= HandleServerStart;
        _networkManager.SceneManager.OnLoadComplete -= HandleSceneLoadComplete;
        _networkManager.OnClientDisconnectCallback -= HandleClientDisconnect;
        

        if(_networkManager.IsListening)
        {
            _networkManager.Shutdown();
        }
    }

    public UserData GetUserDataByClientID(ulong clientID) {
        if (_clientToAuthDictionary.TryGetValue(clientID, out string authID)) {
            if (_authToUserDictionary.TryGetValue(authID, out UserData data)) {
                return data;
            }
        }
        
        return null;
    }
}
