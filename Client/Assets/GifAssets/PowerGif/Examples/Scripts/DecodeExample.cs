using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Assets.GifAssets.PowerGif.Examples.Scripts
{	
	/// <summary>
	/// Decoding GIF example.
	/// </summary>
	public class DecodeExample : MonoBehaviour
	{
		[SerializeField] private Transform imagesContainer;
		private AnimatedImage[] animatedImages;

        public string fileName;

        public void Start()
		{
   //         animatedImages = imagesContainer.GetComponentsInChildren<AnimatedImage>();

   //         var path = $"Assets/GifAssets/PowerGif/Examples/Samples/{fileName}.gif";

			//if (path == "") return;

			//var bytes = File.ReadAllBytes(path);
			//var gif = Gif.Decode(bytes);

   //         foreach (var i in  animatedImages)
   //         {
			//	i.Play(gif);
   //         }
		}

		public void Review()
		{
			Application.OpenURL("https://www.assetstore.unity3d.com/#!/content/121731");
		}
	}
}