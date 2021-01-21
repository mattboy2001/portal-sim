using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{

    public float nearClipPlane = 0.0001f;
    void Start()
    {

        GetComponent<Camera>().nearClipPlane = nearClipPlane;


        
    }
}
