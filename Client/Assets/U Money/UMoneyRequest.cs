using ExitGames.Client.Photon;
using Share;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UMoneyRequest : MonoBehaviour
{
    public void SendBuyRequest(int shopItemId)
    {
        var parameters = new Dictionary<byte, object>();

        parameters.Add((byte)Params.ShopItemId, shopItemId);

        PhotonManager.Inst.peer.SendOperation((byte)Request.Buy,parameters, PhotonManager.Inst. sendOptions);
    }

}
