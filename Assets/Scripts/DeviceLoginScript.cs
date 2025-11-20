using PlayFab ClientModels;
using PlayFab;


public class DeviceLoginScript : LoginScript
{
    private string deviceId;
    public DeviceLoginScript(string deviceId)
    {
        this.deviceId = deviceId;
    }

    public void Login(System.Action<LoginResult> onSuccess, System.Action<PlayFabError> onFailure)
    {
        var request = new LoginWithCustomIDRequest
        {
            CustomId = deviceId,
            CreateAccount = true // Create account if it doesn't exists
        };
        PlayFabClientAPI.LoginWithCustomID(request, onSuccess, onFailure);
    }
}
