using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ServerLogger : MonoBehaviour
{
    public static ServerLogger ins;

    private void Awake()
    {
        ins = this;
    }

    [SerializeField] private TextMeshProUGUI Text_Log;

    public void AddLog(string log)
    {
        Text_Log.text += $"\n{log}";
    }

    [SerializeField] private GameObject window;
    public void ShowLog()
    {
        window.SetActive(!window.activeSelf);
    }
}
