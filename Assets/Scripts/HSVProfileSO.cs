using UnityEngine;
using Emgu.CV.Structure;

[CreateAssetMenu(fileName = "HSVProfile", menuName = "ScriptableObjects/HSVProfile")]
public class HSVProfileSO : ScriptableObject
{
    public Vector3 minThreshold;
    public Vector3 maxThreshold;

    public Hsv GetMinThresholdHSV => new Hsv(minThreshold.x, minThreshold.y, minThreshold.z);
    public Hsv GetMaxThresholdHSV => new Hsv(maxThreshold.x, maxThreshold.y, maxThreshold.z);
}
