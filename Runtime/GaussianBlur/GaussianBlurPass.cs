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

    private RenderTextureDescriptor tempDesc;
    private RTHandle tempTarget1;
    private RTHandle tempTarget2;

    public BlurMode blurMode = BlurMode.Full;
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
        CommandBuffer cmd = CommandBufferPool.Get("Gaussian Blur");

        var src = renderingData.cameraData.renderer.cameraColorTargetHandle;

        if(blurMode==BlurMode.Full)
        {
            // Dispatch the compute shader
            cmd.SetComputeTextureParam(computeShader, fullKernalIdx, "InputTexture", src);
            cmd.SetComputeTextureParam(computeShader, fullKernalIdx, "Result", tempTarget1);
            cmd.SetComputeFloatParam(computeShader, "BlurRadius", blurRadius);
            cmd.DispatchCompute(computeShader, fullKernalIdx,
                Mathf.CeilToInt(renderingData.cameraData.cameraTargetDescriptor.width / 8.0f),
                Mathf.CeilToInt(renderingData.cameraData.cameraTargetDescriptor.height / 8.0f), 1);

            cmd.Blit(tempTarget1, src);
        }
        else if(blurMode==BlurMode.Horizontal)
        {
            // Dispatch the compute shader
            cmd.SetComputeTextureParam(computeShader, horizontalKernalIdx, "InputTexture", src);
            cmd.SetComputeTextureParam(computeShader, horizontalKernalIdx, "Result", tempTarget1);
            cmd.SetComputeFloatParam(computeShader, "BlurRadius", blurRadius);
            cmd.DispatchCompute(computeShader, horizontalKernalIdx,
                Mathf.CeilToInt(renderingData.cameraData.cameraTargetDescriptor.width / 8.0f),
                Mathf.CeilToInt(renderingData.cameraData.cameraTargetDescriptor.height / 8.0f), 1);

            cmd.Blit(tempTarget1, src);
        }
        else if(blurMode==BlurMode.Vertical)
        {
            // Dispatch the compute shader
            cmd.SetComputeTextureParam(computeShader, verticalKernalIdx, "InputTexture", src);
            cmd.SetComputeTextureParam(computeShader, verticalKernalIdx, "Result", tempTarget1);
            cmd.SetComputeFloatParam(computeShader, "BlurRadius", blurRadius);
            cmd.DispatchCompute(computeShader, verticalKernalIdx,
                Mathf.CeilToInt(renderingData.cameraData.cameraTargetDescriptor.width / 8.0f),
                Mathf.CeilToInt(renderingData.cameraData.cameraTargetDescriptor.height / 8.0f), 1);

            cmd.Blit(tempTarget1, src);
        }
        else if (blurMode == BlurMode.HorizontalAndVertical)
        {
            // Dispatch the compute shader
            cmd.SetComputeTextureParam(computeShader, horizontalKernalIdx, "InputTexture", src);
            cmd.SetComputeTextureParam(computeShader, horizontalKernalIdx, "Result", tempTarget1);
            cmd.SetComputeFloatParam(computeShader, "BlurRadius", blurRadius);
            cmd.DispatchCompute(computeShader, horizontalKernalIdx,
                Mathf.CeilToInt(renderingData.cameraData.cameraTargetDescriptor.width / 8.0f),
                Mathf.CeilToInt(renderingData.cameraData.cameraTargetDescriptor.height / 8.0f), 1);
            
            // Dispatch the compute shader
            cmd.SetComputeTextureParam(computeShader, verticalKernalIdx, "InputTexture", tempTarget1);
            cmd.SetComputeTextureParam(computeShader, verticalKernalIdx, "Result", tempTarget2);
            cmd.SetComputeFloatParam(computeShader, "BlurRadius", blurRadius);
            cmd.DispatchCompute(computeShader, verticalKernalIdx,
                Mathf.CeilToInt(renderingData.cameraData.cameraTargetDescriptor.width / 8.0f),
                Mathf.CeilToInt(renderingData.cameraData.cameraTargetDescriptor.height / 8.0f), 1);
            
            cmd.Blit(tempTarget2, src);
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
