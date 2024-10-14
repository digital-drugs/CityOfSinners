using ExitGames.Client.Photon;
using Share;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomBuilderUi : MonoBehaviour
{
    [SerializeField] TMP_InputField playerCountIF;
    [SerializeField] TMP_InputField waitTimeIF;

    private void Start()
    {
        CreateAvailablesRoles();
    }

    private void CreateAvailablesRoles()
    {   
        //добавляем все доступные в игре роли в конструктор
        foreach (var element in Enum.GetValues(typeof( RoleType)))
        {
            AddAvailableRole((RoleType)element);
        }

        //добавляем опции для выбора 100% скиллов у доступных ролей
        foreach (var element in Enum.GetValues(typeof(RoleType)))
        {
            AddAvailableRole100((RoleType)element);
        }

    }

    [SerializeField] private AvailableRoleUi availableRoleUiPrefab;
    [SerializeField] private Transform availableRoleUisContainer;
    private void AddAvailableRole(RoleType roleType)
    {
        var newRole = Instantiate(availableRoleUiPrefab);
        UiHelper.AssignObjectToContainer(newRole.gameObject, availableRoleUisContainer);

        newRole.Assign(roleType, this);
    }

    [SerializeField] private AvailableRole100Ui availableRole100UiPrefab;
    [SerializeField] private Transform availableRole100UisContainer;
    private void AddAvailableRole100(RoleType roleType)
    {
        switch (roleType)
        {
            case RoleType.Alien:
            case RoleType.Astronaut:
            case RoleType.GoodClown:
            case RoleType.BadClown:
            case RoleType.Scientist:
            case RoleType.Jane:
            case RoleType.Garry:
            case RoleType.Loki:
            case RoleType.Common:
                return;
        }

        var newRole = Instantiate(availableRole100UiPrefab);
        UiHelper.AssignObjectToContainer(newRole.gameObject, availableRole100UisContainer);

        newRole.Assign(roleType);
    }

    [SerializeField] private GameObject wi_Roles100;
    public void ShowRoles100()
    {
        wi_Roles100.SetActive(true);
    }

    public void HideRoles100()
    {
        wi_Roles100.SetActive(false);
    }

    private List<InGameRoleUi> inGameRoles= new List<InGameRoleUi>();
    [SerializeField] private InGameRoleUi inGameRoleUiPrefab;
    [SerializeField] private Transform inGameRoleUisContainer;
    public void AddInGameRole(RoleType roleType)
    {
        var currentRolesCount = int.Parse(playerCountIF.text);

        //Debug.Log($"currentRolesCount {currentRolesCount} / {inGameRoles.Count}");

        if (inGameRoles.Count >= currentRolesCount ) return;

        var newIngameRole = Instantiate(inGameRoleUiPrefab);
        inGameRoles.Add(newIngameRole);

        UiHelper.AssignObjectToContainer(newIngameRole.gameObject, inGameRoleUisContainer);

        newIngameRole.Assign(roleType, this);

        UpdateInGameRoleCountText();
    }
    public void RemoveInGameRole(InGameRoleUi inGameRoleUi)
    {
        inGameRoles.Remove(inGameRoleUi);

        UiHelper.MoveUiObjectToTrash(inGameRoleUi.gameObject);

        UpdateInGameRoleCountText();
    }

    [SerializeField] private TextMeshProUGUI inGameRoleCountText;
    private void UpdateInGameRoleCountText()
    {
        inGameRoleCountText.text = $"ролей в игре: {inGameRoles.Count}";
    }

    [SerializeField] private GameObject wi_RoomBuilder;
    public void ShowRoomBuilder()
    {
        inGameRoles.Clear();
        UiHelper.ClearContainer(inGameRoleUisContainer);

        waitPlayers.Clear();
        UiHelper.ClearContainer(waitPlayerUisContainer);

        UpdateInGameRoleCountText();

        wi_RoomBuilder.SetActive(true);
    }

    public void HideRoomBuilder()
    {
        wi_RoomBuilder.SetActive(false);
    }

    [SerializeField] private TMP_InputField fNight_IF;
    [SerializeField] private TMP_InputField day_IF;
    [SerializeField] private TMP_InputField night_IF;
    [SerializeField] private TMP_InputField judge_IF;

    [SerializeField] private Toggle botExtra;
    [SerializeField] private Toggle botVote;
    [SerializeField] private Toggle botVisit;

    [SerializeField] private Toggle skill100;
    public void StartGameFromLobby()
    {
        var parameters = new Dictionary<byte, object>();

        //parameters.Add((byte)Params.RoomType, RoomType.Player8);
        var playerCount = int.Parse(playerCountIF.text);
        parameters.Add((byte)Params.PlayerCount, playerCount);

        //var waitTime = int.Parse(waitTimeIF.text);
        //parameters.Add((byte)Params.Timer, waitTime);

        //игроки и присвоенные им роли
        var players = new Dictionary<long, int>();
        var waitPlayers = waitPlayerUisContainer.GetComponentsInChildren<WaitPlayerUi>();
        foreach(var wp in waitPlayers)
        {
            players.Add(wp.id, wp.roleDrop.value);
        }
        parameters.Add((byte)Params.Players, players);

        //роли доступные ботам
        var botRoles = new Dictionary<int, RoleType>();
        for (int i = 0; i < inGameRoles.Count; i++)
        {
            botRoles.Add(i, inGameRoles[i].roleType);
        }
        parameters.Add((byte)Params.Roles, botRoles);

        //длительность фаз
        var fNightDuration = int.Parse(fNight_IF.text);
        parameters.Add((byte)Params.FNight, fNightDuration);

        var dayDuration = int.Parse(day_IF.text);
        parameters.Add((byte)Params.Day, dayDuration);

        var nightDuration = int.Parse(night_IF.text);
        parameters.Add((byte)Params.Night, nightDuration);

        var judgeDuration = int.Parse(judge_IF.text);
        parameters.Add((byte)Params.Judge, judgeDuration);

        //настройки для ботов
        parameters.Add((byte)Params.BotUseExtra, botExtra.isOn);
        parameters.Add((byte)Params.BotUseVote, botVote.isOn);
        parameters.Add((byte)Params.BotUseVisit, botVisit.isOn);

        //настройки скиллов
        var skills100Uis = availableRole100UisContainer.GetComponentsInChildren<AvailableRole100Ui>();
        var skills100 = new Dictionary<byte, object>();
        byte skill100Count = 0;
        foreach(var s in skills100Uis)
        {
            if (s.isOn)
            {
                //Debug.Log($"{s.roleType} is 100");

                skills100.Add(skill100Count++, s.roleType);               
            }
        }

        parameters.Add((byte)Params.Skill100, skills100);

        PhotonManager.Inst.peer.SendOperation((byte)Request.StartGameFromLobby, parameters, PhotonManager.Inst.sendOptions);
    }

    [SerializeField] private WaitPlayerUi waitPlayerUiPrefab;
    [SerializeField] private Transform waitPlayerUisContainer;
    private Dictionary<long, WaitPlayerUi> waitPlayers = new Dictionary<long, WaitPlayerUi>();
    public void AddWaitPlayer(ParameterDictionary parameters)
    {
        var userId = (long)parameters[(byte)Params.UserId];

        var newWaitPlayer = Instantiate(waitPlayerUiPrefab);
        UiHelper.AssignObjectToContainer(newWaitPlayer.gameObject, waitPlayerUisContainer);

        waitPlayers.Add(userId, newWaitPlayer);

        newWaitPlayer.Assign(parameters);
    }

    public void RemoveWaitPlayer(ParameterDictionary parameters)
    {
        var userId = (long)parameters[(byte)Params.UserId];

        var waitPlayerUi = waitPlayers[userId];

        waitPlayers.Remove(userId);

        UiHelper.MoveUiObjectToTrash(waitPlayerUi.gameObject);
    }
    
}

