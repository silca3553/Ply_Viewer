using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Firebase;
using Firebase.Storage;
using Firebase.Extensions;
using System.IO;

public class MOV_FirebaseInit : MonoBehaviour
{
    FirebaseStorage storage;
    /// <summary> 서버로부터 확인된 마지막 파일, 이 숫자 미만으로 사용 </summary>
    public int AvailableMaxIndex = 0;

    string basePath;

    string path;

    void Start()
    {
        basePath = Application.persistentDataPath + '/';

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                // Create and hold a reference to your FirebaseApp,
                // where app is a Firebase.FirebaseApp property of your application class.
                var app = FirebaseApp.DefaultInstance;
                // Set a flag here to indicate whether Firebase is ready to use by your app.
                storage = FirebaseStorage.DefaultInstance;
                GetAvailableList();
            }
            else
            {
                UnityEngine.Debug.LogError(System.String.Format(
                  "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                // Firebase Unity SDK is not safe to use here.
            }
        });
    }
    void GetAvailableList()
    {
        var reference = storage.RootReference.Child($"{AvailableMaxIndex.ToString("000000")}.MOV");

        
        reference.GetMetadataAsync().ContinueWith(task =>
        {
            AvailableMaxIndex++;
            if (task.IsCompletedSuccessfully)
                GetAvailableList();
            else Debug.Log($"리스트 갯수 확인: {AvailableMaxIndex}");
        });
    }
    /// <summary> 마지막에 촬영된 카메라 Transform 정보를 순서에 맞게 업로드처리 </summary>
    public void StartUpload(string target_path)
    {
       

        var reference = storage.RootReference.Child($"{AvailableMaxIndex.ToString("000000")}.MOV");
        NativeGallery.Permission permission = NativeGallery.GetVideoFromGallery( ( path ) =>
        {
            
            
            Debug.Log( "Video path: " + path );
            if( path != null )
            {
                target_path = "file://" + path;   //"file://" +basePath + "tmp.MOV"; // 경로            
            }
            
        }, "Select a video" );
        Debug.Log( "Permission result: " + permission );



        reference.PutFileAsync(target_path).ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                AvailableMaxIndex++;
                Debug.Log("업로드 성공: ");
            }
            else Debug.LogError($"업로드 실패: {task.IsFaulted}");
        });
    }

    

    /// <summary> 번호로 구분된 파일을 다운받기 </summary>
    public void StartDownload(string download_path)
    {
        int index =0;
        var reference = storage.RootReference.Child($"{index.ToString("000000")}.MOV");
        download_path = "file://" + path + "tmp.MOV"; //basePath + "CameraInfo_receive.json"; // 경로
        reference.GetFileAsync(download_path).ContinueWith(task =>
        {
            if (task.IsCompleted)
                Debug.Log("다운로드 성공: ");
            else Debug.LogError($"다운로드 실패: {task.IsFaulted}");
        });
    }
}
