using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SmileUi : MonoBehaviour
{
    [SerializeField] private string smileId;
    [SerializeField] private Button smileButton;

    private void Start()
    {
        smileButton.onClick.AddListener(() => ClickSmile());
    }

    public void Assign()
    {

    }

    public void ClickSmile()
    {
        GeneralChat.instance.AddSmileIdToMessage(smileId);
    }
}
