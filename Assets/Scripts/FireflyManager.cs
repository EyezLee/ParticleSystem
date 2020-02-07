using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireflyManager : MonoBehaviour
{
    struct Firefly
    {
        Vector3 pos;
        Vector3 vol;
        Vector4 col;
        float brightness;
        float scale;
    };

    // boundry setting 
    [SerializeField]
    int[] boundBox = { 0, 8, 0, 8}; // minX maxX minY maxY

    public int fireflyCount = 10000;
    [SerializeField]
    ComputeShader fireflyCompute;
    [SerializeField]
    int randomSeed = 4;
    [SerializeField]
    float stepWidth = 0.1f;
    [SerializeField]
    float spread = 1;
    [SerializeField]
    float noiseFrequency;
    [SerializeField]
    float noiseOffset = 0.1f;
    [SerializeField]
    float brightnessStep = 0.1f;

    [SerializeField]
    Transform playerInput;

    ComputeBuffer fireflyBuffer;
    int kThreadCount = 64;
    int threadGroupCount { get{ return Mathf.CeilToInt((float)fireflyCount / kThreadCount); } }
    int bufferStride = 48; // 4 * 12

    // init fireflies
    void InitFirefly()
    {
        // set the kernel
        var initKernel = fireflyCompute.FindKernel("PreSnycInit");
        // allocate firefly buffer
        fireflyBuffer = new ComputeBuffer(fireflyCount, bufferStride);
        // pass variables to compute shader
        fireflyCompute.SetInt("randomSeed", randomSeed);
        fireflyCompute.SetBuffer(initKernel, "FireflyBuffer", fireflyBuffer);
        fireflyCompute.SetInts("boundBox", boundBox);
        // execute kernel
        fireflyCompute.Dispatch(initKernel, threadGroupCount, 1, 1);
    }

    // update fireflies
    void UpdateFirefly()
    {
        var updateKernel = fireflyCompute.FindKernel("PreSnycUpdate");
        // pass variables to compute shader
        fireflyCompute.SetFloat("deltaTime", Time.deltaTime);
        fireflyCompute.SetFloat("time", Time.time);
        fireflyCompute.SetBuffer(updateKernel, "FireflyBuffer", fireflyBuffer);
        fireflyCompute.SetFloat("stepWidth", stepWidth);
        fireflyCompute.SetFloat("spread", spread);
        fireflyCompute.SetFloat("noiseFrequency", noiseFrequency);
        fireflyCompute.SetFloat("noiseOffset", noiseOffset);
        fireflyCompute.SetFloat("brightnessStep", brightnessStep);
        // get cursor position
        float[] inputPos = { playerInput.position.x, playerInput.position.y};
        fireflyCompute.SetFloats("inputPos", inputPos);
        fireflyCompute.Dispatch(updateKernel, threadGroupCount, 1, 1);
    }

    public ComputeBuffer PassDataToRend()
    {
        // pass the firefly buffer to render
        return fireflyBuffer;
    }
    private void Awake()
    {
        InitFirefly();
        PassDataToRend();
    }
    private void Update()
    {
        UpdateFirefly();
        PassDataToRend();
    }
    private void OnDestroy()
    {
        fireflyBuffer.Release();
    }
}
