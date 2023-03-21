using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PtUtils
{
    public static Vector3 toVec3(Vector2 v)
    {
        return new Vector3(v.x, 0, v.y);
    }

    public static Color multColors(Color a, Color b) {
        return new Color(a.r * b.r, a.g * b.g, a.b * b.b);
    }

    public static Color multScalarColor(float a, Color color) {
        return new Color(a*color.r, a*color.g, a*color.b);
    }

    public static Color addColors(Color a, Color b) {
        return new Color(a.r + b.r, a.g + b.g, a.b + b.b);   
    }

}
