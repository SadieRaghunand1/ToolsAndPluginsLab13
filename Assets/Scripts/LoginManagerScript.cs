using PlayFab.ClientModels;
using PlayFab;
using UnityEngine;

public class LoginManagerScript 
{
    private LoginScript loginMethod;
    public void SetLoginMethod(LoginScript method)
    {
        loginMethod = method;
    }
    public void Login(System.Action<LoginResult> onSuccess, System.Action<PlayFabError> onFailure)
    {
        if (loginMethod != null)
        {
            loginMethod.Login(onSuccess, onFailure);
        }
        else
        {
            Debug.LogError("No login method set!");
        }
    }
}
