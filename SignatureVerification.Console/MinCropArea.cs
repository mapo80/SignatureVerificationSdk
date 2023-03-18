//// See https://aka.ms/new-console-template for more information

//internal class MinCropArea
//{
//    public MinCropArea()
//    {
//    }

//    //calculate the min crop area that contain all image
//    public (int x, int y, int width, int height) CalculateMinCropArea(Image<Rgb24> image)
//    {
//        var width = image.Width;
//        var height = image.Height;
//        var x = 0;
//        var y = 0;
//        var total = 0;
//        for (var r = 0; r < height; r++)
//        {
//            for (var c = 0; c < width; c++)
//            {
//                var color = image[c, r];
//                var gray = (byte)Math.Round((color.R * 1.0) + (color.G * 1.0) + (color.B * 1.0));
//                if (gray > 0)
//                {
//                    x += c;
//                    y += r;
//                    total++;
//                }
//            }
//        }
//        return (x / total, y / total, width, height);
//    }
//}