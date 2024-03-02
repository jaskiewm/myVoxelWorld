using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Rendering;

//Class runs all the compute shader code
public class ComputeManager : MonoBehaviour
{
    public ComputeShader noiseShader;

    private List<NoiseBuffer> allNoiseComputeBuffers = new List<NoiseBuffer>();
    private Queue<NoiseBuffer> availableNoiseComputeBuffers = new Queue<NoiseBuffer>();

    private int xThreads;
    private int yThreads;

    public void Initialize(int count = 256)
    {
        xThreads = (WorldManager.WorldSettings.containerSize / 8) + 1; //Set xThread to be chunk size / 8
        yThreads = WorldManager.WorldSettings.maxHeight / 8;

        //Set everything for the noise generation
        noiseShader.SetInt("containerSizeX", WorldManager.WorldSettings.containerSize);
        noiseShader.SetInt("containerSizeY", WorldManager.WorldSettings.maxHeight);

        noiseShader.SetBool("generateCaves", true);
        noiseShader.SetBool("forceFloor", true);

        noiseShader.SetInt("maxHeight", WorldManager.WorldSettings.maxHeight); //World Height
        noiseShader.SetInt("oceanHeight", 42);

        noiseShader.SetFloat("noiseScale",0.004f); //Noise
        noiseShader.SetFloat("caveScale", 0.01f);
        noiseShader.SetFloat("caveThreshold",0.8f);

        noiseShader.SetInt("surfaceVoxelID", 1); //atm just goes to colours
        noiseShader.SetInt("subSurfaceVoxelID", 2);

        for (int i = 0; i<count; i++)
        {
            CreateNewNoiseBuffer();
        }
    }

    public NoiseBuffer GetNoiseBuffer() //Checks if we have a buffer in the queue
    {
        if (availableNoiseComputeBuffers.Count > 0)
            return availableNoiseComputeBuffers.Dequeue();
        else
        {
            return CreateNewNoiseBuffer(false);
        }
    }

    public NoiseBuffer CreateNewNoiseBuffer(bool enqueue = true)
    {
        NoiseBuffer buffer = new NoiseBuffer();
        buffer.InitializeBuffer();
        allNoiseComputeBuffers.Add(buffer);

        if (enqueue)
            availableNoiseComputeBuffers.Enqueue(buffer);

        return buffer;
    }

    public void ClearAndRequeueBuffer(NoiseBuffer buffer)
    {
        ClearVoxelData(buffer);
        availableNoiseComputeBuffers.Enqueue(buffer);
    }

    public void GenerateVoxelData(ref Container cont)
    {
        //Assigns buffer to voxel arrays
        noiseShader.SetBuffer(0, "voxelArray", cont.data.noiseBuffer);
        noiseShader.SetBuffer(0, "count", cont.data.countBuffer);

        noiseShader.SetVector("chunkPosition", cont.containerPosition);
        noiseShader.SetVector("seedOffset", Vector3.zero);

        noiseShader.Dispatch(0, xThreads, yThreads, xThreads);


        AsyncGPUReadback.Request(cont.data.noiseBuffer, (callback) =>
        {
            callback.GetData<voxel>(0).CopyTo(WorldManager.Instance.container.data.voxelArray.array);
            WorldManager.Instance.container.RenderMesh();
        });
    }

    private void ClearVoxelData(NoiseBuffer buffer)
    {
        buffer.countBuffer.SetData(new int[] { 0 });
        noiseShader.SetBuffer(1, "voxelArray", buffer.noiseBuffer);
        noiseShader.Dispatch(1, xThreads, yThreads, xThreads);
    }

    private void OnApplicationQuit()
    {
        DisposeAllBuffers();
    }

    public void DisposeAllBuffers()
    {
        foreach (NoiseBuffer buffer in allNoiseComputeBuffers)
            buffer.Dispose();
    }

    private static ComputeManager _instance;

    public static ComputeManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindFirstObjectByType<ComputeManager>();
            return _instance;
        }
    }
}

public struct NoiseBuffer
{
    public ComputeBuffer noiseBuffer;
    public ComputeBuffer countBuffer;
    public bool Initialized;
    public bool Cleared;
    public IndexedArray<voxel> voxelArray;

    public void InitializeBuffer()
    {
        countBuffer = new ComputeBuffer(1, 4, ComputeBufferType.Counter);
        countBuffer.SetCounterValue(0);
        countBuffer.SetData(new uint[] { 0 });

        voxelArray = new IndexedArray<voxel>();
        noiseBuffer = new ComputeBuffer(voxelArray.Count, 4);
        noiseBuffer.SetData(voxelArray.GetData);
        Initialized = true;
    }

    public void Dispose()
    {
        countBuffer?.Dispose(); //?. == if not null
        noiseBuffer?.Dispose();

        Initialized = false;
    }

    public voxel this[Vector3 index]
    {
        get
        {
            return voxelArray[index];
        }

        set
        {
            voxelArray[index] = value;
        }
    }
}
