// See https://aka.ms/new-console-template for more information

using System.Globalization;
using CsvHelper;
using SignatureVerification.Console.Dto;
using SixLabors.ImageSharp;
using SignatureVerificationSdk;
using SixLabors.ImageSharp.Processing;
using CsvHelper.Configuration;

var executeImageCleaning = false;
var executeImageVerification = false;
var executeTest = false;
var analyzePerformance = true;


#region Temp directory creation
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
#endregion

#region Old code
//var image01Clean = File.ReadAllBytes("Images/001_01-clean.png");
//var newImg = Image.Load<L8>(image01Clean)
//    .Clone(p => p.Resize(new ResizeOptions
//     {
//         Mode = ResizeMode.Max,
//         Size = new Size(600, 600)

//     }));

//var massInfo = ImagePreprocessing.CalculateMassInfo(newImg, 0);

//var minCropArea = massInfo.MinumumArea;
//var centerOfMass = massInfo.CenterOfMass;

//// Crop the image with a tight box
//var croppedOriginal = newImg
//    .Clone(p => p.Crop(minCropArea));


//croppedOriginal.SaveAsPng("Output/Cleaning/temp6.png");

//var inputSize = new Size(220, 150);
//var imgSize = new Size(256, 256);
//var canvasSize = new Size(1360, 952);
////var result = newImg.PreprocessImage(inputSize, imgSize, canvasSize);

////result.SaveAsPng("Output/Cleaning/temp.png");
#endregion

#region Image cleaning

if (executeImageCleaning)
{
    var imageToClean = File.ReadAllBytes("Images/001_01.png");
    var signatureCleaning = new SignatureImageCleaning();
    var outputCleanedImage = signatureCleaning.CleanImage(imageToClean);
    //    .Clone(p => p.Resize(new ResizeOptions
    //{
    //    Mode = ResizeMode.Max,
    //    Size = new Size(600, 600)

    //}));

    outputCleanedImage.SaveAsync("Output/Cleaning/001_01.png");

    imageToClean = File.ReadAllBytes("Images/001_forg_00.png");
    signatureCleaning = new SignatureImageCleaning();
    outputCleanedImage = signatureCleaning.CleanImage(imageToClean);
        //.Clone(p => p.Resize(new ResizeOptions
        //{
        //    Mode = ResizeMode.Max,
        //    Size = new Size(600, 600)

        //}));

    outputCleanedImage.SaveAsync("Output/Cleaning/001_forg_00.png");
}

#endregion

#region Image verification

if (executeImageVerification)
{
    var image01 = File.ReadAllBytes("Output/Cleaning/001_01.png");
    var image02 = File.ReadAllBytes("Output/Cleaning/001_forg_00.png");

    //var image01 = File.ReadAllBytes("Images/001_01.png");
    //var image02 = File.ReadAllBytes("Images/001_forg_00.png");

    var signatureVerification = new SignatureVerificationSdk.SignatureVerification();
    var outputVerification = signatureVerification.VerifySignatures(image01, image02);

    Console.WriteLine($"Confidence: {outputVerification.Confidence} - Similarity: {outputVerification.Similarity}");

    signatureVerification.PreprocessedSourceImage1.SaveAsync("Output/Verification/001_preprocessed.png");
    signatureVerification.PreprocessedSourceImage2.SaveAsync("Output/Verification/002_preprocessed.png");
}


#endregion

#region Test
// create dto for test_data.csv for csvhelper


// Load test_data.csv using CsvHelper
if (executeTest)
{
    var results = new List<SignatureVerificationOutputDto>();
    var config = new CsvConfiguration(CultureInfo.InvariantCulture)
    {
        HasHeaderRecord = false,
    };
    using var reader = new StreamReader("test_data.csv");
    using var csv = new CsvReader(reader, config);
    var records = csv.GetRecords<SignatureVerificationDto>();
    var signatureVerification = new SignatureVerificationSdk.SignatureVerification();

    try
    {
        Parallel.ForEach(records, new ParallelOptions {MaxDegreeOfParallelism = 4},record =>
        {
            try
            {
                var image01 = File.ReadAllBytes("test/" + record.Image1);
                var image02 = File.ReadAllBytes("test/" + record.Image2);
                var outputVerification = signatureVerification.VerifySignatures(image01, image02);
                var isForged = outputVerification.Similarity < 0.5;

                var output = new SignatureVerificationOutputDto
                {
                    IsForged = record.IsForged,
                    CalculatedResult = isForged,
                    IsSameResult = isForged == record.IsForged,
                    Confidence = outputVerification.Confidence,
                    Similarity = outputVerification.Similarity,
                    Image1 = record.Image1,
                    Image2 = record.Image2,
                };
                results.Add(output);

                Console.WriteLine($"Is really forged: {record.IsForged} - Is forged: {isForged} - Confidence: {outputVerification.Confidence} - Similarity: {outputVerification.Similarity}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        });
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
    }

    // save results to csv
    using var writer = new StreamWriter("Output/test_data_results.csv");
    using var csvWriter = new CsvWriter(writer, config);
    csvWriter.WriteRecords(results);
}

#endregion

#region Performance analysis
// Load test_data_result.csv using CsvHelper
if (analyzePerformance)
{
    var config = new CsvConfiguration(CultureInfo.InvariantCulture)
    {
        HasHeaderRecord = false,
    };

    using var reader = new StreamReader("test_data_results.csv");
    using var csv = new CsvReader(reader, config);
    var records = csv.GetRecords<SignatureVerificationOutputDto>().ToList();
    var truePositives = records.Count(x => x.IsForged && x.CalculatedResult);
    var trueNegatives = records.Count(x => !x.IsForged && !x.CalculatedResult);
    var falsePositives = records.Count(x => !x.IsForged && x.CalculatedResult);
    var falseNegatives = records.Count(x => x.IsForged && !x.CalculatedResult);
    var accuracy = (truePositives + trueNegatives) / (double)records.Count();
    var precision = truePositives / (double)(truePositives + falsePositives);
    var recall = truePositives / (double)(truePositives + falseNegatives);
    var f1Score = 2 * ((precision * recall) / (precision + recall));
    Console.WriteLine($"Accuracy: {accuracy}");
    Console.WriteLine($"Precision: {precision}");
    Console.WriteLine($"Recall: {recall}");
    Console.WriteLine($"F1-Score: {f1Score}");
}
#endregion