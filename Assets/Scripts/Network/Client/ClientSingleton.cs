using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ClientSingleton : MonoBehaviour
{
    private static ClientSingleton instance;
    public static ClientSingleton Instance
    {
        get
        {
            if (instance != null) return instance;

            instance = FindObjectOfType<ClientSingleton>();

            if(instance == null)
            {
                Debug.LogError("No Client Singleton");
            }

            return instance;
        }
    }

    public ClientGameManager GameManager { get; set; }

    public async Task<bool> CreateClient()
    {
        GameManager = new ClientGameManager();
        return await GameManager.InitAsync();
    }

    private void OnDestroy()
    {
        GameManager?.Dispose();
    }
}
