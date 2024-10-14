using Assets.GifAssets.PowerGif;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Share;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using System.Drawing.Imaging;

public class ImageLoader : MonoBehaviour
{
    public UnityEngine.UI.Image avatarImage;

    // This method is called by the button (Button component)
    public void UpdateAvatar()
    {
        // Requesting a file from the user
        FileUploaderHelper.RequestFile((path) =>
        {
            // If the path is empty, ignore it.
            if (string.IsNullOrWhiteSpace(path))
                return;

            // Run a coroutine to load an image
            StartCoroutine(UploadImage(path));
        });
    }

    // Coroutine for image upload
    IEnumerator UploadImage(string path)
    {
        // This is where the texture will be stored.
        //Texture2D texture;

        // using to automatically call Dispose, create a request along the path to the file
        using (UnityWebRequest imageWeb = new UnityWebRequest(path, UnityWebRequest.kHttpVerbGET))
        {
            // We create a "downloader" for textures and pass it to the request
            //imageWeb.downloadHandler = new DownloadHandlerTexture();

            imageWeb.downloadHandler = new DownloadHandlerBuffer();

           // We send a request, execution will continue after the entire file have been downloaded
           yield return imageWeb.SendWebRequest();

            // Getting the texture from the "downloader"
            //texture = ((DownloadHandlerTexture)imageWeb.downloadHandler).texture;

            byte[] bytes = imageWeb.downloadHandler.data;
        }

        // Create a sprite from a texture and pass it to the avatar image on the UI
        //avatarImage.sprite = Sprite.Create(
        //    texture,
        //    new Rect(0.0f, 0.0f, texture.width, texture.height),
        //    new Vector2(0.5f, 0.5f));
    }

    [SerializeField] private Transform imagesContainer;
    private AnimatedImage[] animatedImages;
    private void LoadGif(byte[] bytes, bool use)
    {
        animatedImages = imagesContainer.GetComponentsInChildren<AnimatedImage>();

        //var path = $"Assets/GifAssets/PowerGif/Examples/Samples/{fileName}.gif";

        //if (path == "") return;

        //var bytes = File.ReadAllBytes(path);
        var gif = Gif.Decode(bytes);

        foreach (var i in animatedImages)
        {
            i.Play(gif);
        }
    }

    public string loadingGifPath;
    public float speed = 1;
    public Vector2 drawPosition;

    static List<Texture2D> gifFrames = new List<Texture2D>();
    private  void LoadGif(byte[] bytes, string res)
    {
        Stream stream = new MemoryStream(bytes);

        var gifImage = System.Drawing.Image.FromStream(stream);

        var dimension = new FrameDimension(gifImage.FrameDimensionsList[0]);
        int frameCount = gifImage.GetFrameCount(dimension);
        for (int i = 0; i < frameCount; i++)
        {
            gifImage.SelectActiveFrame(dimension, i);
            var frame = new Bitmap(gifImage.Width, gifImage.Height);
            System.Drawing.Graphics.FromImage(frame).DrawImage(gifImage, System.Drawing.Point.Empty);
            var frameTexture = new Texture2D(frame.Width, frame.Height);
            for (int x = 0; x < frame.Width; x++)
                for (int y = 0; y < frame.Height; y++)
                {
                    System.Drawing.Color sourceColor = frame.GetPixel(x, y);
                    frameTexture.SetPixel(frame.Width - 1 - x, -y, new Color32(sourceColor.R, sourceColor.G, sourceColor.B, sourceColor.A)); // for some reason, x is flipped
                }
            frameTexture.Apply();
            gifFrames.Add(frameTexture);
        }
    }

    

    public void DownLoadGif(GifData gifData)
    {
        StartCoroutine(DownloadImage(gifData));
    }

    IEnumerator DownloadImage(GifData gifData)
    {
        //Debug.Log($"path {path}");

        // This is where the texture will be stored.
        Texture2D texture;

        // using to automatically call Dispose, create a request along the path to the file
        using (UnityWebRequest imageWeb = new UnityWebRequest(gifData.url, UnityWebRequest.kHttpVerbGET))
        {
            // We create a "downloader" for textures and pass it to the request
            imageWeb.downloadHandler = new DownloadHandlerTexture();
            // We send a request, execution will continue after the entire file have been downloaded
            yield return imageWeb.SendWebRequest();

            // Getting the texture from the "downloader"
            texture = DownloadHandlerTexture.GetContent(imageWeb);

            var sprite =  Sprite.Create(texture,
            new Rect(0.0f, 0.0f, texture.width, texture.height),
            new Vector2(0f, 1f));

            AssingSprite(sprite, gifData);
        }
    }

    [SerializeField] private AnimatedSprite animatedSprite;
    private void AssingSprite(Sprite sprite, GifData gifData)
    {
        animatedSprite.Assing(sprite, gifData);
    }
}
