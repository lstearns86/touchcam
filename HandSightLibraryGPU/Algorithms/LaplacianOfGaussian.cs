using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandSightLibrary.ImageProcessing
{
    public class LaplacianOfGaussian
    {
        private const float pi = (float)Math.PI;
        public static float[,] Generate(float sigma)
        {
            int w = (int)(3 * sigma);
            float[,] filter = new float[2 * w + 1, 2 * w + 1];
            for (int y = -w; y <= w; y++)
                for (int x = -w; x <= w; x++)
                    filter[y + w, x + w] = -1.0f / (pi * sigma * sigma) * (1 - (x * x + y * y) / (2 * sigma * sigma)) * (float)Math.Exp(-(x * x + y * y) / (2 * sigma * sigma));
            return filter;
        }
    }
}
