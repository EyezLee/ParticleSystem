using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireflyRender : MonoBehaviour
{
    [SerializeField] Material rendMat;
    [SerializeField] Texture fireflyTex;
    [SerializeField] FireflyManager fireflyManager;
    [SerializeField] Mesh fireflyMesh;

    ComputeBuffer fireflyBuffer;
    ComputeBuffer drawArgsBuffer;

    void GetFireflyBuffer()
    {
        if(fireflyManager != null)
            fireflyBuffer = fireflyManager.PassDataToRend();
        rendMat.SetBuffer("FireflyBuffer", fireflyBuffer);
    }
    private void Start()
    {
        // initialize the indirect draw args buffer
        drawArgsBuffer = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);
        drawArgsBuffer.SetData(new uint[5]
        {
            fireflyMesh.GetIndexCount(0), (uint)fireflyManager.FireflyCount, 0, 0, 0
        });

        rendMat.SetTexture("_MainTex", fireflyTex);

        GetFireflyBuffer();
    }

    private void Update()
    {
        Graphics.DrawMeshInstancedIndirect(
            fireflyMesh, 0, rendMat,
            new Bounds(Vector3.zero, new Vector3(100.0f, 100.0f, 100.0f)), drawArgsBuffer);
    }

    private void OnDestroy()
    {
        if (drawArgsBuffer != null) drawArgsBuffer.Release();
    }
}