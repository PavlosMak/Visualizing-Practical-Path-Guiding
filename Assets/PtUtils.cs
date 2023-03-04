using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PtUtils
{
    public static Vector3 toVec3(Vector2 v)
    {
        return new Vector3(v.x, 0, v.y);
    }
}
