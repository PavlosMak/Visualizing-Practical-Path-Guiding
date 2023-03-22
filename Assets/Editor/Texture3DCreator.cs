using System.IO;
using UnityEditor;
using UnityEngine;

public class Texture3DCreator : MonoBehaviour {
    
    [MenuItem("Create Radiance Texture/Create Radiance from file")]
    static void CreateRadianceTexture() {
        // Configure the texture
        int size = 80;
        TextureFormat format = TextureFormat.RGBA32;
        TextureWrapMode wrapMode = TextureWrapMode.Clamp;

        // Create the texture and apply the configuration
        Texture3D texture = new Texture3D(size, size, size, format, false);
        texture.wrapMode = wrapMode;

        // Create a 3-dimensional array to store color data
        Color[] colors = new Color[size * size * size];

        float inverseResolution = 1.0f / (size - 1.0f);
        for (int z = 0; z < size; z++)
        {
            int zOffset = z * size * size;
            for (int y = 0; y < size; y++)
            {
                int yOffset = y * size;
                for (int x = 0; x < size; x++)
                {
                    colors[x + yOffset + zOffset] = new Color(0f, 0f, 0f, 0.0f);
                }
            }
        }

        float scale = -1.0f;

        string path = "Assets/radiance.txt";
        FileInfo radianceFile = new FileInfo(path);
        StreamReader reader = radianceFile.OpenText();
        string fileLine = reader.ReadLine(); 
        while(fileLine != null) {
            string[] parsed = fileLine.Split(" : ");
            string[] coords = parsed[0].Split(' ');
            string[] rgba = parsed[1].Substring(5, parsed[1].Length - 6).Split(", ");
            //Create color and point
            Vector3 colorVector = new Vector3(float.Parse(rgba[0]), float.Parse(rgba[1]), float.Parse(rgba[2]));
            scale = Mathf.Max(scale, colorVector.magnitude);
            //Read next line
            fileLine = reader.ReadLine();
        }
        // scale = 1.0f;
        Debug.Log("scale: " + 1.0f / scale);
        // scale = 1.0f;
        //Write colors
        reader = radianceFile.OpenText();
        fileLine = reader.ReadLine();
        while (fileLine != null) {
            //Parse the line into coordinates and color
            string[] parsed = fileLine.Split(" : ");
            string[] coords = parsed[0].Split(' ');
            string[] rgba = parsed[1].Substring(5, parsed[1].Length - 6).Split(", ");
            //Create color and point
            Color color = new Color(float.Parse(rgba[0]), float.Parse(rgba[1]), float.Parse(rgba[2]), 1);

            Vector3 parsedPoint = new Vector3(
                float.Parse(coords[0]),
                float.Parse(coords[1]),
                float.Parse(coords[2])
            );

            Vector3 textureCoordinate = WorldToTexture(parsedPoint);

            Vector3Int indexVector = GetTextureIndex(textureCoordinate, size);
            int flatIx = GetFlatIndex(indexVector, size);

            if (flatIx > colors.Length) {
                Debug.Log(parsedPoint);
                Debug.Log(textureCoordinate);
            }

            if (color == Color.magenta) {
                colors[flatIx] = new Color(0, 0, 0, -1);
            }
            else {
                colors[flatIx] = PtUtils.multScalarColor(scale, color);
            }

            //Read next line
            fileLine = reader.ReadLine();
        }


        // Copy the color values to the texture
        texture.SetPixels(colors);

        // Apply the changes to the texture and upload the updated texture to the GPU
        texture.Apply();

        // Save the texture to your Unity Project
        AssetDatabase.CreateAsset(texture, "Assets/radianceTexture.asset");
    }

    private static int GetFlatIndex(Vector3Int index, int size) {
        int zOffset = index.z * size * size;
        int yOffset = index.y * size;
        return index.x + yOffset + zOffset;
    }

    private static Vector3Int GetTextureIndex(Vector3 point, int resolution) {
        float step = 1.0f / resolution;
        return new Vector3Int(
            (int)Mathf.Floor(point.x / step),
            (int)Mathf.Floor(point.y / step),
            (int)Mathf.Floor(point.z / step)
        );
    }

    private static Vector3 WorldToTexture(Vector3 point) {
        return new Vector3(
            Mathf.Clamp((point.x + 3.0f) / 11.0f, 0, 0.99f),
            Mathf.Clamp((point.y + 7.0f) / 11.0f, 0, 0.99f),
            Mathf.Clamp(point.z / 360.0f, 0, 0.99f)
        );
    }
}