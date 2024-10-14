using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Newtonsoft.Json;
using Share;

namespace Mafia_Server
{
    internal class GifConverter
    {
        public GifConverter()
        {
            //LoadGif();
            ConvertGifToPng();
        }


        private static string fileName = "4";

        private static GifData gifData;

        private void ConvertGifToPng()
        {
            var resultFileName = $"E:/GIF/{fileName}.gif";

            if (!File.Exists(resultFileName)) return;

            var gifImage = Image.FromFile($"E:/GIF/{fileName}.gif");

            var dimension = new FrameDimension(gifImage.FrameDimensionsList[0]);

            int frameCount = gifImage.GetFrameCount(dimension);

            var resultImage = new Bitmap(gifImage.Width * frameCount, gifImage.Height, PixelFormat.Format16bppArgb1555);

            var index = 0;
            int[] delays = new int[frameCount];

            for (int i = 0; i < frameCount; i++)
            {
                gifImage.SelectActiveFrame(dimension, i);
                var frame = new Bitmap(gifImage.Width, gifImage.Height);
                Graphics.FromImage(frame).DrawImage(gifImage, Point.Empty);

               var this_delay = BitConverter.ToInt32(gifImage.GetPropertyItem(20736).Value, index) * 10;
                index += 4;

                delays[i]= this_delay;

                for (int x = 0; x < frame.Width; x++)
                    for (int y = 0; y < frame.Height; y++)
                    {
                        Color sourceColor = frame.GetPixel(x, y);

                        resultImage.SetPixel(i * frame.Width + x, y, sourceColor);
                    }
            }

            gifData = new GifData($"E:/GIF/LongGif/{fileName}.png", delays);

            resultImage.Save($"E:/GIF/LongGif/{fileName}.png");
        }

        public static string GetGifBytes()
        {
            return $"E:/GIF/LongGif/{fileName}.png";
        }

        public static string GetGifData()
        {
            var result = JsonConvert.SerializeObject(gifData);
            return result;
        }
    }
}

