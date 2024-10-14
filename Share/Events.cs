namespace Share
{
    public enum Events : int
    {  
        GeneralChatMessage,
        RoomChatMessage,
        RoomGroupChatMessage,
        RoomPublicSystemMessage,

        AuctionTimer,
        StartAuction,
        AuctionSlots,
        AuctionBet,
        AuctionBuy,

        StartGame,

        ChangeGamePhase,
        GameTimer,

        ChangeGoodTeam,
        ChangeBadTeam,
        ChangeTeam_Werewolf,

        StartDay,
        StartNight,

        PlayerToJail,
        PlayerToMorgue,
        ResurectPlayer,
		ChangePlayerRole,

        CreateChat,

        StartJudging,
		SeeJudging,
        EndJudging,

        UnlockRole_PlayerToGroup,
		UnlockRole_PlayerToRoom,

        RoomSystemMessage_Skill,
        RoomSystemMessage_Extra,        
        RoomSystemMessage_Role,

        Role_PublicMessage,

        TeamSystemMessage,

        SetPlayerVoteCount,

        AddPlayerToLobby,
        RemovePlayerFromLobby,
        CloseLobby,

        LobbyInfo_AddLobby,
        LobbyInfo_RemoveLobby,
        LobbyInfo_PlayersCount,

        TeamCompound,
    }
}
