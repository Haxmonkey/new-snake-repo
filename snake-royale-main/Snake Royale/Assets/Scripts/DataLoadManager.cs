using PlayFab.ClientModels;
using PlayFab;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.UI;
using TMPro;

public class DataLoadManager : MonoBehaviour
{
    public Slider loadingSlider;
    public TMP_Text loadingText;

    int currentLoading = 0;
    float sliderVelocity;

    public float sliderSmoothness = 0.1f;

    // Start is called before the first frame update
    void Start()
    {
        currentLoading = 100;
        Invoke(nameof(LoadNext), 1f);
    }

    private void Update()
    {
        loadingSlider.value = Mathf.SmoothDamp(loadingSlider.value, currentLoading, ref sliderVelocity, sliderSmoothness);
        loadingText.text = "Loading... " + loadingSlider.value + "%";
    }

    void LoadNext()
    {
        SceneManager.LoadScene(Scene.MainScene);
    }
}
