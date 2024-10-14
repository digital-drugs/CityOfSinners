using Share;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeUi : MonoBehaviour
{
    [SerializeField] private Image ico;
    [SerializeField] private TextMeshProUGUI nameText;
    //[SerializeField] private TextMeshProUGUI countText;
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI buttonText;
    [SerializeField] private Image imageOk;

    private Dictionary<byte, object> upgradeData;

    public void Assign(Dictionary<byte, object> data, bool right)
    {
        upgradeData = data;

        //if (clanData.ContainsKey((byte)Params.Image))
        //{
        //    var imageUrl = (string)clanData[(byte)Params.Image];

        //    /ameRoomUi.instance.StartLoadTextureToImage(ico, imageUrl);
        //}

        nameText.text = (string)data[(byte)Params.Description];

        gameObject.name = $"{nameText.text}";

        if (data.ContainsKey((byte)Params.Level))
        {
            imageOk.gameObject.SetActive(true);
        }
        else if (right)
        {
            button.gameObject.SetActive(true);
            buttonText.text = ((int)data[(byte)Params.Cost]).ToString();
        }

        //if ((string)data[(byte)Params.Invite] == InviteType.fromOwner.ToString())
        //{
        //    buttonText.text = "Accept";
        //    button.onClick.AddListener(() => AcceptInvite());
        //}

        //if ((string)data[(byte)Params.Invite] == InviteType.fromUser.ToString())
        //{
        //    buttonText.text = "Delete";
        //    button.onClick.AddListener(() => DeleteProposal());
        //}

        button.onClick.AddListener(() => BuyUpgrade());
    }

    public void BuyUpgrade()
    {
        var parameters = new Dictionary<byte, object>();
        parameters.Add((byte)Params.Id, upgradeData[(byte)Params.Id]);

        PhotonManager.Inst.peer.SendOperation(
            (byte)Request.BuyUpgrade,
            parameters,
            PhotonManager.Inst.sendOptions);
    }

    //public void DeleteProposal()
    //{
    //    //AdminGifts.instance.AssignGiftToForm(giftData);
    //    UnityEngine.Debug.Log("delete proposal");

    //    var parameters = new Dictionary<byte, object>();
    //    parameters.Add((byte)Params.Id, (int)clanData[(byte)Params.Id]);

    //    PhotonManager.Inst.peer.SendOperation(
    //        (byte)Request.DeleteProposal,
    //        parameters,
    //        PhotonManager.Inst.sendOptions);
    //}

    //public void AcceptInvite()
    //{
    //    UnityEngine.Debug.Log("Accept Invite");

    //    var parameters = new Dictionary<byte, object>();
    //    parameters.Add((byte)Params.Id, (int)clanData[(byte)Params.Id]);

    //    PhotonManager.Inst.peer.SendOperation(
    //        (byte)Request.AcceptInvite,
    //        parameters,
    //        PhotonManager.Inst.sendOptions);
    //}


}
