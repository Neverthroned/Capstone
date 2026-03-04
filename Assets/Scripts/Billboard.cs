using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Transform cam;

    void Start()
    {
        cam = Camera.main.transform;
    }

    void LateUpdate()
    {
        if (cam == null) return;

        transform.LookAt(cam.position);
        transform.Rotate(0, 180f, 0); // Flip because planes face backward
    }
}