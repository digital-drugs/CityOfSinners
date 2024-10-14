using ExitGames.Client.Photon;
using ntw.CurvedTextMeshPro;
using Share;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomPlayerUi : MonoBehaviour, IMouseAction
{
    [SerializeField] private Image playerAvatar;
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private Button playerButton;
    public long playerId { get; private set; }
    public string playerName;

    [SerializeField] private TextProOnACircle circleText;
    [SerializeField] private float degressPerChar=11f;
    public void Assign(long id, Dictionary<byte,object> player)
    {
        playerName = (string)player[(byte)Params.UserName];

        var summaryDegress = playerName.Length * degressPerChar;
        if (summaryDegress > 180) summaryDegress = 180;

        circleText.m_arcDegrees = summaryDegress;

        playerNameText.text = playerName;

        playerId = id;

        playerButton.onClick.AddListener(() => SelectPlayer());

        privateMessageButton.SetActive(false);

        SetVoteCount(0);
    }   

    private void SelectPlayer()
    {
        //Debug.Log($"select button");
        GameRoomUi.instance.SelectPlayer(playerId);     
    }

    [SerializeField] private Image Image_plateBg;
    public void SetRole(RoleType role)
    {
        var roleUi =  GameRoomUi.instance.FindRoleUi(role);

        SetRole(roleUi.roleUrl);       
    }

    public void SetRole(string url)
    {
        GameRoomUi.instance.StartLoadTexture(playerAvatar, url);

        if (playerId == GameManager.instance.userId && !inJail && !inMorgue)
        {
            Image_plateBg.color = Color.green;
        }
    }

    [SerializeField] private GameObject Go_deadSign;

    public bool inJail = false;
    public void SendToJail()
    {
        inJail = true;

        Image_plateBg.color = Color.gray;
        playerAvatar.color = Color.gray;

        Go_deadSign.SetActive(true);

        HideVisit();
        HideVote();
    }



    public bool inMorgue=false;
    public void SendToMorgue()
    {
        inMorgue = true;

        Image_plateBg.color = Color.gray;
        playerAvatar.color = Color.gray;

        Go_deadSign.SetActive(true);

        HideVisit();
        HideVote();
    }

    public void ResurectPlayer()
    {
        inMorgue = false;
        inJail = false;

        Image_plateBg.color = Color.white;
        playerAvatar.color = Color.white;

        Go_deadSign.SetActive(false);
    }    

    [SerializeField] private GameObject voteMark;
    [SerializeField] private TextMeshProUGUI voteText;
    public void ShowVote()
    {
        voteMark.SetActive(true);
        playerButton.enabled = true;
        voteText.text = "";

    }

    public void SetVoteCount(ParameterDictionary data)
    {
        var count = (int)data[(byte)Params.Votes];

        SetVoteCount(count);
    }

    private void SetVoteCount(int count)
    {
        if (count > 0)
        {
            voteText.text = $"{count}";
        }
        else
        {
            voteText.text = $"";
        }
      
    }

    public void HideVote()
    {
        voteMark.SetActive(false);
        playerButton.enabled = false;
        voteText.text = "";
    }

    [SerializeField] private GameObject visitMark;
    [SerializeField] private Image visitImage;
    public void ShowVisit()
    {
        var roleAction = GameRoomUi.instance.GetRoleAction();

        if (roleAction != null)
        {
            visitImage.sprite = roleAction.sprite;
        }
        else
        {
            visitImage.sprite = GameRoomUi.instance.visitSprite;
        }     

        visitMark.SetActive(true);
        playerButton.enabled = true;
    }
    public void HideVisit()
    {
        visitMark.SetActive(false);
        playerButton.enabled = false;
    }


    [SerializeField] private ExtraEffectUi extraEffectUiPrefab;
    [SerializeField] private Transform extraEffectUisContainer;
    private Dictionary<int, ExtraEffectUi> extraEffectUis = new Dictionary<int, ExtraEffectUi>();
    public void AddExtraEffect(ParameterDictionary parameters)
    {
        var effectId = (int)parameters[(byte)Params.ExtraEffectId];
        var extraId = (string)parameters[(byte)Params.ExtraId];

        var newEffectUi = Instantiate(extraEffectUiPrefab);
        extraEffectUis.Add(effectId, newEffectUi);

        UiHelper.AssignObjectToContainer(newEffectUi.gameObject, extraEffectUisContainer);

        var extraUi = ExtraScreenUi.instance.extraUis[extraId];

        newEffectUi.Assign(extraUi.extraIco.sprite);
    }

    public void RemoveExtraEffect(ParameterDictionary parameters)
    {
        var effectId = (int)parameters[(byte)Params.ExtraEffectId];

        ExtraEffectUi effectUi = null;
        if (extraEffectUis.ContainsKey(effectId))
        {
            effectUi = extraEffectUis[effectId];
        }
        else
        {
            Debug.Log($"cant remove extra effect. effect ui not found");
            return;
        }

        extraEffectUis.Remove(effectId);

        UiHelper.MoveUiObjectToTrash(effectUi.gameObject);     
    }


    [SerializeField] private GameObject privateMessageButton;
    public void StartAction()
    {
        privateMessageButton.SetActive(true);
    }

    public void EndAction()
    {
        if (Image_PrivateMessage.activeSelf) return;

        privateMessageButton.SetActive(false);
    }

    public void OpenPrivateChat()
    {
        GameRoomUi.instance.roomChatUi.OpenPrivateChat(playerId);
        Image_PrivateMessage.SetActive(false);
    }

    [SerializeField] private GameObject Image_PrivateMessage;
    public void ShowNewPrivateMessage()
    {
        privateMessageButton.SetActive(true);
        Image_PrivateMessage.SetActive(true);
    }

    [SerializeField] private GameObject targetExtraMarker;
    public void DisableMarker()
    {
        targetExtraMarker.SetActive(false);
    }

    public void EnableMarker()
    {
        targetExtraMarker.SetActive(true);
    }
}
