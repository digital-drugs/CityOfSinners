using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class GifImage : MonoBehaviour
{
    //public string loadingGifPath;
    //public float speed = 1;
    //public Vector2 drawPosition;

    //List<Texture2D> gifFrames = new List<Texture2D>();
    //void Awake()
    //{
    //    var gifImage = Image.FromFile($"Assets/GifAssets/PowerGif/Examples/Samples/{loadingGifPath}.gif");
    //    var dimension = new FrameDimension(gifImage.FrameDimensionsList[0]);
    //    int frameCount = gifImage.GetFrameCount(dimension);
    //    for (int i = 0; i < frameCount; i++)
    //    {
    //        gifImage.SelectActiveFrame(dimension, i);
    //        var frame = new Bitmap(gifImage.Width, gifImage.Height);
    //        System.Drawing.Graphics.FromImage(frame).DrawImage(gifImage, Point.Empty);
    //        var frameTexture = new Texture2D(frame.Width, frame.Height);
    //        for (int x = 0; x < frame.Width; x++)
    //            for (int y = 0; y < frame.Height; y++)
    //            {
    //                System.Drawing.Color sourceColor = frame.GetPixel(x, y);
    //                frameTexture.SetPixel(frame.Width - 1 - x, -y, new Color32(sourceColor.R, sourceColor.G, sourceColor.B, sourceColor.A)); // for some reason, x is flipped
    //            }
    //        frameTexture.Apply();
    //        gifFrames.Add(frameTexture);
    //    }
    //}


    //void OnGUI()
    //{
    //    for (int i = 0; i < 5; i++)
    //    {
    //        for (int j = 0; j < 5; j++)
    //        {
    //            GUI.DrawTexture(new Rect(drawPosition.x +i*50, drawPosition.y+j*50, gifFrames[0].width, gifFrames[0].height), gifFrames[(int)(Time.frameCount * speed) % gifFrames.Count]);

    //        }
    //    }
    //}
}