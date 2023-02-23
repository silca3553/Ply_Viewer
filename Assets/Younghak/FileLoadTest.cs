using System.Diagnostics;
using System.Numerics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;
using Debug = UnityEngine.Debug;


public class FileLoadTest : MonoBehaviour
{
    public Transform TakingPosition;
    public GameObject marker;

    public GameObject recorded2Dvideo;
    


    Vector3 CamPosition;
    Vector3 CamRotation;

    private void Start() {

        //recorded2Dvideo.SetActive(false);    
    }

    public void makeMarker()
    {
        //cameraMarker = Instantiate(marker, TakingPosition.position, TakingPosition.rotation);
        CamPosition = TakingPosition.position;
        CamRotation = TakingPosition.rotation.eulerAngles;

        marker.transform.position = new Vector3(CamPosition.x, CamPosition.y, CamPosition.z);
        marker.transform.eulerAngles = new Vector3(CamRotation.x, CamRotation.y, CamRotation.z);
        
        //recorded2Dvideo.SetActive(true);
        
        //videoClip.URL = "";
        //cameraMarker.transform.SetParent(CenterOfSpace.transform, true);
        //cameraMarker.transform.SetParent(YH_ARSpace.transform, true);
        //spawnPoint = cameraMarker.transform.GetChild(0).gameObject;

        
    }

    public void GetVideo()
    {
        var YH_videoPlayer = recorded2Dvideo.GetComponent<UnityEngine.Video.VideoPlayer>();
        YH_videoPlayer.isLooping = true;

        YH_videoPlayer.url = "file:///Users/younghaklee/Downloads/tmp.MOV";
        
    }

    public void PlayRecordedVideo()
    {
        var YH_videoPlayer = recorded2Dvideo.GetComponent<UnityEngine.Video.VideoPlayer>();
        YH_videoPlayer.Play();
    }

    public void PickAndLoadVideo()
    {
        var VidScript = recorded2Dvideo.GetComponent<SampleVid>();
        NativeGallery.Permission permission = NativeGallery.GetVideoFromGallery( ( path ) =>
        {
            Debug.Log( "Video path: " + path );
            Debug.Log(path);
            if( path != null )
            {
                var YH_videoPlayer = recorded2Dvideo.GetComponent<UnityEngine.Video.VideoPlayer>();
                YH_videoPlayer.isLooping = true;

                // 아래 코드로 해보고 안되면
                YH_videoPlayer.url = "file://" + path;

                // 아래 코드로 대체

                // YH_videoPlayer.url = path;

                VidScript.enabled = true;
                YH_videoPlayer.Play();
                //recorded2Dvideo.Video
                //Handheld.PlayFullScreenMovie( "file://" + path );
            }
        }, "Select a video" );

        Debug.Log( "Permission result: " + permission );

    }
    
    public void LoadVideo()
    {

    }
    // Update is called once per frame
    void Update()
    {
        
    }


    
}
