using ExitGames.Client.Photon;
using Share;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Tops : MonoBehaviour
{
    [SerializeField] private Transform topContent;
    [SerializeField] private ReitingButtonScript reitingButton;

    [SerializeField] private Transform reitingContent;
    [SerializeField] private TopRowScript reitingRowPrefab;

    [SerializeField] private Transform periodsButtonsContent;
    [SerializeField] private PeriodButtonScript periodButtonPrefab;

    [SerializeField] private Transform prizesContent;
    [SerializeField] private PrizeRowScript prizePrefab;

    [SerializeField] private TMP_Text message;
    [SerializeField] private TMP_Text endTime;

    Dictionary<int, object> Reitings;
    Dictionary<byte, object> Periods;

    public static Tops inst;

    [SerializeField] private GameObject window_Tops;
    public void ShowTab()
    {
        window_Tops.SetActive(true);
        //UnityEngine.Debug.Log("SHOW TAB");
        ClearReiting();

        PhotonManager.Inst.peer.SendOperation(
            (byte)Request.GetReitings,
            new Dictionary<byte, object>(),
            PhotonManager.Inst.sendOptions);

    }

    


    // Start is called before the first frame update
    void Start()
    {
        inst = this;
        //this.gameObject.SetActive(false);
        message.gameObject.SetActive(false);
    }

    public void BuildButtons(ParameterDictionary parameters)
    {
        
        var reitings = (Dictionary<int, object>)parameters[(byte)Params.reitings];
        var periods = (Dictionary<byte, object>)parameters[(byte)Params.periods];
        Periods = periods;

        foreach(var p in Periods)
        {
            UnityEngine.Debug.Log("period " + p);
        }

        UnityEngine.Debug.Log("periods count " + periods.Count);

        //UnityEngine.Debug.Log(reitings.Count);
        UiHelper.ClearContainer(topContent);
        foreach (var el in reitings)
        {
            var ed = (Dictionary<byte,object>)el.Value;
            //UnityEngine.Debug.Log(ed[(byte)Params.Name]);
            AddButtonUi(ed);
        }
    }

    public void BuildPeriodButtons(Dictionary<byte, object> data)
    {
        var periods = (Dictionary<int, object>)data[(byte)Params.periods];
        UnityEngine.Debug.Log("periods " + periods.Count);

        UiHelper.ClearContainer(periodsButtonsContent);
        foreach (var period in periods)
        {
            var pd = (Dictionary<byte, object>)period.Value;
            //UnityEngine.Debug.Log("period id " + pd[(byte)Params.Id]);

            AddPeriodButton(pd, data);
        }
    }

    public void BuildPrizes(Dictionary<byte, object> data)
    {
        UiHelper.ClearContainer(prizesContent);
        if (!data.ContainsKey((byte)Params.sets))
        {
            return;
        }

        var sets = (Dictionary<int, object>)data[(byte)Params.sets];
        int count = 1;
        foreach(var set in sets)
        {
            var setData = (Dictionary<byte, object>)set.Value;

            UnityEngine.Debug.Log("add set");
            
            var newEleemntUi = Instantiate(prizePrefab);
            UiHelper.AssignObjectToContainer(newEleemntUi.gameObject, prizesContent);
            newEleemntUi.SetText(count++ + " место:");

            var items = (Dictionary<int, object>)setData[(byte)Params.items];

            foreach(var item in items)
            {
                var itemData = (Dictionary<byte, object>)item.Value;
                string resourse = ((ResourseType)itemData[(byte)Params.Resourse]).ToString();
                int amount = (int)itemData[(byte)Params.Amount];

                newEleemntUi = Instantiate(prizePrefab);
                UiHelper.AssignObjectToContainer(newEleemntUi.gameObject, prizesContent);
                newEleemntUi.SetText(amount + " " + resourse);
            }
        }

        //var periods = (Dictionary<int, object>)data[(byte)Params.periods];
        //UnityEngine.Debug.Log("periods " + periods.Count);

        //UiHelper.ClearContainer(periodsButtonsContent);
        //foreach (var period in periods)
        //{
        //    var pd = (Dictionary<byte, object>)period.Value;
        //    AddPeriodButton(pd, data);
        //}
    }

    public void AddPeriodButton(Dictionary<byte, object> period_data, Dictionary<byte, object> reiting_data)
    {
        var newEleemntUi = Instantiate(periodButtonPrefab);
        UiHelper.AssignObjectToContainer(newEleemntUi.gameObject, periodsButtonsContent);

        newEleemntUi.Assign(((ReitingInterval)period_data[(byte)Params.Id]).ToString(), (ReitingType)reiting_data[(byte)Params.Id], (ReitingInterval)period_data[(byte)Params.Id], this);
    }

    public void AddButtonUi(Dictionary<byte, object> data)
    {
        var newEleemntUi = Instantiate(reitingButton);
        UiHelper.AssignObjectToContainer(newEleemntUi.gameObject, topContent);

        newEleemntUi.Assign(data, this);
    }

    public void BuildReiting(ParameterDictionary parameters)
    {
        UnityEngine.Debug.Log("Build reiting");

        if (parameters.ContainsKey((byte)Params.message))
        {
            message.gameObject.SetActive(true);
            message.text = parameters[(byte)Params.message].ToString();
            return;
        }
        else
        {
            message.gameObject.SetActive(false);
        }

        var reiting = (Dictionary<int, object>)parameters[(byte)Params.Rating];

        //UnityEngine.Debug.Log(reiting.Count);

        UiHelper.ClearContainer(reitingContent);
        foreach (var row in reiting)
        {
            var row_data = (Dictionary<byte, object>)row.Value;

            //foreach(var k in row_data.Keys)
            //{
            //    UnityEngine.Debug.Log((Params)k);
            //}
            //UnityEngine.Debug.Log(row_data[(byte)Params.Name]);
            AddRow(row_data);
        }
    }

    public void AddRow(Dictionary<byte, object> data)
    {
        var newEleemntUi = Instantiate(reitingRowPrefab);
        UiHelper.AssignObjectToContainer(newEleemntUi.gameObject, reitingContent);

        newEleemntUi.Assign(data);
    }

    public void ClearReiting()
    {
        UiHelper.ClearContainer(reitingContent);
        endTime.text = "";
    }

    public void SetEndTime(byte i)
    {
        endTime.text = "Окончание: " + Periods[i].ToString();
    }























    // Update is called once per frame
    void Update()
    {
        
    }
}
