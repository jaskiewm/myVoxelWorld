using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PolygonGenerator : MonoBehaviour
{
    //Creating lists for vertices, triangles, and UV

    // This first list contains every vertex of mesh that we are going to render
    public List<Vector3> newVertices = new List<Vector3>();

    // The triangles tell Unity how to build each section of the mesh
    // by joining vertices
    public List<int> newTriangles = new List<int>();

    //The UV list tells Unity how the texture is aligned on each polygon
    public List<Vector2> newUV = new List<Vector2>();

    //A mesh is made up of vertices, triangles, and UVs.
    //After made, it'll be saved in this mesh
    private Mesh mesh;

    private void Start()
    {
        //Using "mesh" to access the mesh filter
        mesh = GetComponent<MeshFilter>().mesh;

        float x = transform.position.x;
        float y = transform.position.y;
        float z = transform.position.z;

        //Creates 4 new vertices to make a rectangle
        //Which is dependent on the float values above
        newVertices.Add(new Vector3(x, y, z));
        newVertices.Add(new Vector3(x+1, y, z));
        newVertices.Add(new Vector3(x+1, y-1, z));
        newVertices.Add(new Vector3(x, y-1, z));


        //Defining triangle #1. It is adding list values from newVertices
        //Make sure it is clockwise as this defines your "output" (+Z vs -Z)
        //0 is x,y
        //1 is x+1,y
        //3 is x,y-1
        newTriangles.Add(0);
        newTriangles.Add(1);
        newTriangles.Add(3);
        newTriangles.Add(1);
        newTriangles.Add(2);
        newTriangles.Add(3);

        mesh.Clear(); //Clears existing mesh
        mesh.vertices = newVertices.ToArray(); //.ToArray(); //Set mesh vertices to vertices created
        mesh.triangles = newTriangles.ToArray(); //Set mesh triangle to triangles created
        mesh.Optimize();
        mesh.RecalculateNormals();
    }
}
