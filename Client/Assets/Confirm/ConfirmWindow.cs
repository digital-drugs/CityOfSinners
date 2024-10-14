using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmWindow : MonoBehaviour
{
    public static ConfirmWindow instance;

    private void Start()
    {
        instance = this;

        buttonNo.onClick.AddListener(() => ButtonNo());
    }

    [SerializeField] private GameObject window;
    [SerializeField] private TextMeshProUGUI captionText;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Button buttonYes;
    [SerializeField] private Button buttonNo;

    public void ShowConfirm(string caption, string message, Action action)
    {
        captionText.text = caption;
        messageText.text = message;

        buttonYes.onClick.RemoveAllListeners();

        buttonYes.onClick.AddListener(() => { action(); HideConfirm(); });

        window.SetActive(true);
    }

    public void HideConfirm()
    {
        window.SetActive(false);
    }

    public void ButtonNo()
    {
        HideConfirm();
    }   
}
