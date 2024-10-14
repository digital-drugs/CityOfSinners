using Share;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AdminExtraUi : MonoBehaviour
{
    [SerializeField] private Image ico;
    [SerializeField] private TextMeshProUGUI extraNameText;
    [SerializeField] private Button button;

    private Dictionary<byte, object> extraData;

    public void Assign(Dictionary<byte, object> data)
    {
        extraData = data;

        var imageUrl = (string)extraData[(byte)Params.ExtraImageUrl];

        GameRoomUi.instance.StartLoadTextureToImage(ico, imageUrl);

        extraNameText.text = (string)data[(byte)Params.ExtraName];

        gameObject.name = $"{extraNameText.text}";

        button.onClick.AddListener(() => SelectExtra());
    }

    public void SelectExtra()
    {
        AdminExtra.instance.AssignExtraToForm(extraData);
    }

}
