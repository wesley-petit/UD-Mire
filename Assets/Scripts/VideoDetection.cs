using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

using Emgu.CV;
using Emgu.CV.Util;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;

public class VideoDetection : MonoBehaviour
{
    VideoCapture video;
    //[Range(0, 255)]
    public Vector3 hsv1Min;
    public Vector3 hsv1Max;

    public Vector3 hsv2Min;
    public Vector3 hsv2Max;

    public Vector2 centre;
    public GameObject prefab;

    private PointF[] transformedPoints = new PointF[4];

    // Start is called before the first frame update
    void Start()
    {
        // video = new VideoCapture("Assets/Videos/lego.avi");
        video = new VideoCapture(0);
    }

    // Update is called once per frame
    void Update()
    {
        Mat orig = new Mat();

        if(video.IsOpened)
            orig = video.QueryFrame();

        if (orig.IsEmpty) 
            return;

        if (orig != null)
        {
            CvInvoke.Imshow("Webcam View", orig);
            CvInvoke.WaitKey(24);

            Mat image2 = orig.Clone();
            Mat final = ImageTreatment(image2);
            CvInvoke.Imshow("Webcam HSV", final);

            Mat image3 = final.Clone();

            centre = Borders(final, orig);
            prefab.transform.position = new Vector3(centre.x / 10, 0, centre.y / 10);

            PointF[] corners = new PointF[4];
            corners[0] = new PointF(45, 202);
            corners[1] = new PointF(551, 196);
            corners[2] = new PointF(582, 335);
            corners[3] = new PointF(11, 355);

            PointF[] wrapCorners = new PointF[4];
            wrapCorners[0] = new PointF(-11619.55f, -1819.93f);
            wrapCorners[1] = new PointF(-11726.7f, -1784f);
            wrapCorners[2] = new PointF(-11746.52f, -1853.58f);
            wrapCorners[3] = new PointF(-11639.84f, -1786.4f);

            Mat perspectiveMatrix = CvInvoke.GetPerspectiveTransform(corners, wrapCorners);
            Debug.Log(perspectiveMatrix);
            transformedPoints = CvInvoke.PerspectiveTransform(corners, perspectiveMatrix);

            //Mat borders = Borders(final, orig);
            //CvInvoke.Imshow("Blue Detection", borders);
        }
        else
            CvInvoke.DestroyAllWindows();

    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(new Vector3(transformedPoints[0].X, 0, transformedPoints[0].Y), 2);
        Gizmos.DrawSphere(new Vector3(transformedPoints[1].X, 0, transformedPoints[1].Y), 2);
        Gizmos.DrawSphere(new Vector3(transformedPoints[2].X, 0, transformedPoints[2].Y), 2);
        Gizmos.DrawSphere(new Vector3(transformedPoints[3].X, 0, transformedPoints[3].Y), 2);
    }



    void OnDestroy()
    {
        CvInvoke.DestroyAllWindows();
    }

    Mat ImageTreatment(Mat image)
    {
        CvInvoke.CvtColor(image, image, ColorConversion.Bgr2Hsv);
        CvInvoke.MedianBlur(image, image, 21);


        Hsv lower = new Hsv(hsv1Min.x, hsv1Min.y, hsv1Min.z);
        Hsv higher = new Hsv(hsv1Max.x, hsv1Max.y, hsv1Max.z);

        Image<Hsv, Byte> i = image.ToImage<Hsv, Byte>();
        Mat result = i.InRange(lower, higher).Mat;

        lower = new Hsv(hsv2Min.x, hsv2Min.y, hsv2Min.z);
        higher = new Hsv(hsv2Max.x, hsv2Max.y, hsv2Max.z);

        Mat result2 = i.InRange(lower, higher).Mat;

        result = result + result2;

        int operationSize = 1;

        Mat structuringElement = CvInvoke.GetStructuringElement(ElementShape.Ellipse,
                                                                new Size(2 * operationSize + 1, 2 * operationSize + 1),
                                                                new Point(operationSize, operationSize));

        CvInvoke.Erode(result, result, structuringElement, new Point(-1, -1), 1, BorderType.Default, new MCvScalar(0));
        CvInvoke.Dilate(result, result, structuringElement, new Point(-1, -1), 1, BorderType.Default, new MCvScalar(0));

        return result;
    }

    Vector2 Borders(Mat image, Mat origin)
    {
        VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
        VectorOfPoint biggestContour = new VectorOfPoint();
        int biggestContourIndex = -1;
        double biggestContourArea = 0;

        Mat hierarchy = new Mat();
        CvInvoke.FindContours(image, contours, hierarchy, RetrType.List, ChainApproxMethod.ChainApproxNone);

        for (int i = 0; i < contours.Size; i++)
        {
            if (CvInvoke.ContourArea(contours[i]) > biggestContourArea)
            {
                biggestContour = contours[i];
                biggestContourIndex = i;
                biggestContourArea = CvInvoke.ContourArea(contours[i]);
            }
        }

        if (biggestContourIndex > -1)
            CvInvoke.DrawContours(origin, contours, biggestContourIndex, new MCvScalar(0, 0, 255), 2);

        var moments = CvInvoke.Moments(image);
        var Centroid = new Point((int)(moments.M10 / moments.M00), (int)(moments.M01 / moments.M00));
        CvInvoke.Circle(origin, Centroid, 2, new MCvScalar(0, 0, 255), 2);

        //Debug.Log(Centroid);

        return new Vector2(Centroid.X, Centroid.Y);
    }
}