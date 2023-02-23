using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class CameraData
{
    public Vector3[] CameraPosition; //ARCamera�� position ����
    public Quaternion[] CameraRotation; //ARCamera�� rotation ����
    public float[] TimeStamp;
}

public class Json_Recorder : MonoBehaviour
{
    // Start is called before the first frame update

    public Transform CamTransform;

    public GameObject StopButton;

    List<Vector3> CamPos = new List<Vector3>();
    List<Quaternion> CamRot = new List<Quaternion>();
    List<float> TimeRecord = new List<float>();

    bool isRecording;
    float timer;
    string basePath;

    void Start()
    {
        isRecording = false;
        timer = 0;
        StopButton.SetActive(false);
        basePath = Application.persistentDataPath + '/';
    }

    // Update is called once per frame
    void Update()
    {
        if (isRecording)
        {
            Record_Camera();
            Debug.Log($"Timer: {timer}");
        }
    }

    public void Record_Start()
    {
        isRecording = true;
        timer = 0;
        CamPos.Clear();
        CamRot.Clear();
        TimeRecord.Clear();
        StopButton.SetActive(true);
    }

    void Record_Camera()
    {
        timer += Time.deltaTime;
        TimeRecord.Add(timer);
        CamPos.Add(CamTransform.position);
        CamRot.Add(CamTransform.rotation);

    }

    public void Record_Stop()
    {
        isRecording = false;

        string filePath = basePath + "CameraInfo.json";

        SaveData(filePath);

        LoadData(filePath);

        StopButton.SetActive(false);
    }

    void SaveData(string filePath)
    {
        CameraData data = new CameraData() { CameraPosition = CamPos.ToArray(), CameraRotation = CamRot.ToArray(), TimeStamp = TimeRecord.ToArray() };
        string jsonData = JsonUtility.ToJson(data); //json �������� ��ȯ

        if (System.IO.File.Exists(filePath))
            System.IO.File.Delete(filePath);

        File.WriteAllText(filePath, jsonData); //filePath ��ġ�� jsonData�� ����
    }

    void LoadData(string filePath)
    {
        string data = File.ReadAllText(filePath);
        CameraData loadData = JsonUtility.FromJson<CameraData>(data); //CameraData �������� �״�� �о�� �� ����
        
        Debug.Log("!");
        Debug.Log(loadData.CameraRotation[20]);
        Debug.Log("!");
    }
}