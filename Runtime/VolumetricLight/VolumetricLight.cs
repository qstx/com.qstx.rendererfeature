
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[System.Serializable]
public class VolumetricLightScatteringSettings
{
    [Header("Properties")]
    [Range(0.1f, 1.0f)]
    public float resolutionScale = 0.5f;

    [Range(0.0f, 1.0f)] public float intensity = 1.0f;
        
    [Range(0.0f, 1.0f)] public float blurWidth = 0.85f;
    
    [Header("Shaders")]
    public Shader radialBlurShader;
    
    public Shader occuludersShader;
}

public class VolumetricLightPass : ScriptableRenderPass
{
    private RTHandle occluder;
    private readonly float resolutionScale;
    private readonly float intensity;
    private readonly float blurWidth;

    private readonly List<ShaderTagId> shaderTagIdList = new List<ShaderTagId>();
    private FilteringSettings filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
    private readonly Material occuludersMaterial;
    private readonly Material radialBlurMaterial;

    public VolumetricLightPass(VolumetricLightScatteringSettings settings)
    {
        resolutionScale = settings.resolutionScale;
        intensity = settings.intensity;
        blurWidth = settings.blurWidth;

        occuludersMaterial = new Material(settings.occuludersShader);
        radialBlurMaterial = new Material(settings.radialBlurShader);
        
        shaderTagIdList.Add( new ShaderTagId( "UniversalForward" )); 
        shaderTagIdList.Add( new ShaderTagId( "UniversalForwardOnly" )); 
        shaderTagIdList.Add( new ShaderTagId( "SRPDefaultUnlit" ));
    }

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        RenderTextureDescriptor cameraTextureDescriptor = renderingData.cameraData.cameraTargetDescriptor;

        cameraTextureDescriptor.depthBufferBits = 0;

        cameraTextureDescriptor.width = Mathf.RoundToInt(cameraTextureDescriptor.width * resolutionScale);
        cameraTextureDescriptor.height = Mathf.RoundToInt(cameraTextureDescriptor.height * resolutionScale);

        RenderingUtils.ReAllocateIfNeeded(ref occluder, cameraTextureDescriptor, FilterMode.Bilinear);
        
        ConfigureTarget(occluder);
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (!occuludersMaterial)
            return;

        CommandBuffer cmd = CommandBufferPool.Get();

        using (new ProfilingScope(cmd, new ProfilingSampler("VolumetricLight")))
        {
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            Camera camera = renderingData.cameraData.camera;
            context.DrawSkybox(camera);

            DrawingSettings drawingSettings =
                CreateDrawingSettings(shaderTagIdList, ref renderingData, SortingCriteria.CommonOpaque);
            drawingSettings.overrideMaterial = occuludersMaterial;
            
            context.DrawRenderers(renderingData.cullResults,ref drawingSettings,ref filteringSettings);

            Vector3 sunDirectionWorldSpace = RenderSettings.sun.transform.forward;
            Vector3 cameraPositionWorldSpace = camera.transform.position;
            Vector3 sunPositionWorldSpace = cameraPositionWorldSpace + sunDirectionWorldSpace;
            Vector3 sunPositionViewportSpace = camera.WorldToViewportPoint(sunPositionWorldSpace);
            
            radialBlurMaterial.SetVector("_Center",sunPositionViewportSpace);
            radialBlurMaterial.SetFloat("_Intensity",intensity);
            radialBlurMaterial.SetFloat("_BlurWidth",blurWidth);
            
            cmd.Blit(occluder,renderingData.cameraData.renderer.cameraColorTargetHandle,radialBlurMaterial);
            //Blit(cmd,occluder,renderingData.cameraData.renderer.cameraColorTargetHandle,radialBlurMaterial);
            //Blitter.BlitTexture2D(cmd,occluder,new Vector4(1.0f,1.0f,0.0f,0.0f),0,true);
        }
        
        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    public override void OnCameraCleanup(CommandBuffer cmd)
    {
        base.OnCameraCleanup(cmd);
    }
}

public class VolumetricLight:ScriptableRendererFeature
{
    public VolumetricLightPass pass;
    public VolumetricLightScatteringSettings settings = new VolumetricLightScatteringSettings();
    
    public override void Create()
    {
        pass = new VolumetricLightPass(settings);
        pass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(pass);
    }
}