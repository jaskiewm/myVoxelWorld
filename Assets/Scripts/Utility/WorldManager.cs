using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Handles instantiation of chunks
public class WorldManager : MonoBehaviour
{
    public Material worldMaterial;
    public Container container;

    public WorldSettings worldSettings;

    public VoxelColour[] worldColours;

    // Initalize
    void Start()
    {
        //Used for transitioning scenes. This lets us only have one manager at a time
        //_instance is the private static reference (at bottome of page)
        //Note that "this" is the Public Voxel [Vector 3] in the Container script
        if (_instance != null)
        {
            if (_instance != this)
                Destroy(this);
        }
        else //If our _instance is empty, the assign it to "this"
        {
            _instance = this;
        }

        WorldSettings = worldSettings;
        ComputeManager.Instance.Initialize(1);
        GameObject cont = new GameObject("Container");
        cont.transform.parent = transform; //transform is same as world manager object
        container = cont.AddComponent<Container>(); //add containe
        container.Initialize(worldMaterial, Vector3.zero);

        //References the chunk as we don't want to pass in a class, since it'll copy it if we pass
        ComputeManager.Instance.GenerateVoxelData(ref container); 
    }

    // This section of code lets us have access to the world and object without having a reference (It is a singleton)
    public static WorldSettings WorldSettings;
    private static WorldManager _instance; //set up private static reference to our world manager
    public static WorldManager Instance //set up public static reference to our world manager
    {
        get
        {
            if (_instance == null) //checking if private instance is null
                _instance = FindFirstObjectByType<WorldManager>(); //not efficient, but only has to run once
            return _instance;
        }
    }
}

[System.Serializable]
public class WorldSettings //Used as a static reference to set the size of the environment
{
    public int containerSize = 16;
    public int maxHeight = 128;
}

