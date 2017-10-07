using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Emgu.CV;
using Emgu.CV.Features2D;
using Emgu.CV.Util;
using System.Drawing;

namespace TouchCamLibrary
{
    public class GeometricVerification
    {
        public static Matrix<double> FindHomographyOpenCV(VectorOfKeyPoint model, VectorOfKeyPoint observed, VectorOfVectorOfDMatch matches, Mat mask, double reprojectionThreshold)
        {
            //Mat H = Features2DToolbox.GetHomographyMatrixFromMatchedFeatures(model, observed, matches, mask, reprojectionThreshold);
            //if (H == null || H.DataPointer == null)
            //{
            //    return null;
            //}
            //else
            //{
            //    Matrix<double> H2 = new Matrix<double>(3, 3, 1, H.DataPointer, 8 * 3);
            //    return H2;
            //}   

            Matrix<double> H = new Matrix<double>(3, 3);
            Matrix<double> from = new Matrix<double>(matches.Size, 2);
            for (int i = 0; i < matches.Size; i++) { from[i, 0] = model[matches[i][0].TrainIdx].Point.X; from[i, 1] = model[matches[i][0].TrainIdx].Point.Y; }
            Matrix<double> to = new Matrix<double>(matches.Size, 2);
            for (int i = 0; i < matches.Size; i++) { to[i, 0] = observed[matches[i][0].QueryIdx].Point.X; to[i, 1] = observed[matches[i][0].QueryIdx].Point.Y; }
            CvInvoke.FindHomography(from, to, H, Emgu.CV.CvEnum.HomographyMethod.Ransac, reprojectionThreshold, mask);
            return H;
        }

        public static Mat FindHomography(VectorOfKeyPoint model, VectorOfKeyPoint observed, VectorOfVectorOfDMatch matches, Mat mask, double reprojectionThreshold, int maxIterations = 10000)
        {
            Mat bestHomography = null;
            //int maxInliers = 0;

            int nIter = maxIterations;
            for (int iter = 0; iter < nIter; iter++)
            {
                Matrix<int> subset = SelectMinimalSubset(model, observed, matches);
                if (subset == null) continue;

                // TODO check geometric consistency

                // TODO compute homography

                // TODO count inliers, compare with current best
            }

            return bestHomography;
        }

        private static Random rand = new Random();
        private static Matrix<int> SelectMinimalSubset(VectorOfKeyPoint model, VectorOfKeyPoint observed, VectorOfVectorOfDMatch matches, int maxAttempts = 1000)
        {
            int[,] subset = new int[4, 2];
            int[] selectedMatches = new int[4];
            int matchCount = matches.Size;

            int iter;
            for (iter = 0; iter < maxAttempts; iter++)
            {
                // pick 4 unique matches
                for (int i = 0; i < 4; i++)
                {
                    selectedMatches[i] = rand.Next(matchCount);
                    for (int j = 0; j < i; j++)
                        if(selectedMatches[i] == selectedMatches[j])
                        {
                            i--;
                            break;
                        }
                }

                for (int i = 0; i < 4; i++)
                {
                    subset[i, 0] = matches[selectedMatches[i]][0].TrainIdx;
                    subset[i, 0] = matches[selectedMatches[i]][0].QueryIdx;
                }

                // check that the points are not collinear
                bool collinear = false;
                for (int i = 0; i < 4; i++)
                {
                    int a = i == 0 ? 3 : 0;
                    int b = i == 1 ? 3 : 1;
                    int c = i == 2 ? 3 : 2;

                    double dx1 = model[subset[a, 0]].Point.X - model[subset[b, 0]].Point.X;
                    double dy1 = model[subset[a, 0]].Point.Y - model[subset[b, 0]].Point.Y;
                    double dx2 = model[subset[b, 0]].Point.X - model[subset[c, 0]].Point.X;
                    double dy2 = model[subset[b, 0]].Point.Y - model[subset[c, 0]].Point.Y;

                    if (Math.Abs(dx2 * dy1 - dy2 * dx1) <= 1e-7 * (Math.Abs(dx1) + Math.Abs(dy1) + Math.Abs(dx2) + Math.Abs(dy2)))
                    {
                        collinear = true;
                        break;
                    }
                }
                if (!collinear) break;
            }

            return iter == maxAttempts ? null : new Matrix<int>(subset);
        }

        private static Matrix<double> ComputeHomography(VectorOfKeyPoint model, VectorOfKeyPoint observed, Matrix<int> subset)
        {
            Matrix<double> P = new Matrix<double>(8, 9);
            for (int i = 0; i < 4; i++)
            {
                PointF p0 = model[subset[i, 0]].Point;
                PointF p1 = observed[subset[i, 1]].Point;

                P[2 * i, 0] = -p0.X;
                P[2 * i, 1] = -p0.Y;
                P[2 * i, 2] = -1;
                P[2 * i, 3] = 0;
                P[2 * i, 4] = 0;
                P[2 * i, 5] = 0;
                P[2 * i, 6] = p0.X * p1.X;
                P[2 * i, 7] = p0.Y * p1.X;
                P[2 * i, 8] = p1.X;
                P[2 * i + 1, 0] = 0;
                P[2 * i + 1, 1] = 0;
                P[2 * i + 1, 2] = 0;
                P[2 * i + 1, 3] = -p0.X;
                P[2 * i + 1, 4] = -p0.Y;
                P[2 * i + 1, 5] = -1;
                P[2 * i + 1, 6] = p0.X * p1.Y;
                P[2 * i + 1, 7] = p0.Y * p1.Y;
                P[2 * i + 1, 8] = p1.Y;
            }

            return null;
        }
    }
}
