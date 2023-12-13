using Facebook.Unity;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using LoginResult = PlayFab.ClientModels.LoginResult;

public class FacebookAuthenticationManager : MonoBehaviour
{
    private void Start()
    {
        if(PlayerPrefs.GetString("LoggedIn") == "Facebook")
        {
            LoginWithFacebook();
        }


    }
    public void LoginWithFacebook()
    {
        UtilityCanvas.instance.SetLoadingScreen(true);

        FB.Init(OnFacebookInitialized);
    }

    private void OnFacebookInitialized()
    {
        if (FB.IsLoggedIn)
            FB.LogOut();

        var perms = new List<string>() { "public_profile" };
        FB.LogInWithReadPermissions(perms, OnFacebookLoggedIn);
    }

    private void OnFacebookLoggedIn(ILoginResult result)
    {
        if (result == null || string.IsNullOrEmpty(result.Error))
        {
            UtilityCanvas.instance.SetMessage("Facebook connected!", MessageType.INFO);

            GetPlayerCombinedInfoRequestParams request = new GetPlayerCombinedInfoRequestParams();
            request.GetPlayerProfile = true;
            request.GetUserAccountInfo = true;
            request.GetUserVirtualCurrency = true;
            PlayFabClientAPI.LoginWithFacebook(new LoginWithFacebookRequest { CreateAccount = true, AccessToken = AccessToken.CurrentAccessToken.TokenString, InfoRequestParameters = request }
                , OnPlayfabFacebookAuthComplete, OnPlayfabFacebookAuthFailed);
        }
        else
        {
            UtilityCanvas.instance.SetMessage("Facebook not connected!", MessageType.ERROR);
            UtilityCanvas.instance.SetLoadingScreen(false);

            Debug.Log("Facebook Auth Failed: " + result.Error + "\n" + result.RawResult);
        }
    }

    private void OnPlayfabFacebookAuthComplete(LoginResult result)
    {
        UserData.playfabID = result.PlayFabId;


        if (result.NewlyCreated || result.InfoResultPayload.PlayerProfile.DisplayName == "" || result.InfoResultPayload.PlayerProfile.DisplayName == null)
        {
            UtilityCanvas.instance.SetMessage("Setting new account!", MessageType.INFO);
            FB.API("/me?fields=first_name", HttpMethod.GET, UpdateDisplayName);
        }
        else
        {
            PlayerPrefs.SetString("LoggedIn", "Facebook");

            UserData.displayName = result.InfoResultPayload.PlayerProfile.DisplayName;
            UserData.avatarURL = result.InfoResultPayload.AccountInfo.TitleInfo.AvatarUrl;
            UserData.totalCoins = int.Parse(result.InfoResultPayload.UserVirtualCurrency["VC"].ToString());
            UserData.IsLoggedIn = true;

            UtilityCanvas.instance.SetMessage("Welcome back!", MessageType.INFO);
            UtilityCanvas.instance.SetLoadingScreen(false);

            SceneManager.LoadScene(Scene.LoadingScene);

        }
    }
    void UpdateDisplayName(IResult result)
    {
        if (result.Error == null)
        {
            string name = result.ResultDictionary["first_name"].ToString();
            PlayFabClientAPI.UpdateUserTitleDisplayName(new UpdateUserTitleDisplayNameRequest { DisplayName = name }, 
                result => 
                { 
                    UserData.displayName = name;
                    UpdateProfilePicture(); 
                },
                error => 
                {
                    UtilityCanvas.instance.SetMessage(error.ErrorMessage, MessageType.ERROR);
                    UtilityCanvas.instance.SetLoadingScreen(false);
                });
        }
        else
        {
            UtilityCanvas.instance.SetMessage(result.Error, MessageType.ERROR);
            UtilityCanvas.instance.SetLoadingScreen(false);

        }
    }

    void UpdateProfilePicture()
    {
        string avatarURL = "https://graph.facebook.com/" + AccessToken.CurrentAccessToken.UserId + "/picture?type=large";

        PlayFabClientAPI.UpdateAvatarUrl(new UpdateAvatarUrlRequest { ImageUrl = avatarURL }, 
            result => 
            {
                PlayerPrefs.SetString("LoggedIn", "Facebook");

                UserData.avatarURL = avatarURL;
                UserData.IsLoggedIn = true;
                UtilityCanvas.instance.SetLoadingScreen(false);
                UtilityCanvas.instance.SetMessage("Welcome!", MessageType.INFO);

                SceneManager.LoadScene(Scene.LoadingScene);

            }, 
            error => 
            {
                UtilityCanvas.instance.SetMessage(error.ErrorMessage, MessageType.ERROR);
                UtilityCanvas.instance.SetLoadingScreen(false);
            });
    }


    private void OnPlayfabFacebookAuthFailed(PlayFabError error)
    {
        Debug.Log("PlayFab Facebook Auth Failed: " + error.GenerateErrorReport());

        UtilityCanvas.instance.SetMessage("An error occured!", MessageType.ERROR);
        UtilityCanvas.instance.SetLoadingScreen(false);
    }
}