using ExitGames.Client.Photon;
using Share;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShareChatMessageUi : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Image icoImage;
    private long id;
    private IChat chat;

    [SerializeField] private Button shareButton;
    private int messageId;
    public void Assign(long id, ParameterDictionary parameters, IChat chat)
    {
        this.id = id;
        this.chat = chat;

        var mesage = (string)parameters[(byte)Params.ChatMessage];

        messageText.fontSize = GameManager.instance.systemFontSize;

        messageText.text = $"{mesage}";

        messageId = (int)parameters[(byte)Params.messageId];

        //загрузить картинку экстры
        var extraId = (ExtraEffect)parameters[(byte)Params.ExtraId];
        var extraUi = ExtraScreenUi.instance.FindExtraUi(extraId);
        if (extraUi != null)
        {
            icoImage.sprite = extraUi.extraIco.sprite;
        }

        shareButton.onClick.AddListener(() => RequestShareMessage());

        StartCoroutine(UpdateTextSize());
    }

    private void RequestShareMessage()
    {
        var parameters = new Dictionary<byte, object>();

        parameters.Add((byte)Params.messageId, messageId);

        PhotonManager.Inst.peer.SendOperation((byte)Request.ShareMessage, parameters, PhotonManager.Inst.sendOptions);
    }

    [SerializeField] private float topOffsetY = 0;
    [SerializeField] private float bottomOffsetY = 0;
    [SerializeField] private float chatPostionOffsetX = 0;
    [SerializeField] private float messageWidth = 400;
    IEnumerator UpdateTextSize()
    {
        Canvas.ForceUpdateCanvases();
        yield return new WaitForEndOfFrame();

        //yield return new WaitForSeconds(0.05f);

        var messageHeight = messageText.GetComponent<RectTransform>().rect.size.y;
        var height = topOffsetY + messageHeight + bottomOffsetY;

        //Debug.Log($"messageHeight {messageHeight} // {messageText.text}");
        //Debug.Log($"height {height}");

        var rectTransform = transform.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(messageWidth, height);

        //Debug.Log($"height {height} // {messageText.text}");

        ChatHelper.SetMessageToChat(id, gameObject, chat, height, chatPostionOffsetX);
    }
}
