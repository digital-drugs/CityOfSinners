using ExitGames.Client.Photon;
using Share;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ExtraInGameRoom : MonoBehaviour, IEndDragHandler, IBeginDragHandler, IDragHandler, IDropHandler, IExtraAction
{
    [SerializeField] private Image imageBg;
    [SerializeField] private Image imageIco;
    [SerializeField] private GameObject counter;
    [SerializeField] private TextMeshProUGUI counterText;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Button descriptionButton;
    public int slotId { get; private set; }
    public Transform slot { get; private set; }   
    public void Assign(Transform slot, int slotId,float slotSizeX, Dictionary<byte, object> extraData)
    {
        //размещаем экстру в слоте
        this.slot = slot;
        transform.SetParent(slot.transform);
        transform.localScale = Vector3.one;

        //настраиваем размер экстры под размер слота
        var rect =  GetComponent<RectTransform>();
        var factor = slotSizeX / rect.rect.size.x;
        rect.sizeDelta = rect.rect.size * factor;        

        //настраиваем расположение экстры в слоте
        GoToSlot();
        
        this.slotId = slotId;

        descriptionButton.onClick.AddListener(() => OnClick());

        AssignExtraData(extraData);
    }

    public string extraId { get; private set; }
    public string extraDescription { get; private set; }
    private void AssignExtraData(Dictionary<byte, object> extraData)
    {
        SetExtraCount(extraData);


        SetUseType(extraData);

        //ico
        extraId = (string)extraData[(byte)Params.ExtraId];
        imageIco.enabled = true;

        //bg

        ///получить сырую дату по экстре.
        var extraUi = ExtraScreenUi.instance.extraUis[extraId];
        
        //var extraType = (string)extraUi.extraData[(byte)Params.ExtraType];
        //imageBg.color = Color.red;

        //Debug.Log($"Assign Extra Data");
        imageIco.sprite = extraUi.extraIco.sprite;

        nameText.text = (string)extraData[(byte)Params.ExtraName];
        extraDescription = (string)extraUi.extraData[(byte)Params.ExtraDescription];
    }

    public ExtraUseType extraUseType { get; private set; }
    //[SerializeField] TextMeshProUGUI extraUseTypeText;
    [SerializeField] private Image Image_TypeIco;
    private void SetUseType(Dictionary<byte, object> extraData)
    {
        var useType = (string)extraData[(byte)Params.ExtraUseType];
        extraUseType = (ExtraUseType)Helper.GetEnumElement<ExtraUseType>(useType);

        switch (extraUseType)
        {
            case ExtraUseType.Auto: { Image_TypeIco.sprite = ExtraScreenUi.instance.autoExtra; } break;
            case ExtraUseType.Self: { Image_TypeIco.sprite = ExtraScreenUi.instance.clickExtra; } break;
            case ExtraUseType.Target: { Image_TypeIco.sprite = ExtraScreenUi.instance.targetExtra; } break;
            case ExtraUseType.Shop: { Image_TypeIco.sprite = ExtraScreenUi.instance.bunkerExtra; } break;
            default: { Image_TypeIco.sprite = ExtraScreenUi.instance.bunkerExtra; } break;
        }
    }

    public void ClearSlot()
    {
        imageBg.color = Color.gray;

        imageIco.enabled = false;

        counter.SetActive(false);

        extraId = "";
    }

    private bool canSelectExtra=false;
    private int extraCount = 0;
    public void SetExtraCount(Dictionary<byte, object> extraData)
    {
        extraCount = 0;

        if (extraData.ContainsKey((byte)Params.ExtraCurrentCount))
        {
            extraCount = (int)extraData[(byte)Params.ExtraCurrentCount];
        }

        UpdateExtraCount();
    }

    public void SetExtraCount(ParameterDictionary extraData)
    {
        extraCount = 0;

        if (extraData.ContainsKey((byte)Params.ExtraCurrentCount))
        {
            extraCount = (int)extraData[(byte)Params.ExtraCurrentCount];
        }

        UpdateExtraCount();
    }

    private void UpdateExtraCount()
    {
        if (extraCount > 0)
        {
            counterText.text = $"{extraCount}";
            counter.SetActive(true);
            canSelectExtra = true;
        }
        else
        {
            counterText.text = $"";
            counter.SetActive(false);
            imageIco.color = Color.gray;

            canSelectExtra = false;

            var rect = GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(0, -30);
        } 
    }

    //public void SelectSlot()
    //{
    //    return;

    //    if (extraUseType == ExtraUseType.Target)
    //    {
    //        Debug.Log($"cant drag {extraUseType}");
    //        return;
    //    }

    //    //Debug.Log($"select slot");

    //    var parameters = new Dictionary<byte, object>();

    //    parameters.Add((byte)Params.SlotId, slotId);
    //    parameters.Add((byte)Params.ExtraId, extraId);

    //    PhotonManager.Inst.peer.SendOperation((byte)Request.UseExtra, parameters, PhotonManager.Inst.sendOptions);
    //}

    //Vector2 startPosition;
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (extraCount == 0) return;

        if (extraUseType == ExtraUseType.Auto)
        {
            return;
        }

        if(extraUseType == ExtraUseType.Self)
        {
            GameRoomUi.instance.EnableSelfExtraMarker();
        }

        if (extraUseType == ExtraUseType.Target)
        {
            GameRoomUi.instance.EnableTargetExtraMarker();
        }

        transform.localScale = Vector3.one ;

        ImageManager.instance.HideExtraUnderCursorDescription();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (extraCount == 0) return;

        if (extraUseType == ExtraUseType.Auto)
        {
            Debug.Log($"cant drag {extraUseType}");
            return;
        }

        var offset = GetComponent<RectTransform>().rect.size * 0.5f;

        transform.position = new Vector2(
            Input.mousePosition.x, 
            Input.mousePosition.y + offset.y * GameManager.instance.mainCanvas.scaleFactor);
    }

    public void OnDrop(PointerEventData eventData)
    {
        Debug.Log($"drop");

        //Debug.Log($"{transform.localPosition}"); 

        GoToSlot();

        //Debug.Log($"{transform.localPosition}");


       
    }
 
    public void OnEndDrag(PointerEventData eventData)
    {
        //Debug.Log($"end drag");

        if (extraUseType == ExtraUseType.Self)
        {
            GameRoomUi.instance.DisableSelfExtraMarker();
        }

        if (extraUseType == ExtraUseType.Target)
        {
            GameRoomUi.instance.DisableTargetExtraMarker();
        }

        List<RaycastResult> raycastResults = new List<RaycastResult>();

        EventSystem.current.RaycastAll(eventData, raycastResults);

        foreach (var r in raycastResults)
        {
            var roomPlayer = r.gameObject.GetComponent<RoomPlayerUi>();

            if (roomPlayer != null && extraUseType == ExtraUseType.Target)
            {
                if (roomPlayer.playerId == GameManager.instance.userId) return;

                    var parameters = new Dictionary<byte, object>();

                    parameters.Add((byte)Params.SlotId, slotId);
                    parameters.Add((byte)Params.ExtraId, extraId);
                    parameters.Add((byte)Params.UserId, roomPlayer.playerId);

                    PhotonManager.Inst.peer.SendOperation((byte)Request.UseExtra, parameters, PhotonManager.Inst.sendOptions);
            }

            var roomDesk = r.gameObject.GetComponent<RoomDeskUi>();

            if (roomDesk != null && extraUseType == ExtraUseType.Self)
            {
                var parameters = new Dictionary<byte, object>();

                parameters.Add((byte)Params.SlotId, slotId);
                parameters.Add((byte)Params.ExtraId, extraId);

                PhotonManager.Inst.peer.SendOperation((byte)Request.UseExtra, parameters, PhotonManager.Inst.sendOptions);
            }
        }
    }

    private void GoToSlot()
    {
        var rect = GetComponent<RectTransform>();
        rect.anchoredPosition = Vector2.zero;
    }

    [SerializeField] private int onZoomY=180;
    public void StartAction()
    {
        if (!canSelectExtra) return;

        slot.SetAsLastSibling();

        transform.localScale = Vector3.one * 2;        

        var rect = GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(0, onZoomY);
        //transform.localPosition = new Vector2(0, 100);

        ImageManager.instance.ShowExtraUnderCursorDescription(this);

        counter.SetActive(false);
    }

    public void EndAction()
    {
        if (!canSelectExtra) return;

        transform.localScale = Vector3.one;

        var rect = GetComponent<RectTransform>();
        rect.anchoredPosition = Vector2.zero;

        ImageManager.instance.HideExtraUnderCursorDescription();

        counter.SetActive(true);
    }

    public void blockExtraByGamePhase()
    {

    }

    public void OnClick()
    {
        Debug.Log($"click extra help");
        ImageManager.instance.SwitchExtraDescription(this);
    }
}

public interface IExtraAction
{
    void StartAction();
    void EndAction();
}