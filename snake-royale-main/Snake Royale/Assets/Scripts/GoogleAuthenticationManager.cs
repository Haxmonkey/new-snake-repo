using GooglePlayGames;
using GooglePlayGames.BasicApi;
using PlayFab.ClientModels;
using PlayFab;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using Facebook.Unity;

public class GoogleAuthenticationManager : MonoBehaviour
{
    private void Start()
    {
        PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder()
        .AddOauthScope("profile")
        .RequestServerAuthCode(false)
        .Build();
        PlayGamesPlatform.InitializeInstance(config);

        PlayGamesPlatform.DebugLogEnabled = true;

        PlayGamesPlatform.Activate();

        if (PlayerPrefs.GetString("LoggedIn") == "Google")
        {
            LoginWithGoogle();
        }

    }

    public void LoginWithGoogle()
    {
        UtilityCanvas.instance.SetLoadingScreen(true);
        OnSignInButtonClicked();
    }

    private void OnSignInButtonClicked()
    {
        Social.localUser.Authenticate((bool success) => {

            if (success)
            {
                var serverAuthCode = PlayGamesPlatform.Instance.GetServerAuthCode();

                GetPlayerCombinedInfoRequestParams request = new GetPlayerCombinedInfoRequestParams();
                request.GetPlayerProfile = true;
                request.GetUserAccountInfo = true;
                request.GetUserVirtualCurrency = true;
                PlayFabClientAPI.LoginWithGoogleAccount(new LoginWithGoogleAccountRequest()
                {
                    TitleId = PlayFabSettings.TitleId,
                    ServerAuthCode = serverAuthCode,
                    CreateAccount = true,
                    InfoRequestParameters = request
                }, OnGoogleLoginSuccess, OnPlayFabError);
            }
            else
            {

                UtilityCanvas.instance.SetMessage("Google not connected!", MessageType.ERROR);
                UtilityCanvas.instance.SetLoadingScreen(false);
            }

        });

    }
    void OnGoogleLoginSuccess(PlayFab.ClientModels.LoginResult result)
    {
        UserData.playfabID = result.PlayFabId;

        if (result.NewlyCreated || result.InfoResultPayload.PlayerProfile.DisplayName == "" || result.InfoResultPayload.PlayerProfile.DisplayName == null)
        {
            UtilityCanvas.instance.SetMessage("Setting new account!", MessageType.INFO);
            UpdateDisplayName();
        }
        else
        {
            PlayerPrefs.SetString("LoggedIn", "Google");

            UserData.displayName = result.InfoResultPayload.PlayerProfile.DisplayName;
            UserData.avatarURL = result.InfoResultPayload.AccountInfo.TitleInfo.AvatarUrl;
            UserData.totalCoins = int.Parse(result.InfoResultPayload.UserVirtualCurrency["VC"].ToString());
            UserData.IsLoggedIn = true;

            UtilityCanvas.instance.SetMessage("Welcome back!", MessageType.INFO);
            UtilityCanvas.instance.SetLoadingScreen(false);

            SceneManager.LoadScene(Scene.LoadingScene);
        }
    }
    void UpdateDisplayName()
    {
        PlayFabClientAPI.UpdateUserTitleDisplayName(new UpdateUserTitleDisplayNameRequest { DisplayName = Social.localUser.userName }, 
            result => 
            { 
                UserData.displayName = Social.localUser.userName;
                UpdateProfilePicture(); 
            }, 
            error => {
                UtilityCanvas.instance.SetMessage(error.ErrorMessage, MessageType.ERROR);
                UtilityCanvas.instance.SetLoadingScreen(false);
            });
    }
    void UpdateProfilePicture()
    {
        PlayFabClientAPI.UpdateAvatarUrl(new UpdateAvatarUrlRequest { ImageUrl = PlayGamesPlatform.Instance.GetUserImageUrl() }, 
            result => 
            {
                PlayerPrefs.SetString("LoggedIn", "Google");

                UserData.avatarURL = PlayGamesPlatform.Instance.GetUserImageUrl();
                UserData.IsLoggedIn = true;

                UtilityCanvas.instance.SetLoadingScreen(false);
                UtilityCanvas.instance.SetMessage("Welcome!", MessageType.INFO);

                SceneManager.LoadScene(Scene.LoadingScene);
            }, 
            error => {
                UtilityCanvas.instance.SetMessage(error.ErrorMessage, MessageType.ERROR);
                UtilityCanvas.instance.SetLoadingScreen(false);
            });
    }

    private void OnPlayFabError(PlayFabError error)
    {
        UtilityCanvas.instance.SetMessage(error.ErrorMessage, MessageType.ERROR);
        UtilityCanvas.instance.SetLoadingScreen(false);
    }
}
