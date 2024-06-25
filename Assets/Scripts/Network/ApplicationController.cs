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
                //여기에 어드레서블을 비롯한 에셋들을 로딩하는 코드가 들어가야하고
                //기타등등 게임 클라이언트 준비작업
                //로딩이 모두 완료되었으면 메뉴씬으로 이동한다.
                ClientSingleton.Instance.GameManager.GotoMenuScene();
            }
            else
            {
                Debug.LogError("UGS Service login failed");
            }

            
        }
    }
}
