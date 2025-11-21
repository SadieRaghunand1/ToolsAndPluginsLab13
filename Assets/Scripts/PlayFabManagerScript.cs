using PlayFab.ClientModels;
using PlayFab;
using UnityEngine;

public class PlayFabManagerScript
{
    private LoginManagerScript loginManager;
    private string savedEmailKey = "SavedEmail";
    private string userEmail;
    

    private void Start()
    {
        loginManager = new LoginManagerScript();
        //checks if email is saved
        if (PlayerPrefs.HasKey(savedEmailKey))
        {
            string savedEmail = PlayerPrefs.GetString(savedEmailKey);
            // Auto-login with saved email
            //EmailLoginButtonClicked(savedEmail, "SavedPassword");
        }
    }

    //method for triggering email login 
    //public void EmailLoginButtonClicked(string email, string password)
    //{
       //userEmail = email;
       //loginManager.SetLoginMethod(new EmailLogin(email, password));
       //loginManager.Login(OnLoginSuccess, OnLoginFailure); 
    //}

    private void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("Login successful!");
        
        if (!string.IsNullOrEmpty(userEmail))
            PlayerPrefs.SetString(savedEmailKey, userEmail);
        //load player data
        LoadPlayerData(result.PlayFabId);
    }

    private void OnLoginFailure(PlayFabError error)
    {
        Debug.LogError("Login failed:" + error.ErrorMessage);
    }

    private void LoadPlayerData(string playFabId)
    {
        var request = new GetUserDataRequest
        {
            PlayFabId = playFabId
        };
        PlayFabClientAPI.GetUserData(request, OnDataSuccess, OnDataFailure);
    }

    private void OnDataSuccess(GetUserDataResult result)
    {
        Debug.Log("Player data loaded successfully");
    }

    private void OnDataFailure(PlayFabError error)
    {
        Debug.LogError("Failed to load player data: " + error.ErrorMessage);
    }

}
