using ExitGames.Client.Photon;
using Share;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExtraSlotUi : MonoBehaviour
{
    [SerializeField] private Image imageBg;
    [SerializeField] private Image imageIco;
    [SerializeField] private GameObject counter;
    [SerializeField] private TextMeshProUGUI counterText;
    //[SerializeField] private Button selectButton;
    public int slotId { get; private set; }
    internal void SetId(int id)
    {
        slotId = id;
    }

    private ExtraUi extraUi;
    public void Assign(ExtraUi extraUi, bool save = true)
    {
        if(this.extraUi != null)
        {
            if(this.extraUi == extraUi)
            {
                MoveToSlot(extraUi);
                return;
            }            
           
            if(this.extraUi != extraUi)
            {
                var card = this.extraUi;

                card.FreeSlot(card.slot);
                card.DestroyCard(card);                        
            }          
               
            //ClearSlot();
            //this.extraUi = null;
        }

        if (this.extraUi == null)
        {

        }

        extraUi.FreeSlot(extraUi.slot);

        this.extraUi = extraUi;
        extraUi.SetSlot(this);

        if (extraUi == null)
        {
            Debug.Log($"cant assign extra to slot");
            return;
        }
        else
        {
            MoveToSlot(extraUi);

            if (save)
            {
                var parameters = new Dictionary<byte, object>();

                parameters.Add((byte)Params.SlotId, slotId);
                parameters.Add((byte)Params.ExtraId, extraUi.extraId);

                PhotonManager.Inst.peer.SendOperation(
                    (byte)Request.SaveUserSlot,
                    parameters,
                    PhotonManager.Inst.sendOptions);

                Debug.Log($"save extra {extraUi.extraId} in slot {slotId}");
            }         
        }
    }

    private void MoveToSlot(ExtraUi extraUi)
    {
        var extraRect = extraUi.GetComponent<RectTransform>();
        var slotRect = GetComponent<RectTransform>();

        extraUi.transform.SetParent(transform);

        //extraRect.anchoredPosition = new Vector2(0  , 0);
        extraUi.transform.localPosition = Vector3.zero;
        extraRect.sizeDelta = slotRect.sizeDelta;

        //Debug.Log($"pivot {extraRect.pivot} pos {extraRect.anchoredPosition}");
    }
   

    public void ClearSlot()
    {
        extraUi = null;

        //Debug.Log($"clear slot {slotId} from {extraUi.extraId}");

        //Destroy(extraUi.gameObject);

        var parameters = new Dictionary<byte, object>();

        parameters.Add((byte)Params.SlotId, slotId);

        PhotonManager.Inst.peer.SendOperation(
          (byte)Request.ClearUserSlot,
          parameters,
          PhotonManager.Inst.sendOptions);
    }
}
