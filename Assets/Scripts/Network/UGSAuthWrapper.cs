using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.VisualScripting;
using UnityEngine;

public enum AuthState
{
    NotAuthenticated,
    Authenticating,
    Authenticated,
    Error,
    TimeOut
}

public class UGSAuthWrapper
{
    public static AuthState State { get; private set; } = AuthState.NotAuthenticated;

    
    public static async Task<AuthState> DoAuth(int maxTries = 5)
    {
        if (State == AuthState.Authenticated)
        {
            return State;
        }

        if(State == AuthState.Authenticating)
        {
            Debug.LogWarning("already authenticating");
            await Authenticating();
            return State;
        }

        await SignInAnonymouslyAsync(maxTries);

        return State;
    }

    private static async Task<AuthState> Authenticating()
    {
        while(State == AuthState.Authenticating || State == AuthState.NotAuthenticated)
        {
            await Task.Delay(200);
        }
        return State;
    }

    private static async Task SignInAnonymouslyAsync(int maxTries)
    {
        State = AuthState.Authenticating; //인증작업 시작

        int tries = 0;
        while (State == AuthState.Authenticating && tries < maxTries)
        {
            try
            {
                var instance = AuthenticationService.Instance;
                await instance.SignInAnonymouslyAsync();

                if (instance.IsSignedIn && instance.IsAuthorized)
                {
                    State = AuthState.Authenticated;
                    break;
                }
            }catch (AuthenticationException ex)
            {
                Debug.Log(ex);
                State = AuthState.Error;
            }catch (RequestFailedException ex)
            {
                Debug.Log(ex);
                State = AuthState.Error;
            }

            tries++;

            await Task.Delay(1000); //1초 기다렸다가 다시 시도
        }

        if(State != AuthState.Authenticated)
        {
            Debug.LogWarning($"UGS not signed in successfully after : {tries} tries");
            State = AuthState.TimeOut;
        }
    }

    
}
