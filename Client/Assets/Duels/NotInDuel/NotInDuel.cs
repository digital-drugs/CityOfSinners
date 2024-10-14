using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NotInDuel : MonoBehaviour
{
    [SerializeField] private TMP_Text message;

    public void ShowTab(string message)
    {
        this.gameObject.SetActive(true);
        this.message.text = message;
    }
}
