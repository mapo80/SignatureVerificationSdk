using System;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace SignatureVerificationSdk.Helpers
{
    internal static class ImagePreprocessing
    {
        #region Calculate Otsu threshold
        /// <summary>
        /// Calculate Otsu threshold
        /// </summary>
        /// <param name="image">Input image</param>
        /// <returns>Return otsu threshold</returns>
        internal static byte CalculateOtsuThreshold(this Image<L8> image)
        {
            // Calculate the histogram of the image
            var histogram = new int[256];

            image.ProcessPixelRows(accessor =>
            {
                for (var y = 0; y < accessor.Height; y++)
                {
                    var pixelSpan = accessor.GetRowSpan(y);
                    for (var x = 0; x < accessor.Width; x++)
                    {
                        histogram[pixelSpan[x].PackedValue]++;

                    }
                }
            });

            // Calculate the total number of pixels in the image
            var totalPixels = image.Width * image.Height;

            // Calculate the sum of pixel values
            var sum = 0;
            for (var i = 0; i < 256; i++)
            {
                sum += i * histogram[i];
            }

            // Calculate the maximum between-class variance
            var maxVariance = double.MinValue;
            byte threshold = 0;
            var backgroundPixels = 0;
            var backgroundSum = 0;
            for (var i = 0; i < 256; i++)
            {
                backgroundPixels += histogram[i];
                if (backgroundPixels == 0) continue;

                var foregroundPixels = totalPixels - backgroundPixels;
                if (foregroundPixels == 0) break;

                backgroundSum += i * histogram[i];
                var foregroundSum = sum - backgroundSum;

                var backgroundMean = (double)backgroundSum / backgroundPixels;
                var foregroundMean = (double)foregroundSum / foregroundPixels;

                var variance = (double)backgroundPixels * foregroundPixels * Math.Pow(backgroundMean - foregroundMean, 2);

                if (!(variance > maxVariance)) continue;

                maxVariance = variance;
                threshold = (byte)i;
            }

            return threshold;
        }

        #endregion

        #region Convert image to grayscale
        /// <summary>
        /// Convert image to grayscale
        /// </summary>
        /// <param name="image">Input image</param>
        /// <returns>Return grayscaled image</returns>
        internal static Image<L8> ConvertToGrayscale(this Image<Rgb24> image)
        {
            return image.CloneAs<L8>();
        }
        #endregion

        #region Calculate center of mass and minimum area of the input image
        /// <summary>
        /// Calculate center of mass and minimum area of the input image
        /// </summary>
        /// <param name="image">Input image</param>
        /// <param name="otsuThreshold">Otsu threshold</param>
        /// <returns></returns>
        internal static (Point CenterOfMass, Rectangle MinumumArea) CalculateMassInfo(this Image<L8> image,
            int otsuThreshold)
        {
            var minX = image.Width;
            var minY = image.Height;
            var maxX = 0;
            var maxY = 0;
            var xSum = 0;
            var ySum = 0;
            var totalPixels = 0;

            image.ProcessPixelRows(accessor =>
            {
                for (var y = 0; y < accessor.Height; y++)
                {
                    var pixelSpan = accessor.GetRowSpan(y);
                    for (var x = 0; x < accessor.Width; x++)
                    {
                        if (pixelSpan[x].PackedValue > otsuThreshold) continue;

                        if (x < minX) minX = x;
                        if (x > maxX) maxX = x;
                        if (y < minY) minY = y;
                        if (y > maxY) maxY = y;

                        xSum += x;
                        ySum += y;
                        totalPixels++;
                    }
                }
            });

            var centerX = (int)Math.Round(xSum / (double)totalPixels, 0) - minX;
            var centerY = (int)Math.Round(ySum / (double)totalPixels, 0) - minY;

            return (
                new Point(centerX, centerY),
                new Rectangle(minX, minY, maxX - minX,
                    maxY - minY)
            );
        }

        #endregion

        #region Remove noise from image using threshold

        /// <summary>
        /// Remove noise from image using threshold
        /// </summary>
        /// <param name="image">Input image</param>
        /// <param name="threshold">Otsu threshold</param>
        internal static void Denoise(this Image<L8> image, int threshold)
        {
            image.ProcessPixelRows(accessor =>
            {
                for (var y = 0; y < accessor.Height; y++)
                {
                    var pixelSpan = accessor.GetRowSpan(y);
                    for (var x = 0; x < accessor.Width; x++)
                    {
                        if (pixelSpan[x].PackedValue > threshold)
                        {
                            pixelSpan[x].PackedValue = 255;
                        }
                    }
                }
            });
        }

        #endregion

        #region Preprocess image for validation
        /// <summary>
        /// Preprocess image to pass to validation
        /// </summary>
        /// <param name="inputStream">Input image</param>
        /// <param name="inputSize">Size of the output image</param>
        /// <param name="imageSize">Image size</param>
        /// <param name="canvasSize">Internal resize image size</param>
        /// <returns>Preprocessed image</returns>
        /// <exception cref="Exception"></exception>
        internal static Image<L8> PreprocessImage(this Stream inputStream, Size inputSize, Size imageSize,
            Size canvasSize)
        {

            #region Grayscale image

            var grayscaleImage = Image
                .Load<Rgb24>(inputStream)
                .ConvertToGrayscale();

            #endregion

            #region Apply gaussian blur and calculate otsu

            // 1) Crop the image before getting the center of mass

            // Apply a gaussian filter on the image to remove small components
            // Note: this is only used to define the limits to crop the image
            var blurredImage = grayscaleImage
                .Clone(i => i.GaussianBlur(2));

            // Binarize the image using OTSU's algorithm. This is used to find the center
            // of mass of the image, and find the threshold to remove background noise
            var threshold = blurredImage.CalculateOtsuThreshold();

            #endregion

            #region Calculate center of mass and minimum area of the input image

            // Find the center of mass
            var massInfo = blurredImage.CalculateMassInfo(threshold);
            var minCropArea = massInfo.MinumumArea;
            var centerOfMass = massInfo.CenterOfMass;

            // Crop the image with a tight box
            var croppedOriginal = grayscaleImage
                .Clone(p => p.Crop(minCropArea));

            #endregion

            #region Center the image inside white canvas

            var maxRows = (float)canvasSize.Width;
            var maxCols = (float)canvasSize.Height;

            // 2) Center the image
            var imgRows = minCropArea.Height * 1.0f;
            var imgCols = minCropArea.Width * 1.0f;

            var rStart = Math.Floor(maxRows / 2) - centerOfMass.Y;
            var cStart = Math.Floor(maxCols / 2) - centerOfMass.X;

            // Make sure the new image does not go off bounds
            // Emit a warning if the image needs to be cropped, since we don't want this
            // for most cases (may be ok for feature learning, so we don't raise an error)
            if (imgRows > maxRows || imgCols > maxCols)
            {
                throw new Exception("Canvas size is smaller than signature image size");
            }

            var extraR = (rStart + imgRows) - maxRows;
            if (extraR > 0)
                rStart -= extraR;
            if (rStart < 0)
                rStart = 0;

            var extraC = (cStart + imgCols) - maxCols;
            if (extraC > 0)
                cStart -= extraC;
            if (cStart < 0)
                cStart = 0;

            // Add the image to the blank canvas
            var normalizedImage = new Image<Rgb24>((int)maxCols, (int)maxRows, Color.White).CloneAs<L8>();

            normalizedImage.Mutate(p => p.DrawImage(croppedOriginal,
                new Point((int)cStart, (int)rStart),
                new Rectangle(0, 0, (int)(cStart + imgCols), (int)(rStart + imgRows)),
                1));

            #endregion

            #region Remove noise of canvas image

            //Remove noise - anything higher than the threshold. Note that the image is still grayscale
            // If pixel value >= threshold assign 255
            normalizedImage.Denoise(threshold);

            #endregion

            #region Resize and crop image

            // Invert image
            var invertedImage = normalizedImage.Clone(p => p.Invert());

            // Check which dimension needs to be cropped
            // (assuming the new height-width ratio may not match the original size)
            var newHeight = (float)imageSize.Width;
            var newWidth = (float)imageSize.Height;
            var widthRatio = normalizedImage.Width / newWidth;
            var heightRatio = normalizedImage.Height / newHeight;
            double resizeHeight;
            double resizeWidth;

            if (widthRatio > heightRatio)
            {
                resizeHeight = newHeight;
                resizeWidth = Math.Round(normalizedImage.Width / heightRatio);
            }
            else
            {
                resizeWidth = newWidth;
                resizeHeight = Math.Round(normalizedImage.Height / widthRatio);
            }

            Rectangle cropRectangle;
            // # Crop to exactly the desired new_size, using the middle of the image:
            if (widthRatio > heightRatio)
            {
                var start = (int)Math.Round((resizeWidth - newWidth) / 2.0);
                cropRectangle = new Rectangle(start, 0, (int)newWidth, (int)newHeight);
            }
            else
            {
                var start = (int)Math.Round((resizeHeight - newHeight) / 2.0);
                cropRectangle = new Rectangle(0, start, (int)newWidth, (int)newHeight);
            }

            //# Resize and crop the image
            invertedImage.Mutate(p =>
                p
                    .Resize((int)resizeWidth, (int)resizeHeight)
                    .Crop(cropRectangle)
            );

            #endregion

            #region Crop image to center and resize to inputSize

            // Crop to the center
            var resultImage = invertedImage.Clone();

            if (inputSize.Width != imageSize.Width || inputSize.Height != imageSize.Height)
            {
                var startX = (int)Math.Floor((double)(resultImage.Width - inputSize.Width) / 2);
                var startY = (int)Math.Floor((double)(resultImage.Height - inputSize.Height) / 2);

                resultImage.Mutate(p => p.Crop(new Rectangle(startX, startY, inputSize.Width, inputSize.Height)));

            }

            return resultImage;

            #endregion
        }

        #endregion

    }


}
