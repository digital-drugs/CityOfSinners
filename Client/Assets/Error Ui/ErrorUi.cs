using ExitGames.Client.Photon;
using Share;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ErrorUi : MonoBehaviour
{
    public static ErrorUi instance;

    void Start()
    {
        instance=this;
    }

    [SerializeField] private GameObject wi_Error;
    [SerializeField] private TextMeshProUGUI errorText;
    public void ShowError(ParameterDictionary parameters)
    {
        var message = (string)parameters[(byte)Params.Error];

        ShowError(message);
    }

    public void ShowError(string message)
    {
        errorText.text = message;

        wi_Error.SetActive(true);
    }

    public void HideError()
    {
        wi_Error.SetActive(false);
    }
}
