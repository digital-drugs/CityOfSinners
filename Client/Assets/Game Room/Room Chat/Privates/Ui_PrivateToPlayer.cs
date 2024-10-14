using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Ui_PrivateToPlayer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI Text_Name;
    [SerializeField] private Image Image_NewMessageIco;
    //[SerializeField] private Button Button_OpenPrivate;

    private long id;
    public void Assign(long id, Dictionary<byte, object> player)
    {
        this.id = id;

        Text_Name.text = (string)player[(byte)Params.UserName];

        //Image_NewMessageIco.color = new Color(1, 1, 1, 0.1f);

        Image_NewMessageIco.enabled = false;
    }

    public void Button_OpenPrivate()
    {
        GameRoomUi.instance.roomChatUi.OpenPrivateChat(id);

        Image_NewMessageIco.enabled = false;

        //Image_NewMessageIco.color = new Color(1, 1, 1, 0.1f);
    }

    public void ShowNewMessageIco()
    {
        Image_NewMessageIco.enabled = true;

        //Image_NewMessageIco.color = new Color(1, 1, 1, 1);
    }
}
