using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Handles instantiation of chunks
public class WorldManager : MonoBehaviour
{
    public Material worldMaterial;
    private Container container;

    // Initalize
    void Start()
    {
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
}
