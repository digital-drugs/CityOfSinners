using Share;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TopRowScript : MonoBehaviour
{
    [SerializeField] private Image ico;
    [SerializeField] private TextMeshProUGUI placeText;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private Button button;

    private Dictionary<byte, object> data;

    public void Assign(Dictionary<byte, object> data)
    {
        this.data = data;

        //var imageUrl = (string)data[(byte)Params.Image];

        //GameRoomUi.instance.StartLoadTextureToImage(ico, imageUrl);

        //UnityEngine.Debug.Log("place " + (int)data[(byte)Params.Number]);
        //UnityEngine.Debug.Log("score " + (int)data[(byte)Params.Amount]);

        nameText.text = (string)data[(byte)Params.Name];
        placeText.text = ((int)data[(byte)Params.Number]).ToString();
        scoreText.text = ((int)data[(byte)Params.Amount]).ToString();

        gameObject.name = $"{nameText.text}";

        //button.onClick.AddListener(() => EnterClan());
    }
}
