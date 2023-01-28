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
    public LegoDetectorSettings legoDetectorSettings;
    public string colorName;
    public bool bShowDebugWindow;
    public Vector3 contourColor = new Vector3(0, 255, 0);
    public VectorOfVectorOfPoint validContours = new VectorOfVectorOfPoint();
    public List<Graffiti> streetArts = new();
    public GameObject motif;
    public SpriteSpawner spriteSpawner;
    
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
       
        // Limit all pixel value in a given range
        Image<Hsv, byte> imgConverti = imageHSV.ToImage<Hsv, byte>();
        var filterImg = imgConverti.InRange(legoProfile.GetMinThresholdHSV, legoProfile.GetMaxThresholdHSV);

        if (bShowDebugWindow)
        {
            CvInvoke.Imshow(colorName, filterImg);
        }

        return filterImg;
    }
    
    public void UpdateDetection(ref Mat webcamFrame, in Image<Gray, byte> filterImage)
    {
        ResetFlags();
        
        // Dilate to delete small object from analysis
        var structElement = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(5, 5), new Point(2, 2));
        CvInvoke.Dilate(filterImage, filterImage, structElement, new Point(2, 2), 1, BorderType.Default, new MCvScalar());

        // Draw all contours of valid form
        VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
        Mat m = new Mat();
        CvInvoke.FindContours(filterImage, contours, m, Emgu.CV.CvEnum.RetrType.List, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);

        List<VectorOfPoint> largeContous = new List<VectorOfPoint>();
        
        // Accept only large contour as a lego
        for (int i = 0; i < contours.Size; i++)
        {
            double perimeter = CvInvoke.ArcLength(contours[i], true);
            VectorOfPoint approx = new VectorOfPoint();
            CvInvoke.ApproxPolyDP(contours[i], approx, 0.04 * perimeter, true);
            if (legoDetectorSettings.MinValidContourArea < CvInvoke.ContourArea(approx, false) && CvInvoke.ContourArea(approx, false) < legoDetectorSettings.MaxValidContourArea)
            {
                largeContous.Add(contours[i]);

                var moments = CvInvoke.Moments(contours[i]);
                int x = (int)(moments.M10 / moments.M00);
                int y = (int)(moments.M01 / moments.M00);
                var detectedPosition = new Vector2(x, y);

                if (colorName == "Blue")
                {
                    Debug.Log(detectedPosition);
                }
                
                // Check if it's a new detection                
                bool bFound = false;
                foreach (var current in streetArts)
                {
                    if (Vector2.Distance(current.Position, detectedPosition) < legoDetectorSettings.MaxDistanceBetweenLegos)
                    {
                        current.bPresent = true;
                        bFound = true;
                        break;
                    }
                }

                if (!bFound)
                {
                    streetArts.Add(new Graffiti(detectedPosition, true, true));    
                }
            }

            validContours = new VectorOfVectorOfPoint(largeContous.ToArray());
        }
        
  
        // Debug.LogWarning( colorName + " - " + contours.Size);
        // return null;
        //
        // List<RotatedRect> boxList = new List<RotatedRect>();
        // var contours = new VectorOfVectorOfPoint();
        //
        // for (int i = 0; i < contours.Size; i++)
        // {
        //     double perimeter = CvInvoke.ArcLength(contours[i], true);
        //     VectorOfPoint approx = new VectorOfPoint();
        //     CvInvoke.ApproxPolyDP(contours[i], approx, 0.04 * perimeter, true);
        //     if (CvInvoke.ContourArea(approx, false) > 250) //only consider contours with area greater than 250
        //     {
        //         if (approx.Size == 4)//The contour has 4 vertices, it is a rectangle
        //         {
        //             bool isRectangle = true;
        //             Point[] pts = approx.ToArray();
        //             LineSegment2D[] edges = PointCollection.PolyLine(pts, true);
        //
        //             for (int j = 0; j < edges.Length; j++)
        //             {
        //                 double angle = Math.Abs(
        //                     edges[(j + 1) % edges.Length].GetExteriorAngleDegree(edges[j]));
        //                 if (angle < 80 || angle > 100)
        //                 {
        //                     isRectangle = false;
        //                     break;
        //                 }
        //             }
        //             if (isRectangle) boxList.Add(CvInvoke.MinAreaRect(approx));
        //         }
        //
        //         foreach (RotatedRect box in boxList)
        //         {
        //             CvInvoke.Polylines(webcamFrame, Array.ConvertAll(box.GetVertices(), Point.Round), true,
        //                 new Bgr(System.Drawing.Color.Green).MCvScalar, 2);
        //             var moments = CvInvoke.Moments(contours[i]);
        //             int x = (int)(moments.M10 / moments.M00);
        //             int y = (int)(moments.M01 / moments.M00);
        //             CvInvoke.PutText(webcamFrame, colorName, new Point(x, y), Emgu.CV.CvEnum.FontFace.HersheySimplex, 0.5, new MCvScalar(0, 0, 255), 2);
        //         }
        //     }
        // }
        //
        // return boxList;
    }

    private void ResetFlags()
    {
        foreach (var current in streetArts)
            current.bPresent = false;
    }

    //fonction pour dessiner les limits des obj et creer leur centroide
    public void DrawLegos(ref Mat webcamFrame)
    {
        var mcvColor = new MCvScalar(contourColor.x, contourColor.y, contourColor.z);

        if (validContours.Size != 0)
        {
            CvInvoke.DrawContours(webcamFrame, validContours, -1, mcvColor);
        }

        List<int> oldGraffitisIndex = new List<int>();
        for (int i = 0; i < streetArts.Count; i++)
        {
            var current = streetArts[i];
            if (!current.bPresent)
            {
                oldGraffitisIndex.Add(i);
                Debug.Log("Remove");
                continue;
            }
            
            CvInvoke.PutText(webcamFrame, colorName, new Point((int)current.Position.x, (int)current.Position.y), Emgu.CV.CvEnum.FontFace.HersheySimplex, 0.5, new MCvScalar(0, 0, 255), 2);

            // New motif detected since some time
            if (current.bNew)
            {
                current.PregnancyTimeInSeconds += Time.deltaTime;

                if (legoDetectorSettings.BirthTimeInSecondes < current.PregnancyTimeInSeconds)
                {
                    // spriteSpawner.Spawn(colorName);
                    Debug.Log("New Graffitis");

                    // var worldPosition = legoDetectorSettings.emguPositionTransfer.Convert(current.Position);
                    //
                    // current.Motif = Instantiate(motif, worldPosition, Quaternion.identity);
                    // current.Motif.transform.position = worldPosition;
                    current.bNew = false;
                }
            }
        }
        
        // // Reverse to delete last index first
        // // If we remove the first index, all following index will change. But it's not the case if we start at the last index.
        // oldGraffitisIndex.Reverse();
        // while (oldGraffitisIndex.Count != 0)
        // {
        //     var graf = streetArts[oldGraffitisIndex[0]];
        //     Destroy(graf.Motif);
        //     streetArts.Remove(graf);
        //     oldGraffitisIndex.RemoveAt(0);
        // }
    }
}
