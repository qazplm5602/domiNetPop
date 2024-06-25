using UnityEngine;

public class ApplicationController : MonoBehaviour
{
    [SerializeField] private ClientSingleton _clientPrefab;
    [SerializeField] private HostSingleton _hostPrefab;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);

        bool isDedicated = SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null;

        LaunchInMode(isDedicated);
    }

    private async void LaunchInMode(bool isDedicatedServer)
    {
        if (isDedicatedServer)
        {
            //Do something later...
        }
        else
        {
            HostSingleton hostSingleton = Instantiate(_hostPrefab, transform);
            hostSingleton.CreateHost();

            ClientSingleton clientSingleton = Instantiate(_clientPrefab, transform);
            bool authenticated = await clientSingleton.CreateClient();

            if(authenticated)
            {
                //���⿡ ��巹������ ����� ���µ��� �ε��ϴ� �ڵ尡 �����ϰ�
                //��Ÿ��� ���� Ŭ���̾�Ʈ �غ��۾�
                //�ε��� ��� �Ϸ�Ǿ����� �޴������� �̵��Ѵ�.
                ClientSingleton.Instance.GameManager.GotoMenuScene();
            }
            else
            {
                Debug.LogError("UGS Service login failed");
            }

            
        }
    }
}
