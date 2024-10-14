using Share;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PeriodButtonScript : MonoBehaviour
{
    [SerializeField] TMP_Text name;
    [SerializeField] private Button button;

    ReitingType reitingType;
    ReitingInterval reitingInterval;
    Tops tops;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Assign(string name, ReitingType type, ReitingInterval period, Tops tops)
    {
        this.reitingType = type;
        this.reitingInterval = period;

        this.tops = tops;

        this.name.text = name;

        button.onClick.AddListener(() => GetReiting());
    }

    public void GetReiting()
    {
        UnityEngine.Debug.Log("Period button clicked");
        
        var parametrs = new Dictionary<byte, object>();
        parametrs.Add((byte)Params.Id, reitingType);
        parametrs.Add((byte)Params.periods, reitingInterval);

        //tops.BuildPeriodButtons(data);
        tops.ClearReiting();

        PhotonManager.Inst.peer.SendOperation(
            (byte)Request.GetReiting,
            parametrs,
            PhotonManager.Inst.sendOptions);
    }
}
