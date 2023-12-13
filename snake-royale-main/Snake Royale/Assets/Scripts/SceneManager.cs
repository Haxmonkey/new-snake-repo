
using UnityEngine;

public static class SceneManager
{
    public static void LoadScene(string sceneName)
    {
        Debug.Log("Switching Scene to " + sceneName);
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName); ;
    }
}

public class Scene
{
    public static string AuthScene = "Auth";
    public static string LoadingScene = "Loading";
    public static string MainScene = "Main";
    public static string GameScene = "Game";
}
