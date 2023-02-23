using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstLocalization : MonoBehaviour
{


    
    public GameObject recordButton;

    private void Start() {
        recordButton.SetActive(false);
    }
    public void RevealRecordButton()
    {
        recordButton.SetActive(true);
    }
}
