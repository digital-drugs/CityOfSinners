using Share;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClanUi : MonoBehaviour
{
    [SerializeField] private Image ico;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI countText;
    [SerializeField] private Button button;

    private Dictionary<byte, object> clanData;

    public void Assign(Dictionary<byte, object> data)
    {
        clanData = data;

        var imageUrl = (string)clanData[(byte)Params.Image];

        GameRoomUi.instance.StartLoadTextureToImage(ico, imageUrl);

        nameText.text = (string)data[(byte)Params.Name];
        countText.text = ((int)data[(byte)Params.Count]).ToString();

        gameObject.name = $"{nameText.text}";

        button.onClick.AddListener(() => EnterClan());
    }

    public void EnterClan()
    {
        UnityEngine.Debug.Log("Enter clan " + (int)clanData[(byte)Params.Id]);

        var parameters = new Dictionary<byte, object>();
        parameters.Add((byte)Params.Id, (int)clanData[(byte)Params.Id]);

        PhotonManager.Inst.peer.SendOperation(
            (byte)Request.CreateProposal,
            parameters,
            PhotonManager.Inst.sendOptions);
    }

}
