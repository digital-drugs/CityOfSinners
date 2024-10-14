using Share;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AdminItemUi : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI numberText;
    [SerializeField] private TextMeshProUGUI resourseText;
    [SerializeField] private TextMeshProUGUI resourseIdText;
    [SerializeField] private TextMeshProUGUI amountText;

    [SerializeField] private Button button;

    private Dictionary<byte, object> data;

    public void Assign(Dictionary<byte, object> data)
    {
        this.data = data;

        numberText.text = ((int)data[(byte)Params.Number]).ToString();
        amountText.text = ((int)data[(byte)Params.Amount]).ToString();

        resourseText.text = (string)data[(byte)Params.Resourse];
        resourseIdText.text = (string)data[(byte)Params.ResourseId];

        button.onClick.AddListener(() => LoadItem());
    }

    public void LoadItem()
    {
        Debug.Log("Select item");

        AdminStocks.instance.LoadItem((int)data[(byte)Params.Id]);
    }

    
}
