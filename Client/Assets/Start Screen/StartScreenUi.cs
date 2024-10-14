using ExitGames.Client.Photon;
using Share;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StartScreenUi : MonoBehaviour
{
    public static StartScreenUi instance;

    private void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        GameManager.instance.OnPlayerStatusChange += OnPlayerStatusChange;
    }

    [SerializeField] private Image queueButtonImage;
    [SerializeField] private TextMeshProUGUI queueButtonText;
    private void OnPlayerStatusChange(object sender, EventArgs e)
    {
        switch (GameManager.instance.playerStatus)
        {
            case PlayerQueueStatus.Idle:
                {
                    queueButtonImage.color = Color.white;
                    queueButtonText.text = $"найти игру";
                }
                break;
            case PlayerQueueStatus.InQueue:
                {
                    queueButtonImage.color = Color.green;
                    queueButtonText.text = $"отмена поиска";
                }
                break;
            case PlayerQueueStatus.InLobby:
                {
                    queueButtonImage.color = Color.green;
                    queueButtonText.text = $"выйти из лобби";
                }
                break;
            case PlayerQueueStatus.InGame:
                {
                    queueButtonImage.color = Color.green;
                    queueButtonText.text = $"продолжить игру";
                }
                break;
        }
    }

    public void LoadStartScreen(ParameterDictionary parameters)
    {
        var userId = (long)parameters[(byte)Params.UserId];
        var userName = (string)parameters[(byte)Params.UserName];

        var userData = (Dictionary<byte, object>)parameters[(byte)Params.UserData];

        if (userData.Count > 0)
        {
            var coins = (int)userData[(byte)Params.Coins];
            var diamonds = (int)userData[(byte)Params.Diamonds];

            userNameText.text = $"профиль [{userName}] [{userId}]";
            userNameText.text += $"\nCoins:{coins} Diamonds:{diamonds}";
        }
        else
        {
            userNameText.text = $"профиль [{userName}] [{userId}]";
            userNameText.text += $"\nno data";
        }    

        SkillScreenUi.instance.SetupSkills(parameters);
        ExtraScreenUi.instance.SetupExtras(parameters);
        AchieveScreenUi.instance.SetupAchieves(parameters);
        RoleScreenUi.ins.SetupRoles(parameters);

        Admin.instance.CheckSystemRole(parameters);

        ShowWiStartScreen();
    }

    [SerializeField] private GameObject _wi_StartScreen;
    [SerializeField] private TextMeshProUGUI userNameText;
    public  void ShowWiStartScreen()
    {
        _wi_StartScreen.SetActive(true);
    }
    public  void HideWiStartScreen()
    {
        _wi_StartScreen.SetActive(false);
    }

    internal void SetUserName(string userName)
    {
        userNameText.text = $"профиль [{userName}]";
    }

    public void RequestStartGame()
    {
        var parameters = new Dictionary<byte, object>();

        PhotonManager.Inst.peer.SendOperation((byte)Request.StartGame, parameters, PhotonManager.Inst.sendOptions);
    }

    public RoomBuilderUi roomBuilderUi;
    public void RequestCreateGame()
    {
        var parameters = new Dictionary<byte, object>();

        PhotonManager.Inst.peer.SendOperation((byte)Request.CreateGame, parameters, PhotonManager.Inst.sendOptions);
    }

   

    public void ShowRoomBuilder()
    {
        roomBuilderUi.ShowRoomBuilder();
    }

    public void HideLobby()
    {
        roomBuilderUi.HideRoomBuilder();
    }

    public void SwitchFullScreen()
    {
        Screen.fullScreen =!Screen.fullScreen;
    }

    public void Button_RequestRoomList()
    {
        var parameters = new Dictionary<byte, object>();
        PhotonManager.Inst.peer.SendOperation((byte)Request.GetLobbyList, parameters, PhotonManager.Inst.sendOptions);
    }
}
