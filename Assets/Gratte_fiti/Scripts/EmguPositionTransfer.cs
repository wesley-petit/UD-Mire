using UnityEngine;

public class EmguPositionTransfer : MonoBehaviour
{
    public Transform place;
    public Vector2 minEmguCV = new Vector2(33, 100);
    public Vector2 maxEmguCv = new Vector2(379, 308);
    
    public Vector2 minUnity = new Vector2(3.094f, -0.418f);
    public Vector2 maxUnity = new Vector2(0.029f, 1.574f);

    private void Start()
    {
        Convert(new Vector2(57, 247));
        Convert(new Vector2(388, 85));
    }

    public Vector3 Convert(Vector2 emguPosition)
    {
        double widthEmgu = Mathf.Abs(maxEmguCv.x - minEmguCV.x);
        double lengthEmgu = Mathf.Abs(maxEmguCv.y - minEmguCV.y);

        double widthUnity = Mathf.Abs(maxUnity.x - minUnity.x);
        double lengthUnity = Mathf.Abs(maxUnity.y - minUnity.y);

        var xRatio = (emguPosition.x - minEmguCV.x) / widthEmgu;
        var yRatio = (emguPosition.y - minEmguCV.y) / lengthEmgu;

        var xUnity = minUnity.x - xRatio * widthUnity;
        var yUnity = minUnity.y + yRatio * lengthUnity;

        // place.transform.position = new Vector3((float)xUnity, 0.043f, (float)yUnity);
        return new Vector3((float)xUnity, 0.043f,(float)yUnity);
    }
}
