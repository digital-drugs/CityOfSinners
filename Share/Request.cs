namespace Share
{
    public enum Request : int
    {
        Login,
        LoginSucces,

        InQueue,
        SetPlayerRole,

        ChangeUserName,

        Buy,

        Gif,

        BuySecceeded,

        StartGame,

        CreateLobby,
        ForceStartGameFromLobby,

        CloseLobby,
        StartGameFromLobby,

        AddLobbyPlayer,
        RemoveLobbyPlayer,

        CreateGame,
        RequestExitGameRoom,
        ConfirmExitGameRoom,

        SendChat,

        PrivateChatMessage,

        Bet,

        SelectPlayer,

        TeamCompound,

        RoomPersonalSystemMessage,

        SaveSkill,
        LoadSkills,

        LevelUpSkill,

        SaveExtra,
        LoadExtras,
        BuyExtra,

        SaveUserSlot,
        ClearUserSlot,

        CreateGameSlots,

        UseExtra,
        ExtraGameCount,

        SaveAchieve,

        EndGameResult,

        ErrorMessage,

        PlayerStatus,

        AddExtraEffectToPlayer,
        RemoveExtraEffectFromPlayer,

        Judging,

        EnableVote,
        DisableVote,

        EnableVisit,
        DisableVisit,

        UnlockRole_PlayerToPlayer,
        UnlockRole_GroupToPlayer, 

        ShareMessage,

        LoadGifts,
        SaveGift,

        LoadClans,
        
        LoadStocks,
        SaveStock,
        SaveSet,
        SaveItem,

        Reload,

        GetStocksList,
        LoadStock,
        LoadSet,
        LoadItem,

        GetUserClan,
        GetClansForProposal,
        GetUserInvites,
        CreateProposal,
        DeleteProposal,
        GetCoinsForClan,
        TryCreateClan,
        GetClanUsers,
        GetUserRights,

        GetInvitesTab,
        CreateInvite,
        DeleteInvite,

        AcceptInvite,
        AcceptProposal,

        JoinChat,
        LeaveChat,

        SystemMessage_Extra,
        SystemMessage_Role,
        SystemMessage_Skill,
		
		ChangePlayerRole,

        GetClanUpgrades,
        BuyUpgrade,

        RequestDuelsTab,
        RotateWheel,

        GetReitings,
        GetReiting,

        RemoveSkills,

        PingFromServer,

        GetLobbyList,
        JoinLobby,
        LeaveLobby,
        RemoveLobby,

        LobbyInfo,
        UnSubscribeLobby,

        StartRoomBuilder,

        LockSlot,
    }
}

