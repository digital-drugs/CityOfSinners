using Share;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnimatedSprite : MonoBehaviour
{

    [SerializeField] private Image image;
    public void Assing(Sprite sprite, GifData gifData)
    {
        this.gifData = gifData;

        image.sprite = sprite;
        image.GetComponent<RectTransform>().sizeDelta= new Vector2(sprite.texture.width, sprite.texture.height);
        frameCount = sprite.texture.width / 256;

    }

    GifData gifData;
    private float nextFrameTime;
    private int frameCount;
    private int currentFrame=0;
    void Update()
    {
        if (image.sprite == null) return;

        if(Time.time> nextFrameTime)
        {
            nextFrameTime = Time.time + gifData.delays[currentFrame] / 1000f;

            image.GetComponent<RectTransform>().anchoredPosition = new Vector2(-currentFrame * 256, 0);

            currentFrame++;
            if(currentFrame >= frameCount ) 
            {
                currentFrame = 0;
            }
        }
    }
}
