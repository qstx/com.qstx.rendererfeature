using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using NormalSpace = NormalDebugFeature.NormalSpace;
public class NormalDebugSettingUI : MonoBehaviour
{
    public UniversalRendererData rendererdata;

    public TMP_Dropdown debugModeDropdown;
    public Toggle ColorRemapToggle;
    public Slider normalScaleSlider;

    private NormalDebugFeature _rendererFeature;
    
    // Start is called before the first frame update
    void Start()
    {
        foreach (var feature in rendererdata.rendererFeatures)
        {
            _rendererFeature = feature as NormalDebugFeature;
            if(_rendererFeature)
                break;
        }

        if (!_rendererFeature)
        {
            Debug.LogError("NormalDebugFeature不存在");
            return;
        }
        
        _rendererFeature.settings.normalScale = normalScaleSlider.value;
        _rendererFeature.settings.colorRemap = ColorRemapToggle.isOn;
        _rendererFeature.settings.mode = (NormalSpace)debugModeDropdown.value;
        
        normalScaleSlider.onValueChanged.AddListener(value => { _rendererFeature.settings.normalScale = value;});
        ColorRemapToggle.onValueChanged.AddListener(value => { _rendererFeature.settings.colorRemap = value;});
        debugModeDropdown.onValueChanged.AddListener(value => { _rendererFeature.settings.mode = (NormalSpace)value;});
    }

    // Update is called once per frame
    void Update()
    {
    }
}
