using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Firefly
{
    public Vector3 pos;
    public Vector3 vol;
    public Vector4 col;
    public float phase;
    public float scale;
};
public class FireflyManager : MonoBehaviour
{
    ComputeBuffer fireflyBuffer;

    // boundry setting 
    [SerializeField]
    int[] boundBox = { 0, 8, 0, 8 }; // minX maxX minY maxY

    [SerializeField] int _fireflyCount = 1000;
    public int FireflyCount { get { return _fireflyCount; } }
    uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
    const int kThreadCount = 256;
    int threadGroupCount { get { return Mathf.CeilToInt((float)_fireflyCount / kThreadCount); } }
    int bufferStride = 48; // 4 * 12
    [SerializeField] ComputeShader fireflyCompute;

    // firefly movement properties
    [SerializeField] int randomSeed = 4;
    [SerializeField] float stepWidth = 0.1f;
    [SerializeField] float spread = 1;
    [SerializeField] float noiseFrequency;
    [SerializeField] float noiseOffset = 0.1f;
    [SerializeField] float motionFreq = 3;
    [SerializeField] float motionFreqOffset = 1;

    // firefly shine-sync properties
    [SerializeField] float shineSpeed;
    [SerializeField] float shineRange;
    [Range(0, 1)]
    [SerializeField] float coupling = 2;
    [Range(1f, 10)]
    [SerializeField] float couplingRange = 5;
    [SerializeField] bool isReset = false;
    Firefly[] fireflyCopy;
    float[] coherencePhi; // ψ: average phase in coupling range for per fly
    float[] coherenceRad; // r: 
    float[] speed;

    [SerializeField] Transform playerInput;

    // init fireflies
    void InitFirefly()
    {
        // set kernels
        var initFirefly = fireflyCompute.FindKernel("FireflyInit");
        // allocate firefly buffer
        fireflyBuffer = new ComputeBuffer(FireflyCount, bufferStride);
        // pass variables to compute shader
        fireflyCompute.SetInt("randomSeed", randomSeed);
        fireflyCompute.SetBuffer(initFirefly, "FireflyBuffer", fireflyBuffer);
        fireflyCompute.SetInts("boundBox", boundBox);
        // execute kernel
        fireflyCompute.Dispatch(initFirefly, threadGroupCount, 1, 1);
    }

    // update fireflies
    void UpdateFirefly()
    {
        var updateKernel = fireflyCompute.FindKernel("FireflyUpdate");
        // pass variables to compute shader
        fireflyCompute.SetFloat("deltaTime", Time.deltaTime);
        fireflyCompute.SetFloat("time", Time.time);
        fireflyCompute.SetBuffer(updateKernel, "FireflyBuffer", fireflyBuffer);
        fireflyCompute.SetFloat("stepWidth", stepWidth);
        fireflyCompute.SetFloat("spread", spread);
        fireflyCompute.SetFloat("noiseFrequency", noiseFrequency);
        fireflyCompute.SetFloat("noiseOffset", noiseOffset);
        fireflyCompute.SetFloat("freq", motionFreq);
        fireflyCompute.SetFloat("freqOffset", motionFreqOffset);
        // get cursor position
        float[] inputPos = { playerInput.position.x, playerInput.position.y};
        fireflyCompute.SetFloats("inputPos", inputPos);
        // execute compute shader func
        fireflyCompute.Dispatch(updateKernel, threadGroupCount, 1, 1);
    }

    void ResetSync()
    {
        var resetSync = fireflyCompute.FindKernel("SyncReset");
        fireflyCompute.SetFloat("time", Time.time);
        fireflyCompute.SetBuffer(resetSync, "FireflyBuffer", fireflyBuffer);
        fireflyCompute.Dispatch(resetSync, threadGroupCount, 1, 1);
    }

    void UpdateSync()
    {
        // update sync part
        var updateSync = fireflyCompute.FindKernel("SyncUpdate");
        // pass sync properties
        fireflyCompute.SetBuffer(updateSync, "FireflyBuffer", fireflyBuffer);
        fireflyCompute.SetInt("fireflyCount", FireflyCount);
        fireflyCompute.SetFloat("coupling", coupling);
        fireflyCompute.SetFloat("couplingRange", couplingRange);
        fireflyCompute.SetFloat("shineSpeed", shineSpeed);
        fireflyCompute.SetFloat("shineRange", shineRange);
        fireflyCompute.SetFloat("deltaTime", Time.deltaTime);
        // execute compute shader func
        fireflyCompute.Dispatch(updateSync, threadGroupCount, 1, 1);
    }

    public ComputeBuffer PassDataToRend()
    {
        // pass the firefly buffer to render
        return fireflyBuffer;
    }
    private void Awake()
    {
        InitFirefly();
        ResetSync();

        PassDataToRend();
    }
    private void Update()
    {
        UpdateFirefly();
        if (isReset)
        {
            isReset = false;
            ResetSync();
        }
        UpdateSync();
        PassDataToRend();
    }
    private void OnDestroy()
    {
        fireflyBuffer.Release();
    }

    private void OnValidate()
    {
        isReset = true;
    }
}