using ExitGames.Client.Photon;
using Share;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RoomChatUi : MonoBehaviour/*, IChat*/
{
    public static RoomChatUi instance;

    private void Start()
    {
        instance = this;
    }

    [SerializeField] private ScrollRect chatScroll;
    public void AddMessageFromServer(ParameterDictionary parameters)
    {    
        var chatType = (ChatType)parameters[(byte)Params.ChatType];

        //Debug.Log($"new message {chatType}");

        switch (chatType)
        {
            case ChatType.RoomChat:
                {
                    AddGroupChatMessage(parameters);
                }
                break;
            //case ChatType.GroupChat:
            //    {
            //        AddGroupChatMessage(parameters);
            //    }
            //    break;
            case ChatType.PrivateChat:
                {
                    AddPrivateChatMessage(parameters);
                }
                break;
        }
        //проверка на изображения (смайлы/стикеры)            
    }

    //[SerializeField] private Sprite newMessageSprite;
    //[SerializeField] private Sprite sameMessageSprite;
    [SerializeField] private ChatMessageUi chatMessageUiPrefab;
    [SerializeField] private SameChatMessageUi sameChatMessageUiPrefab;
    
    [SerializeField] private SelfChatMessageUi selfChatMessageUiPrefab;
    [SerializeField] private SameSelfChatMessageUi sameSelfChatMessageUiPrefab;

    //[SerializeField] private Transform roomChatContainer;
    //[SerializeField] private ScrollRect roomChatScroll;


    private void AddGroupChatMessage(ParameterDictionary parameters)
    {
        var chatId = (ChatId)parameters[(byte)Params.ChatId];

        var chat = groupChats[chatId];

        IChatMessage message = null;

        var ownerId = (long)parameters[(byte)Params.OwnerId];

        if (GameManager.instance.userId == ownerId)
        {
            if (chat.lastMessageId == ownerId)
            {
                message = Instantiate(sameSelfChatMessageUiPrefab);
            }
            else
            {
                message = Instantiate(selfChatMessageUiPrefab);
            }
        }
        else
        {
            if (chat.lastMessageId == ownerId)
            {
                message = Instantiate(sameChatMessageUiPrefab);
            }
            else
            {
                message = Instantiate(chatMessageUiPrefab);
            }
        }

        chat.lastMessageId = ownerId;

        message.Assign(parameters, chat);
    }

    private void AddPrivateChatMessage(ParameterDictionary parameters)
    {
        if (Button_OpenPrivates != null)
        {
            Button_OpenPrivates.ShowNewMessage();
        }

        var ownerId = (long)parameters[(byte)Params.OwnerId];
        var fromId = (long)parameters[(byte)Params.FromId];
        var toId = (long)parameters[(byte)Params.ToId];

        RoomGroupChatUi chat = null;

        if(GameManager.instance.userId == ownerId)
        {
            if (privateChats.ContainsKey(toId))
            {
                chat = privateChats[toId];
            }
            else
            {
                chat = CreatePrivateChat(toId);
            }
        }
        else
        {
            if (privateChats.ContainsKey(fromId))
            {
                chat = privateChats[fromId];
            }
            else
            {
                chat = CreatePrivateChat(fromId);
            }
        }       

        IChatMessage message = null;

        if (GameManager.instance.userId == ownerId)
        {
            if (chat.lastMessageId == ownerId)
            {
                message = Instantiate(sameSelfChatMessageUiPrefab);
            }
            else
            {
                message = Instantiate(selfChatMessageUiPrefab);
            }
        }
        else
        {
            if (chat.lastMessageId == ownerId)
            {
                message = Instantiate(sameChatMessageUiPrefab);
            }
            else
            {
                message = Instantiate(chatMessageUiPrefab);
            }
        }

        chat.lastMessageId = ownerId;

        message.Assign(parameters, chat);

        //send ping to desk from new message
        if (GameManager.instance.userId != ownerId)
        {
            var playerUi = GameRoomUi.instance.FindPlayerUi(fromId);
            if (!chat.ChatIsOpen()) playerUi.ShowNewPrivateMessage();
        }

        if (ui_PrivatesToPlayer.ContainsKey(ownerId))
        {
            var ui = ui_PrivatesToPlayer[ownerId];

            ui.ShowNewMessageIco();
        }
    }

    private GroupChatButtonUi Button_OpenPrivates;
    private GameObject Container_PrivateButtons;
    internal void CreatePrivates(Dictionary<long, object> players)
    {
        //создаем контейнер для кнопок приватов
        var container = CreateChat(ChatId.Private);
        var content = container.GetTransform();

        //создаем кнопку открытия списка приватов
        var grid = content.gameObject.AddComponent<GridLayoutGroup>();

        grid.cellSize = new Vector2(400, 35);
        grid.spacing = new Vector2(0, 5);
        grid.padding.bottom = 10;

        grid.startCorner = GridLayoutGroup.Corner.LowerLeft;
        grid.childAlignment = TextAnchor.LowerLeft;

        //создаем кнопки для открытия приватных чатов
        foreach (var p in players) 
        {
            var player = (Dictionary<byte, object>)p.Value;

            CreateUiPrivateToPlayer(p.Key, player, container, content);
        }
    }

    //кнопка для открытия приватного часа с конкретным игроком
    [SerializeField] private Ui_PrivateToPlayer ui_PrivateToPlayer;
    //[SerializeField] private Transform Container_Ui_PrivateToPlayer;
    private Dictionary<long, Ui_PrivateToPlayer> ui_PrivatesToPlayer = new Dictionary<long, Ui_PrivateToPlayer>();
    private void CreateUiPrivateToPlayer(
        long key, 
        Dictionary<byte, object> player, 
        RoomGroupChatUi container, 
        Transform content)
    {
        var newRoomPlayer = Instantiate(ui_PrivateToPlayer, content);

        ui_PrivatesToPlayer.Add(key, newRoomPlayer);

        newRoomPlayer.Assign(key, player);

        //container.AddPrivatePlayer(ui_PrivatesToPlayer.Count, newRoomPlayer);
    }

    IEnumerator ScrollChat(ScrollRect scroll)
    {
        //Canvas.ForceUpdateCanvases();
        yield return new WaitForEndOfFrame();
        scroll.normalizedPosition = new Vector2(0, 0);
    }

    public void Clear()
    {
        //удаляем сообщения из общего чата в комнате
        //UiHelper.ClearContainer(roomChatContainer);
        
        //удаляем все окна в групповых чатах
        groupChats.Clear();
        privateChats.Clear();
        UiHelper.ClearContainer(groupChatsContainer);

        //удаляем все кнопки для переключения между групповыми чатами
        groupchatButtons.Clear();
        UiHelper.ClearContainer(groupChatButtonsContainer);

        //firstChatSelected = false;

        ui_PrivatesToPlayer.Clear();
    }

    [SerializeField] private TMP_InputField groupMessageInput;
    public void SendMessageToGroupChat(bool checkEnter)
    {
        if (checkEnter && !Input.GetKeyDown(KeyCode.Return)) return;

        var message = groupMessageInput.text;

        message = Regex.Replace(message, @"\t|\n|\r", "");

        if (string.IsNullOrEmpty(message)) return;

        var parameters = new Dictionary<byte, object>();

        parameters.Add((byte)Params.ChatType, ChatType.RoomChat);

        parameters.Add((byte)Params.ChatId, currentGroupChat);
        
        parameters.Add((byte)Params.UserId, currentPrivateChatId);

        parameters.Add((byte)Params.ChatMessage, message);

        PhotonManager.Inst.peer.SendOperation((byte)Request.SendChat, parameters, PhotonManager.Inst.sendOptions);

        UpdateChatInput();
    }

    [SerializeField] private TextMeshProUGUI chatInputPlaceHolder;
    private void UpdateChatInput()
    {
        groupMessageInput.text = "";
        //groupMessageInput.sele();

        switch (currentGroupChat)
        {
            case ChatId.General: chatInputPlaceHolder.text = "общий чат"; break;
            case ChatId.BadTeam: chatInputPlaceHolder.text = "чат злой команды"; break;
            case ChatId.MafiaRole: chatInputPlaceHolder.text = "чат мафиози"; break;
        }
    }

    /// <summary>
    /// присоединяемся к чату
    /// </summary>
    /// <param name="parameters"></param>
    public void JoinChat(ParameterDictionary parameters)
    {
        var chatId = (ChatId)parameters[(byte)Params.ChatId];

        CreateChat(chatId);
    }

    [SerializeField] private GameObject groupChatInput;
    [SerializeField] private RoomGroupChatUi groupChatPrefab;
    [SerializeField] private Transform groupChatsContainer;
    private Dictionary<ChatId, RoomGroupChatUi> groupChats = new Dictionary<ChatId, RoomGroupChatUi>();
    //private bool firstChatSelected = false;
    public RoomGroupChatUi CreateChat(ChatId chatId)
    {
        //создаем окно чата
        var newGroupChat = Instantiate(groupChatPrefab);
        UiHelper.AssignObjectToContainer(newGroupChat.gameObject, groupChatsContainer);
        newGroupChat.gameObject.SetActive(false);
        groupChats.Add(chatId, newGroupChat);

        newGroupChat.GetComponent<RectTransform>().sizeDelta = Vector2.zero;

        //создаем кнопку для переключения на это окно чата
        AddChatButton(chatId);

        //выделяем общий чат
        if (chatId == ChatId.General)
        {
            //firstChatSelected = true;
            SelectGroupChat(chatId);
        }

        if(Button_OpenPrivates != null)
        {
            Button_OpenPrivates.transform.SetAsLastSibling();
        }

        return newGroupChat;
    }

    public void LeaveChat(ParameterDictionary parameters)
    {
        var chatId = (ChatId)parameters[(byte)Params.ChatId];

        DeleteChat(chatId);
    }   

    public void DeleteChat(ChatId chatId)
    {     
        if (!groupChats.ContainsKey(chatId)) return;     

        var chat = groupChats[chatId];

        groupChats.Remove(chatId);

        Destroy(chat.gameObject);

        var chatButton = groupchatButtons[chatId];

        groupchatButtons.Remove(chatId);

        Destroy(chatButton.gameObject);

        Debug.Log($"delete chat id {chatId}");
    }

    [Header("Group chat buttons")]

    public Sprite Sprite_groupButtonChatEnabled;
    public Color Color_groupButtonChatEnabled;

    public Sprite Sprite_groupButtonChatDisabled;
    public Color Color_groupButtonChatDisabled;

    [SerializeField] private GroupChatButtonUi groupChatButtonPrefab;
    [SerializeField] private Transform groupChatButtonsContainer;
    private Dictionary<ChatId, GroupChatButtonUi> groupchatButtons= new Dictionary<ChatId, GroupChatButtonUi>();
    [SerializeField] private Sprite Sprite_generalChat;
    [SerializeField] private Sprite Sprite_BadTeamChat;
    [SerializeField] private Sprite Sprite_PiratTeamChat;
    [SerializeField] private Sprite Sprite_MafiaChat;
    [SerializeField] private Sprite Sprite_SinnerChat;
    [SerializeField] private Sprite Sprite_SaintChat;
    private GroupChatButtonUi AddChatButton(ChatId chatId) 
    {
        var newGroupChatButton = Instantiate(groupChatButtonPrefab);
        UiHelper.AssignObjectToContainer(newGroupChatButton.gameObject, groupChatButtonsContainer);

        //получаем имя кнопки
        var chatName = GetChatNameById(chatId);

        newGroupChatButton.Assign(chatName);

        groupchatButtons.Add(chatId, newGroupChatButton);

        Action action = () =>
        {
            SelectGroupChat(chatId);
            newGroupChatButton.HideNewMessage();
        };

        newGroupChatButton.button.onClick.AddListener(() => action());

        if(chatId == ChatId.Private)
        {
            Button_OpenPrivates = newGroupChatButton;
        }

        //switch (chatId)
        //{
        //    case ChatId.General: newGroupChatButton.image.sprite = Sprite_generalChat; break;
        //    case ChatId.BadTeam: newGroupChatButton.image.sprite = Sprite_BadTeamChat; break;
        //    case ChatId.PiratTeam: newGroupChatButton.image.sprite = Sprite_PiratTeamChat; break;
        //    case ChatId.MafiaRole: newGroupChatButton.image.sprite = Sprite_MafiaChat; break;
        //    case ChatId.SinnerRole: newGroupChatButton.image.sprite = Sprite_SinnerChat; break;
        //    case ChatId.SaintRole: newGroupChatButton.image.sprite = Sprite_SaintChat; break;
        //}

        return newGroupChatButton;
    }

    private string GetChatNameById(ChatId chatId)
    {
        var result = "";

        switch (chatId)
        {
            case ChatId.General: { result = "Общий"; } break;
            case ChatId.BadTeam: { result = "Мафия"; } break;
            case ChatId.PiratTeam: { result = "Пираты"; } break;
            case ChatId.SaintRole: { result = "Святые"; } break;
            case ChatId.Private: { result = "Приваты"; } break;
        }

        return result;
    }

    private ChatType currentChatType = ChatType.GroupChat;
    private ChatId currentGroupChat = ChatId.General;
    private long currentPrivateChatId;

    private void ResetGroupChats()
    {
        foreach (var gc in groupChats.Values)
        {
            gc.gameObject.SetActive(false);
        }

        foreach (var gc in groupchatButtons.Values)
        {
            gc.DisableButton();
        }
    }

    private void SelectGroupChat(ChatId chatId)
    {
        currentChatType = ChatType.GroupChat;

        ResetGroupChats();

        ResetPrivateChats();

        currentGroupChat = chatId;

        var groupChat = groupChats[chatId];

        groupChat.gameObject.SetActive(true);

        var groupchatButton = groupchatButtons[chatId];

        groupchatButton.EnableButton();
    }


    //[SerializeField] private long _lastMessageId;
    //[SerializeField] private float _messageOffset;
    //[SerializeField] private float _sameMessageOffset;
    //[SerializeField] private float _lastPosition;
    //public long lastMessageId { get => _lastMessageId; set => _lastMessageId = value; }
    //public float lastPosition { get => _lastPosition; set => _lastPosition = value; }
    //public float sameMessageOffset => _sameMessageOffset;
    //public float messageOffset => _messageOffset;
    //public Transform GetTransform()
    //{
    //    return null;// roomChatContainer.transform;
    //}

    //public RectTransform GetRectTransform()
    //{
    //    return null;// roomChatContainer.GetComponent<RectTransform>();
    //}

    private Dictionary<long, RoomGroupChatUi> privateChats = new Dictionary<long, RoomGroupChatUi>();
    public void OpenPrivateChat(long playerId)
    {
        currentChatType = ChatType.PrivateChat;

        currentPrivateChatId = playerId;

        RoomGroupChatUi chat = null;

        if (privateChats.ContainsKey(playerId))
        {
            chat = privateChats[playerId];
        }
        else
        {
            chat = CreatePrivateChat(playerId);
        }        

        ShowPrivateChat(chat);

        //Debug.Log($"current chat type {currentChatType}");
    }

    private RoomGroupChatUi CreatePrivateChat(long playerId)
    {
        //создаем окно чата
        var newChat = Instantiate(groupChatPrefab);
        UiHelper.AssignObjectToContainer(newChat.gameObject, groupChatsContainer);
        newChat.gameObject.SetActive(false);
        privateChats.Add(playerId, newChat);

        newChat.GetComponent<RectTransform>().sizeDelta = Vector2.zero;

        return newChat;
    }

    private void ShowPrivateChat(RoomGroupChatUi chat)
    {
        ResetGroupChats();

        ResetPrivateChats();

        chat.gameObject.SetActive(true);
    }

    private void ResetPrivateChats()
    {
        foreach(var pc in privateChats.Values)
        {
            pc.gameObject.SetActive(false);
        }
    }
}

public interface IChatMessage
{
    void Assign(ParameterDictionary parameters, IChat chat);
}