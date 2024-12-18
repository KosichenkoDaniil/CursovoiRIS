using SkiaSharp;
using MathNet.Numerics.Distributions;

namespace Solver
{
    class AdaptiveBlur
    {
        public static SKBitmap ApplyAdaptiveBlurToSegment(SKBitmap sourceSegment, bool isTopEdge, bool isBottomEdge, double significanceLevel = 0.05)
        {
            int width = sourceSegment.Width;
            int height = sourceSegment.Height;

            SKBitmap resultSegment = new SKBitmap(width, height);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {                    
                    if ((y == 0 && !isTopEdge) || (y == height - 1 && !isBottomEdge))
                    {
                        resultSegment.SetPixel(x, y, sourceSegment.GetPixel(x, y));
                        continue;
                    }
                                        
                    var neighbors = GetNeighborPixels(sourceSegment, x, y, isTopEdge, isBottomEdge);
                                        
                    var centralPixel = sourceSegment.GetPixel(x, y);
                                        
                    if (IsSignificantDifference(neighbors, centralPixel, significanceLevel))
                    {                        
                        resultSegment.SetPixel(x, y, centralPixel);
                    }
                    else
                    {                        
                        var blurredPixel = AverageColor(neighbors);
                        resultSegment.SetPixel(x, y, blurredPixel);
                    }
                }
            }

            return resultSegment;
        }

        private static SKColor[] GetNeighborPixels(SKBitmap bitmap, int x, int y, bool isTopEdge, bool isBottomEdge)
        {
            var neighbors = new SKColor[9];
            int index = 0;

            for (int dy = -1; dy <= 1; dy++)
            {
                for (int dx = -1; dx <= 1; dx++)
                {
                    int neighborX = x + dx;
                    int neighborY = y + dy;
                                        
                    if (neighborX < 0 || neighborX >= bitmap.Width ||
                        (neighborY < 0 && !isTopEdge) ||
                        (neighborY >= bitmap.Height && !isBottomEdge))
                    {
                        neighbors[index++] = bitmap.GetPixel(x, y); 
                    }
                    else if (neighborY >= 0 && neighborY < bitmap.Height)
                    {
                        neighbors[index++] = bitmap.GetPixel(neighborX, neighborY);
                    }
                }
            }

            return neighbors;
        }

        private static bool IsSignificantDifference(SKColor[] neighbors, SKColor centralPixel, double significanceLevel)
        {            
            double[] intensities = Array.ConvertAll(neighbors, c => ToGrayScale(c));
            double centralIntensity = ToGrayScale(centralPixel);
                        
            double mean = Mean(intensities);
            double stdDev = StandardDeviation(intensities, mean);
                        
            double tStatistic = Math.Abs(centralIntensity - mean) / (stdDev / Math.Sqrt(intensities.Length));
                        
            double criticalValue = GetCriticalTValue(intensities.Length - 1, significanceLevel);

            return tStatistic > criticalValue;
        }

        private static double ToGrayScale(SKColor color)
        {
            return 0.299 * color.Red + 0.587 * color.Green + 0.114 * color.Blue;
        }

        private static double Mean(double[] values)
        {
            double sum = 0;
            foreach (var v in values) sum += v;
            return sum / values.Length;
        }

        private static double StandardDeviation(double[] values, double mean)
        {
            double sum = 0;
            foreach (var v in values) sum += Math.Pow(v - mean, 2);
            return Math.Sqrt(sum / values.Length);
        }

        private static double GetCriticalTValue(int degreesOfFreedom, double significanceLevel)
        {
            if (degreesOfFreedom <= 0)
                throw new ArgumentException("Degrees of freedom must be greater than 0.");
                        
            return StudentT.InvCDF(0, 1, degreesOfFreedom, 1 - significanceLevel / 2);
        }

        private static SKColor AverageColor(SKColor[] colors)
        {
            int totalR = 0, totalG = 0, totalB = 0, totalA = 0;

            foreach (var color in colors)
            {
                totalR += color.Red;
                totalG += color.Green;
                totalB += color.Blue;
                totalA += color.Alpha;
            }

            int count = colors.Length;
            return new SKColor((byte)(totalR / count), (byte)(totalG / count), (byte)(totalB / count), (byte)(totalA / count));
        }
    }
}
