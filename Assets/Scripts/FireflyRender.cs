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
    [SerializeField] float speedOffset;
    [Range(0, 20)]
    [SerializeField] float couplingStrength = 2;
    [Range(1f, 10)]
    [SerializeField] float couplingRange = 5;

    [SerializeField] bool isReset = false;
    Firefly[] fireflyCopy;
    float[] coherencePhi; // ψ: average phase in coupling range for per fly
    float[] coherenceRad; // r: 
    float[] speed;

    const float toRadian = 2 * Mathf.PI; // circle in radius
    const float normRadian = 1 / toRadian;

    #region Karumoto Sync Model
    /* KARUMOTO MODEL:
     * dt(theta) = Wi + K * r * sin(averageTheta - theta[i]);
     */

    // -------------------------------------functions for Karumoto model--------------------------------------
    void ResetKaru()
    {
        int dataSize = fireflyManager.fireflyCount;
        // load current phase
        fireflyCopy = new Firefly[dataSize];
        fireflyBuffer.GetData(fireflyCopy);
        for(int i = 0; i < dataSize; i++)
        {
            fireflyCopy[i].phase += Noise() * 0.1f;
        }
        // allocate phi and rad arrays
        coherencePhi = new float[dataSize];
        coherenceRad = new float[dataSize];
        speed = new float[dataSize];
        for(int i = 0; i < dataSize; i++)
        {
            speed[i] = shineSpeed * Random.Range(1 - speedOffset, 1 + speedOffset);
        }
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
                    float theta = (fireflyCopy[j].phase) * toRadian;
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
    void UpdateKaru()
    {
        //if(isReset)
        //// reset phases
        //{
        //    isReset = false;
        //    ResetKaru();
        //}
        Coherence();
        float t = Time.deltaTime;
        for (int i = 0; i < fireflyCopy.Length; i++)
        {
            float theta = fireflyCopy[i].phase;
            float cphi = coherencePhi[i];
            float crad = coherenceRad[i];
            theta += t * (speed[i] + couplingStrength * crad * Mathf.Sin((cphi - theta) * toRadian));
            theta -= (int)theta;
            fireflyCopy[i].phase = theta;
        }
        // update buffer with new phase
        fireflyBuffer.SetData(fireflyCopy);
    }

    float Noise()
    {
        return 2f * Random.value - 1f;
    }
    #endregion

    void GetFireflyBuffer()
    {
        if(fireflyManager != null)
            fireflyBuffer = fireflyManager.PassDataToRend();
        rendMat.SetBuffer("FireflyBuffer", fireflyBuffer);
    }

    // interaction input triggered!
    private void OnValidate()
    {
        isReset = true;
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
            ResetKaru();
    }

    private void Update()
    {
        // nudge the phase here (karumoto model)
        UpdateKaru();
        Graphics.DrawMeshInstancedIndirect(
            fireflyMesh, 0, rendMat,
            new Bounds(Vector3.zero, new Vector3(100.0f, 100.0f, 100.0f)), drawArgsBuffer);
    }

    private void OnDestroy()
    {
        if (drawArgsBuffer != null) drawArgsBuffer.Release();
    }
}