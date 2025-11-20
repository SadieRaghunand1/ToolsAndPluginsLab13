using PlayFab ClientModels;
using PlayFab;


public interface LoginScript 
{
    void Login(System.Action<LoginResult> onSuccess, System.Action<PlayFabError> onFailure);
}
