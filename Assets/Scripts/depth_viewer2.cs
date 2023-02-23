using System.Collections;
using System.Collections.Generic;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.UI;
using UnityEngine;

public class depth_viewer2 : MonoBehaviour
{

    const string k_MaxDistanceName = "_MaxDistance";
    const string k_DisplayRotationPerFrameName = "_DisplayRotationPerFrame";
    const float k_DefaultTextureAspectRadio = 1.0f;
    static readonly int k_MaxDistanceId = Shader.PropertyToID(k_MaxDistanceName);
    static readonly int k_DisplayRotationPerFrameId = Shader.PropertyToID(k_DisplayRotationPerFrameName);
    ScreenOrientation m_CurrentScreenOrientation;
    float m_TextureAspectRatio = k_DefaultTextureAspectRadio;
    Matrix4x4 m_DisplayRotationMatrix = Matrix4x4.identity; //Depth ������ ȭ�� ���⿡ �°� ����

    public AROcclusionManager m_OcclusionManager; //Depth ������ �ޱ� ����
    public Material depth_material; //�޾ƿ� Depth ������ ������� �����ϴ� material ( unity�� �����ϴ� arfoundation sample�� shader�� �Ϻ� ����)

    public RenderTexture depthRender; //Depth ������ ����
    public RenderTexture rgbRender; //Rgb ������ ����
    public RenderTexture mergeRender; //depthRender�� rgbRender�� video_canvas�� ������ ������� �� ȭ���� ����

    public ARCameraManager m_CameraManager; //RGB ������ �ޱ� ����

    public float maxDistance = 4; //depth�� �ν��Ϸ��� �ִ� ���� (������ ���� ����, �Ÿ��� ���� ���� ���̰� Ŀ�� )

    public RawImage rgb_RawImage; //rgbRender ȭ���� video_canvas�� ��� 
    public RawImage depth_RawImage; //depthRender ȭ���� video_canvas�� ���

    private Texture2D envDepth;

    void OnEnable()
    {
        // Subscribe to the camera frame received event, and initialize the display rotation matrix.
        Debug.Assert(m_CameraManager != null, "no camera manager");
        m_CameraManager.frameReceived += OnCameraFrameEventReceived;
        m_DisplayRotationMatrix = Matrix4x4.identity;

        // When enabled, get the current screen orientation, and update the raw image UI.
        m_CurrentScreenOrientation = Screen.orientation;
    }

    void OnDisable()
    {
        // Unsubscribe to the camera frame received event, and initialize the display rotation matrix.
        Debug.Assert(m_CameraManager != null, "no camera manager");
        m_CameraManager.frameReceived -= OnCameraFrameEventReceived;
        m_DisplayRotationMatrix = Matrix4x4.identity;
    }

    // Start is called before the first frame update
    void Start()
    {
        envDepth = m_OcclusionManager.environmentDepthTexture;
        UpdateRenderTexture();
        depth_material.SetFloat(k_MaxDistanceId, maxDistance);
        depth_material.SetMatrix(k_DisplayRotationPerFrameId, m_DisplayRotationMatrix);
    }

    // Update is called once per frame
    void Update()
    {
        envDepth = m_OcclusionManager.environmentDepthTexture;

        // Get the aspect ratio for the current texture.
        float textureAspectRatio = (envDepth == null) ? 1.0f : ( (float)envDepth.width / (float)envDepth.height );

        // If the raw image needs to be updated because of a device orientation change or because of a texture
        // aspect ratio difference, then update the raw image with the new values.
        if ((m_CurrentScreenOrientation != Screen.orientation) //���� �� ȭ���� ȸ���ߴٸ�
            || !Mathf.Approximately(m_TextureAspectRatio, textureAspectRatio))
        {
            m_CurrentScreenOrientation = Screen.orientation;
            m_TextureAspectRatio = textureAspectRatio;
            UpdateRenderTexture();
        }

        Graphics.Blit(envDepth, depthRender, depth_material); //envDepth ������ depth_material�� ���� ��, depthRender�� ���� 
    }

    void OnCameraFrameEventReceived(ARCameraFrameEventArgs cameraFrameEventArgs)
    {
            // Copy the display rotation matrix from the camera.
            Matrix4x4 cameraMatrix = cameraFrameEventArgs.displayMatrix ?? Matrix4x4.identity;

            Vector2 affineBasisX = new Vector2(1.0f, 0.0f);
            Vector2 affineBasisY = new Vector2(0.0f, 1.0f);
            Vector2 affineTranslation = new Vector2(0.0f, 0.0f);
#if UNITY_IOS
                affineBasisX = new Vector2(cameraMatrix[0, 0], cameraMatrix[1, 0]);
                affineBasisY = new Vector2(cameraMatrix[0, 1], cameraMatrix[1, 1]);
                affineTranslation = new Vector2(cameraMatrix[2, 0], cameraMatrix[2, 1]);
#endif // UNITY_IOS
#if UNITY_ANDROID
                affineBasisX = new Vector2(cameraMatrix[0, 0], cameraMatrix[0, 1]);
                affineBasisY = new Vector2(cameraMatrix[1, 0], cameraMatrix[1, 1]);
                affineTranslation = new Vector2(cameraMatrix[0, 2], cameraMatrix[1, 2]);
#endif // UNITY_ANDROID

            // The camera display matrix includes scaling and offsets to fit the aspect ratio of the device. In most
            // cases, the camera display matrix should be used directly without modification when applying depth to
            // the scene because that will line up the depth image with the camera image. However, for this demo,
            // we want to show the full depth image as a picture-in-picture, so we remove these scaling and offset
            // factors while preserving the orientation.
            affineBasisX = affineBasisX.normalized;
            affineBasisY = affineBasisY.normalized;
            m_DisplayRotationMatrix = Matrix4x4.identity;
            m_DisplayRotationMatrix[0, 0] = affineBasisX.x;
            m_DisplayRotationMatrix[0, 1] = affineBasisY.x;
            m_DisplayRotationMatrix[1, 0] = affineBasisX.y;
            m_DisplayRotationMatrix[1, 1] = affineBasisY.y;
            m_DisplayRotationMatrix[2, 0] = Mathf.Round(affineTranslation.x);
            m_DisplayRotationMatrix[2, 1] = Mathf.Round(affineTranslation.y);

#if UNITY_ANDROID
                m_DisplayRotationMatrix = k_AndroidFlipYMatrix * m_DisplayRotationMatrix;
#endif // UNITY_ANDROID

            // Set the matrix to the raw image material.
            depth_material.SetMatrix(k_DisplayRotationPerFrameId, m_DisplayRotationMatrix);
    }



    void UpdateRenderTexture() //renderTexture���� ȭ�� ������ �°� ����. test���� �� textrue �� �ػ󵵸� 96 X 128�� ��Ƽ� ���� 
    {
        //occlusion_manager�� fastest�� ������, �ػ󵵰� 256X196���� ����. Best�� �����ϸ� 2560X1960 ����
        int minDimension = 192; //����
        int maxDimension = 256; //����

        Vector2 renderSize; //depth,rgbRender �ػ� ����
        Vector2 mergeRenderSize; //merger_render�� �ػ� ����, depth,rgbRender�� ������ ���� ĵ���� ȭ���� �����ϴ� ���̹Ƿ� renderSize�� ���� ���� 2��.

        switch (m_CurrentScreenOrientation)
        {
            case ScreenOrientation.LandscapeRight: //��ȭ���� ���ζ��
            case ScreenOrientation.LandscapeLeft:
                renderSize = new Vector2(maxDimension/2, minDimension/2);
                mergeRenderSize = new Vector2(maxDimension, minDimension/2);
                break;
            case ScreenOrientation.PortraitUpsideDown: //��ȭ���� ���ζ��
            case ScreenOrientation.Portrait:
            default:
                renderSize = new Vector2(minDimension/2, maxDimension/2);
                mergeRenderSize = new Vector2(minDimension, maxDimension/2);
                break;
        }

        // Update the render texture size]
        
        mergeRender.Release();
        mergeRender.width = (int)mergeRenderSize.x;
        mergeRender.height = (int)mergeRenderSize.y;

        depthRender.Release();
        depthRender.width = (int)renderSize.x;
        depthRender.height = (int)renderSize.y;


        //rgbRender.Release();
        //rgbRender.width = (int)renderSize.x;
        //rgbRender.height = (int)renderSize.y;

        rgb_RawImage.rectTransform.sizeDelta = renderSize;
        depth_RawImage.rectTransform.sizeDelta = renderSize;
          
        depth_material.SetMatrix(k_DisplayRotationPerFrameId, m_DisplayRotationMatrix);

    }


}




