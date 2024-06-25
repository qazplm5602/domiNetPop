using UnityEngine;

public class HostSingleton : MonoBehaviour
{
    private static HostSingleton instance;
    public static HostSingleton Instance
    {
        get
        {
            if (instance != null) return instance;
            instance = FindObjectOfType<HostSingleton>();

            if (instance == null)
            {
                Debug.LogError("No host singleton");
            }
            return instance;
        }
    }

    public HostGameManager GameManager { get; set; }

    public void CreateHost()
    {
        GameManager = new HostGameManager();
    }

    private void OnDestroy()
    {
        GameManager?.Dispose();
    }
}
