using Share;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReitingButtonScript : MonoBehaviour
{
    [SerializeField] TMP_Text name;
    [SerializeField] private Button button;

    Dictionary<byte, object> data;
    Tops tops;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Assign(Dictionary<byte, object> data, Tops tops)
    {
        this.data = data;
        this.tops = tops;

        UnityEngine.Debug.Log("reiting id " + data[(byte)Params.Id]);

        name.text = (string)data[(byte)Params.Name];

        button.onClick.AddListener(() => GetReiting());
    }

    public void GetReiting()
    {
        UnityEngine.Debug.Log("BUTTON 3");
        
        var parametrs = new Dictionary<byte, object>();
        parametrs.Add((byte)Params.Id, data[(byte)Params.Id]);
        tops.ClearReiting();

        if (data.ContainsKey((byte)Params.awardPeriod))
        {
            UnityEngine.Debug.Log("Award period " + data[(byte)Params.awardPeriod]);
            int p = (int)data[(byte)Params.awardPeriod];
            byte bp = (byte)p;
            tops.SetEndTime(bp);
        }

        tops.BuildPrizes(data);
        //if (data.ContainsKey((byte)Params.sets))
        //{
        //    UnityEngine.Debug.Log("HAVE prizes");
        //}

        tops.BuildPeriodButtons(data);
        
        PhotonManager.Inst.peer.SendOperation(
            (byte)Request.GetReiting,
            parametrs,
            PhotonManager.Inst.sendOptions);
    }
}
