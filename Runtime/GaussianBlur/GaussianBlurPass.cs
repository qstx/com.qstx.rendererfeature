using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

using BlurMode = GaussianBlurRendererFeature.BlurMode;
public class GaussianBlurPass : ScriptableRenderPass
{
    public ComputeShader computeShader;
    public int blurRadius = 5;
    public BlurMode blurMode = BlurMode.Full;

    private RenderTextureDescriptor tempDesc;
    private RTHandle tempTarget1;
    private RTHandle tempTarget2;

    private int horizontalKernalIdx = -1;
    private int verticalKernalIdx = -1;
    private int fullKernalIdx = -1;
    
    public GaussianBlurPass(ComputeShader shader)
    {
        computeShader = shader;
        renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        
        horizontalKernalIdx = computeShader.FindKernel("GaussianBlurHorizontal");
        verticalKernalIdx = computeShader.FindKernel("GaussianBlurVertical");
        fullKernalIdx = computeShader.FindKernel("GaussianBlurFull");
    }

    public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
    {
        tempDesc = cameraTextureDescriptor;
        tempDesc.depthStencilFormat = GraphicsFormat.None;
        tempDesc.enableRandomWrite = true;
        
        RenderingUtils.ReAllocateIfNeeded(ref tempTarget1, in tempDesc, name: "TempTarget1");
        if (blurMode == BlurMode.HorizontalAndVertical)
            RenderingUtils.ReAllocateIfNeeded(ref tempTarget2, in tempDesc, name: "TempTarget2");
    }
    
    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        int curKernelIdx = -1;
        switch (blurMode)
        {
            case BlurMode.Full:
                curKernelIdx = fullKernalIdx;
                break;
            case BlurMode.Horizontal:
            case BlurMode.HorizontalAndVertical:
                curKernelIdx = horizontalKernalIdx;
                break;
            case BlurMode.Vertical:
                curKernelIdx = verticalKernalIdx;
                break;
        }
        
        CommandBuffer cmd = CommandBufferPool.Get("Gaussian Blur");
        var src = renderingData.cameraData.renderer.cameraColorTargetHandle;

        // Dispatch the compute shader
        cmd.SetComputeTextureParam(computeShader, curKernelIdx, "InputTexture", src);
        cmd.SetComputeTextureParam(computeShader, curKernelIdx, "Result", tempTarget1);
        cmd.SetComputeFloatParam(computeShader, "BlurRadius", blurRadius);
        cmd.DispatchCompute(computeShader, curKernelIdx,
            Mathf.CeilToInt(renderingData.cameraData.cameraTargetDescriptor.width / 8.0f),
            Mathf.CeilToInt(renderingData.cameraData.cameraTargetDescriptor.height / 8.0f), 1);

        if (blurMode == BlurMode.HorizontalAndVertical)
        {
            // Dispatch the compute shader
            cmd.SetComputeTextureParam(computeShader, verticalKernalIdx, "InputTexture", tempTarget1);
            cmd.SetComputeTextureParam(computeShader, verticalKernalIdx, "Result", tempTarget2);
            cmd.SetComputeFloatParam(computeShader, "BlurRadius", blurRadius);
            cmd.DispatchCompute(computeShader, verticalKernalIdx,
                Mathf.CeilToInt(renderingData.cameraData.cameraTargetDescriptor.width / 8.0f),
                Mathf.CeilToInt(renderingData.cameraData.cameraTargetDescriptor.height / 8.0f), 1);
            
            cmd.Blit(tempTarget2, src);
        }
        else
        {
            cmd.Blit(tempTarget1, src);
        }
        
        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    public void Dispose()
    {
        tempTarget1?.Release();
        tempTarget2?.Release();
    }
}
