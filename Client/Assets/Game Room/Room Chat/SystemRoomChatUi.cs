using ExitGames.Client.Photon;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public class SystemRoomChatUi : MonoBehaviour, IChat
{
    [SerializeField] private SystemChatMessageUi systemChatMessageUiPrefab;
    [SerializeField] private Transform systemChatContainer;
    public void RoomPersonalSystemMessage(ParameterDictionary parameters)
    {
        var newChatMessage = Instantiate(systemChatMessageUiPrefab);
        
        newChatMessage.Assign(4,parameters, this);        
    }

    [SerializeField] private ShareChatMessageUi shareChatMessageUiPrefab;
    public void ShareSystemMessage(ParameterDictionary parameters)
    {
        var newChatMessage = Instantiate(shareChatMessageUiPrefab);

        newChatMessage.Assign(4, parameters, this);
    }    

    public void RoomPublicSystemMessage(ParameterDictionary parameters)
    {
        var newChatMessage = Instantiate(systemChatMessageUiPrefab);

        newChatMessage.Assign(4,parameters, this);
    }

    [SerializeField] private Ui_SystemMessage_Skill ui_SystemMessage_Skill;
    public void RoomSystemMessage_Skill(ParameterDictionary parameters)
    {
        var newChatMessage = Instantiate(ui_SystemMessage_Skill);

        newChatMessage.Assign(4, parameters, this);
    }

    [SerializeField] private Ui_SystemMessage_Extra ui_SystemMessage_Extra;
    public void RoomSystemMessage_Extra(ParameterDictionary parameters)
    {
        var newChatMessage = Instantiate(ui_SystemMessage_Extra);

        newChatMessage.Assign(4, parameters, this);
    }

    [SerializeField] private Ui_SystemMessage_Role ui_SystemMessage_Role;
    public void RoomSystemMessage_Role(ParameterDictionary parameters)
    {
        var newChatMessage = Instantiate(ui_SystemMessage_Role);

        newChatMessage.Assign(4, parameters, this);
    }

    [SerializeField] private Transform messageContainer;

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
        return messageContainer.transform;
    }

    public RectTransform GetRectTransform()
    {
        return messageContainer.GetComponent<RectTransform>();
    }

    public void Clear()
    {  
        UiHelper.ClearContainer(messageContainer);        
    }

    [SerializeField] private SystemChatMessageUi dayMessagePrefab;
    public void SetDayPhase(ParameterDictionary parameters)
    {
        var newChatMessage = Instantiate(dayMessagePrefab);

        newChatMessage.Assign(1, parameters, this);
    }

    [SerializeField] private SystemChatMessageUi nightMessagePrefab;
    public void SetNightPhase(ParameterDictionary parameters)
    {
        var newChatMessage = Instantiate(nightMessagePrefab);

        newChatMessage.Assign(2,parameters, this);
    }
}
