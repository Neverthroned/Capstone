using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(MeshFilter))]
public class DrawNormals : MonoBehaviour
{
    public float length = 0.2f;

    void OnDrawGizmos()
    {
        MeshFilter mf = GetComponent<MeshFilter>();
        if (mf == null || mf.sharedMesh == null) return;

        Gizmos.color = Color.green;
        Vector3[] verts = mf.sharedMesh.vertices;
        Vector3[] normals = mf.sharedMesh.normals;

        for (int i = 0; i < verts.Length; i++)
        {
            Vector3 worldPos = transform.TransformPoint(verts[i]);
            Vector3 worldNormal = transform.TransformDirection(normals[i]);
            Gizmos.DrawLine(worldPos, worldPos + worldNormal * length);
        }
    }
}
