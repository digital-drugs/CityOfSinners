using Share;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TableRowScript : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI MissionId;
    [SerializeField] private TextMeshProUGUI Name1;
    [SerializeField] private TextMeshProUGUI Amount1;
    [SerializeField] private TextMeshProUGUI Amount2;
    [SerializeField] private TextMeshProUGUI Name2;

    private Dictionary<byte, object> data;

    public void Assign(string missinId, string name1, string amount1, string amount2, string name2)
    {
        MissionId.text = missinId;
        Name1.text = name1;
        Amount1.text = amount1;
        Amount2.text = amount2;
        Name2.text = name2;
    }
}
