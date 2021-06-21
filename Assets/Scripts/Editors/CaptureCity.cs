using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CameraCity))]
public class CaptureCity : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Create City"))
        {
            ((CameraCity)target).CreateBuilding();
        }
        if(GUILayout.Button("Visualize at red triangle"))
        {
            ((CameraCity)target).VisualizeBuilding();
        }
    }
}
