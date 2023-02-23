using System.ComponentModel;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class SampleVid : MonoBehaviour
{


    [SerializeField]
    public GameObject m_pixel;
     [SerializeField] 
     public GameObject Screen;
    //  public GameObject Maker;
    static int m_width = 128; //실제 rgb영상 width
    static int m_height = 96; //실제 rgb영상 height
    GameObject[,] m_pixels = new GameObject[m_height, m_width];
    SpriteRenderer[,] m_sr = new SpriteRenderer[m_height, m_width];


    //Vector3 m_originPos = new Vector3;
    Vector3 m_originPos;
    Vector3 m_right = new Vector3(0.01f, 0, 0);
    Vector3 m_up = new Vector3(0, 0.01f, 0);

    public RenderTexture rentex;

    void Start()
    {        

        for (int i = 0; i < m_height; i++)
        {
            for (int j = 0; j < m_width; j++)
            {
                m_pixels[i, j] = Instantiate(m_pixel, Screen.transform.position + (m_right * (j-m_width/2)) + (m_up * (i-m_height/2)), Quaternion.identity);
                m_pixels[i, j].transform.parent = GameObject.Find("Screen").transform;
                m_sr[i, j] = m_pixels[i, j].GetComponent<SpriteRenderer>();
            }
        }
        tex = new Texture2D(m_width * 2, m_height, TextureFormat.RGB24, false);
        readTex = new Rect(0, 0, m_width * 2, m_height);
    }
    Texture2D tex;
    Rect readTex;
    Color[] pixelData;

    private void Update()
    {
        RenderTexture.active = rentex;
        tex.ReadPixels(readTex, 0, 0);

        pixelData = tex.GetPixels();

        for (int i = 0; i < m_height; i++)
        {
            for (int j = 0; j < m_width; j++)
            {
                m_sr[i, j].color = new Color(pixelData[2 * m_width * i + j + m_width].r, pixelData[2 * m_width * i + j + m_width].g, pixelData[2 * m_width * i + j + m_width].b);

                float m_depth = Map(pixelData[2 * m_width * i + j].r, 0, 1, 0, 4);
                Vector3 m_depthPos = new Vector3(m_pixels[i, j].transform.position.x, m_pixels[i, j].transform.position.y,Screen.transform.position.z + m_depth);
                m_pixels[i, j].transform.position = m_depthPos;
            }
        }

        RenderTexture.active = null;
    }


    public float Map(float x, float in_min, float in_max, float out_min, float out_max)
    {
        return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
    }
}

