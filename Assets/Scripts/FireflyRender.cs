using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireflyRender : MonoBehaviour
{
    [SerializeField]
    Material rendMat;

    [SerializeField]
    FireflyManager fireflyManager;

    ComputeBuffer fireflyBuffer;

    void GetBuffer()
    {
        fireflyBuffer = fireflyManager.PassDataToRend();
        rendMat.SetBuffer("FireflyBuffer", fireflyBuffer);
    }

    private void Start()
    {
        GetBuffer();
    }

    private void OnRenderObject()
    {
        rendMat.SetPass(0);
        GetBuffer();
        Graphics.DrawProceduralNow(MeshTopology.Points, 1, fireflyManager.fireflyCount);
    }
}
