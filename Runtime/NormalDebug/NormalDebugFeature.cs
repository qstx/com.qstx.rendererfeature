using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class NormalDebugFeature:ScriptableRendererFeature
{
    public enum NormalSpace:int
    {
        Off,
        WS,
        OS,
    }

    [Serializable]
    public class NormalDebugSettings
    {
        [Range(0.0f, 2.0f)]
        public float normalScale = 1.0f;

        public NormalSpace mode = NormalSpace.Off;
    }
    public class NormalDebugPass : ScriptableRenderPass
    {
        private readonly float _normalScale;
        private readonly NormalSpace _mode;
        private readonly ShaderTagId _passTag = new ShaderTagId("NormalDebug");
        private FilteringSettings _filteringSettings = new FilteringSettings();

        public NormalDebugPass(NormalDebugSettings settings)
        {
            _normalScale = settings.normalScale;
            _mode = settings.mode;
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
                cmd.SetGlobalFloat("_NormalScale", _normalScale);
                var drawingSettings = CreateDrawingSettings(_passTag, ref renderingData, SortingCriteria.CommonOpaque);
                foreach (string s in Enum.GetNames(typeof(NormalSpace)))
                {
                    cmd.DisableShaderKeyword(($"_NormalSpace_{s}").ToUpper());
                    Debug.Log(($"Close _NormalSpace_{s}").ToUpper());
                }
                cmd.EnableShaderKeyword(($"_NormalSpace_{_mode}").ToUpper());
                //cmd.EnableKeyword(new GlobalKeyword($"_NormalSpace_OS"));
                Debug.Log(($"_NormalSpace_{_mode}").ToUpper());
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
        _pass = new NormalDebugPass(settings);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if(settings.mode!=NormalSpace.Off)
            renderer.EnqueuePass(_pass);
    }
}