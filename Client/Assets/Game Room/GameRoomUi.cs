using ExitGames.Client.Photon;
using ntw.CurvedTextMeshPro;
using Share;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using UnityEngine.UIElements.Experimental;

public class GameRoomUi : MonoBehaviour
{
    public static GameRoomUi instance;

    public RoomChatUi roomChatUi;
    public SystemRoomChatUi systemRoomChatUi;

    public Sprite defaultExtraSprite;

    public Sprite knifeSprite;
    public Sprite gunSprite;
    public Sprite visitSprite;

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
    }

    [SerializeField] private GameObject wi_GameRoom;

    [SerializeField] private DayNightTimer timer;
    public void StartGame(ParameterDictionary parameters)
    {
        wi_EndGameResult.SetActive(false);

        //images
        Image_Street.sprite = Sprite_Street;
        Go_DeadTime.SetActive(false);

        roomChatUi.Clear();

        systemRoomChatUi.Clear();

        AddRoomRoles(parameters);

        AddRoomPlayers(parameters);      

        //ChangeGamePhase(parameters);       

        StartCoroutine(UpdateCanvas());

        timer.ResetTimer();

        EndJudging();       

        deadRoles.Clear();
        UiHelper.ClearContainer(goodDeadRolesContainer);
        UiHelper.ClearContainer(badDeadRolesContainer);

        //////////////////////////
        wi_GameRoom.SetActive(true);
    } 

    [SerializeField] private RoomPlayerUi roomPlayerPrefab;
    [SerializeField] private Transform roomPlayersContainer;
    private Dictionary<long, RoomPlayerUi> playerUis = new Dictionary<long, RoomPlayerUi>();
    public List<Transform> playerSlots = new List<Transform>(); 
    private void AddRoomPlayers(ParameterDictionary parameters)
    {
        //очищаем комнату
        foreach(var pui in playerUis.Values)
        {
            UiHelper.MoveUiObjectToTrash(pui.gameObject);
        }

        playerUis.Clear();

        UiHelper.ClearContainer(roomPlayersContainer);

        //создаем плашки игроков в центре игровой доски
        var playersData = (Dictionary<long, object>)parameters[(byte)Params.Players];

        var playerSlotCount = 0;
        foreach (var p in playersData)
        {
            var player = (Dictionary<byte, object>)p.Value;

            var newRoomPlayer = Instantiate(roomPlayerPrefab, roomPlayersContainer);

            newRoomPlayer.Assign(p.Key, player);

            playerUis.Add(p.Key, newRoomPlayer);

            var playerSlot = playerSlots[playerSlotCount++];

            UiHelper.AssignObjectToContainer(newRoomPlayer.gameObject, playerSlot);
        }

        roomChatUi.CreatePrivates(playersData);      
    }      

    public RoomPlayerUi FindPlayerUi(long id)
    {
        if (playerUis.ContainsKey(id))
        {
            return playerUis[id];
        }

        return null;
    }


    private bool canSelect = true;
    public void SelectPlayer(long playerId)
    {
        if (!canSelect) { return; }

        canSelect = false;

        foreach (var p in playerUis.Values)
        {
            if (p.playerId != playerId)
            {
                p.HideVisit();
                p.HideVote();
            }
        }

        var parameters = new Dictionary<byte, object>();

        parameters.Add((byte)Params.UserId, playerId);

        PhotonManager.Inst.peer.SendOperation((byte)Request.SelectPlayer, parameters, PhotonManager.Inst.sendOptions);
    }

    [SerializeField] private RoomRoleUi roomRolePrefab;
    [SerializeField] private Transform goodRolesContainer;
    [SerializeField] private Transform badRolesContainer;
    [SerializeField] private TextMeshProUGUI teamCountText;
    private Dictionary<RoleType, RoomRoleUi> roleUis = new Dictionary<RoleType, RoomRoleUi>();
    private void AddRoomRoles(ParameterDictionary parameters)
    {
        roleUis.Clear();

        UiHelper.ClearContainer(goodRolesContainer);
        UiHelper.ClearContainer(badRolesContainer);

        var rolesData = (Dictionary<byte, object>)parameters[(byte)Params.Roles];

        foreach (var r in rolesData.Values)
        {
            var roleData = (Dictionary<byte,object>)r;

            AddRoleUi(roleData);
        }

        teamCountText.text = $"{goodCount} : {badCount}";
    }

    private RoomRoleUi AddRoleUi(Dictionary<byte, object> roleData)
    {
        //Debug.Log($"roleData {roleData.Count}");

        RoomRoleUi result = null;

        var roleTypeString = (string)roleData[(byte)Params.RoleId];
        //Debug.Log($"roleTypeString {roleTypeString}");

        var roleType = (RoleType)Helper.GetEnumElement<RoleType>(roleTypeString);

        if (roleUis.ContainsKey(roleType))
        {
            result = roleUis[roleType];
            result.IncreaseCount();

            UpdateRoleCount(roleType);
        }
        else
        {
            Transform container = UpdateRoleCount(roleType);

            result = Instantiate(roomRolePrefab, container);
            result.transform.localScale = Vector3.one;
            result.Assign(roleData);

            roleUis.Add(roleType, result);
        }

        return result;
    }

    public RoomRoleUi FindRoleUi(RoleType roleType)
    {
        if (roleUis.ContainsKey(roleType))
        {
            return roleUis[roleType];
        }

        return null;
    }

    int goodCount = 0;
    int badCount = 0;

    private Transform UpdateRoleCount(RoleType roleType)
    {
        Transform container = null;

        switch (roleType)
        {
            case RoleType.Astronaut:
            case RoleType.GoodClown:
            case RoleType.Citizen:
            case RoleType.Doctor:
            case RoleType.Guerilla:
            case RoleType.Saint:
            case RoleType.Witness:
            case RoleType.Commissar: { goodCount++; container = goodRolesContainer; } break;

            case RoleType.Alien:
            case RoleType.BadClown:
            case RoleType.Scientist:

            case RoleType.Jane:
            case RoleType.Loki:
            case RoleType.Garry:

            case RoleType.Maniac:
            case RoleType.Werewolf:
            case RoleType.Mafia:
            case RoleType.MafiaBoss:
            case RoleType.Sinner: { badCount++; container = badRolesContainer; } break;
        }

        return container;
    }


    IEnumerator UpdateCanvas()
    {
        yield return new WaitForEndOfFrame();

        Canvas.ForceUpdateCanvases();
    }

    private GamePhase gamePhase;
    private string phaseString;
    public void ChangeGamePhase(ParameterDictionary parameters)
    {
        gamePhase = (GamePhase)parameters[(byte)Params.GamePhase];
        //Debug.Log($"set game phase {gamePhase}");

        switch (gamePhase) 
        {
            case GamePhase.FirstNight: 
                {
                    StartCoroutine(NightStreet());
                    //timer.SetNight(parameters);
                    phaseString = "Первая ночь"; 
                } break;

            case GamePhase.Day: 
                {
                    StartCoroutine(DayStreet());
                    timer.SetDay(parameters);
                    var dayCount = (int)parameters[(byte)Params.DayCount];
                    parameters.Add((byte)Params.ChatMessage,$"Наступил день #{dayCount}");
                    systemRoomChatUi.SetDayPhase(parameters);
                    phaseString = "День";
                    //StartDay(parameters); 
                } break;

            case GamePhase.Judging: { phaseString = "Суд"; } break;

            case GamePhase.Night: 
                {
                    StartCoroutine(NightStreet());
                    timer.SetNight(parameters);
                    var dayCount = (int)parameters[(byte)Params.DayCount];
                    parameters.Add((byte)Params.ChatMessage, $"Наступила ночь #{dayCount}");
                    systemRoomChatUi.SetNightPhase(parameters);
                    phaseString = "Ночь";
                    //StartNight(parameters); 
                } break;
        }

        SetGameTimer(parameters);
    }

    [SerializeField] private Image Image_Street;
    [SerializeField] private Sprite Sprite_Street;
    [SerializeField] private float streetColorSpeed = 0.1f;
    [SerializeField] private float streetColorTick = 0.05f;
    [SerializeField] private Color32 nightColor;
    [SerializeField] private Color32 dayColor;
    private IEnumerator NightStreet()
    {
        for (int i = 0; i < 100; i++)
        {
            yield return new WaitForSeconds(streetColorTick);

            Image_Street.color = Color32.Lerp(Image_Street.color, nightColor, streetColorSpeed);
        }
    }

    private IEnumerator DayStreet()
    {
        for (int i = 0; i < 100; i++)
        {
            yield return new WaitForSeconds(streetColorTick);

            Image_Street.color = Color32.Lerp(Image_Street.color, dayColor, streetColorSpeed);
        }
    }

    public void EnableVote(ParameterDictionary parameters)
    {
        canSelect = true;

        //отобразить доступные действия против других игроков
        foreach (var p in playerUis.Values)
        {
            p.HideVisit();
        }

        var voteIds = (Dictionary<byte, long>)parameters[(byte)Params.Votes];

        foreach (var v in voteIds.Values)
        {
            var playerUi = playerUis[v];
            playerUi.ShowVote();
        }

        //foreach (var p in playerUis.Values)
        //{
        //    if (!p.inJail && !p.inMorgue) p.ShowVote();
        //}
    }

    public void DisableVote()
    {
        //отобразить доступные действия против других игроков
        foreach (var p in playerUis.Values)
        {
            p.HideVote();
        }
    }

    public void EnableVisit(ParameterDictionary parameters)
    {
        canSelect = true;

        //отобразить доступные действия против других игроков
        foreach (var p in playerUis.Values)
        {
            p.HideVote();
        }

        var visitIds = (Dictionary<byte, long>)parameters[(byte)Params.Visits];

        foreach (var v in visitIds.Values)
        {
            var playerUi = playerUis[v];
            playerUi.ShowVisit();
        }

        //foreach (var p in playerUis.Values)
        //{
        //    if (!p.inJail && !p.inMorgue) p.ShowVisit();
        //}
    }

    public void DisableVisit()
    {
        //отобразить доступные действия против других игроков
        foreach (var p in playerUis.Values)
        {
            p.HideVisit();
        }

        //foreach (var p in playerUis.Values)
        //{
        //    if (!p.inJail && !p.inMorgue) p.ShowVote();
        //}
    }

    [SerializeField] private TextMeshProUGUI timerText;
    public void SetGameTimer(ParameterDictionary parameters)
    {
        var time = (int)parameters[(byte)Params.Timer];

        timerText.text = $"{phaseString}: {time}";
    }

    public void TeamCompound(ParameterDictionary parameters)
    {
        var playersData = (Dictionary<long, object>)parameters[(byte)Params.Roles];

        foreach(var d in playersData)
        {
            var playerUi = playerUis[d.Key];

            var roleData = (Dictionary<byte,object>)d.Value;

            var roleId = (RoleType)roleData[(byte)Params.RoleId];

            playerUi.SetRole(roleId);
        }
    }

    public void ChangeGoodTeamCompound(ParameterDictionary parameters)
    {
        //var roleType = (RoleType)parameters[(byte)Params.RoleId];

        //var roleUi = AddRole(roleType);

        //if(roleType == RoleType.Werewolf)
        //{
        //    roleUi.transform.SetParent(goodRolesContainer);
        //    roleUi.transform.localScale = Vector3.one;

        //    goodCount++;
        //}

        //StartCoroutine(UpdateCanvas());

        //teamCountText.text = $"{goodCount} : {badCount}";
    }

    public void ChangeBadTeamCompound(ParameterDictionary parameters)
    {
        //var roleType = (RoleType)parameters[(byte)Params.RoleId];

        //var roleUi = roles[roleType];

        //if (roleType == RoleType.Werewolf)
        //{
        //    roleUi.transform.SetParent(badRolesContainer);
        //    roleUi.transform.localScale= Vector3.one;

        //    badCount++;
        //}

        //StartCoroutine(UpdateCanvas());

        //teamCountText.text = $"{goodCount} : {badCount}";
    }

    public void ChangeTeam_Werewolf(ParameterDictionary parameters)
    {
        if(roleUis.ContainsKey(RoleType.Werewolf) == false)
        {
            Debug.Log("werewolf id no found in UI");
            return;
        }
        var roleUi = roleUis[RoleType.Werewolf];

        var teamType = (TeamType)parameters[(byte)Params.TeamType];

        switch (teamType)
        {
            case TeamType.Bad:
            case TeamType.Neutral:
                {
                    roleUi.transform.SetParent(badRolesContainer); 
                }
                break;

            case TeamType.Good:
                {
                    roleUi.transform.SetParent(goodRolesContainer);
                }
                break;
        }

        Debug.Log($"werewolf change team to {teamType}");
    }

    public void PlayerToJail(ParameterDictionary parameters)
    {
        var playerId = (long)parameters[(byte)Params.UserId];
        var playerUi = playerUis[playerId];

        playerUi.SendToJail();

        var roleId  = (RoleType)parameters[(byte)Params.RoleId];
        var roleUi = roleUis[roleId];

        roleUi.DecreaseCount();

        AddDeadRole(roleUi);

        TryKillSelf(playerId);       
    }

    public void PlayerToMorgue(ParameterDictionary parameters)
    {
        var playerId = (long)parameters[(byte)Params.UserId];
        var playerUi = playerUis[playerId];

        playerUi.SendToMorgue();

        var roleId = (RoleType)parameters[(byte)Params.RoleId];
        var roleUi = roleUis[roleId];

        roleUi.DecreaseCount();

        AddDeadRole(roleUi);

        TryKillSelf(playerId);
    }

    [SerializeField] private Sprite Sprite_DeadPlayer;
    [SerializeField] private GameObject Go_DeadTime;
   
    private void TryKillSelf(long id)
    {
        if (id == GameManager.instance.userId)
        {
            Image_Street.sprite = Sprite_DeadPlayer;
            Go_DeadTime.SetActive(true);

            EndJudging();

            DisableVote();
            DisableVisit();
        }
    }

    private void ResurectSelf()
    {

    }

    private Dictionary<RoleType, RoomRoleUi> deadRoles = new Dictionary<RoleType, RoomRoleUi>();
    private void AddDeadRole(RoomRoleUi roomRoleUi)
    {       
        RoomRoleUi result = null;       

        if (deadRoles.ContainsKey(roomRoleUi.roleType))
        {
            result = deadRoles[roomRoleUi.roleType];
            result.IncreaseCount();
        }
        else
        {
            var roleContainer = GetDeadRoleContainer(roomRoleUi.roleType);

            result = Instantiate(roomRolePrefab, roleContainer);
            result.Assign(roomRoleUi.roleData);

            result.KillRole();

            deadRoles.Add(roomRoleUi.roleType, result);
        }
    }

    [SerializeField] private Transform goodDeadRolesContainer;
    [SerializeField] private Transform badDeadRolesContainer;
    private Transform GetDeadRoleContainer(RoleType roleType)
    {
        Transform container = null;

        switch (roleType)
        {
            case RoleType.Astronaut:
            case RoleType.GoodClown:
            case RoleType.Citizen:
            case RoleType.Doctor:
            case RoleType.Guerilla:
            case RoleType.Saint:
            case RoleType.Witness:
            case RoleType.Commissar: { container = goodDeadRolesContainer; } break;

            case RoleType.Alien:
            case RoleType.BadClown:
            case RoleType.Scientist:

            case RoleType.Jane:
            case RoleType.Loki:
            case RoleType.Garry:

            case RoleType.Maniac:
            case RoleType.Werewolf:
            case RoleType.Mafia:
            case RoleType.MafiaBoss:
            case RoleType.Sinner: { container = badDeadRolesContainer; } break;
        }

        return container;
    }

   

    public void ResurectPlayer(ParameterDictionary parameters)
    {
        var playerId = (long)parameters[(byte)Params.UserId];

        var playerUi = playerUis[playerId];

        playerUi.ResurectPlayer();
    }    

    [SerializeField] private ExtraSlotInGameRoomUi extraSlotUiPrefab;
    [SerializeField] private ExtraInGameRoom extraUiPrefab;
    [SerializeField] private Transform extraSlotUisContainer;
    private Dictionary<int, ExtraSlotInGameRoomUi> gameRoomSlots = new Dictionary<int, ExtraSlotInGameRoomUi>();
    private Dictionary<int, ExtraInGameRoom> gameRoomExtras = new Dictionary<int, ExtraInGameRoom>();
    [SerializeField] private Canvas mainCanvas;
    public void CreateExtraSlots(ParameterDictionary parameters)
    {
        gameRoomSlots.Clear();
        gameRoomExtras.Clear();

        UiHelper.ClearContainer(extraSlotUisContainer);

        //создаем слоты
        var userSlotCount = (int)parameters[(byte)Params.SlotCount];

        var slotContainerRect = extraSlotUisContainer.GetComponent<RectTransform>().rect;

        var slotSizeX = slotContainerRect.size.x / userSlotCount;       

        //Debug.Log($"create slots => {slotContainerRect.size.x} {userSlotCount} slot => {slotSizeX}");
        //Debug.Log($"canvas scaler => {mainCanvas.scaleFactor} slot => {slotSizeX* mainCanvas.scaleFactor}");

        for (int i = 0; i < userSlotCount; i++)
        {
            var newExtraSlot = Instantiate(extraSlotUiPrefab);
            gameRoomSlots.Add(i, newExtraSlot);

            var localPosition = new Vector3(i * slotSizeX, 0, 0);

            newExtraSlot.transform.SetParent(extraSlotUisContainer);
            newExtraSlot.transform.localScale = Vector3.one;

            newExtraSlot.GetComponent<RectTransform>().anchoredPosition = localPosition;

            //UiHelper.AssignObjectToContainer(newExtraSlot.gameObject, extraSlotUisContainer, localPosition);            
        }

        //создем экстры в слотах
        var slotsData = (Dictionary<int, object>)parameters[(byte)Params.ExtraSlots];

        foreach (var s in slotsData)
        {
            var slotId = s.Key;

            var extraData = (Dictionary<byte, object>)s.Value;

            //var extraId = (string)extraData[(byte)Params.ExtraId];

            var newExtraUi = Instantiate(extraUiPrefab);

            var slot = gameRoomSlots[slotId];

            newExtraUi.Assign(slot.transform, slotId, slotSizeX, extraData);

            gameRoomExtras.Add(slotId, newExtraUi);
        }
    }

    public void UpdateExtraGameCount(ParameterDictionary parameters)
    {
        var slotId = (int)parameters[(byte)Params.SlotId];

        var slotUi = gameRoomExtras[slotId];

        slotUi.SetExtraCount(parameters);
    }

    [SerializeField] private GameObject wi_EndGameResult;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TextMeshProUGUI coinsText;
    [SerializeField] private TextMeshProUGUI ratingText;
    [SerializeField] private TextMeshProUGUI experienceText;  

    public void ShowEndGameResult(ParameterDictionary parameters)
    {
        var message = (string)parameters[(byte)Params.EndGameResult];

        statusText.text = message;

        Debug.Log($"{message}");

        //var endGameResult = (EndGameResult)parameters[(byte)Params.EndGameResult];

        //switch (endGameResult)
        //{
        //    case EndGameResult.LiveWinner: statusText.text = $"Ваша команда победила и Вы остались живы!"; break;
        //    case EndGameResult.DeadWinner: statusText.text = $"Ваша команда победила, но Вы погибли!"; break;
        //    case EndGameResult.Looser: statusText.text = $"Вы проиграли"; break;
        //}

        //var coins = (EndGameResult)parameters[(byte)Params.Coins];
        //coinsText.text = $"Монеты: {coins}";

        //var rating = (EndGameResult)parameters[(byte)Params.Rating];
        //ratingText.text = $"Рейтинг: {rating}";

        //experienceText.text = $"Опыт:";

        wi_EndGameResult.SetActive(true);
    }


    public void ChangePlayerRole(ParameterDictionary parameters)
    {
        var oldRole = (RoleType)parameters[(byte)Params.OldRole];

        var newRole = (Dictionary<byte, object>)parameters[(byte)Params.NewRole];

        var oldRoleUi = FindRoleUi(oldRole);
        oldRoleUi.DecreaseCount();

        AddRoleUi(newRole);
    }

    internal void ChangePersonalPlayerRole(ParameterDictionary parameters)
    {
        var playerId = (long)parameters[(byte)Params.UserId];

        var playerUi = FindPlayerUi(playerId);

        var newRoleData = (Dictionary<byte,object>)parameters[(byte)Params.NewRole];

        var roleTypeString = (string)newRoleData[(byte)Params.RoleId];

        var roleType = (RoleType)Helper.GetEnumElement<RoleType>(roleTypeString);

        playerUi.SetRole(roleType);
    }

    //[]
    //public void StartDay(ParameterDictionary parameters)
    //{

    //}

    //public void StartNight(ParameterDictionary parameters)
    //{

    //}
    public void StartLoadTexture(Image roleImage, string manUrl)
    {
        StartCoroutine(LoadTexture(roleImage, manUrl));
    }

    private IEnumerator LoadTexture(Image roleImage, string url)
    {
        UnityEngine.Texture2D texture;

        // using to automatically call Dispose, create a request along the path to the file
        using (UnityWebRequest imageWeb = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET))
        {

            // We create a "downloader" for textures and pass it to the request
            imageWeb.downloadHandler = new DownloadHandlerTexture();
            // We send a request, execution will continue after the entire file have been downloaded
            yield return imageWeb.SendWebRequest();

            //Debug.Log($"image url {url}");

            // Getting the texture from the "downloader"
            texture = DownloadHandlerTexture.GetContent(imageWeb);

            var sprite = Sprite.Create(texture,
            new Rect(0.0f, 0.0f, texture.width, texture.height),
            new Vector2(0f, 1f));

            roleImage.sprite = sprite;
        }
    }

    public void StartLoadTextureToImage(Image image, string url)
    {
        StartCoroutine(LoadTextureToImage(image, url));
    }
    public IEnumerator LoadTextureToImage(Image image, string url)
    {
        //extraUi.extraIco.sprite = GameRoomUi.instance.defaultExtraSprite;
        //yield break;

        if (string.IsNullOrEmpty(url))
        {
            image.sprite = defaultExtraSprite;
            //Debug.Log($"set empty sprite for {extraUi.extraId}");
        }
        else
        {
            Texture2D texture;

            // using to automatically call Dispose, create a request along the path to the file
            UnityWebRequest imageWeb = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET);

            // We create a "downloader" for textures and pass it to the request
            imageWeb.downloadHandler = new DownloadHandlerTexture();
            // We send a request, execution will continue after the entire file have been downloaded
            yield return imageWeb.SendWebRequest();

            //Debug.Log($"image url {url}");

            // Getting the texture from the "downloader"
            texture = DownloadHandlerTexture.GetContent(imageWeb);

            var sprite = Sprite.Create(texture,
            new Rect(0.0f, 0.0f, texture.width, texture.height),
            new Vector2(0f, 1f));

            image.sprite = sprite;
        }
    }

    public RoleType myRole;
    public void SetPlayerRole(ParameterDictionary parameters)
    {
        myRole = (RoleType)parameters[(byte)Params.RoleId];
        var roleUi = roleUis[myRole];

        var playerUi = playerUis[GameManager.instance.userId];

        playerUi.SetRole(roleUi.roleUrl);
    }

    [SerializeField] private List<RoomPlayerActionUi> roomPlayerActionUis = new List<RoomPlayerActionUi>();
    public RoomPlayerActionUi GetRoleAction() 
    {
        return roomPlayerActionUis.Find(x => x.role == myRole);
    }

 
    public void UnlockRole_PlayerToPlayer(ParameterDictionary parameters)
    {      
        var roleId = (RoleType)parameters[(byte)Params.RoleId];
        var roleUi = roleUis[roleId];

        var playerId = (long)parameters[(byte)Params.UserId];
        var playerUi = playerUis[playerId];

        playerUi.SetRole(roleUi.roleUrl);




    }

    public void UnlockRole_GroupToPlayer(ParameterDictionary parameters)
    {
        var roles = (Dictionary<long,int>)parameters[(byte)Params.Roles];

        //Debug.Log($"roles => {roles.GetType()}");

        //var roles = (Dictionary<long, RoleType>)parameters[(byte)Params.Roles];

        foreach (var r in roles)
        {
            var playerId = r.Key;
            var playerUi = playerUis[playerId];

            var roleId = (RoleType)r.Value;
            var roleUi = roleUis[roleId];

            playerUi.SetRole(roleUi.roleUrl);
        }
    }

    public void UnlockRole_PlayerToRoom(ParameterDictionary parameters)
    {
        var roleId = (RoleType)parameters[(byte)Params.RoleId];
        var roleUi = roleUis[roleId];

        var playerId = (long)parameters[(byte)Params.UserId];
        var playerUi = playerUis[playerId];

        playerUi.SetRole(roleUi.roleUrl);
    }

    public void UnlockRole_PlayerToGroup(ParameterDictionary parameters)
    {
        var roleId = (RoleType)parameters[(byte)Params.RoleId];
        var roleUi = roleUis[roleId];

        var playerId = (long)parameters[(byte)Params.UserId];
        var playerUi = playerUis[playerId];

        playerUi.SetRole(roleUi.roleUrl);
    }

    public void AddExtraEffectToPlayer(ParameterDictionary parameters)
    {
        var playerId = (long)parameters[(byte)Params.UserId];

        var playerUi = playerUis[playerId];

        playerUi.AddExtraEffect(parameters);
    }

    public void RemoveExtraEffectFromPlayer(ParameterDictionary parameters)
    {
        var playerId = (long)parameters[(byte)Params.UserId];

        var playerUi = playerUis[playerId];

        playerUi.RemoveExtraEffect(parameters);
    }

    public void ButtonExitRoom()
    {
        var parameters = new Dictionary<byte, object>();

        PhotonManager.Inst.peer.SendOperation((byte)Request.RequestExitGameRoom, parameters, PhotonManager.Inst.sendOptions);
        Debug.Log($"requested exit");       
    }

    public void RequestExitGameRoom(ParameterDictionary parameters)
    {
        var caption = (string)parameters[(byte)Params.caption];
        var message = (string)parameters[(byte)Params.message];

        Action action = () => 
        { 
            PhotonManager.Inst.peer.SendOperation(
                (byte)Request.ConfirmExitGameRoom, 
                parameters, 
                PhotonManager.Inst.sendOptions);

            ExitGameRoom();
        };

        ConfirmWindow.instance.ShowConfirm(caption, message, action);
    }

    public void ExitGameRoom()
    {
        StartScreenUi.instance.ShowWiStartScreen();

        wi_EndGameResult.SetActive(false);
        wi_GameRoom.SetActive(false);
    }

    [SerializeField] private Image jailPlayerImage;
    [SerializeField] private TextMeshProUGUI jailPlayerNameText;
    [SerializeField] private GameObject judgingWindow;
    [SerializeField] private TextProOnACircle nameCircleText;
    [SerializeField] private float degressPerChar=8;
    public void StartJudging(ParameterDictionary parameters)
    {
        ResetJudging();

        var playerId = (long)parameters[(byte)Params.UserId];

        var playerUi = playerUis[playerId];

        var playerName = playerUi.playerName;

        var summaryDegress = playerName.Length * degressPerChar;
        if (summaryDegress > 180) summaryDegress = 180;

        nameCircleText.m_arcDegrees = summaryDegress;

        jailPlayerNameText.text = playerName;

        judgingWindow.SetActive(true);
    }

    public void SeeJudging(ParameterDictionary parameters)
    {
        GO_SentenceButton.SetActive(false);
        GO_SentenceText.SetActive(false);

        GO_JustifyButton.SetActive(false);       
        GO_JustifyText.SetActive(false);

        var playerId = (long)parameters[(byte)Params.UserId];

        var playerUi = playerUis[playerId];

        var playerName = playerUi.playerName;

        var summaryDegress = playerName.Length * degressPerChar;
        if (summaryDegress > 180) summaryDegress = 180;

        nameCircleText.m_arcDegrees = summaryDegress;

        jailPlayerNameText.text = playerName;

        judgingWindow.SetActive(true);
    }

    [SerializeField] private GameObject GO_SentenceButton;
    [SerializeField] private GameObject GO_SentenceText;
    [SerializeField] private GameObject GO_JustifyButton;
    [SerializeField] private GameObject GO_JustifyText;
    private void ResetJudging()
    {
        GO_SentenceButton.SetActive(true);
        GO_SentenceText.SetActive(true);

        GO_JustifyButton.SetActive(true);
        GO_JustifyText.SetActive(true);
    }

    public void EndJudging(ParameterDictionary parameters)
    {
        EndJudging();
    }

    public void EndJudging()
    {
        judgingWindow.SetActive(false);
    }

    public void SentenceButton()
    {
        RequestJudging(true);
        //judgingWindow.SetActive(false);

        GO_JustifyButton.SetActive(false);
        GO_JustifyText.SetActive(false);
    }

    public void JustifyButton()
    {
        RequestJudging(false);
        //judgingWindow.SetActive(false);

        GO_SentenceButton.SetActive(false);
        GO_SentenceText.SetActive(false);
    }

    private void RequestJudging(bool sentence)
    {
        var parameters = new Dictionary<byte, object>();

        parameters.Add((byte)Params.Sentence, sentence);

        PhotonManager.Inst.peer.SendOperation((byte)Request.Judging, parameters, PhotonManager.Inst.sendOptions);

    }

    [SerializeField] private RoomDeskUi roomDeskUi;
    public void EnableSelfExtraMarker()
    {
        roomDeskUi.EnableMarker();
    }
    public void DisableSelfExtraMarker()
    {
        roomDeskUi.DisableMarker();
    }

    public void EnableTargetExtraMarker()
    {
        foreach(var p in playerUis.Values)
        {
            if (p.playerId == GameManager.instance.userId) continue;
            p.EnableMarker();
        }
    }
    public void DisableTargetExtraMarker()
    {
        foreach (var p in playerUis.Values)
        {
            p.DisableMarker();
        }
    }

    public void SetPlayerVoteCount(ParameterDictionary parameters)
    {
        var playerId = (long)parameters[(byte)Params.UserId];

        var playerUi = FindPlayerUi(playerId);

        playerUi.SetVoteCount(parameters);
    }

}
