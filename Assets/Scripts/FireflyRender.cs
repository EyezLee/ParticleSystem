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

    // firefly effect properties
    [SerializeField] float shineSpeed;
    [Range(0, 5)]
    [SerializeField] float couplingStrength = 2;
    [Range(1, 10)]
    [SerializeField] float couplingRange = 5;

    Firefly[] fireflyCopy;
    float[] coherencePhi; // ψ: average phase in coupling range for per fly
    float[] coherenceRad; // r: 

    const float toRadian = 2 * Mathf.PI; // circle in radius
    const float normRadian = 1 / toRadian;

    #region Karumoto Sync Model
    /* KARUMOTO MODEL:
     * dt(theta) = Wi + K * r * sin(averageTheta - theta[i]);
     */

    // -------------------------------------functions for Karumoto model--------------------------------------
    void InitKaru()
    {
        int dataSize = fireflyManager.fireflyCount;
        // load current phase
        fireflyCopy = new Firefly[dataSize];
        fireflyBuffer.GetData(fireflyCopy);
        // allocate phi and rad arrays
        coherencePhi = new float[dataSize];
        coherenceRad = new float[dataSize];
    }

    void Coherence()
    {
        // get fireflies within the range
        float sumX = 0, sumY = 0;
        int count = 0; // count for fireflies in coupling range
        for(int i = 0; i < fireflyCopy.Length; i++)
        {
            Firefly currFly = fireflyCopy[i]; // each fly as center
            for(int j = 0; j < fireflyCopy.Length; j++) // loop over all the flies for that center
            {
                float dist = Vector3.Distance(currFly.pos, fireflyCopy[j].pos); // calculate the distance
                if(dist < couplingRange && i != j) // within the coupling range and not itself
                {
                    float theta = fireflyCopy[j].phase * toRadian;
                    sumX += Mathf.Cos(theta);
                    sumY += Mathf.Sin(theta);
                    count++;
                }
            }
            // average the theta 
            sumX /= count;
            sumY /= count;
            coherencePhi[i] = Mathf.Atan2(sumY, sumX) * normRadian;
            coherenceRad[i] = Mathf.Sqrt(sumX * sumX + sumY * sumY); // isn't it always going to be 1???
        }
    }
    [SerializeField] bool deSync = false;
    void UpdateKaru()
    {
        Coherence();
        float t = Time.deltaTime;
        for(int i = 0; i < fireflyCopy.Length; i++)
        {
            float theta = fireflyCopy[i].phase;
            float cphi = coherencePhi[i];
            float crad = coherenceRad[i];
            if (!deSync)
            {
                theta += t * (couplingStrength * crad * Mathf.Sin(cphi - theta) * toRadian);
            }
            else
            {
                theta -= t * (couplingStrength * crad * Mathf.Sin(cphi - theta) * toRadian);
            }
            theta -= (int)theta; // why???
            fireflyCopy[i].phase = theta;
        }
    }
    #endregion

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
            fireflyMesh.GetIndexCount(0), (uint)fireflyManager.fireflyCount, 0, 0, 0
        });

        rendMat.SetTexture("_MainTex", fireflyTex);

        GetFireflyBuffer();

        // initialization for Karumoto sync
        if(fireflyBuffer != null)
            InitKaru();
    }

    private void Update()
    {
        rendMat.SetFloat("_shineSpeed", shineSpeed);

        // nudge the phase here (karumoto model)
        UpdateKaru();
        // update buffer with new phase/phase
        fireflyBuffer.SetData(fireflyCopy);

        Graphics.DrawMeshInstancedIndirect(
            fireflyMesh, 0, rendMat,
            new Bounds(Vector3.zero, new Vector3(100.0f, 100.0f, 100.0f)), drawArgsBuffer
            );
    }

    private void OnDestroy()
    {
        if (drawArgsBuffer != null) drawArgsBuffer.Release();
    }
}
