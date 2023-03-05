using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public record Ray
{
    public Vector2 dir;
    public Vector2 og;
    public float t;

    public Color color;

    public Ray(Vector2 dir, Vector2 og, float t, Color color) {
        this.dir = dir;
        this.og = og;
        this.t = t;
        this.color = color;
    }
}
