using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class DatamoshTrigger : MonoBehaviour
{
    private DatamoshPass datamoshPass;

    void Start()
    {
        // Find the Custom Pass Volume in the scene
        CustomPassVolume volume = Object.FindAnyObjectByType<CustomPassVolume>();

        if (volume != null)
        {
            // Get the specific pass from the Volume's list of custom passes
            datamoshPass = volume.customPasses.Find(p => p is DatamoshPass) as DatamoshPass;
        }

        if (datamoshPass == null)
        {
            Debug.LogError("DatamoshPass not found in a CustomPassVolume! The effect will not trigger.");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            if (datamoshPass != null)
            {
                datamoshPass.TakeSnapshot();

                Debug.Log("Key pressed, snapshot triggered!");
            }
        }
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            if (datamoshPass != null)
            {
                datamoshPass.EndEffect();
            }
        }
    }
}