using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

public class Texture3DCreator : MonoBehaviour
{

    [MenuItem("Create Radiance Texture/Test")]
    static void CreateTexture3D()
    {
        // Configure the texture
        int size = 32;
        TextureFormat format = TextureFormat.RGBA32;
        TextureWrapMode wrapMode =  TextureWrapMode.Clamp;

        // Create the texture and apply the configuration
        Texture3D texture = new Texture3D(size, size, size, format, false);
        texture.wrapMode = wrapMode;

        // Create a 3-dimensional array to store color data
        Color[] colors = new Color[size * size * size];

        // Populate the array so that the x, y, and z values of the texture will map to red, blue, and green colors
        float inverseResolution = 1.0f / (size - 1.0f);
        for (int z = 0; z < size; z++)
        {
            int zOffset = z * size * size;
            for (int y = 0; y < size; y++)
            {
                int yOffset = y * size;
                for (int x = 0; x < size; x++)
                {
                    colors[x + yOffset + zOffset] = new Color(x * inverseResolution,
                        y * inverseResolution, z * inverseResolution, 1.0f);
                }
            }
        }

        // Copy the color values to the texture
        texture.SetPixels(colors);

        // Apply the changes to the texture and upload the updated texture to the GPU
        texture.Apply();        

        // Save the texture to your Unity Project
        AssetDatabase.CreateAsset(texture, "Assets/Example3DTexture.asset");
    }

    [MenuItem("Create Radiance Texture/Create Radiance from file")]
    static void CreateRadianceTexture() {
        // Configure the texture
        int size = 100;
        TextureFormat format = TextureFormat.RGBA32;
        TextureWrapMode wrapMode =  TextureWrapMode.Clamp;

        // Create the texture and apply the configuration
        Texture3D texture = new Texture3D(size, size, size, format, false);
        texture.wrapMode = wrapMode;

        // Create a 3-dimensional array to store color data
        Color[] colors = new Color[size * size * size];

        // Populate the array so that the x, y, and z values of the texture will map to red, blue, and green colors
        // float inverseResolution = 1.0f / (size - 1.0f);
        // for (int z = 0; z < size; z++)
        // {
        //     int zOffset = z * size * size;
        //     for (int y = 0; y < size; y++)
        //     {
        //         int yOffset = y * size;
        //         for (int x = 0; x < size; x++)
        //         {
        //             colors[x + yOffset + zOffset] = new Color(x * inverseResolution,
        //                 y*inverseResolution, z*inverseResolution, 1.0f);
        //         }
        //     }
        // }


        string path = "Assets/radiance.txt";
        FileInfo radianceFile = new FileInfo(path);
        StreamReader reader = radianceFile.OpenText();
        
        string fileLine = reader.ReadLine();
        while(fileLine != null) {
            //Parse the line into coordinates and color
            string[] parsed = fileLine.Split(" : ");
            string[] coords = parsed[0].Split(' '); 
            string[] rgba = parsed[1].Substring(5, parsed[1].Length - 6).Split(", ");
            //Create color and point
            Color color = new Color(float.Parse(rgba[0]), float.Parse(rgba[1]), float.Parse(rgba[2]), float.Parse(rgba[3]));
            Vector3 textureCoordinate =  WorldToTexture(
                new Vector3(
                    float.Parse(coords[0]), 
                    float.Parse(coords[1]), 
                    float.Parse(coords[2])
                ));
            //
            Vector3Int indexVector = GetTextureIndex(textureCoordinate, size);
            int flatIx = GetFlatIndex(indexVector,size); 
            colors[flatIx] = color;
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
        //     int zOffset = z * size * size;
        //     for (int y = 0; y < size; y++)
        //     {
        //         int yOffset = y * size;
        //         for (int x = 0; x < size; x++)
        //         {
        //             colors[x + yOffset + zOffset] = new Color(x * inverseResolution,
        //                 y*inverseResolution, z*inverseResolution, 1.0f);
        //         }
        int zOffset = index.z*size*size;
        int yOffset = index.y*size;
        return index.x + yOffset + zOffset;
    }

    private static Vector3Int GetTextureIndex(Vector3 point, int resolution) {
        float step = 1.0f / resolution;
        return new Vector3Int(
            (int) Mathf.Floor(point.x / step),
            (int) Mathf.Floor(point.y / step),      
            (int) Mathf.Floor(point.z / step)      
        );
    }

    private static Vector3 WorldToTexture(Vector3 point) {
        return new Vector3(
            (point.x + 3.0f) / 11.0f,
            (point.y + 7.0f) / 11.0f,
            (point.z + 90.0f) / 180.0f
        );
    }

}
