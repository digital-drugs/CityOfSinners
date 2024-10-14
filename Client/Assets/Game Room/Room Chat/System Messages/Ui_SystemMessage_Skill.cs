using ExitGames.Client.Photon;
using Share;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Ui_SystemMessage_Skill : MonoBehaviour
{
    [SerializeField] private Image skillIco;
    [SerializeField] private TextMeshProUGUI messageText;
    private long id;
    private IChat chat;
    [SerializeField] private bool useAutoSize;
    public void Assign(long id, ParameterDictionary parameters, IChat chat)
    {
        this.id = id;
        this.chat = chat;

        var skillid = (SkillEffect)parameters[(byte)Params.SkillId];

        var skillUi = SkillScreenUi.instance.FindSkillUi(skillid);

        if (skillUi != null)
        {
            skillIco.sprite = skillUi.skillIco.sprite;
        }

        var mesage = (string)parameters[(byte)Params.ChatMessage];

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

        var rectTransform = transform.GetComponent<RectTransform>();
        var height = rectTransform.rect.size.y;

        if (useAutoSize)
        {
            var messageHeight = messageText.GetComponent<RectTransform>().rect.size.y;
            height = topOffsetY + messageHeight + bottomOffsetY;
            rectTransform.sizeDelta = new Vector2(messageWidth, height);
        }          

        ChatHelper.SetMessageToChat(id, gameObject, chat, height, chatPostionOffsetX);
    }
}
