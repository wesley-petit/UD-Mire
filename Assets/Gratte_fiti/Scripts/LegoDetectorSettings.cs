using UnityEngine;

/// <summary>
/// Common settingss between lego detector instances
/// </summary>
public class LegoDetectorSettings : MonoBehaviour
{
    public float MinValidContourArea = 7;
    public float MaxValidContourArea = 30;
    public float MaxDistanceBetweenLegos = 0.1f;
    public float BirthTimeInSecondes = 1.5f;                         // Time in seconds to display a lego
    public EmguPositionTransfer emguPositionTransfer;
}
