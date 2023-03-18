// See https://aka.ms/new-console-template for more information

//using System.Runtime.InteropServices.ComTypes;
//using SignatureVerificationSdk;
//using SignatureVerificationSdk.Filters;

//using SignatureVerificationSdk.Filters;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Xml.Linq;
using SignatureVerificationSdk.Utility;


var inputSize = new Size(220, 150);
var imgSize = new Size(256, 256);
var canvasSize = new Size(1360, 952);

//using SixLabors.ImageSharp.PixelFormats;

//var imageToOtsu = File.ReadAllBytes("Images/001_00.png");
var image = await Image.LoadAsync<Rgb24>("Images/001_00.png");
var img = ImageHelper.ConvertToGrayscale(image);
var blurred = image.Clone(i => i.GaussianBlur(2));

var threshold = ImageHelper.CalculateOtsuThreshold(img);
var xx = threshold / 255.0;

//var binarized = blurred.Clone(p => p.BinaryThreshold((float)xx)).CloneAs<L8>();

var result = ImageHelper.CalculateCenterOfMass2(blurred.CloneAs<L8>(), threshold);
var minCropArea = result.minumumArea;
var centerOfMass = result.centerOfMass;

var cropped = img.Clone(p => p.Crop(minCropArea));

//cropped.SaveAsJpeg("c:\\temp\\a-cropped.jpg");

var maxRows = (float)canvasSize.Width;
var maxCols = (float)canvasSize.Height;

var imgRows = minCropArea.Height * 1.0f;
var imgCols = minCropArea.Width * 1.0f;

var rStart = Math.Floor(maxRows / 2) - centerOfMass.Y;
var cStart = Math.Floor(maxCols / 2) - centerOfMass.X;

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

var normalizedImage = new Image<Rgb24>((int)maxCols, (int)maxRows, Color.White).CloneAs<L8>();

normalizedImage.Mutate(p => p.DrawImage(cropped,
    new Point((int)cStart, (int)rStart),
    new Rectangle(0, 0, (int)(cStart + imgCols), (int)(rStart + imgRows)),
    1));

// If pixel value >= threshold assign 255
ImageHelper.Denoise(normalizedImage, (int)threshold);

var invertedImage = normalizedImage.Clone(p => p.Invert());

var newHeight = (float)imgSize.Width;
var newWidth = (float)imgSize.Height;
var width_ratio = normalizedImage.Width / newWidth;
var height_ratio = normalizedImage.Height / newHeight;
double resize_height;
double resize_width;

if (width_ratio > height_ratio)
{
    resize_height = newHeight;
    resize_width = Math.Round(normalizedImage.Width / height_ratio);
}
else
{
    resize_width = newWidth;
    resize_height = Math.Round(normalizedImage.Height / width_ratio);
}

Rectangle cropRectangle;
// # Crop to exactly the desired new_size, using the middle of the image:
if (width_ratio > height_ratio)
{
    var start = (int)Math.Round((resize_width - newWidth) / 2.0);
    cropRectangle = new Rectangle(start, 0, (int)newWidth, (int)newHeight);
}
else
{
    
    var start = (int)Math.Round((resize_height - newHeight) / 2.0);
    cropRectangle = new Rectangle(0, start, (int)newWidth, (int)newHeight);
}

invertedImage.Mutate(p => 
    p
        .Resize((int)resize_width, (int)resize_height)
        .Crop(cropRectangle)
    );

invertedImage.SaveAsJpeg("c:\\temp\\a-res-crop.jpg");

Image<L8> cropped2 = invertedImage.Clone();

if (inputSize.Width != imgSize.Width || inputSize.Height != imgSize.Height)
{
    var startX = (int)Math.Floor((double)(cropped2.Width - inputSize.Width) / 2);
    var startY = (int)Math.Floor((double)(cropped2.Height - inputSize.Height) / 2);

    cropped2.Mutate(p => p.Crop(new Rectangle(startX, startY, inputSize.Width, inputSize.Height)));

}
cropped2.SaveAsJpeg("c:\\temp\\a-cropped-final.jpg");


//img_shape = img.shape
//start_y = (img_shape[0] - size[0]) // 2
//start_x = (img_shape[1] - size[1]) // 2
//cropped = img[start_y: start_y + size[0], start_x: start_x + size[1]]


var k = 0;

// calculate center of mass for this image
// calculate center of mass for image binarized
//var centerOfMass = ImageHelper.CalculateCenterOfMass(binarized);

//// calculate min crop area for binarized
//var minCropArea = ImageHelper.CalculateMinCropArea(binarized);

//

//Console.WriteLine("minCropArea" + minCropArea);
//Console.WriteLine("centerOfMass"+ centerOfMass);

//var imgRows = minCropArea.Height * 1.0f;
//var imgCols = minCropArea.Width * 1.0f;

//var maxRows = 952.0f;
//var maxCols = 1360.0f;

//var rStart = Math.Floor(maxRows / 2) - centerOfMass.Y;
//var cStart = Math.Floor(maxCols / 2) - centerOfMass.X;

//if (imgRows > maxRows)// Case 1: image larger than required (height):  Crop.
//{
//    rStart = 0;
//    var difference = imgRows - maxRows;
//    var cropStart = Math.Floor(difference / 2);
//    //cropped.Mutate(p=>p.Crop());
//    //cropped = cropped[crop_start: crop_start + max_rows,  :]
//    //var cropped = 
//    imgRows = maxRows;
//}
//else // Case 2: centering exactly would require a larger image. relax the centering of the image
//{
//    var extraR = (rStart + imgRows) - maxRows;
//    if (extraR > 0) 
//        rStart -= extraR;
//    if (rStart < 0)
//        rStart = 0;
//} 

//if (imgCols > maxCols) //Case 3: image larger than required (width). Crop.
//{
//    cStart = 0;
//    var difference = imgCols - maxCols;
//    var cropStart = Math.Floor(difference / 2);
//    //cropped = cropped[:, crop_start: crop_start + max_cols]
//    imgCols = maxCols;
//}
//else //# Case 4: centering exactly would require a larger image. relax the centering of the image
//{
//    var extraC = (cStart + imgCols) - maxCols;
//    if (extraC > 0)
//        cStart -= extraC;
//    if (cStart < 0)
//        cStart = 0;
//}

//// create a blank canvas in imagesharp
//var normalizedImage = new Image<Rgb24>((int)maxCols, (int)maxRows, Color.White).CloneAs<L8>();

////normalized_image[r_start: r_start + img_rows, c_start: c_start + img_cols] = cropped

//// Add cropped image on blank canvas
//normalizedImage.Mutate(p => p.DrawImage(cropped,
//    new Point((int)cStart, (int)rStart), 
//    new Rectangle(0, 0, (int)(cStart + imgCols), (int)(rStart + imgRows)), 
//    1));

//// If pixel value >= threshold assign 255
//ImageHelper.Denoise(normalizedImage, (int)threshold);
//await normalizedImage.SaveAsJpegAsync("c:\\temp\\test.jpg");


//normalizedImage.Mutate(p => p.Invert());

//await normalizedImage.SaveAsJpegAsync("c:\\temp\\test-inverted.jpg");


//var newHeight = 256.0;
//var newWidth = 256.0;
//var width_ratio = normalizedImage.Width / newWidth;
//var height_ratio = normalizedImage.Height / newHeight;
//double resize_height;
//double resize_width;

//if (width_ratio > height_ratio)
//{
//    resize_height = newHeight;
//    resize_width = Math.Round(normalizedImage.Width / height_ratio);
//}
//else
//{
//    resize_width = newWidth;
//    resize_height = Math.Round(normalizedImage.Height / width_ratio);
//}


//var center = new Point();
//// # Crop to exactly the desired new_size, using the middle of the image:
//if (width_ratio > height_ratio)
//{
//    var start = (int)Math.Round((resize_width - newWidth) / 2.0);
//    //return img[:, start: start + width]
//    center.X = start;
//    center.Y = (int)(start + newWidth);
//}
//else
//{
//    var start = (int)Math.Round((resize_height - newHeight) / 2.0);
//    //return img[start: start + height, :]
//    center.X = start;
//    center.Y = (int)(start + newHeight);
//}


//normalizedImage.Mutate(p => p.Resize(new ResizeOptions
//{
//    Size = new Size((int)resize_width, (int)resize_height),
//    Mode = ResizeMode.Manual,
//    TargetRectangle = new Rectangle(center, new Size((int)newWidth, (int)newHeight))
//}));

//await normalizedImage.SaveAsJpegAsync("c:\\temp\\test-resized.jpg");

//var j = xx;

////calculate the min crop area that contain all image
//var minCropArea = new MinCropArea();

//// calculate center of mass for this image
//var centerOfMass = new CenterOfMass();


// Create new method to calculate otsu threshold imagesharp
//var otsu = new OtsuThresholder();

//var threshold = otsu.CalculateThreshold(image);




//var clone = image.Clone(Configuration.Default, i => i.GaussianBlur(2));

//var otsu = new OtsuThresholder();
//var threshold = otsu.CalculateThreshold(image);

//int width = image.Width;
//int height = image.Height;
//byte[,] m = new byte[height, width];
//for (int r = 0; r < height; r++)
//{
//    for (int c = 0; c < width; c++)
//    {
//        var color = image[c, r];
//        m[r, c] = (byte)Math.Round((color.R * 1.0) + (color.G * 1.0) + (color.B * 1.0));
//    }
//}

////image.Clone(i=> i.BinaryThreshold(threshold))


//var k = m;

//Console.WriteLine("Threshold: {0}", threshold.threshold);
//var binarizedImage = threshold.binarizedImage;

//var binarizedMatrix = DataTools.Array2Matrix(binarizedImage, image.Width, image.Height);



//#region Image cleaning

//if (!Directory.Exists("Output"))
//{
//    Directory.CreateDirectory("Output");
//}

//if (!Directory.Exists("Output/Cleaning"))
//{
//    Directory.CreateDirectory("Output/Cleaning");
//}

//if (!Directory.Exists("Output/Verification"))
//{
//    Directory.CreateDirectory("Output/Verification");
//}

//var imageToClean = File.ReadAllBytes("Images/001_01.png");
//var signatureCleaning = new SignatureImageCleaning();
//var outputCleanedImage = signatureCleaning.CleanImage(imageToClean);

//outputCleanedImage.SaveAsync("Output/Cleaning/c.png");
//#endregion

//#region Image verification
//var image01 = File.ReadAllBytes("Images/001_00.png");
//var image02 = File.ReadAllBytes("Images/001_forg_00.png");

//var signatureVerification = new SignatureVerification();
//var outputVerification = signatureVerification.VerifySignatures(image01, image02);

//Console.WriteLine($"Confidence: {outputVerification.Confidence} - Similarity: {outputVerification.Similarity}");

//signatureVerification.PreprocessedSourceImage1.SaveAsync("Output/Verification/001_preprocessed.png");
//signatureVerification.PreprocessedSourceImage2.SaveAsync("Output/Verification/002_preprocessed.png");

//#endregion