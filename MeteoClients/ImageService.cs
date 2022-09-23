using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing;

namespace GIBS.API.Client
{
    public class ImageService
    {
        public async Task<bool> MergeImages(string filePathA, string filePathB, Stream newFileStream)
        {
            if (string.IsNullOrWhiteSpace(filePathA))
                throw new ArgumentException();

            return await Task.Run(() =>
            {
                var height = 1024;
                var width = 512;

                using (var bitmap = new Bitmap(width, height))
                {
                    using (var canvas = Graphics.FromImage(bitmap))
                    {
                        using var topFrame = Image.FromFile(filePathA);
                        using var bottomFrame = Image.FromFile(filePathB);

                        canvas.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        canvas.DrawImage(topFrame,
                                         new Rectangle(0,
                                                       0,
                                                       width,
                                                       height / 2),
                                         new Rectangle(0,
                                                       0,
                                                       topFrame.Width,
                                                       topFrame.Height),
                                         GraphicsUnit.Pixel);
                        canvas.DrawImage(bottomFrame, (bitmap.Width / 2) - (bottomFrame.Width / 2), (bitmap.Height / 2));
                        canvas.Save();
                    }
                    try
                    {
                        bitmap.Save(newFileStream, ImageFormat.Png);
                    }
                    catch
                    {
                        Console.WriteLine("Some error happened");
                        return false;
                    }

                    return true;
                }
            });
        }
    }
}



