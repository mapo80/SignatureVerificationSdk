//using SixLabors.ImageSharp;
//using SixLabors.ImageSharp.PixelFormats;
//using SixLabors.ImageSharp.Processing;

//public static class ImageHelper
//{
//    public static byte CalculateOtsuThreshold(Image<L8> image)
//    {
//        // Calculate the histogram of the image
//        int[] histogram = new int[256];

//        image.ProcessPixelRows(accessor =>
//        {
//            for (var y = 0; y < accessor.Height; y++)
//            {
//                Span<L8> pixelSpan = accessor.GetRowSpan(y);
//                for (var x = 0; x < accessor.Width; x++)
//                {
//                    histogram[pixelSpan[x].PackedValue]++;

//                }
//            }
//        });

//        // Calculate the total number of pixels in the image
//        int totalPixels = image.Width * image.Height;

//        // Calculate the sum of pixel values
//        int sum = 0;
//        for (int i = 0; i < 256; i++)
//        {
//            sum += i * histogram[i];
//        }

//        // Calculate the sum of squared pixel values
//        int sumSquared = 0;
//        for (int i = 0; i < 256; i++)
//        {
//            sumSquared += i * i * histogram[i];
//        }

//        // Calculate the maximum between-class variance
//        double maxVariance = double.MinValue;
//        byte threshold = 0;
//        int backgroundPixels = 0;
//        int foregroundPixels = 0;
//        int backgroundSum = 0;
//        int foregroundSum = 0;
//        for (int i = 0; i < 256; i++)
//        {
//            backgroundPixels += histogram[i];
//            if (backgroundPixels == 0) continue;

//            foregroundPixels = totalPixels - backgroundPixels;
//            if (foregroundPixels == 0) break;

//            backgroundSum += i * histogram[i];
//            foregroundSum = sum - backgroundSum;

//            double backgroundMean = (double)backgroundSum / backgroundPixels;
//            double foregroundMean = (double)foregroundSum / foregroundPixels;

//            double variance = (double)backgroundPixels * foregroundPixels * Math.Pow(backgroundMean - foregroundMean, 2);

//            if (variance > maxVariance)
//            {
//                maxVariance = variance;
//                threshold = (byte)i;
//            }
//        }

//        return threshold;
//    }

//    //Convert to grayscale L8
//    public static Image<L8> ConvertToGrayscale(Image<Rgb24> image)
//    {
//        var grayscaleImage = image.CloneAs<L8>();
//        return grayscaleImage;
//    }
//    // calculate min crop area that contain all grayscale image
//    public static Rectangle CalculateMinCropArea(Image<L8> image)
//    {
//        int minX = image.Width;
//        int minY = image.Height;
//        int maxX = 0;
//        int maxY = 0;
//        image.ProcessPixelRows(accessor =>
//        {
//            for (var y = 0; y < accessor.Height; y++)
//            {
//                Span<L8> pixelSpan = accessor.GetRowSpan(y);
//                for (var x = 0; x < accessor.Width; x++)
//                {
//                    if (pixelSpan[x].PackedValue == 0)
//                    {
//                        if (x < minX) minX = x;
//                        if (x > maxX) maxX = x;
//                        if (y < minY) minY = y;
//                        if (y > maxY) maxY = y;
//                    }
//                }
//            }
//        });
//        return new Rectangle(minX, minY, maxX - minX, maxY - minY);
//    }


//    //create method to calculate center of mass for image binarized
//    public static Point CalculateCenterOfMass(Image<L8> image)
//    {
//        int xSum = 0;
//        int ySum = 0;
//        int totalPixels = 0;
//        image.ProcessPixelRows(accessor =>
//        {
//            for (var y = 0; y < accessor.Height; y++)
//            {
//                Span<L8> pixelSpan = accessor.GetRowSpan(y);
//                for (var x = 0; x < accessor.Width; x++)
//                {
//                    if (pixelSpan[x].PackedValue == 0)
//                    {
//                        xSum += x;
//                        ySum += y;
//                        totalPixels++;
//                    }
//                }
//            }
//        });
//        return new Point(xSum / totalPixels, ySum / totalPixels);
//    }

//    public static (Point centerOfMass, Rectangle minumumArea) CalculateCenterOfMass2(Image<L8> image, int val)
//    {
//        int minX = image.Width;
//        int minY = image.Height;
//        int maxX = 0;
//        int maxY = 0;
//        int xSum = 0;
//        int ySum = 0;
//        int totalPixels = 0;

//        image.ProcessPixelRows(accessor =>
//        {
//            for (var y = 0; y < accessor.Height; y++)
//            {
//                Span<L8> pixelSpan = accessor.GetRowSpan(y);
//                for (var x = 0; x < accessor.Width; x++)
//                {
//                    if (pixelSpan[x].PackedValue <= val)
//                    {
//                        if (x < minX) minX = x;
//                        if (x > maxX) maxX = x;
//                        if (y < minY) minY = y;
//                        if (y > maxY) maxY = y;

//                        xSum += x;
//                        ySum += y;
//                        totalPixels++;
//                    }
//                }
//            }
//        });

//        var centerX = (int)Math.Round(xSum / (double)totalPixels, 0) - minX;
//        var centerY = (int)Math.Round(ySum / (double)totalPixels, 0) - minY;

//        // create rectangle between two points

//        return (new Point(centerX, centerY),
//            new Rectangle(minX, minY, maxX - minX,
//                maxY - minY)); //GetRectangle(new Point(maxX, maxY), new Point(minX, minY)));
//    }

//    //public static Image<Bgr24> ApplyOtsuThreshold(Image<Bgr24> image)
//    //{
//    //    // Convert the image to grayscale
//    //    var grayscaleImage = image.CloneAs<L8>();

//    //    // Calculate the Otsu threshold
//    //    byte threshold = CalculateOtsuThreshold(grayscaleImage);

//    //    // Apply the threshold to the image
//    //    var thresholdedImage = grayscaleImage.CloneAs<Bgr24>();
//    //    thresholdedImage.Mutate(x => x.BinaryThreshold(threshold));

//    //    return thresholdedImage;
//    //}

//    //create method to calculate center of mass for image binarized
//    public static void Denoise(Image<L8> image, int threshold)
//    {
//        image.ProcessPixelRows(accessor =>
//        {
//            for (var y = 0; y < accessor.Height; y++)
//            {
//                Span<L8> pixelSpan = accessor.GetRowSpan(y);
//                for (var x = 0; x < accessor.Width; x++)
//                {
//                    if (pixelSpan[x].PackedValue > threshold)
//                    {
//                        pixelSpan[x].PackedValue = 255;
//                    }
//                }
//            }
//        });
//    }
//}