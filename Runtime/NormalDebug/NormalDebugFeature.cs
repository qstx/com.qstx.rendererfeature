using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class NormalDebugFeature:ScriptableRendererFeature
{
    public enum NormalSpace:int
    {
        Off,
        Corrected,
        Uncorrected,
    }

    [Serializable]
    public class NormalDebugSettings
    {
        [Range(0.0f, 2.0f)]
        public float normalScale = 1.0f;
        public NormalSpace mode = NormalSpace.Off;
        public bool colorRemap = false;
    }
    public class NormalDebugPass : ScriptableRenderPass
    {
        public float normalScale;
        public NormalSpace mode;
        public bool colorRemap;
        private readonly ShaderTagId _passTag = new ShaderTagId("NormalDebug");
        private FilteringSettings _filteringSettings = new FilteringSettings();

        public NormalDebugPass()
        {
            _filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
            renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            CommandBuffer cmd = CommandBufferPool.Get();

            using (new ProfilingScope(cmd, new ProfilingSampler("NormalDebug")))
            {
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
                cmd.SetGlobalFloat("_NormalScale", normalScale);
                var drawingSettings = CreateDrawingSettings(_passTag, ref renderingData, SortingCriteria.CommonOpaque);
                if(mode==NormalSpace.Uncorrected)
                    cmd.EnableShaderKeyword("_NORMAL_UNCORRECTED");
                else
                    cmd.DisableShaderKeyword("_NORMAL_UNCORRECTED");
                if(colorRemap)
                    cmd.EnableShaderKeyword("_COLOR_REMAP");
                else
                    cmd.DisableShaderKeyword("_COLOR_REMAP");
                context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref _filteringSettings);
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }

    private NormalDebugPass _pass;
    public NormalDebugSettings settings;
    public override void Create()
    {
        _pass = new NormalDebugPass();
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if(settings.mode!=NormalSpace.Off)
        {
            _pass.normalScale = settings.normalScale;
            _pass.mode = settings.mode;
            _pass.colorRemap = settings.colorRemap;
            renderer.EnqueuePass(_pass);
        }
    }
}