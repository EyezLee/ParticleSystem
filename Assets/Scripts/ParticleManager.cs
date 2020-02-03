using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    // particle data
    struct Particle
    {
        public Vector3 position;
        public Vector3 velocity;
        public float lifetime;
    };

    int perParticle = 28; // data size of per particle

    [SerializeField]
    int particleCount = 100000;

    // compute shader data
    [SerializeField]
    ComputeShader particleFactory;

    int updataParticleKernel;
    ComputeBuffer particleBuffer;
    const int perGroupSize = 256; // workspace in x
    int threadGroupCount;

    [SerializeField]
    Material renderMat;

    // init compute shader
    void InitParticle()
    {
        threadGroupCount = Mathf.CeilToInt((float)particleCount / perGroupSize); // int / int == int; float / int == float

        // initalize particles
        Particle[] particleArray = new Particle[particleCount];
        for(int i = 0; i < particleCount; i++)
        {
            float x = Random.value * 2 - 1.0f;
            float y = Random.value * 2 - 1.0f;
            float z = Random.value * 2 - 1.0f;
            Vector3 pos = new Vector3(x, y, z);
            pos.Normalize();
            pos *= Random.value;
            pos *= 0.5f;

            particleArray[i].position.x = pos.x;
            particleArray[i].position.y = pos.y;
            particleArray[i].position.z = pos.z;
            particleArray[i].velocity.x = 0;
            particleArray[i].velocity.y = 0;
            particleArray[i].velocity.z = 0;
            particleArray[i].lifetime = Random.value * 5.0f + 1.0f;
        }

        // set particle buffer
        particleBuffer = new ComputeBuffer(particleCount, perParticle); // count and stride
        particleBuffer.SetData(particleArray);
        updataParticleKernel = particleFactory.FindKernel("UpdateParticle");
        // bind buffer to compute shader
        particleFactory.SetBuffer(updataParticleKernel, "particleBuffer", particleBuffer);

        // pass it to render shader
        renderMat.SetBuffer("particleBuffer", particleBuffer);
    }

    void UpdateParticle()
    {
        float[] mousePos = { Input.mousePosition.x / Screen.width, Input.mousePosition.y / Screen.height};

        // pass data to compute shader
        particleFactory.SetFloat("deltaTime", Time.deltaTime);
        particleFactory.SetFloats("mousePos", mousePos);

        // execute compute shader
        particleFactory.Dispatch(updataParticleKernel, threadGroupCount, 1, 1);
    }

    void Start()
    {
        InitParticle();
    }

    // excute compute shader
    private void Update() 
    {
        UpdateParticle();
    }

    void OnDestroy()
    {
        if(particleBuffer != null)
        {
            particleBuffer.Release();
        }
    }

    void OnRenderObject()
    {
        renderMat.SetPass(0);
        Graphics.DrawProceduralNow(MeshTopology.Points, 1, particleCount);
    }
    // render 
}
