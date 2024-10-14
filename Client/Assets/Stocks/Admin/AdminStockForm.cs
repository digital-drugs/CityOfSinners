using ExitGames.Client.Photon;
using Share;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AdminStockForm : MonoBehaviour
{
    [SerializeField] public AdminSetForm setForm;

    [SerializeField] public Transform setsContent;
    [SerializeField] public Transform message;
    [SerializeField] public Transform saveButton;
    [SerializeField] public Transform addButton;

    [SerializeField] private AdminSetUi setPrefab;

    [SerializeField] private TMP_InputField stockName;
    [SerializeField] private TMP_InputField start;
    [SerializeField] private TMP_InputField end;


    private int id = 0;
    private int lastNumber = 0;

    public void Start()
    {
        var now = DateTime.Today;
        start.text = now.ToString();
        //end.text = (now.AddDays(1)).ToString();
    }

    public void ShowStock()
    {
        this.gameObject.SetActive(true);
        message.gameObject.SetActive(true);
        addButton.gameObject.SetActive(false);

        UiHelper.ClearContainer(setsContent);

        id = 0;
        //lastNumber = 0;
    }
    public void ShowStock(ParameterDictionary data)
    {
        this.gameObject.SetActive(true);
        message.gameObject.SetActive(false);
        addButton.gameObject.SetActive(true);

        this.id = (int)data[(byte)Params.Id];

        stockName.text = (string)data[(byte)Params.Name];
        start.text = (string)data[(byte)Params.Start];
        end.text = (string)data[(byte)Params.End];

        var sets = (Dictionary<int, object>)data[(byte)Params.sets];

        //UnityEngine.Debug.Log(sets.Count);
        UiHelper.ClearContainer(setsContent);
        lastNumber = 0;
                
        foreach (var el in sets)
        {
            var setData = (Dictionary<byte, object>)el.Value;

            AddSetElementUi(setData);

            lastNumber++;
        }
    }

    private void AddSetElementUi(Dictionary<byte, object> data)
    {
        var newEleemntUi = Instantiate(setPrefab);
        UiHelper.AssignObjectToContainer(newEleemntUi.gameObject, setsContent);

        newEleemntUi.Assign(data);
    }

    public void SaveStock()
    {
        var data = new Dictionary<byte, object>();

        data.Add((byte)Params.Id, id);
        data.Add((byte)Params.Name, stockName.text); ;
        data.Add((byte)Params.Start, start.text);
        data.Add((byte)Params.End, end.text);

        PhotonManager.Inst.peer.SendOperation(
            (byte)Request.SaveStock,
            data,
            PhotonManager.Inst.sendOptions);
    }

    public void AddSet()
    {
        UnityEngine.Debug.Log("lastNumber " + lastNumber);
        setForm.ShowSet(lastNumber, id);
    }

    public void ShowSet(ParameterDictionary data)
    {
        setForm.ShowSet(data);
    }

    public void ShowItem(ParameterDictionary data)
    {
        setForm.ShowItem(data);
    }

    public void OnStartChange()
    {
        var date = DateTime.Parse(start.text);
        end.text = (date.AddDays(1)).ToString();
    }

}
