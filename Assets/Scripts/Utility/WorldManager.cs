using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Handles instantiation of chunks
public class WorldManager : MonoBehaviour
{
    public Material worldMaterial;
    private Container container;

    public VoxelColour[] worldColours;

    // Initalize
    void Start()
    {
        //Used for transitioning scenes. This lets us only have one manager at a time
        //_instance is the private static reference (at bottome of page)
        //Note that "this" is the Public Voxel [Vector 3] in the Container script
        if(_instance != null)
        {
            if (_instance != this)
                Destroy(this);
        }
        else //If our _instance is empty, the assign it to "this"
        {
            _instance = this;
        }

        GameObject cont = new GameObject("Container");
        cont.transform.parent = transform; //transform is same as world manager object
        container = cont.AddComponent<Container>(); //add containe
        container.Initialize(worldMaterial, Vector3.zero);

        for(int x=1; x<9; x++) // Length of the procedural chunk (# of blocks in length)
        {
            for (int z = 1; z < 9; z++) // Width of the procedural chunk (# of blocks in width)
            {
                int randomYHeight = Random.Range(1, 5);
                for (int y = 0; y < randomYHeight; y++) //If y is less than a random height, it becomes solid
                {
                    container[new Vector3(x, y, z)] = new voxel() { ID = 1 }; //sets to be a solid block
                }
            }
        }

        container.GenerateMesh();
        container.UploadMesh();
    }

    // This section of code lets us have access to the world and object without having a reference (It is a singleton)

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
