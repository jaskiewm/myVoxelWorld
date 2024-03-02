using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]

public class Container : MonoBehaviour
{
    public Vector3 containerPosition;
    private MeshData meshData = new MeshData();
    public NoiseBuffer data;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private MeshCollider meshCollider;

    //Function used to assign material to mesh renderer
    //And sets the local position variable
    public void Initialize(Material mat, Vector3 position)
    {
        ConfigureComponents();
        data = ComputeManager.Instance.GetNoiseBuffer();
        meshRenderer.sharedMaterial = mat;
        containerPosition = position;
    }


    public void ClearData()
    {
        ComputeManager.Instance.ClearAndRequeueBuffer(data);
    }

    //Gets components from the game object for all mesh variables
    private void ConfigureComponents()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshCollider = GetComponent<MeshCollider>();
    }

    public void RenderMesh()
    {
        meshData.ClearData(); //Last time we're running on main thread
        GenerateMesh();
        UploadMesh();
    }

    public void GenerateMesh()
    {
        //single block to draw a cube (position of 8,8,8)
        Vector3 blockPos = new Vector3(8, 8, 8);
        voxel block = new voxel() { ID = 1 }; //ID 0 = air, 1 = block

        //Variables are set outside the for loop so you don't have to reallocate variables
        int counter = 0;
        Vector3[] faceVertices = new Vector3[4];
        Vector2[] faceUVs = new Vector2[4];

        //Declared appearance variables for the cubes
        VoxelColour voxelColour;
        Color voxelColorAlpha;
        Vector2 voxelSmoothness;

        for (int x =1; x< WorldManager.WorldSettings.containerSize + 1; x++)
            for(int y = 0; y < WorldManager.WorldSettings.maxHeight; y++)
                for (int z = 1; z < WorldManager.WorldSettings.containerSize + 1; z++)
                {
                    blockPos = new Vector3(x, y, z);
                    block = this[blockPos];
                    //Only check on solid blocks
                    if (!block.isSolid)
                        continue;
           
                    //assigns voxel colour based on the array in World Manager. block.ID = 0 is air so we need -1
                    voxelColour = WorldManager.Instance.worldColours[block.ID - 1];
                    voxelColorAlpha = voxelColour.colour;
                    voxelColorAlpha.a = 1;
                    voxelSmoothness = new Vector2(voxelColour.metallic, voxelColour.smoothness);

                    //Iterate over each face of the cube
                    //This does not check the face of the cube
                    for (int i =0; i<6; i++)
                    {
                        if (checkVoxelIsSolid(blockPos + voxelFaceChecks[i])) // Checks if block is solid or air, if solid it continues
                            continue;
                        //Draw this face

                        //Collect appropriate vertices from default vertices and add the block position

                        for (int j = 0; j < 4; j++)
                        {
                            faceVertices[j] = voxelVertices[voxelVerticeIndex[i, j]] + blockPos; //i == face, j == vertice
                            faceUVs[j] = voxelUVs[j]; //map UVs to each quad
                        }
                        for (int j = 0; j < 6; j++) //iterate through 6 points for the triangles
                        {
                            meshData.vertices.Add(faceVertices[voxelTriangles[i, j]]);
                            meshData.UVs.Add(faceUVs[voxelTriangles[i, j]]);

                            meshData.colours.Add(voxelColorAlpha);
                            meshData.UVs2.Add(voxelSmoothness);

                            meshData.triangles.Add(counter++); //No shared vertice (36 for a cube) so counting up to 6 (6*6)
                        }
           

                    }
                }
    }

    public void UploadMesh()
    {
        meshData.UploadMesh(); // calls the upload mesh from mesh data (sets vertices and triangles)

        if(meshRenderer == null) // Makes sure components are set up if not null
        {
            ConfigureComponents();
        }

        meshFilter.mesh = meshData.mesh; //assign mesh filter with the one that's just made
        if(meshData.vertices.Count > 3)
        {
            meshCollider.sharedMesh = meshData.mesh;
        }
    }

    //Checks if point is within the bounds so a face isn't drawn (Edge of chunk, no face is drawn)
    public bool checkVoxelIsSolid(Vector3 point)
    {
        if (point.y < 0 || (point.x > WorldManager.WorldSettings.containerSize + 2) || (point.z > WorldManager.WorldSettings.containerSize + 2))
            return true;
        else
            return this[point].isSolid;
    }

    public voxel this[Vector3 index]
    {
        get
        {
            return data[index];
        }
        set
        {
                data[index] = value;
        }
    }

    public struct MeshData
    {
        public Mesh mesh;

        //List contains every vertex of mesh that we are going to render
        public List<Vector3> vertices;

        //The UV list tells Unity how the texture is aligned on each polygon
        public List<Vector2> UVs; //Used for textures
        public List<Vector2> UVs2; //Used for smoothness

        // The triangles tell Unity how to build each section of the mesh by joining vertices
        public List<int> triangles;

        public List<Color> colours;

        public bool Initialized; 

        public void ClearData()
        {
            if (!Initialized) //If it's the programs' first time running
            {
                //New lists are created for each variable
                vertices = new List<Vector3>();
                UVs = new List<Vector2>();
                triangles = new List<int>();
                mesh = new Mesh();

                UVs2 = new List<Vector2>();
                colours = new List<Color>();

                Initialized = true;
            }
            else //Clears the lists if already initialized
            {
                vertices.Clear();
                UVs.Clear();
                UVs2.Clear();
                colours.Clear();
                triangles.Clear();
                mesh.Clear();

            }
        }

        public void UploadMesh(bool sharedVertices = false)
        {
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; //Set index format to Unsigned Int 32 (go past 65k vertice limit)

            mesh.SetVertices(vertices); //Set mesh vertices to vertices created
            mesh.SetUVs(0,UVs);
            mesh.SetUVs(2, UVs2); //second channel with UVs2

            mesh.SetColors(colours);
            mesh.SetTriangles(triangles, 0, false); //Set mesh triangle to triangles created

            mesh.Optimize();
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            mesh.UploadMeshData(false);
        }
    }

    //Region for setting up the faces, triangles, and vertices of the cubes
    #region Voxel Statics
    static readonly Vector3[] voxelVertices = new Vector3[8]
    {
        new Vector3(0,0,0), //0 Bottom Left of cube
        new Vector3(1,0,0), //1
        new Vector3(0,1,0), //2
        new Vector3(1,1,0), //3

        new Vector3(0,0,1), //4
        new Vector3(1,0,1), //5
        new Vector3(0,1,1), //6
        new Vector3(1,1,1), //7 Top Right of cube
    };

    static readonly Vector3[] voxelFaceChecks = new Vector3[6]
    {
        new Vector3(0,0,-1),    //back
        new Vector3(0,0,1),    //front
        new Vector3(-1,0,0),    //left
        new Vector3(1,0,0),    //right
        new Vector3(0,-1,0),    //bottom
        new Vector3(0,1,0)     //top
    };

    static readonly int[,] voxelVerticeIndex = new int[6,4]
    {
        //The vertex indexes on the cube (each face)
        //Each number (0-7) is based on var voxelVertices numbers
        {0,1,2,3}, //Face 1
        {4,5,6,7}, //Face 2
        {4,0,6,2}, //Face 3
        {5,1,7,3}, //Face 4
        {0,1,4,5}, //Face 5
        {2,3,6,7}, //Face 6
    };

    static readonly Vector2[] voxelUVs = new Vector2[4]
    {
        new Vector2(0,0),
        new Vector2(0,1),
        new Vector2(1,0),
        new Vector2(1,1),
    };

    //Only uses the 4 vertices on a face
    static readonly int[,] voxelTriangles = new int[6, 6]
    {
        {0,2,3,0,3,1},
        {0,1,2,1,3,2},
        {0,2,3,0,3,1},
        {0,1,2,1,3,2},
        {0,1,2,1,3,2},
        {0,2,3,0,3,1},
    };

    #endregion

}
