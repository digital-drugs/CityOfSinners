using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ImageManager : MonoBehaviour
{
    public static ImageManager instance;

    private void Awake()
    {
        instance=this;

        HideImageUnderCursor();
        HideExtraUnderCursorDescription();
    }

    internal void StartLoadTexture(Image image, string imageUrl)
    {
        StartCoroutine(LoadTexture(image, imageUrl));
    } 

    public IEnumerator LoadTexture(Image image, string url)
    {
        //extraUi.extraIco.sprite = GameRoomUi.instance.defaultExtraSprite;
        //yield break;

        if (string.IsNullOrEmpty(url))
        {
            image.sprite = GameRoomUi.instance.defaultExtraSprite;
            //Debug.Log($"set empty sprite for {extraUi.extraId}");
        }
        else
        {
            UnityEngine.Texture2D texture;

            // using to automatically call Dispose, create a request along the path to the file
            UnityWebRequest imageWeb = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET);

            // We create a "downloader" for textures and pass it to the request
            imageWeb.downloadHandler = new DownloadHandlerTexture();
            
            // We send a request, execution will continue after the entire file have been downloaded
            yield return imageWeb.SendWebRequest();

            //Debug.Log($"image url {url}");

            // Getting the texture from the "downloader"
            texture = DownloadHandlerTexture.GetContent(imageWeb);

            var sprite = Sprite.Create(texture,
            new Rect(0.0f, 0.0f, texture.width, texture.height),
            new Vector2(0f, 1f));

            image.sprite = sprite;
        }

        //extraUi.ImageLoaded();
    }

    [Header("images under cursor")]
    [SerializeField] private RectTransform go_CursorMaskedImage;
    [SerializeField] private Image image_CursorMaskedImage;
    public void ShowImageUnderCursor(Image image, Vector3 position)
    {
        image_CursorMaskedImage.sprite = image.sprite;
        go_CursorMaskedImage.position = position; //Input.mousePosition;
        go_CursorMaskedImage.gameObject.SetActive(true);
    }

    public void HideImageUnderCursor()
    {
        image_CursorMaskedImage.sprite = null;     
        go_CursorMaskedImage.gameObject.SetActive(false);
    }


    [Header("extras under cursor")]
    private bool showExtraDescription=false;
    public void SwitchExtraDescription(ExtraInGameRoom extra)
    {
        showExtraDescription = !showExtraDescription;
        
        if (showExtraDescription)
        {
            ShowExtraUnderCursorDescription(extra);
        }
        else
        {
            HideExtraUnderCursorDescription();
        }
    }

    [SerializeField] private RectTransform go_CursorExtraDescription;
    [SerializeField] private TextMeshProUGUI text_cursorExtraDescription;
    [SerializeField] private TextMeshProUGUI text_cursorExtraUseType;
    [SerializeField] private float offsetY=100;
    public void ShowExtraUnderCursorDescription(ExtraInGameRoom extra)
    {
        if (!showExtraDescription) return;

        var dragCard = "";
        var cardTarget = "";

        switch (extra.extraUseType)
        {
            case ExtraUseType.Self:
                {
                    dragCard = "тяните карту";
                    cardTarget = "в центр поля";
                }
                break;

            case ExtraUseType.Target:
                {
                    dragCard = "тяните карту";
                    cardTarget = "на игрока";
                }
                break;

            case ExtraUseType.Auto:
                {
                    dragCard = "карта используется";
                    cardTarget = "автоматически";
                }
                break;
        }

        text_cursorExtraUseType.text = $"<size=11>{dragCard}</size>\n<size=14>{cardTarget}</size>";

        text_cursorExtraDescription.text = extra.extraDescription;
        go_CursorExtraDescription.position = extra.transform.position + new Vector3(0, offsetY, 0);
        go_CursorExtraDescription.gameObject.SetActive(true);
    }
        
    public void HideExtraUnderCursorDescription()
    {
        text_cursorExtraDescription.text = "";
        go_CursorExtraDescription.gameObject.SetActive(false);
    }

   
}

public interface IUnderCursor
{
    void StartAction();

    void StopAction();
}
