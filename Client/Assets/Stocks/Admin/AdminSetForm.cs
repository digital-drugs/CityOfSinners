using ExitGames.Client.Photon;
using Share;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AdminSetForm : MonoBehaviour
{
    int id = 0;
    int stockId = 0;
    
    int lastNumber = 0;

    [SerializeField] private TMP_InputField number;
    [SerializeField] private TMP_InputField cost;

    [SerializeField] public Transform message;
    [SerializeField] public Transform addButton;

    [SerializeField] public Transform itemsContent;

    [SerializeField] private AdminItemUi itemPrefab;

    [SerializeField] private AdminItemForm itemForm;

    public void ShowSet(int lastNumber, int stockId)
    {
        this.gameObject.SetActive(true);
        this.id = 0;
        this.stockId = stockId;
        this.lastNumber = 0;

        number.text = (lastNumber + 1).ToString();
        cost.text = "0";

        UiHelper.ClearContainer(itemsContent);

        this.message.gameObject.SetActive(true);
        this.addButton.gameObject.SetActive(false);
    }

    public void ShowSet(ParameterDictionary data)
    {
        this.gameObject.SetActive(true);
        this.id = (int)data[(byte)Params.Id];
        this.stockId = (int)data[(byte)Params.StockId];
        this.lastNumber = 0;

        number.text = ((int)data[(byte)Params.Number]).ToString();
        cost.text = ((int)data[(byte)Params.Cost]).ToString();

        this.message.gameObject.SetActive(false);
        this.addButton.gameObject.SetActive(true);

        UiHelper.ClearContainer(itemsContent);

        var items = (Dictionary<int, object>)data[(byte)Params.items];

        foreach (var el in items)
        {
            var itemData = (Dictionary<byte, object>)el.Value;

            AddItemElementUi(itemData);

            lastNumber++;
        }


    }

    private void AddItemElementUi(Dictionary<byte, object> data)
    {
        var newEleemntUi = Instantiate(itemPrefab);
        UiHelper.AssignObjectToContainer(newEleemntUi.gameObject, itemsContent);

        newEleemntUi.Assign(data);
    }

    public void SaveSet()
    {
        //UnityEngine.Debug.Log("save set");

        var data = new Dictionary<byte, object>();

        data.Add((byte)Params.Id, id);
        data.Add((byte)Params.StockId, stockId); ;
        data.Add((byte)Params.Number, int.Parse(number.text));
        data.Add((byte)Params.Cost, int.Parse(cost.text));

        UnityEngine.Debug.Log("id " + id);
        UnityEngine.Debug.Log("stockId " + stockId);
        UnityEngine.Debug.Log("number " + int.Parse(number.text));
        UnityEngine.Debug.Log("cost " + int.Parse(cost.text));

        PhotonManager.Inst.peer.SendOperation(
            (byte)Request.SaveSet,
            data,
            PhotonManager.Inst.sendOptions);
    }

    public void ShowItem()
    {
        itemForm.ShowItem(newNumber:lastNumber, setId:id) ;
    }

    public void ShowItem(ParameterDictionary data)
    {
        itemForm.ShowItem(data);
    }


}
