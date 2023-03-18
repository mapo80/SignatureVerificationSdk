//// <copyright file="OtsuThresholder.cs" company="QutEcoacoustics">
//// All code in this file and all associated files are the copyright and property of the QUT Ecoacoustics Research Group (formerly MQUTeR, and formerly QUT Bioacoustics Research Group).
//// </copyright>

//using System;
//using SignatureVerificationSdk.Utility;
//using SixLabors.ImageSharp;
//using SixLabors.ImageSharp.PixelFormats;

//namespace SignatureVerificationSdk.Filters
//{
//    /// <summary>
//    /// Go to following link for info on Otsu threshold
//    /// http://www.labbookpages.co.uk/software/imgProc/otsuThreshold.html.
//    /// </summary>
//    public class OtsuThresholder
//    {

//        private readonly int[] _histData;
//        private int _maxLevelValue;
//        private int _threshold;

//        /// <summary>
//        /// Initializes a new instance of the <see cref="OtsuThresholder"/> class.
//        /// CONSTRUCTOR.
//        /// </summary>
//        public OtsuThresholder()
//        {
//            _histData = new int[256];
//        }

//        public int[] GetHistData()
//        {
//            return _histData;
//        }

//        public int GetMaxLevelValue()
//        {
//            return _maxLevelValue;
//        }

//        public int GetThreshold()
//        {
//            return _threshold;
//        }

//        public int CalculateThreshold(Image<Rgb24> srcImage)
//        {
//            int width = srcImage.Width;
//            int height = srcImage.Height;
                
//            // Get raw image data
//            byte[,] M = ConvertColourImageToGreyScaleMatrix(srcImage);

//            // Sanity check image
//            if ((width * height) != (M.GetLength(0) * M.GetLength(1)))
//            {
//                throw new Exception("Unexpected image data size");
//            }

//            byte[,] matrix = ConvertColourImageToGreyScaleMatrix(srcImage);
//            double[,] ipMatrix = DataTools.ConvertMatrixOfByte2Double(matrix);

//            var normMatrix = DataTools.NormaliseInZeroOne(ipMatrix, out _, out _);
//            var byteMatrix = DataTools.ConvertMatrixOfDouble2Byte(normMatrix);

//            byte[] vector = DataTools.Matrix2Array(byteMatrix);

//            return CalculateThreshold(vector);

//            //#region Create array with threshold
//            //var total = vector.Length;
//            //var monoData = new byte[total];
//            //var ptr = 0;
//            //while (ptr < vector.Length)
//            //{
//            //    monoData[ptr] = (byte)((0xFF & vector[ptr]) >= threshold ? (byte)255 : 0);
//            //    ptr++;
//            //}
//            //#endregion

//            //return (threshold, monoData);
//        }

//        public int CalculateThreshold(byte[] srcData)
//        {
//            int ptr;

//            // Clear histogram data
//            // Set all values to zero
//            ptr = 0;
//            while (ptr < _histData.Length)
//            {
//                _histData[ptr++] = 0;
//            }

//            // Calculate histogram and find the level with the max value
//            // Note: the max level value isn't required by the Otsu method
//            ptr = 0;
//            _maxLevelValue = 0;
//            while (ptr < srcData.Length)
//            {
//                int h = 0xFF & srcData[ptr];
//                _histData[h]++;
//                if (_histData[h] > _maxLevelValue)
//                {
//                    _maxLevelValue = _histData[h];
//                }

//                ptr++;
//            }

//            // Total number of pixels
//            int total = srcData.Length;

//            float sum = 0;
//            for (int t = 0; t < 256; t++)
//            {
//                sum += t * _histData[t];
//            }

//            float sumB = 0;
//            int wB = 0;

//            float varMax = 0;
//            _threshold = 0;

//            for (int t = 0; t < 256; t++)
//            {
//                wB += _histData[t];                 // Weight Background
//                if (wB == 0)
//                {
//                    continue;
//                }

//                var wF = total - wB;
//                if (wF == 0)
//                {
//                    break;
//                }

//                sumB += t * _histData[t];

//                float mB = sumB / wB;               // Mean Background
//                float mF = (sum - sumB) / wF;       // Mean Foreground

//                // Calculate Between Class Variance
//                float varBetween = wB * (float)wF * (mB - mF) * (mB - mF);

//                // Check if new maximum found
//                if (varBetween > varMax)
//                {
//                    varMax = varBetween;
//                    _threshold = t;
//                }
//            }

//            return _threshold;
//        } //doThreshold

//        private byte[,] ConvertColourImageToGreyScaleMatrix(Image<Rgb24> image)
//        {
//            int width = image.Width;
//            int height = image.Height;
//            byte[,] m = new byte[height, width];
//            for (int r = 0; r < height; r++)
//            {
//                for (int c = 0; c < width; c++)
//                {
//                    var color = image[c, r];
//                    m[r, c] = (byte)Math.Round((color.R * 0.2126) + (color.G * 0.7152) + (color.B * 0.0722));
//                }
//            }

//            return m;
//        }
//    }
//}