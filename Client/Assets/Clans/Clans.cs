using ExitGames.Client.Photon;
using Share;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Clans : MonoBehaviour
{
    public static Clans instance;

    int giftId = 0;

    void Start()
    {
        instance = this;

        //UiHelper.SetupTMPDropMenu(costType, typeof(CostType));
        //UiHelper.SetupTMPDropMenu(giftEventType, typeof(GiftEventType));
        //UiHelper.SetupTMPDropMenu(giftType, typeof(GiftType));
        ///UiHelper.SetupTMPDropMenu(giftEffect, typeof(ExtraEffect));
    }

    [SerializeField] private ClanUi clanUiPrefab;
    [SerializeField] private Transform container;

    [SerializeField] private TMP_InputField giftName;
    [SerializeField] private TMP_InputField giftDescription;
    
    [SerializeField] private TMP_InputField giftCost;
    [SerializeField] private TMP_Dropdown costType;

    [SerializeField] private TMP_InputField giftImage;
    
    [SerializeField] private TMP_Dropdown giftEventType;
    [SerializeField] private TMP_Dropdown giftType;
    [SerializeField] private TMP_Dropdown giftEffect;

    public void ButtonSaveGift()
    {
        var giftData = new Dictionary<byte, object>();

        giftData.Add((byte)Params.giftId, giftId);

        giftData.Add((byte)Params.giftName, giftName.text);
        giftData.Add((byte)Params.giftDescription, giftDescription.text);
        giftData.Add((byte)Params.giftCost, giftCost.text);
        giftData.Add((byte)Params.giftImage, giftImage.text);

        giftData.Add((byte)Params.giftEventType, giftEventType.captionText.text);
        giftData.Add((byte)Params.giftType, giftType.captionText.text);
        giftData.Add((byte)Params.giftCostType, costType.captionText.text);
        giftData.Add((byte)Params.giftExtraEffect, giftEffect.captionText.text);

        PhotonManager.Inst.peer.SendOperation((byte)Request.SaveGift, giftData, PhotonManager.Inst.sendOptions);
    }

    public void ButtonLoadClans()
    {
        Debug.Log("Load Clans");

        PhotonManager.Inst.peer.SendOperation(
            (byte)Request.LoadClans,
            new Dictionary<byte, object>(),
            PhotonManager.Inst.sendOptions);
    }

    public void ShowClans(ParameterDictionary data)
    {
        Debug.Log("show clans");
        
        UiHelper.ClearContainer(container);
                
        var clansData = (Dictionary<int, object>)data[(byte)Params.clans];

        Debug.Log(clansData.Count);

        foreach (var el in clansData)
        {
            var clanData = (Dictionary<byte, object>)el.Value;

            AddUiElement(clanData);
        }
    }

    private void AddUiElement(Dictionary<byte, object> data)
    {
        var newUiElement = Instantiate(clanUiPrefab);
        UiHelper.AssignObjectToContainer(newUiElement.gameObject, container);

        newUiElement.Assign(data);
    }

    public void AssignGiftToForm(Dictionary<byte, object> data)
    {
        Debug.Log($"assign gift");
        
        int optionIndex = 0;

        giftId = (int)data[(byte)Params.giftId];

        Debug.Log(giftId);

        giftName.text = (string)data[(byte)Params.giftName];
        giftDescription.text= (string)data[(byte)Params.giftDescription];
        giftCost.text = ((int)data[(byte)Params.giftCost]).ToString();

        Debug.Log(data[(byte)Params.giftCostType]);
        Debug.Log(data[(byte)Params.giftEventType]);
        Debug.Log(data[(byte)Params.giftType]);
        Debug.Log(data[(byte)Params.giftExtraEffect]);

        var giftCostType = ((CostType)data[(byte)Params.giftCostType]).ToString();
        optionIndex = this.costType.options.FindIndex(x => x.text == giftCostType);
        this.costType.value = optionIndex;

        giftImage.text = (string)data[(byte)Params.giftImage];

        var giftEventTypeText = ((GiftEventType)data[(byte)Params.giftEventType]).ToString();
        optionIndex = this.giftEventType.options.FindIndex(x => x.text == giftEventTypeText);
        this.giftEventType.value = optionIndex;

        var giftTypeText = ((GiftType)data[(byte)Params.giftType]).ToString();
        optionIndex = this.giftType.options.FindIndex(x => x.text == giftTypeText);
        this.giftType.value = optionIndex;

        var giftEffectText = ((ExtraEffect)data[(byte)Params.giftExtraEffect]).ToString();
        optionIndex = this.giftEffect.options.FindIndex(x => x.text == giftEffectText);
        this.giftEffect.value = optionIndex;

        
    }
}
