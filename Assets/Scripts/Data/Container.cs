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

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private MeshCollider meshCollider;

    //Function used to assign material to mesh renderer
    //And sets the local position variable
    public void Initialize(Material mat, Vector3 position)
    {
        ConfigureComponents();
        meshRenderer.sharedMaterial = mat;
        containerPosition = position;
    }

    //Gets components from the game object for all mesh variables
    private void ConfigureComponents()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshCollider = GetComponent<MeshCollider>();
    }

    public void GenerateMesh()
    {

    }

    public void UploadMesh()
    {

    }

    public struct MeshData
    {
        public Mesh mesh;

        //List contains every vertex of mesh that we are going to render
        public List<Vector3> vertices;

        //The UV list tells Unity how the texture is aligned on each polygon
        public List<Vector2> UVs;

        // The triangles tell Unity how to build each section of the mesh by joining vertices
        public List<int> triangles;

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

                Initialized = true;
            }
            else //Clears the lists if already initialized
            {
                vertices.Clear();
                UVs.Clear();
                triangles.Clear();
                mesh.Clear();

            }
        }

        public void UploadMesh(bool sharedVertices = false)
        {
            mesh.SetVertices(vertices); //Set mesh vertices to vertices created
            mesh.SetUVs(0,UVs);
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
