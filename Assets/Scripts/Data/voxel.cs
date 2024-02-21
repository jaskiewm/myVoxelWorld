using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct voxel
{
    public byte ID; //Byte for space savings

    public bool isSolid
    {
        get
        {
            return ID != 0; //If ID == 0, the block is air
        }
    }
}
