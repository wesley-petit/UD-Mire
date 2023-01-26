using UnityEngine;

public class Graffiti
{
    public Vector2 Position;
    public bool bPresent;
    public bool bNew;
    public float PregnancyTimeInSeconds;
    public GameObject Motif;

    public Graffiti(Vector2 position, bool isPresent, bool isNew)
    {
        Position = position;
        bPresent = isPresent;
        bNew = isNew;
        PregnancyTimeInSeconds = 0f;
        Motif = null;
    }
}
