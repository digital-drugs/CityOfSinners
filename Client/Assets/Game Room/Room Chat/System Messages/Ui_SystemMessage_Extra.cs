using ExitGames.Client.Photon;
using Share;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Ui_SystemMessage_Extra : MonoBehaviour
{
    [SerializeField] private Image extraIco;
    [SerializeField] private TextMeshProUGUI messageText;
    private long id;
    private IChat chat;
    public void Assign(long id, ParameterDictionary parameters, IChat chat)
    {
        this.id = id;
        this.chat = chat;

        var mesage = (string)parameters[(byte)Params.ChatMessage];

        Debug.Log($"mesage {mesage}");

        var extraId = (ExtraEffect)parameters[(byte)Params.ExtraId];

        Debug.Log($"{extraId}");

        var extraUi = ExtraScreenUi.instance.FindExtraUi(extraId);

        if (extraUi != null)
        {
            extraIco.sprite = extraUi.extraIco.sprite;
        }

        //var mesage = (string)parameters[(byte)Params.ChatMessage];

        messageText.fontSize = GameManager.instance.systemFontSize;

        messageText.text = $"{mesage}";

        StartCoroutine(UpdateTextSize());
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
