using UnityEngine;

/// <summary>
/// Common settingss between lego detector instances
/// </summary>
public class LegoDetectorSettings : MonoBehaviour
{
    public Vector2 WebcamSize = new Vector2(600, 600);
    public int MinValidContourArea = 250;
    public float MaxDistanceBetweenLegos = 0.1f;
    public int BirthTimeInSecondes = 4;                         // Time in seconds to display a lego
    public EmguPositionTransfer emguPositionTransfer;
}
