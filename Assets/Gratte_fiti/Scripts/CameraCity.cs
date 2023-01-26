using UnityEngine;
using Emgu.CV;
using Emgu.CV.Util;//Vectors
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using Emgu.CV.CvEnum;

public class CameraCity : MonoBehaviour
{
    //-----private-----//
    private Mat imageMat;
    private VectorOfVectorOfPoint contours;
    private VideoCapture fluxVideo;
    private List<RotatedRect> buildings;
    private List<Triangle2DF> ListTriangles = new List<Triangle2DF>();
    private List<GameObject> listBuilding =  new List<GameObject>();
    private GameObject goMire;

    //-----public-----//
    public int CameraIndex = 0;
    public Vector2 blackScreenMax = new Vector2(600, 100);
    public int Scale = 40;
    public List<GameObject> listTrees = new List<GameObject>();
    public GameObject Detection;
    public LegoDetector[] legosDetectors = new LegoDetector[0];

    void Start()
    {
        imageMat = new Mat();
        fluxVideo = new VideoCapture(CameraIndex, VideoCapture.API.Any);
        fluxVideo.ImageGrabbed += ProcessFrame;
    }

    // Update is called once per frame
    void Update()
    {
        fluxVideo.Grab();

        // Copy and filter on original to avoid detection on draw legos
        Mat original = new Mat();
        imageMat.CopyTo(original);

        // Game loop        
        foreach (var detector in legosDetectors)
        {
            var filterImage = detector.Filter(ref original);
            detector.UpdateDetection(ref imageMat, filterImage);
            detector.DrawLegos(ref imageMat);
        }
        
        CvInvoke.Imshow("Image", imageMat);
        CvInvoke.WaitKey(24);
    }

    private void OnDestroy()
    {
        fluxVideo.Dispose();
        CvInvoke.DestroyAllWindows();
    }

    //More FPS
    private void ProcessFrame(object sender, EventArgs e)
    {
        try
        {
            fluxVideo.Retrieve(imageMat, 0);
            CvInvoke.Rectangle(imageMat, new Rectangle(0, 0, (int)blackScreenMax.x, (int)blackScreenMax.y), new MCvScalar(0, 0, 0), -1);
        }
        catch (Exception exception)
        {
            Debug.Log(exception.Message);
        }
    }

    public void CreateBuilding()
    {
        //Clear building
        ClearGameObject();

        for (int i = 0; i < buildings.Count; i++)
        {
            GameObject go = Instantiate(listTrees[UnityEngine.Random.Range(0,listTrees.Count)], new Vector3(Scale * buildings[i].Center.X / 600, 1, Scale * buildings[i].Center.Y / 600), Quaternion.identity);
            go.transform.localScale = new Vector3(Scale * buildings[i].Size.Width / 1000, 1, Scale * buildings[i].Size.Height / 1000);
            go.transform.Rotate(new Vector3(0,1,0), buildings[i].Angle);
            listBuilding.Add(go);
        }

        goMire = Instantiate(Detection, new Vector3(Scale * ListTriangles[0].Centeroid.X / 600, 1, Scale * ListTriangles[0].Centeroid.Y / 600), Quaternion.identity);
        goMire.name = "POV";
    }

    public void VisualizeBuilding()
    {
        Camera.main.transform.position = new Vector3(20, 20, -10);
        Camera.main.transform.rotation = Quaternion.Euler(40, 0, 0);
        Camera.main.GetComponent<SmoothCamera>().StartCameraMovment();
    }

    //Clear all gameObject captures
    public void ClearGameObject()
    {
        if (listBuilding.Count > 0)
        {
            for (int i = 0; i < listBuilding.Count; i++)
            {
                Destroy(listBuilding[i]);
            }
            listBuilding.Clear();
        }
        if (goMire!= null)
        {
            Destroy(goMire);
        }
    }
}
