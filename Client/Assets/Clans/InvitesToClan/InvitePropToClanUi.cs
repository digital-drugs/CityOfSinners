using Share;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InvitePropToClanUi : MonoBehaviour
{
    [SerializeField] private Image ico;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI buttonText;

    private Dictionary<byte, object> userData;

    public void Assign(Dictionary<byte, object> data)
    {
        userData = data;

        //var imageUrl = (string)userData[(byte)Params.Image];

        //GameRoomUi.instance.StartLoadTextureToImage(ico, imageUrl);

        nameText.text = (string)data[(byte)Params.Name];

        if ((string)data[(byte)Params.Invite] == InviteType.fromOwner.ToString())
        {
            buttonText.text = "Delete";
            button.onClick.AddListener(() => DeleteInvite());
        }

        if ((string)data[(byte)Params.Invite] == InviteType.fromUser.ToString())
        {
            buttonText.text = "Accept";
            button.onClick.AddListener(() => AcceptProposal());
        }
    }

    public void DeleteInvite()
    {
        UnityEngine.Debug.Log("Delete Invite");

        var parameters = new Dictionary<byte, object>();
        parameters.Add((byte)Params.Id, (long)userData[(byte)Params.Id]);

        PhotonManager.Inst.peer.SendOperation(
            (byte)Request.DeleteInvite,
            parameters,
            PhotonManager.Inst.sendOptions);
    }

    public void AcceptProposal()
    {
        UnityEngine.Debug.Log("Accept proposal");

        var parameters = new Dictionary<byte, object>();
        parameters.Add((byte)Params.Id, (long)userData[(byte)Params.Id]);

        PhotonManager.Inst.peer.SendOperation(
            (byte)Request.AcceptProposal,
            parameters,
            PhotonManager.Inst.sendOptions);
    }
}
