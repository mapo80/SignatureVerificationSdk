using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using SignatureVerificationSdk.Helpers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace SignatureVerificationSdk
{
    public class SignatureImageCleaning
    {
        private const int Width = 224;
        private const int Height = 224;
        private readonly byte[] _model;

        public SignatureImageCleaning()
        {
            _model = LoadModel();
        }

        #region Public methods
        public Image<L8> CleanImage(byte[] image)
        {
            if (image == null)
            {
                throw new ArgumentNullException(nameof(image), "Image cannot be null");
            }

            return CleanImage(image.ToStream());
        }

        public Image<L8> CleanImage(Stream image)
        {
            var inputImage = LoadImage(image);
            var inputTensor = ConvertImageToTensor(inputImage);
            var outputImage = RunInference(inputTensor);

            return outputImage;
        }

        #endregion

        #region Private methods
        private byte[] LoadModel()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "SignatureVerificationSdk.Models.model-image-cleaning.onnx";

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null) return null;
                byte[] ba = new byte[stream.Length];
                stream.Read(ba, 0, ba.Length);
                return ba;
            }

        }

        private Image<Rgb24> LoadImage(Stream image)
        { 
            return Image.Load<Rgb24>(image)
                .Clone(ctx =>
            {
                ctx
                    .GaussianBlur(0.5f)
                    .AdaptiveThreshold(0.8f)
                    .Resize(new ResizeOptions
                    {
                        Size = new Size(Width, Height),
                        Mode = ResizeMode.Pad,
                        Sampler = KnownResamplers.Bicubic,
                        PadColor = Color.White
                    });
            });
        }

        private Tensor<float> ConvertImageToTensor(Image<Rgb24> image)
        {
            var tensor = new DenseTensor<float>(new[] { 1, 224, 224, 3 });
            image.ProcessPixelRows(accessor =>
            {
                for (var y = 0; y < accessor.Height; y++)
                {
                    Span<Rgb24> pixelSpan = accessor.GetRowSpan(y);
                    for (var x = 0; x < accessor.Width; x++)
                    {
                        tensor[0, y, x, 0] = pixelSpan[x].R / 255f;
                        tensor[0, y, x, 1] = pixelSpan[x].G / 255f;
                        tensor[0, y, x, 2] = pixelSpan[x].B / 255f;
                    }
                }
            });

            return tensor;
        }

        private Image<L8> ConvertTensorToImage(Tensor<float> tensor)
        {
            var imageResult = new Image<L8>(224, 224);
            for (var y = 0; y < 224; y++)
            {
                for (var x = 0; x < 224; x++)
                {
                    var luminance = (byte)(tensor[0, y, x, 0] * 255);

                    imageResult[x, y] = new L8(luminance);
                }
            }

            return imageResult;
        }

        private Image<L8> RunInference(Tensor<float> tensor)
        {
            var inferenceInput = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor("input", tensor) };
            var inferenceSession = new InferenceSession(_model);
            var inferenceResult = inferenceSession.Run(inferenceInput);

            if (inferenceResult.FirstOrDefault()?.Value is not Tensor<float> output)
                throw new ApplicationException("Unable to process image");

            return ConvertTensorToImage(output);
        }
        #endregion
    }
}
