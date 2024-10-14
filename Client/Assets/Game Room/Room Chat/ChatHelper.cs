using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ChatHelper 
{
    public static void SetMessageToChat(long id, GameObject message, IChat chat, float height, float position)
    {       
        message.transform.SetParent(chat.GetTransform());
        message.transform.localScale = Vector3.one;

        if (chat.lastMessageId == id)
        {
            chat.lastPosition += chat.sameMessageOffset;

            //Debug.Log($"id ==  {chat.lastPosition} {chat.messageOffset}");
        }
        else
        {
            chat.lastPosition += chat.messageOffset;

            //Debug.Log($"id !=  {chat.lastPosition} {chat.messageOffset}");
        }

        chat.lastMessageId = id;

        var newPosition = new Vector2(position, -chat.lastPosition);

        message.GetComponent<RectTransform>().anchoredPosition = newPosition;

        //message.transform.localPosition = newPosition;

        //Debug.Log($"posi {position} {-chat.lastPosition}");
        //Debug.Log($"{ message.transform.localPosition}");

        chat.lastPosition += height;

        chat.GetRectTransform().sizeDelta = new Vector2(0, chat.lastPosition);

        //Debug.Log($"lastPosition {lastPosition}");
    }
}

public interface IChat
{
    Transform GetTransform();
    long lastMessageId { get; set; }
    float lastPosition { get; set; }
    RectTransform GetRectTransform();
    float sameMessageOffset { get; }
    float messageOffset { get; }
}