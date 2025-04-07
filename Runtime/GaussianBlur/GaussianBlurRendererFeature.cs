using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GaussianBlurRendererFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public enum BlurMode
    {
        Horizontal,
        Vertical,
        HorizontalAndVertical,
        Full,
    }
    
    [System.Serializable]
    public class Settings
    {
        [Range(0,100)]public int blurRadius = 5;
        public BlurMode blurMode = BlurMode.Full;
    }

    public Settings settings = new Settings();
    public ComputeShader computeShader;

    private GaussianBlurPass blurPass;
    
    public override void Create()
    {
        blurPass = new GaussianBlurPass(computeShader);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (computeShader == null)
        {
            Debug.LogError("Compute Shader is missing!");
            return;
        }

        blurPass.computeShader = computeShader;
        blurPass.blurRadius = settings.blurRadius;
        blurPass.blurMode = settings.blurMode;

        renderer.EnqueuePass(blurPass);
    }

    protected override void Dispose(bool disposing)
    {
        blurPass.Dispose();
    }
}
