using System;
using System.Collections.Generic;
using System.Drawing;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using UnityEngine;

public class LegoDetector : MonoBehaviour
{
    public HSVProfileSO legoProfile;
    public string colorName;

    /// <summary>
    /// Remove others objects and convert to black and white
    /// </summary>
    /// <param name="webcamFrame"></param>
    /// <returns></returns>
    public Image<Gray, byte> Filter(ref Mat webcamFrame)
    {
        Mat imageHSV = new Mat();
        CvInvoke.CvtColor(webcamFrame, imageHSV, ColorConversion.Bgr2Hsv);
        CvInvoke.Blur(imageHSV, imageHSV, new Size(4, 4), new Point(1, 1));
        
        Image<Hsv, byte> imgConverti = imageHSV.ToImage<Hsv, byte>();
        var filterImg = imgConverti.InRange(legoProfile.GetMinThresholdHSV, legoProfile.GetMaxThresholdHSV);
        
        CvInvoke.Imshow(colorName, filterImg);
        return filterImg;
    }
    
    //fonction pour dessiner les limits des obj et creer leur centroide
    public List<RotatedRect> DrawLegos(ref Mat webcamFrame, in Image<Gray, byte> filterImage)
    {
        var structElement = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(5, 5), new Point(2, 2));
        CvInvoke.Dilate(filterImage, filterImage, structElement, new Point(2, 2), 1, BorderType.Default, new MCvScalar());

        List<RotatedRect> boxList = new List<RotatedRect>();
        var contours = new VectorOfVectorOfPoint();
        Mat m = new Mat();
        CvInvoke.FindContours(filterImage, contours, m, Emgu.CV.CvEnum.RetrType.External, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);

        for (int i = 0; i < contours.Size; i++)
        {
            double perimeter = CvInvoke.ArcLength(contours[i], true);
            VectorOfPoint approx = new VectorOfPoint();
            CvInvoke.ApproxPolyDP(contours[i], approx, 0.04 * perimeter, true);
            if (CvInvoke.ContourArea(approx, false) > 250) //only consider contours with area greater than 250
            {
                if (approx.Size == 4)//The contour has 4 vertices, it is a rectangle
                {
                    bool isRectangle = true;
                    Point[] pts = approx.ToArray();
                    LineSegment2D[] edges = PointCollection.PolyLine(pts, true);

                    for (int j = 0; j < edges.Length; j++)
                    {
                        double angle = Math.Abs(
                            edges[(j + 1) % edges.Length].GetExteriorAngleDegree(edges[j]));
                        if (angle < 80 || angle > 100)
                        {
                            isRectangle = false;
                            break;
                        }
                    }
                    if (isRectangle) boxList.Add(CvInvoke.MinAreaRect(approx));
                }

                foreach (RotatedRect box in boxList)
                {
                    CvInvoke.Polylines(webcamFrame, Array.ConvertAll(box.GetVertices(), Point.Round), true,
                        new Bgr(System.Drawing.Color.Green).MCvScalar, 2);
                    var moments = CvInvoke.Moments(contours[i]);
                    int x = (int)(moments.M10 / moments.M00);
                    int y = (int)(moments.M01 / moments.M00);
                    CvInvoke.PutText(webcamFrame, colorName, new Point(x, y), Emgu.CV.CvEnum.FontFace.HersheySimplex, 0.5, new MCvScalar(0, 0, 255), 2);
                }
            }
        }

        return boxList;
    }
}
