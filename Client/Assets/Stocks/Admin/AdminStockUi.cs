using Share;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AdminStockUi : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI NameText;
    [SerializeField] private Button button;

    private Dictionary<byte, object> data;

    private int id = 0;

    public void Assign(Dictionary<byte, object> data)
    {
        this.data = data;

        //var imageUrl = (string)data[(byte)Params.Image];

        //GameRoomUi.instance.StartLoadTextureToImage(ico, imageUrl);

        NameText.text = (string)data[(byte)Params.Name];

        gameObject.name = $"{NameText.text}";

        button.onClick.AddListener(() => LoadStock());
    }

    public void Select()
    {
        //Debug.Log("Select");

        //AdminStocks.instance.AssignToForm(data, this.id);
    }

    public void LoadStock()
    {
        AdminStocks.instance.LoadStock((int)data[(byte)Params.Id]);
    }
}
