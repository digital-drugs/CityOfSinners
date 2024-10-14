using ExitGames.Client.Photon;
using Share;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEngine;

public class AdminStocks : MonoBehaviour
{
    public static AdminStocks instance;

    int stockId = 0;

    List<GameObject> elements = new List<GameObject>();

    void Start()
    {
        instance = this;
    }

    [SerializeField] private AdminStockUi stockPrefab;
    [SerializeField] private AdminSetUi setPrefab;
    [SerializeField] private AdminItemUi ItemPrefab;

    [SerializeField] private Transform stocksContent;
    [SerializeField] private Transform setsContent;
    [SerializeField] private Transform ItemsContent;

    [SerializeField] public AdminStockForm stockForm;

    [SerializeField] private TMP_InputField stockName;
    [SerializeField] private TMP_InputField giftDescription;
    
    [SerializeField] private TMP_InputField giftCost;
    [SerializeField] private TMP_Dropdown costType;

    [SerializeField] private TMP_InputField giftImage;
    
    [SerializeField] private TMP_Dropdown giftEventType;
    [SerializeField] private TMP_Dropdown giftType;
    [SerializeField] private TMP_Dropdown giftEffect;

    public void ButtonSave()
    {
        var giftData = new Dictionary<byte, object>();

        //giftData.Add((byte)Params.giftId, giftId);

        //giftData.Add((byte)Params.giftName, giftName.text);
        //giftData.Add((byte)Params.giftDescription, giftDescription.text);
        //giftData.Add((byte)Params.giftCost, giftCost.text);
        //giftData.Add((byte)Params.giftImage, giftImage.text);

        //giftData.Add((byte)Params.giftEventType, giftEventType.captionText.text);
        //giftData.Add((byte)Params.giftType, giftType.captionText.text);
        //giftData.Add((byte)Params.giftCostType, costType.captionText.text);
        //giftData.Add((byte)Params.giftExtraEffect, giftEffect.captionText.text);

        //PhotonManager.Inst.peer.SendOperation((byte)Request.SaveGift, giftData, PhotonManager.Inst.sendOptions);
    }

    public void ButtonLoadStocks()
    {
        UnityEngine.Debug.Log("load stocks");

        PhotonManager.Inst.peer.SendOperation(
            (byte)Request.LoadStocks,
            new Dictionary<byte, object>(),
            PhotonManager.Inst.sendOptions);
    }

    public void ButtonGetStocksList()
    {
        UnityEngine.Debug.Log("Get Stocks list");

        PhotonManager.Inst.peer.SendOperation(
            (byte)Request.GetStocksList,
            new Dictionary<byte, object>(),
            PhotonManager.Inst.sendOptions);
    }

    public void ShowStocks(ParameterDictionary data)
    {
        
        UnityEngine.Debug.Log("show stocks");

        var stocksData = (Dictionary<int, object>)data[(byte)Params.stocks];

        UnityEngine.Debug.Log(stocksData.Count);
        
        UiHelper.ClearContainer(stocksContent);
                
        foreach (var el in stocksData)
        {
            var stockData = (Dictionary<byte, object>)el.Value;

            UnityEngine.Debug.Log((string)stockData[(byte)Params.Name]);

            var sets = (Dictionary<int, object>)stockData[(byte)Params.sets];

            UnityEngine.Debug.Log("sets count " + sets.Count);


            foreach(var set in sets)
            {
                var setData = (Dictionary<byte, object>)set.Value;

                var items = (Dictionary<int, object>)setData[(byte)Params.items];

                UnityEngine.Debug.Log("items count " + items.Count);

                foreach(var item in items)
                {
                    var itemData = (Dictionary<byte, object>)item.Value;

                    UnityEngine.Debug.Log("item amount " + (int)itemData[(byte)Params.Amount]);
                }
            }

            //Add to stocksContent
            //AddStockElementUi(stockData, el.Key);
        }
    }

    public void ShowStocksList(ParameterDictionary data)
    {
        UnityEngine.Debug.Log("Show stocks list");

        var stocksData = (Dictionary<int, object>)data[(byte)Params.stocks];

        UiHelper.ClearContainer(stocksContent);

        foreach (var el in stocksData)
        {
            var stockData = (Dictionary<byte, object>)el.Value;

            AddStockElementUi(stockData);
        }
    }
    private void AddStockElementUi(Dictionary<byte, object> data)
    {
        UnityEngine.Debug.Log("add stock element");

        var newEleemntUi = Instantiate(stockPrefab);
        UiHelper.AssignObjectToContainer(newEleemntUi.gameObject, stocksContent);

        newEleemntUi.Assign(data);
    }

    private void AddSetElementUi(Dictionary<byte, object> data, int key)
    {
        var newEleemntUi = Instantiate(setPrefab);
        UiHelper.AssignObjectToContainer(newEleemntUi.gameObject, setsContent);

        newEleemntUi.Assign(data);
    }

    private void AddItemElementUi(Dictionary<byte, object> data)
    {
        var newEleemntUi = Instantiate(ItemPrefab);
        UiHelper.AssignObjectToContainer(newEleemntUi.gameObject, ItemsContent);

        newEleemntUi.Assign(data);
    }

    public void AssignItemsToForm(Dictionary<byte, object> data)
    {
        UiHelper.ClearContainer(ItemsContent);

        var items = (Dictionary<int, object>)data[(byte)Params.items];

        foreach ( var item in items )
        {
            var itemData = (Dictionary<byte, object>)item.Value;

            AddItemElementUi(itemData);
        }
    }

    public void AssignToForm(Dictionary<byte, object> data, int key)
    {
        this.stockId = key;

        //UnityEngine.Debug.Log("Assign form " + this.stockId);
        //Debug.Log($"assign gift");
        
        //int optionIndex = 0;

        //giftId = (int)data[(byte)Params.giftId];

        stockName.text = (string)data[(byte)Params.Name];

        UiHelper.ClearContainer(setsContent);
        UiHelper.ClearContainer(ItemsContent);

        var sets = (Dictionary<int, object>)data[(byte)Params.sets];

        foreach(var el in sets)
        {
            var setData = (Dictionary<byte, object>)el.Value;

            UnityEngine.Debug.Log("set cost " + (int)setData[(byte)Params.Cost]);

            AddSetElementUi(setData, el.Key);
        }

        //giftDescription.text= (string)data[(byte)Params.giftDescription];
        //giftCost.text = ((int)data[(byte)Params.giftCost]).ToString();

        //var giftCostType = ((CostType)data[(byte)Params.giftCostType]).ToString();
        //optionIndex = this.costType.options.FindIndex(x => x.text == giftCostType);
        //this.costType.value = optionIndex;

        //giftImage.text = (string)data[(byte)Params.giftImage];

        //var giftEventTypeText = ((GiftEventType)data[(byte)Params.giftEventType]).ToString();
        //optionIndex = this.giftEventType.options.FindIndex(x => x.text == giftEventTypeText);
        //this.giftEventType.value = optionIndex;

        //var giftTypeText = ((GiftType)data[(byte)Params.giftType]).ToString();
        //optionIndex = this.giftType.options.FindIndex(x => x.text == giftTypeText);
        //this.giftType.value = optionIndex;

        //var giftEffectText = ((ExtraEffect)data[(byte)Params.giftExtraEffect]).ToString();
        //optionIndex = this.giftEffect.options.FindIndex(x => x.text == giftEffectText);
        //this.giftEffect.value = optionIndex;


    }

    public void CreateStock()
    {
        stockForm.ShowStock();
    }

    public void LoadStock(int id)
    {
        UnityEngine.Debug.Log("load stock " + id);

        var parametrs = new Dictionary<byte, object>();
        parametrs.Add((byte)Params.Id, id);


        PhotonManager.Inst.peer.SendOperation(
            (byte)Request.LoadStock,
            parametrs,
            PhotonManager.Inst.sendOptions);
    }

    public void LoadSet(int id)
    {
        UnityEngine.Debug.Log("load set " + id);

        var parametrs = new Dictionary<byte, object>();
        parametrs.Add((byte)Params.Id, id);


        PhotonManager.Inst.peer.SendOperation(
            (byte)Request.LoadSet,
            parametrs,
            PhotonManager.Inst.sendOptions);
    }

    public void LoadItem(int id)
    {
        UnityEngine.Debug.Log("load item " + id);

        var parametrs = new Dictionary<byte, object>();
        parametrs.Add((byte)Params.Id, id);


        PhotonManager.Inst.peer.SendOperation(
            (byte)Request.LoadItem,
            parametrs,
            PhotonManager.Inst.sendOptions);
    }

    public void ShowStock(ParameterDictionary data)
    {
        stockForm.ShowStock(data);
    }

    public void ShowSet(ParameterDictionary data)
    {
        stockForm.ShowSet(data);
    }

    public void ShowItem(ParameterDictionary data)
    {
        stockForm.ShowItem(data);
    }
}
