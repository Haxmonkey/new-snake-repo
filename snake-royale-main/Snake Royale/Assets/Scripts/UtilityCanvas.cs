using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UtilityCanvas : MonoBehaviour
{
    public static UtilityCanvas instance;

    public GameObject loadingPanel;

    public Image messageBox;
    public Transform messageContent;
    public TMP_Text messageText;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }

    public void SetLoadingScreen(bool active)
    {
        loadingPanel.SetActive(active);
    }

    public void SetMessage(string message, MessageType type)
    {
        messageBox.gameObject.SetActive(false);

        if(type == MessageType.ERROR) 
        {
            messageBox.color = new Color(0.5f, 0, 0, 0.85f);
        }
        else if(type == MessageType.INFO) 
        {
            messageBox.color = new Color(0, 0.5f, 0, 0.85f);
        }
        else if(type == MessageType.WARNING) 
        {
            messageBox.color = new Color(1, 0.5f, 0, 0.85f);
        }

        messageText.text = message;
        messageBox.gameObject.SetActive(true);
        LayoutRebuilder.ForceRebuildLayoutImmediate(messageContent.transform as RectTransform);
    }

}

public enum MessageType
{
    ERROR,
    INFO,
    WARNING
}
