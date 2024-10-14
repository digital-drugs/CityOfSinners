using ExitGames.Client.Photon;
using Share;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AuctionSlotUi : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image Image_Avatar;
    [SerializeField] private TextMeshProUGUI Text_RoleName;
    [SerializeField] private TextMeshProUGUI Text_RoleDescription;

    [SerializeField] private TextMeshProUGUI Text_Bet;
    [SerializeField] private Image Image_BetCurrency;

    [SerializeField] private Button Button_Bet;
    [SerializeField] private TextMeshProUGUI Text_ButtonBet;
    [SerializeField] private Image Image_ButtonBet;
   
    private int slotId;
    public void Assign(Dictionary<byte, object> slot)
    {
        slotId = (int)slot[(byte)Params.SlotId];

        var roleId = (RoleType)slot[(byte)Params.RoleId];

        var roleData = RoleScreenUi.ins.roleDatas[roleId.ToString()];
        var roleImages = RoleScreenUi.ins.roleImages[roleId.ToString()];

        //аватар роли
        Image_Avatar.sprite = roleImages.maleImage.sprite;

        //имя роли
        Text_RoleName.text = roleData.name;

        //описание роли
        Text_RoleDescription.text = roleData.description;

        //текущая цена/ставка
        if (slot.ContainsKey((byte)Params.Bet))
        {
            var currentBet = (int)slot[(byte)Params.Bet];
            Text_Bet.text = $"{currentBet}";
        }
        else
        {
            Text_Bet.enabled = false;
            Image_BetCurrency.enabled = false;
        }
        

        //тип валюты
        var currency = (CurrencyType)slot[(byte)Params.CurrencyType];

        //кнопка ставки
        Button_Bet.onClick.AddListener(() => RequestBet());

        //цена ставки
        var bet = (int)slot[(byte)Params.Cost];
        Text_ButtonBet.text = $"{bet}";
        switch (currency)
        {
            case CurrencyType.Coins: 
                {
                    //Image_ButtonBetCurrency.sprite = SpriteHelper.ins.GetSprite_Coin;
                    Text_ButtonBet.text += $" коинов";
                    Image_BetCurrency.sprite = SpriteHelper.ins.GetSprite_Coin;
                } break;
            case CurrencyType.Diamond:
                {
                    //Image_ButtonBetCurrency.sprite = SpriteHelper.ins.GetSprite_Diamond;
                    Text_ButtonBet.text += $" алмаз";
                    Image_BetCurrency.sprite = SpriteHelper.ins.GetSprite_Diamond;
                }
                break; 
        }
    }

    public void UpdateSlot(ParameterDictionary parameters)
    {
        var bet = (int)parameters[(byte)Params.Bet];
        Text_Bet.text = $"{bet}";

        //var isOpen = (bool)parameters[(byte)Params.SlotOpen];
       
        //if (! isOpen ) 
        //{
        //    BlockSlot();
        //}
    }

    public void BuySlot(ParameterDictionary parameters)
    {
        var playerId = (long)parameters[(byte)Params.OwnerId];

        if (playerId == GameManager.instance.userId) 
        {
            Text_ButtonBet.text = $"Куплено";

            isLocked = true;
            EnableMotion();
        }
        else
        {
            Text_ButtonBet.text = $"Продано";
            BlockSlot();
        }     
    }


    [SerializeField] private CanvasGroup canvasGroup;
    private bool isLocked = false;
    public void BlockSlot()
    {
        isLocked = true;
        canvasGroup.alpha = 0.2f;
        canvasGroup.interactable = false;
        DisableMotion();
    }

    private void RequestBet()
    {
        var parameters = new Dictionary<byte, object>();

        parameters.Add((byte)Params.SlotId, slotId);

        PhotonManager.Inst.peer.SendOperation((byte)Request.Bet, parameters, PhotonManager.Inst.sendOptions);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isLocked) return;

        EnableMotion();
    }

    [SerializeField] private GameObject GameObject_AvatarBg;
    [SerializeField] private Color Color_Enable;    
    private void EnableMotion()
    {
        GameObject_AvatarBg.SetActive(true);
        Image_ButtonBet.enabled = true;
        Text_ButtonBet.color = Color_Enable;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isLocked) return;

        DisableMotion();
    }

    [SerializeField] private Color Color_Disable;
    private void DisableMotion()
    {
        GameObject_AvatarBg.SetActive(false);
        Image_ButtonBet.enabled = false;
        Text_ButtonBet.color = Color_Disable;
    }
}
