using Share;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ExtraUi : MonoBehaviour, IBeginDragHandler,IDragHandler,IEndDragHandler
{
    public Image extraBg;
    public Image extraIco;
    [SerializeField] private TextMeshProUGUI Text_ExtraName;
    [SerializeField] private GameObject extraCounter;
    [SerializeField] private TextMeshProUGUI extraCountText;
    [SerializeField] private Button selectButton;
    public string extraId { get; private set; }
    public Dictionary<byte, object> extraData { get; private set; }
    public void Assign(string extraId, Dictionary<byte, object> extraData)
    {
        this.extraId = extraId;
        this.extraData = extraData;

        Text_ExtraName.text = (string)extraData[(byte)Params.ExtraName];

        SetUseType(extraData);

        if (extraData.ContainsKey((byte)Params.ExtraCount))
        {
            var extraCount = (int)extraData[(byte)Params.ExtraCount];
            SetExtraCount(extraCount);
        }
        else
        {
            SetExtraCount(0);
        }
      
        selectButton.onClick.AddListener(() => SelectExtra());

        var imageUrl = (string)extraData[(byte)Params.ExtraImageUrl];

        ImageManager.instance.StartLoadTexture(extraIco, imageUrl);       
    }

    private ExtraUi parentCard;
    public void Assign(ExtraUi extraUi)
    {
        parentCard = extraUi;

        this.extraId = extraUi.extraId;
        this.extraData = extraUi.extraData;

        //Text_ExtraName.text = (string)extraData[(byte)Params.ExtraName];

        SetUseType(extraData);

        SetExtraCount(extraUi.extraCount);

        //Debug.Log($"extraUi.extraCount {extraUi.extraCount}");

        selectButton.onClick.AddListener(() => SelectExtra());

        if (extraIco.sprite == null)
        {
            var imageUrl = (string)extraData[(byte)Params.ExtraImageUrl];
            ImageManager.instance.StartLoadTexture(extraIco, imageUrl);
        }      
    }

    private ExtraUseType extraUseType;
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
            default: { extraBg.sprite = ExtraScreenUi.instance.bunkerExtra; } break;
        }
    }

    [SerializeField] private GameObject Image_BlockImage;
    public int extraCount { get; private set; }
    public void SetExtraCount(int extraCount)
    {
        this.extraCount = extraCount;

        if (extraCount > 0)
        {
            extraCountText.text = $"{extraCount}";
            extraCounter.SetActive(true);

            if (parentCard != null)
            {
                Image_BlockImage.SetActive(false);
            }
        }
        else
        {
            extraCountText.text = $"";
            extraCounter.SetActive(false);

            if(parentCard != null)
            {
                Image_BlockImage.SetActive(true);
            }           
        }

        if (linkedCard != null)
        {
            linkedCard.SetExtraCount(extraCount);
        }
    }

    public event EventHandler OnCountUpdate;
    public void UpdateExtraCount(int extraCount)
    {
        if( extraData.ContainsKey((byte)Params.ExtraCount))
        {
            extraData[(byte)Params.ExtraCount]= extraCount;
        }
        else
        {
            extraData.Add((byte)Params.ExtraCount,extraCount);
        }

        SetExtraCount(extraCount);

        //OnCountUpdate?.Invoke(this, EventArgs.Empty);
    }

    private void SelectExtra()
    {
        ExtraScreenUi.instance.SelectExtraUi(extraId, extraData);
    }

    public event EventHandler OnImageLoaded;
    internal void ImageLoaded()
    {
        OnImageLoaded?.Invoke(this, EventArgs.Empty);
    }

    public void SetBGColor(Color color)
    {
        extraBg.color= color;
    }

   
  

    private ExtraUi linkedCard;
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (linkedCard == null && parentCard == null)
        { 
            GetClone(); 
        }
    }

    public ExtraUi GetClone()
    {
       var result =  Instantiate(this, GameManager.instance.mainCanvas.transform);

        //создаем копию карточки
        result.GetComponent<RectTransform>().sizeDelta = new Vector2(189, 224);

        result.transform.localScale = Vector3.one;

        result.Assign(this);      

        linkedCard = result;

        return result;
    }

    public void OnDrag(PointerEventData eventData)
    {
        var offset = GetComponent<RectTransform>().rect.size * 0.5f;

        //перемещаем карточку
        if(parentCard == null)
        {
            linkedCard.transform.position = eventData.position- offset * GameManager.instance.mainCanvas.scaleFactor;
        }
        else
        {
            transform.position = eventData.position- offset * GameManager.instance.mainCanvas.scaleFactor;
        }
      
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        //если попали в слот размещаем
        List<RaycastResult> raycastResults = new List<RaycastResult>();

        EventSystem.current.RaycastAll(eventData, raycastResults);

        var findSlot = false;

        foreach (var r in raycastResults)
        {
            var extraSlot = r.gameObject.GetComponent<ExtraSlotUi>();

            if (extraSlot != null)
            {
                if(parentCard == null)
                {
                    extraSlot.Assign(linkedCard);
                    Debug.Log($"find extra slot");
                    findSlot = true;
                    break;
                }
                else
                {
                    extraSlot.Assign(this);
                    Debug.Log($"find extra slot");
                    findSlot = true;
                    break;
                }              
            }

        }

        if (!findSlot)
        {
            //если не попали в слот, удаляем копию карточки
            if(parentCard == null)
            {
                FreeSlot(linkedCard.slot);
                DestroyCard(linkedCard);              
            }
            else
            {
                FreeSlot(slot);
                DestroyCard(this);
            }           
        }
    }

    public ExtraSlotUi slot { get; private set; }
    public void SetSlot(ExtraSlotUi slot)
    {
        if (parentCard != null)
        {
            parentCard.extraBg.color = GameManager.instance.Color_CardInSlot;
        }

        this.slot = slot;
    }

    public void FreeSlot(ExtraSlotUi freeSlot)
    {
        if (freeSlot != null)
        {
            freeSlot.ClearSlot();
            freeSlot = null;
        }
    }

    public void DestroyCard(ExtraUi card)
    {
        if(card.parentCard != null)
        {
            card. parentCard.extraBg.color = Color.black;
        }

        Destroy(card.gameObject);
    }
}
