// #define USE_PARALLEL_COMMAND_PROCESSING
// #define DEBUG_PHOSPHOR

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("Oscilloscope UI/Screen Renderer")]
[RequireComponent(typeof(RawImage))]
public class PhosphorScreenRenderer : MonoBehaviour
{
    [Header("Phosphor Settings")]
    public Color PhosphorColor = new Color(1f, 0.5f, 0.1f);
    public float PhosphorBleed = 2f;
    public float PhosphorResponse = 0.6f;
    public float PhosphorFadeTime = 0.25f;
    public float ScreenRefreshTime = 0.1f;

    [Header("Compute Shader Settings")]
    public ComputeShader PhosphorComputeShader;
    public int TextureSize = 256;
    public RawImage Image;

    private const float PhosphorDecayThreshold = 0.01f;
    private const int DefaultTextureThreadGroupSize = 16;
#if USE_PARALLEL_COMMAND_PROCESSING
    private const int CommandBufferTextureThreadGroupSize = 8;
    private const int CommandBufferThreadGroupSize = 4;
    private const int CommandBlockSize = 32;
#endif

    private const int MaxCommandsPerDispatch = 1024;
    private int defaultThreadGroupCount;
#if USE_PARALLEL_COMMAND_PROCESSING
    private int commandBufferTextureThreadGroupCount;
#endif

    private int fadePhosphorKernel;
#if DEBUG_PHOSPHOR
    private int debugPhosphorKernel;
#endif
    private int activatePhosphorKernel;
    private int colorizePhosphorKernel;

#if USE_PARALLEL_COMMAND_PROCESSING
    private RenderTexture mutexTexture;
#endif
    private RenderTexture phosphorScratchTexture;
    private RenderTexture finalTexture;

    private List<PhosphorCommand> commandBuffer;

    private ComputeBuffer commandComputeBuffer;

    private void Reset()
    {
        this.PhosphorComputeShader = Resources.Load<ComputeShader>("PhosphorCompute");
        this.Image = GetComponent<RawImage>();
    }

    private void OnValidate()
    {
        this.PhosphorBleed = Mathf.Max(this.PhosphorBleed, 0f);
        this.PhosphorResponse = Mathf.Max(this.PhosphorResponse, 0f);
        this.PhosphorFadeTime = Mathf.Max(this.PhosphorFadeTime, 0f);
        this.TextureSize = Mathf.ClosestPowerOfTwo(Mathf.Max(DefaultTextureThreadGroupSize, this.TextureSize));
    }

    private void OnDestroy()
    {
        if (this.commandComputeBuffer != null) this.commandComputeBuffer.Release();
    }

    // Use this for initialization
    void Start()
    {
        this.commandBuffer = new List<PhosphorCommand>();

        this.defaultThreadGroupCount = this.TextureSize / DefaultTextureThreadGroupSize;
#if USE_PARALLEL_COMMAND_PROCESSING
        this.commandBufferTextureThreadGroupCount = this.TextureSize / CommandBufferTextureThreadGroupSize;

        this.mutexTexture = CreateRenderTexture(this.TextureSize, RenderTextureFormat.RInt);
#endif

        this.phosphorScratchTexture = CreateRenderTexture(this.TextureSize, RenderTextureFormat.RFloat);
        this.finalTexture = CreateRenderTexture(this.TextureSize, RenderTextureFormat.ARGBFloat);

        this.fadePhosphorKernel = this.PhosphorComputeShader.FindKernel("FadePhosphor");
#if DEBUG_PHOSPHOR
        this.debugPhosphorKernel = this.PhosphorComputeShader.FindKernel("DebugPhosphor");
#endif
#if USE_PARALLEL_COMMAND_PROCESSING
        this.activatePhosphorKernel = this.PhosphorComputeShader.FindKernel("ActivatePhosphor2");
#else
        this.activatePhosphorKernel = this.PhosphorComputeShader.FindKernel("ActivatePhosphor");
#endif
        this.colorizePhosphorKernel = this.PhosphorComputeShader.FindKernel("ColorizePhosphor");

        SetConstants();

        SetTextureBuffers();

#if DEBUG_PHOSPHOR
        this.PhosphorComputeShader.Dispatch(this.debugPhosphorKernel, this.defaultThreadGroupCount, this.defaultThreadGroupCount, 1);
#endif

        if (this.Image != null) this.Image.texture = this.finalTexture;

        StartCoroutine(RefreshBufferCoroutine);
    }

    private void SetConstants()
    {
        this.PhosphorComputeShader.SetVector("PhosphorColor", Color.white);
        this.PhosphorComputeShader.SetFloat("PhosphorBleed", this.PhosphorBleed);
        this.PhosphorComputeShader.SetFloat("PhosphorResponse", this.PhosphorResponse);
        this.PhosphorComputeShader.SetFloat("DecayTime", this.PhosphorFadeTime);
        this.PhosphorComputeShader.SetFloat("DecayThreshold", PhosphorDecayThreshold);
        this.PhosphorComputeShader.SetInt("TextureSize", this.TextureSize);
    }

    private void SetTextureBuffers()
    {
        this.PhosphorComputeShader.SetTexture(this.fadePhosphorKernel, "FadePhosphorInput", this.phosphorScratchTexture);
        this.PhosphorComputeShader.SetTexture(this.fadePhosphorKernel, "FadePhosphorResult", this.phosphorScratchTexture);
#if DEBUG_PHOSPHOR
        this.PhosphorComputeShader.SetTexture(this.debugPhosphorKernel, "DebugPhosphorResult", this.phosphorScratchTexture);
#endif
#if USE_PARALLEL_COMMAND_PROCESSING
        this.PhosphorComputeShader.SetTexture(this.activatePhosphorKernel, "Mutex", this.mutexTexture);
#endif
        this.PhosphorComputeShader.SetTexture(this.activatePhosphorKernel, "ActivatePhosphorInput", this.phosphorScratchTexture);
        this.PhosphorComputeShader.SetTexture(this.activatePhosphorKernel, "ActivatePhosphorResult", this.phosphorScratchTexture);
        this.PhosphorComputeShader.SetTexture(this.colorizePhosphorKernel, "ColorizePhosphorInput", this.phosphorScratchTexture);
        this.PhosphorComputeShader.SetTexture(this.colorizePhosphorKernel, "ColorizePhosphorResult", this.finalTexture);
    }

    // Update is called once per frame
    void Update()
    {
        this.Image.color = this.PhosphorColor;

        this.PhosphorComputeShader.SetFloat("DeltaTime", Time.deltaTime);
        this.PhosphorComputeShader.Dispatch(this.fadePhosphorKernel, this.defaultThreadGroupCount, this.defaultThreadGroupCount, 1);

        ChunkCommandBuffer(ref this.commandComputeBuffer, this.commandBuffer, MaxCommandsPerDispatch);
#if USE_PARALLEL_COMMAND_PROCESSING
        int commandGroupCount = 0;
        if (this.commandComputeBuffer != null)
        {
            this.PhosphorComputeShader.SetBuffer(this.activatePhosphorKernel, "Commands", this.commandComputeBuffer);

            commandGroupCount = Mathf.CeilToInt(this.commandComputeBuffer.count / (float)(CommandBufferThreadGroupSize * CommandBlockSize));
        }

        this.PhosphorComputeShader.Dispatch(this.activatePhosphorKernel, this.commandBufferTextureThreadGroupCount, this.commandBufferTextureThreadGroupCount, commandGroupCount);
#else
        if (this.commandComputeBuffer != null) this.PhosphorComputeShader.SetBuffer(this.activatePhosphorKernel, "Commands", this.commandComputeBuffer);

        this.PhosphorComputeShader.Dispatch(this.activatePhosphorKernel, this.defaultThreadGroupCount, this.defaultThreadGroupCount, 1);
#endif

        this.PhosphorComputeShader.Dispatch(this.colorizePhosphorKernel, this.defaultThreadGroupCount, this.defaultThreadGroupCount, 1);
    }

    public void SetPhosphorCommandBuffer(PhosphorCommand[] buffer)
    {
        this.commandBuffer.AddRange(buffer);
    }

    private void ClearPhosphorCommandBuffer()
    {
        if (this.commandBuffer != null) this.commandBuffer.Clear();
    }

    private void ChunkCommandBuffer(ref ComputeBuffer buffer, List<PhosphorCommand> commands, int chunkSize)
    {
        if (buffer != null) buffer.Release();
        //RemoveExpiredCommands(commands);

        if (commands.Count == 0)
        {
            buffer = null;
            return;
        }

        int bufferSize = Mathf.Min(commands.Count, chunkSize);

        buffer = new ComputeBuffer(bufferSize, 16);
        buffer.SetData(commands.Take(bufferSize).ToArray());

        commands.RemoveRange(0, bufferSize);
    }

    private RenderTexture CreateRenderTexture(int size, RenderTextureFormat format)
    {
        RenderTexture texture = new RenderTexture(size, size, 0, format);
        texture.enableRandomWrite = true;
        texture.Create();

        return texture;
    }

    private void RemoveExpiredCommands(List<PhosphorCommand> commands)
    {
        int count = commands.TakeWhile(CommandHasExpired).Count();
        commands.RemoveRange(0, count);
    }

    private bool CommandHasExpired(PhosphorCommand command)
    {
        return command.Time + 2.5f * this.PhosphorFadeTime < Time.time;
    }

    private IEnumerator RefreshBufferCoroutine
    {
        get
        {
            WaitForSeconds wait = new WaitForSeconds(ScreenRefreshTime);

            while (true)
            {
                this.ClearPhosphorCommandBuffer();
                yield return wait;
            }
        }
    }
}
