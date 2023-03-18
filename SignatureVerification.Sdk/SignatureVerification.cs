using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using SignatureVerificationSdk.Dto;
using SignatureVerificationSdk.Helpers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SignatureVerificationSdk.Utility;

namespace SignatureVerificationSdk
{
    public class SignatureVerification
    {
        private readonly byte[] _model;
        private readonly SignatureImageCleaning _imageCleaning;
        public Image<L8> PreprocessedSourceImage1 { get; private set; }
        public Image<L8> PreprocessedSourceImage2 { get; private set; }

        public SignatureVerification()
        {
            _model = LoadModel();
            _imageCleaning = new SignatureImageCleaning();
        }

        #region Public methods
        public SignatureVerificationResult VerifySignatures(Stream sourceImage1, Stream sourceImage2)
        {
            if (sourceImage1 == null)
            {
                throw new ArgumentNullException(nameof(sourceImage1), "Image cannot be null");
            }

            if (sourceImage2 == null)
            {
                throw new ArgumentNullException(nameof(sourceImage2), "Image cannot be null");
            }

            var inputSize = new Size(220, 150);
            var imgSize = new Size(256, 256);
            var canvasSize = new Size(1360, 952);

            PreprocessedSourceImage1 = sourceImage1.PreprocessImage(inputSize, imgSize, canvasSize);
            PreprocessedSourceImage2 = sourceImage2.PreprocessImage(inputSize, imgSize, canvasSize);

            var inputTensor1 = ConvertImageToTensor(PreprocessedSourceImage1);
            var inputTensor2 = ConvertImageToTensor(PreprocessedSourceImage2);

            var inferenceOutput = RunInference(inputTensor1, inputTensor2);

            var output0 = inferenceOutput.Output0.Select(p => (double)p).ToArray();
            var output1 = inferenceOutput.Output1.Select(p => (double)p).ToArray();

            var confidence = Sigmoid.CalculateSigmoid(inferenceOutput.Output2.GetValue(0));
            var similarity = CosineSimilarity.GetSimilarityScore(output0, output1);

            return new SignatureVerificationResult
            {
                Confidence = confidence,
                Similarity = similarity
            };

        }

        public SignatureVerificationResult VerifySignatures(byte[] sourceImage1, byte[] sourceImage2)
        {
            if (sourceImage1 == null)
            {
                throw new ArgumentNullException(nameof(sourceImage1), "Image cannot be null");
            }

            if (sourceImage2 == null)
            {
                throw new ArgumentNullException(nameof(sourceImage2), "Image cannot be null");
            }

            return VerifySignatures(sourceImage1.ToStream(), sourceImage2.ToStream());
        }
        #endregion

        #region Private methods
        private byte[] LoadModel()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "SignatureVerificationSdk.Models.model-signature-verification.onnx";

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null) return null;
                byte[] ba = new byte[stream.Length];
                stream.Read(ba, 0, ba.Length);
                return ba;
            }
        }

        private Tensor<byte> ConvertImageToTensor(Image<L8> image)
        {
            var tensor = new DenseTensor<byte>(new[] { 150, 220 });
            image.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < accessor.Height; y++)
                {
                    Span<L8> pixelSpan = accessor.GetRowSpan(y);
                    for (int x = 0; x < accessor.Width; x++)
                    {
                        tensor[y, x] = pixelSpan[x].PackedValue;
                    }
                }
            });

            return tensor;
        }

        private VerificationInferenceResult RunInference(Tensor<byte> inputTensor1, Tensor<byte> inputTensor2)
        {
            var inferenceInput = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("input0", inputTensor1),
                NamedOnnxValue.CreateFromTensor("input1", inputTensor2)
            };

            var inferenceSession = new InferenceSession(_model);
            var inferenceResult = inferenceSession.Run(inferenceInput);

            if (inferenceResult.ElementAt(0)?.Value is not IEnumerable<float> output0)
                throw new ApplicationException("Unable to process image");

            if (inferenceResult.ElementAt(1)?.Value is not IEnumerable<float> output1)
                throw new ApplicationException("Unable to process image");

            if (inferenceResult.ElementAt(2)?.Value is not Tensor<float> output2)
                throw new ApplicationException("Unable to process image");


            return new VerificationInferenceResult
            {
                Output0 = output0,
                Output1 = output1,
                Output2 = output2
            };
        }
        #endregion
    }
}
