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

    public int fireflyCount = 1000;
    [SerializeField]
    ComputeShader fireflyCompute;
    [SerializeField]
    int randomSeed = 1;

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
        // execute kernel
        fireflyCompute.Dispatch(initKernel, threadGroupCount, 1, 1);
    }

    // update fireflies
    void UpdateFirefly()
    {
        var updateKernel = fireflyCompute.FindKernel("PreSnycUpdate");
        // pass variables to compute shader
        fireflyCompute.SetFloat("deltaTime", Time.deltaTime);
        fireflyCompute.SetBuffer(updateKernel, "FireflyBuffer", fireflyBuffer);
        // get cursor position
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        Physics.Raycast(ray, out hit);
        float[] mousePos = { hit.point.x, hit.point.y };
        fireflyCompute.SetFloats("inputPos", mousePos);
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
