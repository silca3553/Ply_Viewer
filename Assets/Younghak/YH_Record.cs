using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.SceneManagement;
using VideoCreator;

[RequireComponent(typeof(AudioSource))]



public class YH_Record : MonoBehaviour
{
    public Transform CamTransform;
    public GameObject marker;
    //public GameObject CenterOfSpace;
    public GameObject YH_ARSpace;
    public GameObject recorded2Dvideo;
    // private GameObject spawnPoint;


    [Header("Recording Buttons")]
    public GameObject recordButton;
    public GameObject stopButton;
    public GameObject pickButton;


    Vector3 CamPosition;
    Vector3 CamRotation;

    [Header("MOV Recorder")]
    [SerializeField]
    private RenderTexture texture;

    [SerializeField]
    private AudioSource audioSource;

    private readonly long startTimeOffset = 6_000_000;

    private bool isRecording = false;
    private bool recordAudio = false;

    private string cachePath = "";
    private float startTime = 0;
    private long amountAudioFrame = 0;

    public int fps = 30;


    //------------------Code Starts----------------------//

    private void Start()
    {
        //SceneManager.LoadSceneAsync("Common", LoadSceneMode.Additive);

        stopButton.SetActive(false);
        pickButton.SetActive(false);

        Application.targetFrameRate = fps;
       

        audioSource.Stop();

        cachePath = "file://" + Application.temporaryCachePath + "/tmp.mov";
        Debug.Log($"cachePath: {cachePath}, {texture.width}x{texture.height}");
    }


    void Update()
    {
        if (!isRecording || !MediaCreator.IsRecording()) return;

        long time = (long)((Time.time - startTime) * 1_000_000) + startTimeOffset;

        Debug.Log($"write texture: {time}");

        MediaCreator.WriteVideo(texture, time);
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        WriteAudio(data, channels);

        for (int i = 0; i < data.Length; i++)
        {
            data[i] = 0;
        }
    }

    private void WriteAudio(float[] data, int channels)
    {
        if (!isRecording || !recordAudio || !MediaCreator.IsRecording()) return;

        long time = (amountAudioFrame * 1_000_000 / 48_000) + startTimeOffset;
        Debug.Log($"write audio: {time}");

        MediaCreator.WriteAudio(data, time);

        amountAudioFrame += data.Length;
    }

    void OnDestroy()
    {
        StopRec();
    }

    public void StartRecMovWithAudio()
    {
        recordButton.SetActive(false);
        stopButton.SetActive(true);
        Debug.Log("Clicked!");

        CamPosition = CamTransform.position;
        CamRotation = CamTransform.rotation.eulerAngles;

        marker.transform.position = new Vector3(CamPosition.x, CamPosition.y, CamPosition.z);
        marker.transform.eulerAngles = new Vector3(CamRotation.x, CamRotation.y, CamRotation.z);

        /*()
        //SceneManager.LoadSceneAsync("VideoCreator/Demo/Scenes/Common", LoadSceneMode.Additive);
        cameraMarker = Instantiate(marker, CamTransform.position, CamTransform.rotation);
        //cameraMarker.transform.SetParent(CenterOfSpace.transform, true);
        cameraMarker.transform.SetParent(YH_ARSpace.transform, true);
        //spawnPoint = cameraMarker.transform.GetChild(0).gameObject;
        */

        if (isRecording) return;

        var clip = Microphone.Start(null, true, 1, 48_000);
        audioSource.clip = clip;
        audioSource.loop = true;
        while (Microphone.GetPosition(null) < 0) { }

        MediaCreator.InitAsMovWithAudio(cachePath, "h264", texture.width, texture.height, 1, 48_000);
        MediaCreator.Start(startTimeOffset);

        startTime = Time.time;

        isRecording = true;
        recordAudio = true;
        amountAudioFrame = 0;

        audioSource.Play();
    }

    public void StartRecMovWithNoAudio()
    {
        //SceneManager.LoadSceneAsync("VideoCreator/Demo/Scenes/Common", LoadSceneMode.Additive);
        
        if (isRecording) return;

        MediaCreator.InitAsMovWithNoAudio(cachePath, "h264", texture.width, texture.height);
        MediaCreator.Start(startTimeOffset);

        startTime = Time.time;

        isRecording = true;
        recordAudio = false;
    }

    public void StopRec()
    {
        stopButton.SetActive(false);
        pickButton.SetActive(true);
        Debug.Log("Clicked!");

        if (!isRecording) return;

        audioSource.Stop();
        Microphone.End(null);

        MediaCreator.FinishSync();
        MediaSaver.SaveVideo(cachePath);

        isRecording = false;
    }

    public void PickVideo()
    {

        //YH_videoPlayer.isLooping = true;


        NativeGallery.Permission permission = NativeGallery.GetVideoFromGallery( ( path ) =>
        {
            Debug.Log( "Video path: " + path );
            if( path != null )
            {
                var VidScript = recorded2Dvideo.GetComponent<SampleVid>();
                var YH_videoPlayer = recorded2Dvideo.GetComponent<UnityEngine.Video.VideoPlayer>();
                YH_videoPlayer.url = "file://" + path;


                VidScript.enabled = true;
                YH_videoPlayer.Play();
            }
        }, "Select a video" );

        Debug.Log( "Permission result: " + permission );
    }


    // 하단은 녹화된 영상을 불러오도록 갤러리에 접근한 뒤,
    // 2D 영상을 3D로 뽑아내어 특정 위치에 띄우는는 코드입니다.
    
    
    public void RecordTest()
    {
        //cameraMarker = Instantiate(marker, CamTransform.position, CamTransform.rotation);
        //cameraMarker.transform.SetParent(CenterOfSpace.transform, true);
        //cameraMarker.transform.SetParent(YH_ARSpace.transform, true);
        //spawnPoint = cameraMarker.transform.GetChild(0).gameObject;
    }

    public void StopTest()
    {
        //playingObject = Instantiate(recorded3DVideo);
        //playingObject.transform.SetParent(YH_ARSpace.transform, true);
        Debug.Log("Clicked!");
    }
    



}
