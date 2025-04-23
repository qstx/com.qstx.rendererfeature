using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class NormalDebugFeature:ScriptableRendererFeature
{
    public class NormalDebugPass : ScriptableRenderPass
    {
        private ShaderTagId _passTag = new ShaderTagId("NormalDebug");
        private FilteringSettings _filteringSettings = new FilteringSettings();

        public NormalDebugPass()
        {
            _filteringSettings = new FilteringSettings(RenderQueueRange.opaque);
            renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            ConfigureTarget(renderingData.cameraData.renderer.cameraColorTargetHandle);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            Debug.Log("NormalDebug");
            CommandBuffer cmd = CommandBufferPool.Get("Draw Normals");
            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();

            using (new ProfilingScope(cmd, new ProfilingSampler("NormalDebug")))
            {
                var drawingSettings = CreateDrawingSettings(_passTag, ref renderingData, SortingCriteria.CommonOpaque);
                context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref _filteringSettings);
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }

    private NormalDebugPass _pass;
    public override void Create()
    {
        _pass = new NormalDebugPass();
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(_pass);
    }
}