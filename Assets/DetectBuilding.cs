using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectBuilding : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Building")
        {
            Debug.Log("Building");
            Camera.main.GetComponent<SmoothCamera>().center = other.transform;
        }
    }
}
