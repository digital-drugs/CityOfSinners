using Share;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InviteUi : MonoBehaviour
{
    [SerializeField] private Image ico;
    [SerializeField] private TextMeshProUGUI nameText;
    //[SerializeField] private TextMeshProUGUI countText;
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI buttonText;

    private Dictionary<byte, object> clanData;

    public void Assign(Dictionary<byte, object> data)
    {
        clanData = data;

        if (clanData.ContainsKey((byte)Params.Image))
        {
            var imageUrl = (string)clanData[(byte)Params.Image];

            GameRoomUi.instance.StartLoadTextureToImage(ico, imageUrl);
        }

        nameText.text = (string)data[(byte)Params.Name];

        gameObject.name = $"{nameText.text}";

        if ((string)data[(byte)Params.Invite] == InviteType.fromOwner.ToString())
        {
            buttonText.text = "Accept";
            button.onClick.AddListener(() => AcceptInvite());
        }

        if ((string)data[(byte)Params.Invite] == InviteType.fromUser.ToString())
        {
            buttonText.text = "Delete";
            button.onClick.AddListener(() => DeleteProposal());
        }
    }

    public void DeleteProposal()
    {
        //AdminGifts.instance.AssignGiftToForm(giftData);
        UnityEngine.Debug.Log("delete proposal");

        var parameters = new Dictionary<byte, object>();
        parameters.Add((byte)Params.Id, (int)clanData[(byte)Params.Id]);

        PhotonManager.Inst.peer.SendOperation(
            (byte)Request.DeleteProposal,
            parameters,
            PhotonManager.Inst.sendOptions);
    }

    public void AcceptInvite()
    {
        UnityEngine.Debug.Log("Accept Invite");

        var parameters = new Dictionary<byte, object>();
        parameters.Add((byte)Params.Id, (int)clanData[(byte)Params.Id]);

        PhotonManager.Inst.peer.SendOperation(
            (byte)Request.AcceptInvite,
            parameters,
            PhotonManager.Inst.sendOptions);
    }


}
