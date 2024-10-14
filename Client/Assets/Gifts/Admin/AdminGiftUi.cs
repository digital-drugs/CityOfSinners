using Share;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AdminGiftUi : MonoBehaviour
{
    [SerializeField] private Image ico;
    [SerializeField] private TextMeshProUGUI giftNameText;
    [SerializeField] private Button button;

    private Dictionary<byte, object> giftData;

    public void Assign(Dictionary<byte, object> data)
    {
        giftData = data;

        var imageUrl = (string)giftData[(byte)Params.giftImage];

        GameRoomUi.instance.StartLoadTextureToImage(ico, imageUrl);

        giftNameText.text = (string)data[(byte)Params.giftName];

        gameObject.name = $"{giftNameText.text}";

        button.onClick.AddListener(() => SelectGift());
    }

    public void SelectGift()
    {
        AdminGifts.instance.AssignGiftToForm(giftData);
    }

}
