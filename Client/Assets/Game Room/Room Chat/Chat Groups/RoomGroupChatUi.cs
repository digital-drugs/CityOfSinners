using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomGroupChatUi : MonoBehaviour, IChat
{
    public Transform container;

    [SerializeField] private long _lastMessageId;
    [SerializeField] private float _messageOffset;
    [SerializeField] private float _sameMessageOffset;
    [SerializeField] private float _lastPosition;
    public long lastMessageId { get => _lastMessageId; set => _lastMessageId = value; }
    public float lastPosition { get => _lastPosition; set => _lastPosition = value; }
    public float sameMessageOffset => _sameMessageOffset;
    public float messageOffset => _messageOffset;
    public Transform GetTransform()
    {
        return container.transform;
    }

    public RectTransform GetRectTransform()
    {
        return container.GetComponent<RectTransform>();
    }

    public bool ChatIsOpen()
    {
        return gameObject.activeSelf;
    }


    //public void AddPrivatePlayer(int index, Ui_PrivateToPlayer newRoomPlayer)
    //{
    //    newRoomPlayer.transform.SetParent(container.transform);
    //    newRoomPlayer.transform.localPosition = new Vector3(0, -index*35, 0) ;
    //}
}
