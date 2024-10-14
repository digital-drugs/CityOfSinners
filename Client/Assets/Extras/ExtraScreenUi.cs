using ExitGames.Client.Photon;
using Share;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using TMPro;
using UnityEngine;

public class ExtraScreenUi : MonoBehaviour
{
    public Sprite clickExtra;
    public Sprite targetExtra;
    public Sprite autoExtra;
    public Sprite bunkerExtra;

    public static ExtraScreenUi instance;
    private void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

   

    [SerializeField] private GameObject wi_Extras;
    [SerializeField] private ExtraUi extraUiPrefab;
    private Dictionary<string, Transform> extrasContainers = new Dictionary<string, Transform>();
    public Dictionary<string, ExtraUi> extraUis { get; private set; } = new Dictionary<string, ExtraUi>();
    public void SetupExtras(ParameterDictionary parameters)
    {
        SetupAllExtras(parameters);

        SetupUserExtrasSlots(parameters);
    }

    public ExtraUi FindExtraUi(ExtraEffect extraId)
    {
        return FindExtraUi(extraId.ToString());
    }

    public ExtraUi FindExtraUi(string extraId)
    {
        if (extraUis.ContainsKey(extraId))
        {
            return extraUis[extraId];
        }

        return null;
    }

    private void SetupAllExtras(ParameterDictionary parameters)
    {
        var extrasData = (Dictionary<string, object>)parameters[(byte)Params.Extras];

        foreach (var ed in extrasData)
        {
            var extraId = ed.Key;
            var extraData = (Dictionary<byte, object>)ed.Value;

            var extraType = (string)extraData[(byte)Params.ExtraType];

            Transform containerTransform = null;

            if (extrasContainers.ContainsKey(extraType))
            {
                containerTransform = extrasContainers[extraType];
            }
            else
            {
                containerTransform = AddExtraPanel(extraType);

                AddExtraButton(extraType, containerTransform);
            }

            var newExtraUi = Instantiate(extraUiPrefab, containerTransform);
            extraUis.Add(extraId, newExtraUi);

            newExtraUi.Assign(extraId, extraData);
        }
    }

    [SerializeField] private ExtraSlotUi extraSlotUiPrefab;
    [SerializeField] private Transform extraSlotUisContainer;
    private Dictionary<int, ExtraSlotUi> extraSlotUis = new Dictionary<int, ExtraSlotUi>();
    private void SetupUserExtrasSlots(ParameterDictionary parameters)
    {
        var slotsData = (Dictionary<int, string>)parameters[(byte)Params.ExtraSlots];

        var userData = (Dictionary<byte, object>)parameters[(byte)Params.UserData];


        //создаем слоты
        var userSlotCount = (int)userData[(byte)Params.SlotCount];

        var slotContainerRect = extraSlotUisContainer.GetComponent<RectTransform>().rect;

        var slotSizeX = slotContainerRect.size.x / userSlotCount;

        //Debug.Log($"create slots => {slotContainerRect.size.x} {userSlotCount} slot => {slotSizeX}");
        //Debug.Log($"canvas scaler => {mainCanvas.scaleFactor} slot => {slotSizeX* mainCanvas.scaleFactor}");

        for (int i = 0; i < userSlotCount; i++)
        {
            var newExtraSlot = Instantiate(extraSlotUiPrefab);
            extraSlotUis.Add(i, newExtraSlot);

            var localPosition = new Vector3(i * slotSizeX, 0, 0);

            newExtraSlot.transform.SetParent(extraSlotUisContainer);
            newExtraSlot.transform.localScale = Vector3.one;

            var coef = slotSizeX/189 ;

            newExtraSlot.GetComponent<RectTransform>().anchoredPosition = localPosition;
            newExtraSlot.GetComponent<RectTransform>().sizeDelta = new Vector2(189* coef, 224* coef);

            newExtraSlot.SetId(i);
        }

        for (int i = 0; i < userSlotCount; i++)
        {
            if (slotsData.ContainsKey(i))
            {
                var extraUi = extraUis[slotsData[i]];

                extraSlotUis[i].Assign(extraUi.GetClone(),false);

                //Debug.Log($"set slot {i}");
            }
            else
            {
                
            }
        }
    }

    [SerializeField] private Transform extraPanelsContainer;
    [SerializeField] private Transform extraPanelPrefab;
    private Transform AddExtraPanel(string extraType)
    {
        var result = Instantiate(extraPanelPrefab);
        UiHelper.AssignObjectToContainer(result.gameObject, extraPanelsContainer);

        result.name = $"{extraType} extras";
        extrasContainers.Add(extraType, result);

        result.gameObject.SetActive(false);

        return result;
    }

    [SerializeField] private Transform extraButtonsContainer;
    [SerializeField] private ExtraButtonUi extraButtonUiPrefab;
    private void AddExtraButton(string extraType, Transform extraContainer)
    {
        var newExtraButton = Instantiate(extraButtonUiPrefab);
        UiHelper.AssignObjectToContainer(newExtraButton.gameObject, extraButtonsContainer);


        Action buttonAction = () => { SelectExtra(extraContainer); };

        newExtraButton.Assign(extraType, buttonAction);
    }

    private void SelectExtra(Transform extraContainer)
    {
        //Debug.Log($"skillContainers {skillContainers.Count}");

        foreach (var c in extrasContainers.Values)
        {
            c.gameObject.SetActive(false);
        }

        extraContainer.gameObject.SetActive(true);
    }

    public void OpenExtraWindow()
    {
        wi_Extras.SetActive(true);
    }

    public void CloseExtraWindow()
    {
        wi_Extras.SetActive(false);
    }

    [SerializeField] private TextMeshProUGUI extraNameText;
    [SerializeField] private TextMeshProUGUI extraDescriptionText;
    [SerializeField] private TextMeshProUGUI extraCountText;
    [SerializeField] private TextMeshProUGUI extraInGameCountText;
    [SerializeField] private TextMeshProUGUI extraBuyCountText;
    [SerializeField] private TextMeshProUGUI extraCostText;

    private string currentSelectedExtraId;
    public void SelectExtraUi(string extraId, Dictionary<byte, object> extraData)
    {
        currentSelectedExtraId = extraId;

        extraNameText.text = (string)extraData[(byte)Params.ExtraName];
        extraDescriptionText.text = (string)extraData[(byte)Params.ExtraDescription];

        if (extraData.ContainsKey((byte)Params.ExtraCount))
        {
            var extraCount = (int)extraData[(byte)Params.ExtraCount];
            extraCountText.text = $"В наличии: {extraCount}";
        }
        else
        {
            extraCountText.text= $"В наличии: 0";
        }

        var extraGameCount = (int)extraData[(byte)Params.ExtraGameCount];
        extraInGameCountText.text = $"Можно использовать в игре: {extraGameCount}";

        //покупка
        var extraBuyCount = (int)extraData[(byte)Params.ExtraBuyCount];
        extraBuyCountText.text = $"x{extraBuyCount}";

        var extraCost = (int)extraData[(byte)Params.ExtraCost];
        extraCostText.text = $"цена: {extraCost}";
    }

    public void BuyExtra()
    {
        if(string.IsNullOrEmpty(currentSelectedExtraId))
        {
            Debug.Log($"no selected extra to buy");
            return;
        }

        Debug.Log($"selected extra id = {currentSelectedExtraId}");

        var extraData = new Dictionary<byte, object>();

        extraData.Add((byte)Params.ExtraId, currentSelectedExtraId);

        PhotonManager.Inst.peer.SendOperation((byte)Request.BuyExtra, extraData, PhotonManager.Inst.sendOptions);
    }

    public void UpdateExtraCount(ParameterDictionary parameters)
    {
        var extraId = (string)parameters[(byte)Params.ExtraId];

        var extraUi = extraUis[extraId];

        var extraCount = (int)parameters[(byte)Params.ExtraCount];

        if (currentSelectedExtraId == extraId)
        {
            extraCountText.text = $"В наличии: {extraCount}";
        }


        extraUi.UpdateExtraCount(extraCount);
    }

    public bool assignExtraToSlot = false;
    //public void AssignExtraToSlot()
    //{
    //    assignExtraToSlot=true;

    //    Debug.Log($"assignExtraToSlot {assignExtraToSlot}");
    //}

    public ExtraUi GetCurrentExtraUi()
    {
        return extraUis[currentSelectedExtraId]; 
    }

    //public bool ExtraInUse(string extraId)
    //{
    //    foreach(var su in extraSlotUis.Values)
    //    {
    //        if(  su.extraId == extraId)
    //        {
    //            return true;
    //        }
    //    }

    //    return false;
    //}
}
