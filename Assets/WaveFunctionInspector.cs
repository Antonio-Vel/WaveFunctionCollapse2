using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WaveFunctionCollapse))]
public class WaveFunctionInspector : Editor 
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        EditorGUILayout.LabelField("Test!");
        WaveFunctionCollapse exam = (WaveFunctionCollapse)target;

        if (GUILayout.Button("YUH"))
        {
            exam.WFC();
        }
    }
}
