using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


class OutlinePass : ScriptableRenderPass
{
    private OutlineFeature.CustomPassSettings settings;

    //private RenderTargetIdentifier colorBuffer, pixelBuffer;
    //private int pixelBufferID = Shader.PropertyToID("_PixelBuffer");
    private RTHandle m_Handle;

    private RTHandle colorBufferHandle;
    private RTHandle m_PixelBufferHandle;

    private Material material;
    private Color outlineColor;
    private float edgeThreshold;

    void Dispose()
    {
        m_Handle?.Release();
    }


    public OutlinePass(OutlineFeature.CustomPassSettings settings)
    {
        this.settings = settings;
        this.renderPassEvent = settings.renderPassEvent;
        if (material == null) material = CoreUtils.CreateEngineMaterial("Assets/Outline");

        m_PixelBufferHandle = RTHandles.Alloc("_OutlinePixelBuffer");
    }

    // This method is called before executing the render pass.
    // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
    // When empty this render pass will render to the active camera render target.
    // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
    // The render pipeline will ensure target setup and clearing happens in a performant manner.
    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        colorBufferHandle = renderingData.cameraData.renderer.cameraColorTargetHandle;

        var descriptor = renderingData.cameraData.cameraTargetDescriptor;
        //descriptor.depthBufferBits = 0;

        RenderingUtils.ReAllocateIfNeeded(ref m_PixelBufferHandle, descriptor, FilterMode.Point, TextureWrapMode.Clamp, name: "_OutlinePixelBuffer");

        //ConfigureInput(ScriptableRenderPassInput.Color);

        outlineColor = settings.outlineColor;
        edgeThreshold = settings.edgeThreshold;

        material.SetColor("_OutlineColor", outlineColor);
        material.SetFloat("_EdgeThreshold", edgeThreshold);

        
        //cmd.GetTemporaryRT(pixelBufferID, descriptor, FilterMode.Point); //filtermode point maybe
        //pixelBuffer = new RenderTargetIdentifier(pixelBufferID);
    }


    // Here you can implement the rendering logic.
    // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
    // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
    // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        CommandBuffer cmd = CommandBufferPool.Get();

        using (new ProfilingScope(cmd, new ProfilingSampler("Outline Pass")))
        {
            Blitter.BlitCameraTexture(cmd, colorBufferHandle, m_PixelBufferHandle, material, 0);
            Blitter.BlitCameraTexture(cmd, m_PixelBufferHandle, colorBufferHandle, 0, false);
        }

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd); 
    }


    // Cleanup any allocated resources that were created during the execution of this render pass.
    public override void OnCameraCleanup(CommandBuffer cmd)
    {
        if (cmd == null) throw new System.ArgumentNullException("cmd");

        RTHandles.Release(m_PixelBufferHandle);
        m_PixelBufferHandle = null;
        //cmd.ReleaseTemporaryRT(pixelBufferID);
    }
}

