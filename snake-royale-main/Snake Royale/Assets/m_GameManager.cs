using Mirror;
using PlayFab;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class m_GameManager : MonoBehaviour
{
    public static m_GameManager instance;
    public GameObject gameOverPanel;
    public GameObject gameWonPanel;

    private void Awake()
    {
        instance = this;
    }


    [ServerCallback]
    private void LateUpdate()
    {
        Snake[] players = FindObjectsOfType<Snake>();
        SnakeBot[] bots = FindObjectsOfType<SnakeBot>();


        if (players.Length == 1 && bots.Length == 0)
        {
            foreach (var snake in players)
            {
                if (!snake.isPlayerDead)
                {
                    snake.TargetGameWon();
                    m_NetworkManager.instance.isGameOver = true;
                    return;
                }
            }
        }
        else if (players.Length <= 0)
        {
            Debug.Log("Quitting Server");
            Application.Quit();
        }
    }

    public void LeaveGame()
    {
        if(NetworkClient.isConnected)
        {
            NetworkClient.Disconnect();
        }

        SceneManager.LoadScene(Scene.MainScene);
    }
}
