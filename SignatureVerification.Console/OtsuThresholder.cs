//// See https://aka.ms/new-console-template for more information

//using SixLabors.ImageSharp.PixelFormats;
//using SixLabors.ImageSharp;

//internal class OtsuThresholder
//{
//    public OtsuThresholder()
//    {
//    }

//    // create method to calculate otsu using imagesharp
//    public int CalculateOtsuThreshold(Image<Rgb24> image)
//    {
//        var histogram = Histogram(image); // Create histogram of image

//        var histogramValues = histogram.Values;
//        var histogramLength = histogramValues.Length;
//        var total = image.Width * image.Height;
//        var sum = 0;
//        for (var i = 0; i < histogramLength; i++)
//        {
//            sum += i * histogramValues[i];
//        }
//        var sumB = 0;
//        var wB = 0;
//        var wF = 0;
//        var max = 0.0;
//        var threshold = 0;
//        for (var i = 0; i < histogramLength; i++)
//        {
//            wB += histogramValues[i];
//            if (wB == 0)
//            {
//                continue;
//            }
//            wF = total - wB;
//            if (wF == 0)
//            {
//                break;
//            }
//            sumB += i * histogramValues[i];
//            var mB = sumB / wB;
//            var mF = (sum - sumB) / wF;
//            var between = wB * wF * (mB - mF) * (mB - mF);
//            if (between >= max)
//            {
//                threshold = i;
//                max = between;
//            }
//        }
//        return threshold;
//    }

//    //create histogram of image
//    private static Histogram Histogram(Image<Rgb24> image)
//    {
//        var histogram = new Histogram(256);
//        var width = image.Width;
//        var height = image.Height;
//        for (var y = 0; y < height; y++)
//        {
//            for (var x = 0; x < width; x++)
//            {
//                var color = image[x, y];
//                var luminance = (int)Math.Round((color.R * 0.3) + (color.G * 0.59) + (color.B * 0.11));
//                histogram.Increment(luminance);
//            }
//        }
//        return histogram;
//    }
//}