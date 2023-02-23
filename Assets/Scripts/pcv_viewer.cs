using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class pcv_viewer : MonoBehaviour
{
    public int fps = 60;
    public RenderTexture rentex;

    public Material point_metarial;
    private GameObject plyObject;

    public GameObject screen;
    public GameObject cube;

    //private MeshFilter meshFilter;
    //private MeshRenderer meshRenderer;

    // Start is called before the first frame update

    static int m_width = 96; //RGB영상 width
    static int m_height = 128; //RGB영상 height

    static int size = m_width * m_height;


    Vector3 pos;
    Quaternion rot;

    void Start()
    {
        Application.targetFrameRate = fps;

        plyObject = new GameObject();

        pos = screen.transform.position;
        rot = screen.transform.rotation;

        Instantiate(plyObject, pos, rot);
        Instantiate(cube, pos, rot);

        plyObject.AddComponent<MeshFilter>();

        var meshRender = plyObject.AddComponent<MeshRenderer>();
        meshRender.sharedMaterial =  point_metarial;
    }


    // Update is called once per frame
    void Update()
    {
        ImportPly();
    }


    class DataBody
    {
        public List<Vector3> vertices;
        public List<Color32> colors;

        public DataBody(int vertexCount)
        {
            vertices = new List<Vector3>(vertexCount);
            colors = new List<Color32>(vertexCount);
        }

        public void AddPoint(
            float x, float y, float z,
            byte r, byte g, byte b, byte a
        )

        {
            vertices.Add(new Vector3(x, y, z));
            colors.Add(new Color32(r, g, b, a));
        }
    }


    public void ImportPly()
    {
        // Mesh container
        // Create a prefab with MeshFilter/MeshRenderer.
        var mesh = ImportAsMesh();

        var meshFilter = plyObject.GetComponent<MeshFilter>();
        meshFilter.sharedMesh = mesh;
        /*
        context.AddObjectToAsset("prefab", gameObject);
        if (mesh != null) context.AddObjectToAsset("mesh", mesh);

        context.SetMainObject(gameObject);
        */
    }

    Mesh ImportAsMesh()
    {
        Texture2D getTex = GetRTPixels(rentex);

        Color32[] pixelData = getTex.GetPixels32();

        var data = new DataBody(size);

        float z = 0;

        int rectSize = 2 * size;

        byte r=0,g=0,b=0,a=0;

        for (int i = m_width; i < rectSize; i += 2 * m_width)
        {
            for (int j = 0; j < m_width; j++)
            {
                r = pixelData[i + j].r;
                g = pixelData[i + j].g;
                b = pixelData[i + j].b;
                a = pixelData[i + j].a;

                z = Map(pixelData[i + j - m_width].r, 0, 1, 0, 4);

                data.AddPoint( (i / (2 * m_width)) * 0.01f , j * 0.01f , z, r, g, b, a);
            }
        }

        Debug.Log("complete");
        Debug.Log($"{r} {g} {b} {a}");

        var mesh = new Mesh();
        mesh.name = "point_mesh";

        mesh.indexFormat = size > 65535 ?
                IndexFormat.UInt32 : IndexFormat.UInt16;

        mesh.SetVertices(data.vertices);
        mesh.SetColors(data.colors);

        mesh.SetIndices(
                Enumerable.Range(0, size).ToArray(),
                MeshTopology.Points, 0
            );

        mesh.UploadMeshData(true);

        return mesh;
    }

    public float Map(float x, float in_min, float in_max, float out_min, float out_max)
    {
        return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
    }

    static public Texture2D GetRTPixels(RenderTexture rt)
    {
        // Remember currently active render texture
        RenderTexture currentActiveRT = RenderTexture.active;

        // Set the supplied RenderTexture as the active one
        RenderTexture.active = rt;

        // Create a new Texture2D and read the RenderTexture image into it
        Texture2D tex = new Texture2D(rt.width, rt.height);
        tex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);

        // Restore previously active render texture
        RenderTexture.active = currentActiveRT;
        return tex;
    }
}