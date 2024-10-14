using ExitGames.Client.Photon;
using Share;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AuthorizationUI : MonoBehaviour
{
    [SerializeField] private GameObject _wi_RequestName;
    private static GameObject wi_RequestName;

    [SerializeField] private TextMeshProUGUI _nameErrorText;
    private static TextMeshProUGUI nameErrorText; 

    private void Start()
    {
        wi_RequestName = _wi_RequestName;
        nameErrorText = _nameErrorText;
        wi_Authorization = _wi_Authorization;
    }

    public static void RequestName()
    {
        wi_RequestName.SetActive(true);
    }

    [SerializeField] private int minCharacter = 3;
    [SerializeField] private int maxCharacter = 20;

    
    [SerializeField] private TMP_InputField nameInputField;
    public void SetName()
    {
        var userName = nameInputField.text;

        var beforeCensuredUserName = CensureFilter.Beatify(userName, true, true, true);

        //проверяем имя Проверяя на исключения:
        var afterCensuredUserName = CensureFilter.Process(beforeCensuredUserName, true, false);

        bool nameIsGood = true;

        if (afterCensuredUserName != beforeCensuredUserName)
        {
            //!ok
            Debug.Log("client name is bad");

            _nameErrorText.text = $"bad name";

            nameIsGood = false;

            return;
        }

        if (userName.Length < minCharacter)
        {
            _nameErrorText.text = $"short name";

            nameIsGood = false;

            return;
        }

        if(userName.Length> maxCharacter)
        {
            _nameErrorText.text = $"long name";

            nameIsGood = false;

            return;
        }

        if (nameIsGood) _nameErrorText.text = $"";

        RequestSetName(userName);
    }

    public  void RequestSetName(string userName)
    {
        var parameters = new Dictionary<byte, object>();

        parameters.Add((byte)Params.UserName, userName);

        PhotonManager.Inst.peer.SendOperation((byte)Request.ChangeUserName, parameters, PhotonManager.Inst.sendOptions);
    }

    public static void HideRequestName()
    {
        wi_RequestName.SetActive(false);
    }

    public static void ShowNameError(string v)
    {
        nameErrorText.text = $"server bad name";
    }

    [SerializeField] private GameObject _wi_Authorization;
    private static GameObject wi_Authorization;
    public static void HideAuthorization()
    {
        wi_Authorization.SetActive(false);
    }

    public static void ShowAuthorization()
    {
        wi_Authorization.SetActive(true);
    }
}
