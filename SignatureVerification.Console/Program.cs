// See https://aka.ms/new-console-template for more information

//using System.Runtime.InteropServices.ComTypes;
//using SignatureVerificationSdk;
//using SignatureVerificationSdk.Filters;

//using SignatureVerificationSdk.Filters;
using SixLabors.ImageSharp;
using SignatureVerificationSdk;


#region Image cleaning

if (!Directory.Exists("Output"))
{
    Directory.CreateDirectory("Output");
}

if (!Directory.Exists("Output/Cleaning"))
{
    Directory.CreateDirectory("Output/Cleaning");
}

if (!Directory.Exists("Output/Verification"))
{
    Directory.CreateDirectory("Output/Verification");
}

var imageToClean = File.ReadAllBytes("Images/001_01.png");
var signatureCleaning = new SignatureImageCleaning();
var outputCleanedImage = signatureCleaning.CleanImage(imageToClean);

outputCleanedImage.SaveAsync("Output/Cleaning/c.png");
#endregion

#region Image verification
var image01 = File.ReadAllBytes("Images/001_00.png");
var image02 = File.ReadAllBytes("Images/001_forg_00.png");

var signatureVerification = new SignatureVerification();
var outputVerification = signatureVerification.VerifySignatures(image01, image02);

Console.WriteLine($"Confidence: {outputVerification.Confidence} - Similarity: {outputVerification.Similarity}");

signatureVerification.PreprocessedSourceImage1.SaveAsync("Output/Verification/001_preprocessed.png");
signatureVerification.PreprocessedSourceImage2.SaveAsync("Output/Verification/002_preprocessed.png");

#endregion