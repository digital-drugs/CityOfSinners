using ExitGames.Client.Photon;
using Share;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AdminExtra : MonoBehaviour
{
   public static AdminExtra instance;

    void Start()
    {
        instance = this;

        UiHelper.SetupTMPDropMenu(extraId, typeof(ExtraEffect));
        UiHelper.SetupTMPDropMenu(extraType, typeof(ExtraType));
        UiHelper.SetupTMPDropMenu(extraCurrencyType, typeof(CurrencyType));
        UiHelper.SetupTMPDropMenu(extraUseType, typeof(ExtraUseType));
        UiHelper.SetupTMPDropMenu(extraPhase, typeof(GamePhase));
    }   

    [SerializeField] private TMP_Dropdown extraId;
    [SerializeField] private TMP_InputField extraName;
    [SerializeField] private TMP_InputField extraDescription;
    [SerializeField] private TMP_Dropdown extraType;
    [SerializeField] private TMP_Dropdown extraCurrencyType;
    [SerializeField] private TMP_InputField extraCost;
    [SerializeField] private TMP_InputField extraBuyCount;
    [SerializeField] private TMP_InputField extraDuration;
    [SerializeField] private TMP_InputField extraCount;
    [SerializeField] private TMP_InputField extraGameCount;
    [SerializeField] private TMP_Dropdown extraUseType;
    [SerializeField] private TMP_Dropdown extraPhase;
    [SerializeField] private TMP_InputField imageUrl;
    public void ButtonSaveExtra()
    {
        var extraData = new Dictionary<byte, object>();

        extraData.Add((byte)Params.ExtraId, extraId.captionText.text);
        extraData.Add((byte)Params.ExtraName, extraName.text);
        extraData.Add((byte)Params.ExtraDescription, extraDescription.text);
        extraData.Add((byte)Params.ExtraType, extraType.captionText.text);
        extraData.Add((byte)Params.CurrencyType, extraCurrencyType.captionText.text);
        extraData.Add((byte)Params.ExtraUseType, extraUseType.captionText.text);

        var extraCost = int.Parse(this.extraCost.text);
        extraData.Add((byte)Params.ExtraCost, extraCost);

        var extraBuyCount = int.Parse(this.extraBuyCount.text);
        extraData.Add((byte)Params.ExtraBuyCount, extraBuyCount);

        var extraDuration = int.Parse(this.extraDuration.text);
        extraData.Add((byte)Params.ExtraDuration, extraDuration);

        var extraGameCount = int.Parse(this.extraGameCount.text);
        extraData.Add((byte)Params.ExtraGameCount, extraGameCount);

        extraData.Add((byte)Params.ExtraPhase, extraPhase.captionText.text);

        extraData.Add((byte)Params.ExtraImageUrl, imageUrl.text);

        PhotonManager.Inst.peer.SendOperation((byte)Request.SaveExtra, extraData, PhotonManager.Inst.sendOptions);
    }

    public void ButtonLoadExtras()
    {
        PhotonManager.Inst.peer.SendOperation(
            (byte)Request.LoadExtras,
            new Dictionary<byte, object>(),
            PhotonManager.Inst.sendOptions);
    }

    public void ShowExtras(ParameterDictionary data)
    {
        UiHelper.ClearContainer(adminExtrasUiContainer);

        var extrasData = (Dictionary<string, object>)data[(byte)Params.Extras];

        Debug.Log(extrasData.Count);

        foreach (var ed in extrasData)
        {
            var extraData = (Dictionary<byte, object>)ed.Value;

            AddAdminExtraUi(extraData);
        }
    }

    [SerializeField] private AdminExtraUi adminExtraUiPrefab;
    [SerializeField] private Transform adminExtrasUiContainer;
    private void AddAdminExtraUi(Dictionary<byte, object> data)
    {
        var newAdminExtraUi = Instantiate(adminExtraUiPrefab);
        UiHelper.AssignObjectToContainer(newAdminExtraUi.gameObject, adminExtrasUiContainer);

        newAdminExtraUi.Assign(data);
    }

    public void AssignExtraToForm(Dictionary<byte, object> data)
    {
        Debug.Log($"assign extra");

        int optionIndex = 0;

        var extraId = (string)data[(byte)Params.ExtraId];
        optionIndex = this.extraId.options.FindIndex(x => x.text == extraId);
        this.extraId.value = optionIndex;

        //this.extraId.captionText.text = (string)data[(byte)Params.ExtraId];

        extraName.text = (string)data[(byte)Params.ExtraName];
        extraDescription.text= (string)data[(byte)Params.ExtraDescription];

        var extraType = (string)data[(byte)Params.ExtraType];
        optionIndex = this.extraType.options.FindIndex(x => x.text == extraType);
        this.extraType.value = optionIndex;

        var currencyType = (string)data[(byte)Params.CurrencyType];
        optionIndex = this.extraCurrencyType.options.FindIndex(x => x.text == currencyType);
        this.extraCurrencyType.value = optionIndex;

        var extraUseType = (string)data[(byte)Params.ExtraUseType];
        optionIndex = this.extraUseType.options.FindIndex(x => x.text == extraUseType);
        this.extraUseType.value = optionIndex;

        this.extraCost.text = $"{(int)data[(byte)Params.ExtraCost]}";

        this.extraBuyCount.text = $"{(int)data[(byte)Params.ExtraBuyCount]}";

        this.extraDuration.text = $"{(int)data[(byte)Params.ExtraDuration]}"; 

        this.extraGameCount.text = $"{(int)data[(byte)Params.ExtraGameCount]}";

        var extraPhase = (string)data[(byte)Params.ExtraPhase];
        optionIndex = this.extraPhase.options.FindIndex(x => x.text == extraPhase);
        this.extraPhase.value = optionIndex;

        this.imageUrl.text = $"{(string)data[(byte)Params.ExtraImageUrl]}";
    }
}
