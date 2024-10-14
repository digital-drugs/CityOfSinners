using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GroupChatButtonUi : MonoBehaviour
{
    public Button button;
    [SerializeField] private TextMeshProUGUI chatNameText;
    public void Assign(string text)
    {
        chatNameText.text = text;
    }

    [SerializeField] private Image Image_Button;
    public void EnableButton()
    {
        Image_Button.sprite = RoomChatUi.instance.Sprite_groupButtonChatEnabled;
        chatNameText.color = RoomChatUi.instance.Color_groupButtonChatEnabled;
    }

    public void DisableButton()
    {
        Image_Button.sprite = RoomChatUi.instance.Sprite_groupButtonChatDisabled;
        chatNameText.color = RoomChatUi.instance.Color_groupButtonChatDisabled;
    }

    [SerializeField] private GameObject Image_NewMessage;
    public void ShowNewMessage()
    {
        Image_NewMessage.SetActive(true);
    }

    public void HideNewMessage()
    {
        Image_NewMessage.SetActive(false);
    }
}
