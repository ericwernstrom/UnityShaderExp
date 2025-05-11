using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class OutlineFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class CustomPassSettings
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        public Color outlineColor = Color.black;
        [Range(0.05f, 2.0f)] public float edgeThreshold = 1.0f;
    }

    [SerializeField] private CustomPassSettings settings;
    private OutlinePass customPass;

    public override void Create()
    {
        customPass = new OutlinePass(settings);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(customPass);
    }

}


