using ExitGames.Client.Photon;
using Share;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SameSelfChatMessageUi : MonoBehaviour, IChatMessage
{
    [SerializeField] private TextMeshProUGUI messageText;
    private long messageOwnerId;
    private IChat chat;
    public void Assign(ParameterDictionary parameters, IChat chat/*, Sprite bg*/)
    {
        messageOwnerId = (long)parameters[(byte)Params.OwnerId];

        this.chat = chat;

        messageText.fontSize = GameManager.instance.dialogFontSize;

        messageText.text = (string)parameters[(byte)Params.ChatMessage];

        StartCoroutine(UpdateTextSize());
    }

    [SerializeField] private float topOffsetY = 0;
    [SerializeField] private float bottomOffsetY = 0;
    [SerializeField] private float chatPostionOffsetX = 0;
    [SerializeField] private float messageWidth = 400;
    IEnumerator UpdateTextSize()
    {
        yield return new WaitForEndOfFrame();

        var messageHeight = messageText.GetComponent<RectTransform>().rect.size.y;
        var height = topOffsetY + messageHeight + bottomOffsetY;

        var rectTransform = transform.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(messageWidth, height);

        ChatHelper.SetMessageToChat(messageOwnerId, gameObject, chat, height, chatPostionOffsetX);
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }
}
