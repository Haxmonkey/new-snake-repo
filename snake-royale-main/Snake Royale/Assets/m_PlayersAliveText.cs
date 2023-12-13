using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class m_PlayersAliveText : MonoBehaviour
{
    TMP_Text text;

    private void Start()
    {
        text = GetComponent<TMP_Text>();  
    }

    private void Update()
    {
        Snake[] snake = FindObjectsOfType<Snake>();
        SnakeBot[] bots = FindObjectsOfType<SnakeBot>();
        int alivePlayers = 0;

      alivePlayers = snake.Length + bots.Length;

        text.text = "Player Alive: " + alivePlayers;
    }
}
