using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class m_ScoreText : MonoBehaviour
{
    TMP_Text text;

    private void Start()
    {
        text = GetComponent<TMP_Text>();
    }

    private void Update()
    {
        if (GameObject.FindGameObjectWithTag("Player") != null && NetworkClient.active)
        {
            text.text = "Score: " + GameObject.FindGameObjectWithTag("Player").GetComponent<Snake>().score;
        }
    }
}
