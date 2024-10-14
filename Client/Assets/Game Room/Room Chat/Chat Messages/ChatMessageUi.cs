using ExitGames.Client.Photon;
using Share;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatMessageUi : MonoBehaviour, IChatMessage
{
    [SerializeField] private Image bgImage;
    [SerializeField] private TextMeshProUGUI userNameText;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button messageButton;
    private long messageOwnerId;
    private IChat chat;
    public void Assign(ParameterDictionary parameters, IChat chat/*, Sprite bg*/)
    {
        this.chat = chat;

        //bgImage.sprite = bg;

        messageOwnerId = (long)parameters[(byte)Params.OwnerId];

        userNameText.text = (string)parameters[(byte)Params.UserName];

        messageText.fontSize = GameManager.instance.dialogFontSize;

        messageText.text = (string)parameters[(byte)Params.ChatMessage];

        messageButton.onClick.AddListener(() => ClickMessage());

        StartCoroutine(UpdateTextSize());
    }

    [SerializeField] private float topOffsetY = 0;
    [SerializeField] private float bottomOffsetY = 0;
    [SerializeField] private float messageWidth = 400;
    IEnumerator UpdateTextSize()
    {
        yield return new WaitForEndOfFrame();

        var messageHeight = messageText.GetComponent<RectTransform>().rect.size.y;  
        var height = topOffsetY + messageHeight + bottomOffsetY;

        var rectTransform = transform.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(messageWidth, height);

        ChatHelper.SetMessageToChat(messageOwnerId, gameObject, chat, height,0);
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    //public void Assign(ExitGames.Client.Photon.ParameterDictionary parameters)
    //{
    //    messageOwnerId = (long)parameters[(byte)Params.UserId];
    //    userNameText.text = (string)parameters[(byte)Params.UserName];
        
    //    var message = (string)parameters[(byte)Params.ChatMessage];

    //    var messageChars = message.ToCharArray();

    //    var messageContents = new List<MessageContent>();
        
    //    var currentChars = new List<char>();

    //    for (int i = 0; i < messageChars.Length; i++)
    //    {
    //        if (messageChars[i] != ':' && i < messageChars.Length)
    //        {
    //            currentChars.Add(messageChars[i]);
    //        }
    //        else if (messageChars[i] == ':')
    //        {
    //            if (i + 3 < messageChars.Length
    //                && messageChars[i] == ':'
    //                && messageChars[i + 1] == '('
    //                && messageChars[i + 3] == ')')
    //            {
    //                if (currentChars.Count > 0)
    //                {
    //                    var newContent = new TextContent(new string(currentChars.ToArray()));

    //                    messageContents.Add(newContent);

    //                    currentChars = new List<char>();
    //                }

    //                {
    //                    var sprite = SmileManager.instance.GetSmileSprite(messageChars[i + 2]);
    //                    var newContent = new ImageContent(sprite);
    //                    messageContents.Add(newContent);
    //                }

    //                i += 3;
    //            } 
    //        }

    //        if(i+1 == messageChars.Length)
    //        {
    //            Debug.Log("last char");

    //            if(  currentChars.Count > 0)
    //            {                
    //                var newContent = new TextContent(new string(currentChars.ToArray()));

    //                messageContents.Add(newContent);    
    //            }
    //        }
    //    }

    //    foreach(var c in messageContents)
    //    {
    //        if(c is TextContent)
    //        {
    //            AddTextContentToChat(((TextContent)c).text);
    //        }
    //        if(c is ImageContent)
    //        {
    //            AddImageContentToChat(((ImageContent)c).image);
    //        }
    //    }

    //    //messageText.text = message;

    //    messageButton.onClick.AddListener(() => ClickMessage());
    //}

    [SerializeField] private Transform contentContainer;

    [SerializeField] private TextContentUi textContentPrefab;
    private void AddTextContentToChat(string text)
    {
        var newTextConentUi = Instantiate(textContentPrefab, contentContainer);
        newTextConentUi.Assign(text);
    }

    [SerializeField] private ImageContentUi imageContentPrefab;
    private void AddImageContentToChat(Sprite sprite)
    {
        var newImageConentUi = Instantiate(imageContentPrefab, contentContainer);
        newImageConentUi.Assign(sprite);
    }    

    public void ClickMessage()
    {
        Debug.Log($"messageOwnerId {messageOwnerId}");
    }
}

public class MessageContent
{
   
}

public class TextContent : MessageContent
{
    public  string text;

    public TextContent()
    {

    }
    public TextContent(string text)
    {
        this.text = text;
    }
}

public class ImageContent: MessageContent
{
    public Sprite image;
    public ImageContent(Sprite image) 
    {
        this.image = image;
    }
}