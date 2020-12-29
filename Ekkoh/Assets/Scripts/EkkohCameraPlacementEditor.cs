
#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EkkohCameraPlacement))]
public class EkkohCameraPlacementEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EkkohCameraPlacement myTarget = (EkkohCameraPlacement)target;

        if (GUILayout.Button("Start recording"))
        {
            myTarget.StartRecording();
        }
        if (GUILayout.Button("Stop recording"))
        {
            myTarget.StopRecording();
        }
    }
}
#endif