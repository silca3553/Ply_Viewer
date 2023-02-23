using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class CameraPositionTest : MonoBehaviour
{
    public Transform ARCam;
    //public TextMeshProUGUI posText;
    //public TextMeshProUGUI rotText;
    public TextMeshProUGUI WorldPosText;
    public TextMeshProUGUI WorldRotText;
    public TextMeshProUGUI StartPositionText;
    public TextMeshProUGUI StartRotationText;
    
    Vector3 WorldCamPosition;
    Vector3 WorldCamRotation;

    private void Update()
    {

        
        WorldCamPosition = ARCam.position;
        WorldCamRotation = ARCam.rotation.eulerAngles;
        //posText.text = "current Position: \n" + localCamPosition;
        //rotText.text = "current Rotation : \n" + localCamRotation;
        WorldPosText.text = "current Position: \n" + WorldCamPosition;
        WorldRotText.text = "current Rotation : \n" + WorldCamRotation;

    }
    public void StartedTransform()
    {
        StartPositionText.text = "Start Position  : \n" + WorldCamPosition;
        StartRotationText.text = "Start Rotation : \n" + WorldCamRotation;   
        Debug.Log(WorldCamPosition);
        Debug.Log(WorldCamRotation);

    }

}
