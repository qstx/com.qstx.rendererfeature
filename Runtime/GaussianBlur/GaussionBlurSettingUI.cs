using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering.Universal;
using BlurMode = GaussianBlurRendererFeature.BlurMode;
public class GaussionBlurSettingUI : MonoBehaviour
{
    public UniversalRendererData rendererdata;

    public TMP_Dropdown blurModeDropdown;
    public Slider blurRadiusSlider;

    private GaussianBlurRendererFeature _rendererFeature;
    
    // Start is called before the first frame update
    void Start()
    {
        foreach (var feature in rendererdata.rendererFeatures)
        {
            _rendererFeature = feature as GaussianBlurRendererFeature;
            if(_rendererFeature)
                break;
        }

        if (!_rendererFeature)
        {
            Debug.LogError("GaussianBlurRendererFeature不存在");
            return;
        }
        
        _rendererFeature.settings.blurRadius = (int)blurRadiusSlider.value;
        _rendererFeature.settings.blurMode = (BlurMode)blurModeDropdown.value;
        
        blurRadiusSlider.onValueChanged.AddListener(value => { _rendererFeature.settings.blurRadius = (int)value;});
        blurModeDropdown.onValueChanged.AddListener(value => { _rendererFeature.settings.blurMode = (BlurMode)value;});
    }

    // Update is called once per frame
    void Update()
    {
    }
}
