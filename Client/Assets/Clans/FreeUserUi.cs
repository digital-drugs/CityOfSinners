using Share;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FreeUserUi : MonoBehaviour
{
    [SerializeField] private Image ico;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI countText;
    [SerializeField] private Button button;

    private Dictionary<byte, object> userData;

    public void Assign(Dictionary<byte, object> data)
    {
        userData = data;

        //var imageUrl = (string)userData[(byte)Params.Image];

        //GameRoomUi.instance.StartLoadTextureToImage(ico, imageUrl);

        nameText.text = (string)data[(byte)Params.Name];
        //countText.text = ((int)data[(byte)Params.Count]).ToString();

        //gameObject.name = $"{nameText.text}";

        button.onClick.AddListener(() => CreateInvite());
    }

    public void CreateInvite()
    {
        UnityEngine.Debug.Log("Create Invite " + userData[(byte)Params.Id]);

        var parameters = new Dictionary<byte, object>();
        parameters.Add((byte)Params.Id, (long)userData[(byte)Params.Id]);

        PhotonManager.Inst.peer.SendOperation(
            (byte)Request.CreateInvite,
            parameters,
            PhotonManager.Inst.sendOptions);
    }

}
