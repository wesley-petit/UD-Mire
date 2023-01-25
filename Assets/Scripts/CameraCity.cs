using UnityEngine;
using System.Drawing;//Point
using Emgu.CV;
using Emgu.CV.Util;//Vectors
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;

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
        foreach (var detector in legosDetectors)
        {
            var filterImage = detector.Filter(ref original);
            detector.DrawLegos(ref imageMat, filterImage);
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
        }
        catch (Exception exception)
        {
            Debug.Log(exception.Message);
        }
    }

    List<Triangle2DF> DrawTriangle(Image<Gray, byte> imageSeuil, String name)
    {
        List<Triangle2DF> triangleList = new List<Triangle2DF>();
        contours = new VectorOfVectorOfPoint();
        Mat m = new Mat();

        CvInvoke.FindContours(imageSeuil, contours, m, Emgu.CV.CvEnum.RetrType.External, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);
        for (int i = 0; i < contours.Size; i++)
        {
            double perimeter = CvInvoke.ArcLength(contours[i], true);
            VectorOfPoint approx = new VectorOfPoint();
            CvInvoke.ApproxPolyDP(contours[i], approx, 0.04 * perimeter, true);
            if (CvInvoke.ContourArea(approx, false) > 250) //only consider contours with area greater than 250
            {
                if (approx.Size == 3) //The contour has 3 vertices, it is a triangle
                {
                    Point[] pts = approx.ToArray();
                    triangleList.Add(new Triangle2DF(
                        pts[0],
                        pts[1],
                        pts[2]
                    ));
                }
                //Draw on the image the recognition
                foreach (Triangle2DF triangle in triangleList)
                {
                    CvInvoke.Polylines(imageMat, Array.ConvertAll(triangle.GetVertices(), Point.Round),
                        true, new Bgr(System.Drawing.Color.DarkBlue).MCvScalar, 2);
                    var moments = CvInvoke.Moments(contours[i]);
                    int x = (int)(moments.M10 / moments.M00);
                    int y = (int)(moments.M01 / moments.M00);
                    CvInvoke.Circle(imageMat, new Point((int)triangle.V0.X, (int)triangle.V0.Y), 7, new MCvScalar(0, 0, 0), -1);
                    CvInvoke.PutText(imageMat, name, new Point(x, y), Emgu.CV.CvEnum.FontFace.HersheySimplex, 0.5, new MCvScalar(0, 0, 255), 2);
                }
            }
        }
        return triangleList;
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
