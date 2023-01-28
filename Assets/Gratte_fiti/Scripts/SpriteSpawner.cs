using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SpriteSpawner : MonoBehaviour
{
    public Vector2 minPoint;
    public Vector2 maxPoint;

    public List<SpriteRenderer> blueModels = new();
    public List<SpriteRenderer> redModels = new();
    public List<SpriteRenderer> greenModels = new();
    public int spriteOrder = 1;
    public GameObject motif;
    public Transform origin;
    
    private void Start()
    {
        
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Spawn("Red");
        }

        if (Input.GetKeyDown(KeyCode.V))
        {
            Spawn("Green");
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            Spawn("Blue");
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            Instantiate(motif, origin.position, motif.transform.rotation);
        }
    }

    public void Spawn(string color)
    {
        var original = GetRandomModelForColor(color);
        var randPosition = GetRandomPosition();
        var clone = Instantiate(original, randPosition, original.transform.rotation, transform);

        var randScale = clone.transform.localScale.x * Random.Range(0.2f, 2.0f);
        clone.transform.localScale = new Vector3(randScale, randScale, randScale);
        
        clone.sortingOrder = spriteOrder;
        spriteOrder++;
    }

    SpriteRenderer GetRandomModelForColor(string color)
    {
        List<SpriteRenderer> container = new List<SpriteRenderer>();
        switch (color)
        {
            case "Blue":
                container = blueModels;
                break;
            
            case "Red":
                container = redModels;
                break;
            
            case "Green":
                container = greenModels;
                break;
        }
        
        var rand =  Random.Range(0, container.Count);
        return container[rand];
    }
    
    Vector3 GetRandomPosition()
    {
        float randomX = Random.Range(minPoint.x, maxPoint.x);
        float randomZ = Random.Range(minPoint.y, maxPoint.y);
        return new Vector3(randomX, 0f, randomZ);
    }
}
