using Share;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AdminSetUi : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI numberText;
    [SerializeField] private TextMeshProUGUI costText;
    //[SerializeField] private TMP_InputField costText;

    [SerializeField] private Button button;

    private Dictionary<byte, object> data;

    public void Assign(Dictionary<byte, object> data)
    {
        this.data = data;

        numberText.text = ((int)data[(byte)Params.Number]).ToString();
        costText.text = ((int)data[(byte)Params.Cost]).ToString();

        button.onClick.AddListener(() => LoadSet());
    }

    public void LoadSet()
    {
        //UnityEngine.Debug.Log("Load set " + (int)data[(byte)Params.Id]);
        AdminStocks.instance.LoadSet((int)data[(byte)Params.Id]);
    }
}
