using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

//This is the container that will store our voxels
//This replaces the Dictionary we used before to store the voxels
//This reduces CPU processing time 

[System.Serializable]

public class IndexedArray<T> where T : struct
{
    private bool initialized = false;

    [SerializeField]
    [HideInInspector]
    public T[] array; //Using flat array as we can't pass a dictionary to a shader

    [SerializeField]
    [HideInInspector]
    private Vector2Int size;

    public IndexedArray()
    {
        Create(WorldManager.WorldSettings.containerSize, WorldManager.WorldSettings.maxHeight);
    }

    public IndexedArray(int sizeX, int sizeY)
    {
        Create(sizeX, sizeY);
    }

    private void Create(int sizeX, int sizeY)
    {
        size = new Vector2Int(sizeX + 3, sizeY + 1);
        array = new T[Count]; //Array of size containerSize * maxHeight
        initialized = true;
    }

    int IndexFromCoord(Vector3 idx) //Gets an integer coordinate from a vector 3
    {
        return Mathf.RoundToInt(idx.x) + (Mathf.RoundToInt(idx.y) * size.x) + (Mathf.RoundToInt(idx.z) * size.x * size.y);
    }

    public void Clear()
    {
        if (!initialized)
            return;
        
        for(int x = 0; x<size.x; x++)
            for (int y = 0; y < size.y; y++)
                for (int z = 0; z < size.x; z++)
                    array[(y * size.x) + (z * size.x * size.y)] = default(T); //Sets every block in array to default which is clear (ID = 0, air)

    }

    public int Count //Size of array
    {
        get { return size.x * size.y * size.x; }
    }

    public T[] GetData
    {
        get
        { return array; }
    }

    public T this[Vector3 coord] //accessor
    {
        get
        {
            if (coord.x < 0 || coord.y < 0 || coord.z < 0 || 
                coord.x > size.x || coord.y > size.y || coord.z > size.x)
            {
                Debug.LogError($"Coordinates out of bounds! {coord}");
                return default(T);
            }
            return array[IndexFromCoord(coord)];
        }
        set
        {
            if (coord.x < 0 || coord.y < 0 || coord.z < 0 ||
                coord.x >= size.x || coord.y >= size.y || coord.z >= size.x)
            {
                Debug.LogError($"Coordinates out of bounds! {coord}");
                return;
            }
            array[IndexFromCoord(coord)] = value;
        }
    }
}
