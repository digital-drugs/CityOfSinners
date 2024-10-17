using System;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
using Utils;

public class VKAuthorization : MonoBehaviour
{
    [SerializeField] private string appId;
    [SerializeField] private string redirectUrl;
    [SerializeField] private string vkRedirectUrl;

    [SerializeField] private TMP_InputField TMP_login;

    [DllImport("__Internal")]
    private static extern void VKAsyncInit(string appId);

    [DllImport("__Internal")]
    private static extern void VKLogin(string objectName, string methodName);

    [DllImport("__Internal")]
    private static extern void VKLogout();

    private void Awake()
    {
        Application.runInBackground = true;
    }

    private void Start()
    {
        return;
#if UNITY_WEBGL && !UNITY_EDITOR
        //��������� � ���������� �� open API ����� 
        VKAsyncInit(appId);
#endif
    }

    /// <summary>
    /// �������� ������� �� ����������� � ��
    /// </summary>
    public void Button_VKLogin()
    {
        PhotonManager.Inst.RequestLogin(TMP_login.text);
        return;
#if UNITY_EDITOR
        PhotonManager.Inst.RequestLogin(TMP_login.text);
#else
        //TODO: get userInfo
        //var userInfo = JsUtils.GetUserInfo();
        //TODO: get id from userInfo
        //PhotonManager.Inst.RequestLogin(id);
#endif
    }

    public event EventHandler OnVKLoginSucces;
    /// <summary>
    /// ����� �� �������� ����������� � ��
    /// </summary>
    /// <param name="result"></param>    
    public void VKAuthResult(string userId)
    {
        PhotonManager.Inst.RequestLogin(userId);
    }

    public void VkLogOut()
    {
        VKLogout();
    }
}