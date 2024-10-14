using Share;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GeneralChat : MonoBehaviour, IChat
{
    public static GeneralChat instance;

    private void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(instance);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    [SerializeField] private TMP_InputField messageInput;
    public void SendMessageToGeneralChat(bool checkEnter)
    {
        if (checkEnter && !Input.GetKeyDown(KeyCode.Return)) return;

        var message = messageInput.text;

        var parameters = new Dictionary<byte, object>();

        parameters.Add((byte)Params.ChatType, ChatType.GlobalChat);
        parameters.Add((byte)Params.ChatMessage, message);

        PhotonManager.Inst.peer.SendOperation((byte)Request.SendChat, parameters, PhotonManager.Inst.sendOptions);

        messageInput.text = "";
    }

    [SerializeField] private Sprite defaultSprite;
    [SerializeField] private ChatMessageUi chatMessageUiPrefab;
    [SerializeField] private Transform chatMessagesContainer;
    [SerializeField] private ScrollRect chatScroll;



    public void AddMessageFromServer(ExitGames.Client.Photon.ParameterDictionary parameters)
    {
        var newChatMessage = Instantiate(chatMessageUiPrefab, chatMessagesContainer);

        //проверка на цензуру

        //проверка на изображения (смайлы/стикеры)

        newChatMessage.Assign(parameters, this/*, defaultSprite*/);

        StartCoroutine(ScrollChat());
    }

    IEnumerator ScrollChat()
    {
        //Canvas.ForceUpdateCanvases();
        yield return new WaitForEndOfFrame();
        chatScroll.normalizedPosition = new Vector2(0, 0);
    }

    public void AddSmileIdToMessage(string smileId)
    {
        messageInput.text += $":({smileId})";
    }


    public long lastMessageId { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public float lastPosition { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public float sameMessageOffset => throw new NotImplementedException();

    public float messageOffset => throw new NotImplementedException();
    public Transform GetTransform()
    {
        throw new NotImplementedException();
    }

    public RectTransform GetRectTransform()
    {
        throw new NotImplementedException();
    }
}
