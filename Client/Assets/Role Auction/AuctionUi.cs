using ExitGames.Client.Photon;
using Share;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AuctionUi : MonoBehaviour
{
    public static AuctionUi instance;

    // Start is called before the first frame update
    void Start()
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

    [SerializeField] private AuctionSlotUi auctionSlotPrefab;
    [SerializeField] private Transform auctionSlotsContainer;
    private Dictionary<int, AuctionSlotUi> auctionSlots = new Dictionary<int, AuctionSlotUi>();
    [SerializeField] private GameObject wi_Auction;
    public void StartAuction(ParameterDictionary parameters)
    {
        auctionSlots.Clear();

        UiHelper.ClearContainer(auctionSlotsContainer);

        var slots = (Dictionary<byte, object>)parameters[(byte)Params.AuctionSlots];

        foreach(var s in slots)
        {
            var slot = (Dictionary<byte, object>)s.Value;

            var slotId = (int)slot[(byte)Params.SlotId];

            var newAuctionSlotUi = Instantiate(auctionSlotPrefab, auctionSlotsContainer);
            auctionSlots.Add(slotId, newAuctionSlotUi);

            newAuctionSlotUi.Assign(slot);
        }

        SetAuctionTimer(parameters);

        wi_Auction.SetActive(true);
    }

    public void EndAuction()
    {
        wi_Auction.SetActive(false);
    }

    public void UpdateAuctionBet(ParameterDictionary parameters)
    {
        var slotId = (int)parameters[(byte)Params.SlotId];

        var slot = auctionSlots[slotId];       

        slot.UpdateSlot(parameters);
    }

    public void UpdateAuctionBuy(ParameterDictionary parameters)
    {
        var slotId = (int)parameters[(byte)Params.SlotId];

        var slot = auctionSlots[slotId];

        slot.BuySlot(parameters);
    }

    [SerializeField] private TextMeshProUGUI timerText;
    public void SetAuctionTimer(ParameterDictionary parameters)
    {
        var time = (int)parameters[(byte)Params.Timer];

        timerText.text = $"Таймер: {time}";
    }

    public void LockSlot(ParameterDictionary parameters)
    {
        foreach(var s in parameters)
        {
            var slotData = (Dictionary<byte, object>)s.Value;

            var slotId = (int)slotData[(byte)Params.SlotId];

            auctionSlots[slotId].BlockSlot();
        }        
    }
}
