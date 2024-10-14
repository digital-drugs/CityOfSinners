using Share;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NewClan : MonoBehaviour
{
    [SerializeField] private TMP_InputField abbrText;
    [SerializeField] private TMP_InputField nameText;
    [SerializeField] private TMP_InputField imageText;

    public void CreateClan()
    {
        var parametrs = new Dictionary<byte, object>();

        parametrs.Add((byte)Params.Abbr, abbrText.text);
        parametrs.Add((byte)Params.Name, nameText.text);
        parametrs.Add((byte)Params.Image, imageText.text);

        PhotonManager.Inst.peer.SendOperation(
            (byte)Request.TryCreateClan,
            parametrs,
            PhotonManager.Inst.sendOptions);
    }
}
