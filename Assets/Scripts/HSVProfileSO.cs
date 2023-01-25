using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HSVProfile", menuName = "ScriptableObjects/HSVProfile")]
public class HSVProfileSO : ScriptableObject
{
    public Vector3 minThreshold;
    public Vector3 maxThreshold;
}
