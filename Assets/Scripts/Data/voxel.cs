using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct voxel
{
    //Was byte for space savings, but we can't pass byte to compute shader
    //Now we have 4 bytes instead of 1 :(
    public int ID; 

    public bool isSolid
    {
        get
        {
            return ID != 0; //If ID == 0, the block is air
        }
    }
}
