using System.Reflection;

namespace DragonEmu.Protocol;

public static class IncomingOpcodes
{
    public static class Handshake
    {
        public const int InfoRetrieveMessageComposer = 7;
        public const int PongMessageComposer = 196;
        public const int InitCryptoMessageComposer = 206;
        public const int SSOTicketMessageComposer = 415;
        public const int DisconnectMessageComposer = 512;
        public const int TryLoginMessageComposer = 756;
        public const int UniqueIDMessageComposer = 813;
        public const int VersionCheckMessageComposer = 1170;
        public const int GetSessionParametersMessageComposer = 1817;
        public const int GenerateSecretKeyMessageComposer = 2002;
    }

    public static class FriendList
    {
        public const int MessengerInitMessageComposer = 12;
        public const int FriendListUpdateMessageComposer = 15;
        public const int SendMsgMessageComposer = 33;
        public const int SendRoomInviteMessageComposer = 34;
        public const int AcceptBuddyMessageComposer = 37;
        public const int DeclineBuddyMessageComposer = 38;
        public const int RequestBuddyMessageComposer = 39;
        public const int RemoveBuddyMessageComposer = 40;
        public const int HabboSearchMessageComposer = 41;
        public const int GetBuddyRequestsMessageComposer = 233;
        public const int FollowFriendMessageComposer = 262;
    }

    public static class InventoryPurse
    {
        public const int GetCreditsInfoComposer = 8;
    }

    public static class InventoryFurni
    {
        public const int RequestFurniInventoryComposer = 404;
    }

    public static class InventoryAvatarEffect
    {
        public const int AvatarEffectSelectedComposer = 372;
        public const int AvatarEffectActivatedComposer = 373;
    }

    public static class InventoryBadges
    {
        public const int GetBadgesComposer = 157;
        public const int SetActivatedBadgesComposer = 158;
    }

    public static class InventoryAchievements
    {
        public const int GetAchievementsComposer = 370;
    }

    public static class InventoryTrading
    {
        public const int UnacceptTradingComposer = 68;
        public const int AcceptTradingComposer = 69;
        public const int CloseTradingComposer = 70;
        public const int OpenTradingComposer = 71;
        public const int AddItemToTradeComposer = 72;
        public const int ConfirmAcceptTradingComposer = 402;
        public const int ConfirmDeclineTradingComposer = 403;
        public const int RemoveItemFromTradeComposer = 405;
    }

    public static class InventoryPets
    {
        public const int GetPetInventoryComposer = 3000;
    }

    public static class Marketplace
    {
        public const int MakeOfferMessageComposer = 3010;
        public const int GetMarketplaceConfigurationMessageComposer = 3011;
        public const int GetMarketplaceCanMakeOfferComposer = 3012;
        public const int BuyMarketplaceTokensMessageComposer = 3013;
        public const int BuyOfferMessageComposer = 3014;
        public const int CancelOfferMessageComposer = 3015;
        public const int RedeemOfferCreditsMessageComposer = 3016;
        public const int GetOffersMessageComposer = 3018;
        public const int GetOwnOffersMessageComposer = 3019;
        public const int GetMarketplaceItemStatsComposer = 3020;
    }

    public static class Navigator
    {
        public const int AddFavouriteRoomMessageComposer = 19;
        public const int DeleteFavouriteRoomMessageComposer = 20;
        public const int CreateFlatMessageComposer = 29;
        public const int GetUserFlatCatsMessageComposer = 151;
        public const int RateFlatMessageComposer = 261;
        public const int CanCreateEventMessageComposer = 345;
        public const int CreateEventMessageComposer = 346;
        public const int CancelEventMessageComposer = 347;
        public const int EditEventMessageComposer = 348;
        public const int GetOfficialRoomsMessageComposer = 380;
        public const int GetPopularRoomTagsMessageComposer = 382;
        public const int UpdateNavigatorSettingsMessageComposer = 384;
        public const int GetGuestRoomMessageComposer = 385;
        public const int UpdateRoomThumbnailMessageComposer = 386;
        public const int CanCreateRoomMessageComposer = 387;
        public const int GetPublicSpaceCastLibsMessageComposer = 388;
        public const int PopularRoomsSearchMessageComposer = 430;
        public const int RoomsWithHighestScoreSearchMessageComposer = 431;
        public const int MyFriendsRoomsSearchMessageComposer = 432;
        public const int RoomsWhereMyFriendsAreSearchMessageComposer = 433;
        public const int MyRoomsSearchMessageComposer = 434;
        public const int MyFavouriteRoomsSearchMessageComposer = 435;
        public const int MyRoomHistorySearchMessageComposer = 436;
        public const int RoomTextSearchMessageComposer = 437;
        public const int RoomTagSearchMessageComposer = 438;
        public const int LatestEventsSearchMessageComposer = 439;
    }

    public static class RoomSettings
    {
        public const int DeleteRoomMessageComposer = 23;
        public const int GetRoomSettingsMessageComposer = 400;
        public const int SaveRoomSettingsMessageComposer = 401;
    }

    public static class RoomEngine
    {
        public const int ApplyRoomEffectMessageComposer = 66;
        public const int PickupObjectMessageComposer = 67;
        public const int MoveObjectMessageComposer = 73;
        public const int SetObjectDataMessageComposer = 74;
        public const int MoveAvatarMessageComposer = 75;
        public const int GetItemDataMessageComposer = 83;
        public const int SetItemDataMessageComposer = 84;
        public const int RemoveItemMessageComposer = 85;
        public const int PlaceObjectMessageComposer = 90;
        public const int GetFurnitureAliasesMessageComposer = 215;
        public const int GetRoomEntryDataMessageComposer = 390;
        public const int UseFurnitureMessageComposer = 392;
        public const int UseWallItemMessageComposer = 393;
        public const int PlacePetMessageComposer = 3002;
        public const int RemovePetFromFlatMessageComposer = 3003;
        public const int GetPetCommandsMessageComposer = 3004;
    }

    public static class RoomSession
    {
        public const int OpenConnectionMessageComposer = 2;
        public const int QuitMessageComposer = 53;
        public const int GoToFlatMessageComposer = 59;
        public const int ChangeQueueMessageComposer = 211;
        public const int OpenFlatConnectionMessageComposer = 391;
    }

    public static class RoomChat
    {
        public const int ChatMessageComposer = 52;
        public const int ShoutMessageComposer = 55;
        public const int WhisperMessageComposer = 56;
        public const int StartTypingMessageComposer = 317;
        public const int CancelTypingMessageComposer = 318;
    }

    public static class RoomAvatar
    {
        public const int LookToMessageComposer = 79;
        public const int DanceMessageComposer = 93;
        public const int WaveMessageComposer = 94;
        public const int SignMessageComposer = 104;
    }

    public static class Users
    {
        public const int ScrGetUserInfoMessageComposer = 26;
        public const int ApproveNameMessageComposer = 42;
        public const int GetSelectedBadgesMessageComposer = 159;
        public const int GetHabboGroupBadgesMessageComposer = 230;
        public const int GetHabboGroupDetailsMessageComposer = 231;
        public const int GetUserTagsMessageComposer = 263;
        public const int IgnoreUserMessageComposer = 319;
        public const int GetIgnoredUsersMessageComposer = 321;
        public const int UnignoreUserMessageComposer = 322;
        public const int RespectUserMessageComposer = 371;
    }

    public static class RoomFurniture
    {
        public const int ThrowDiceMessageComposer = 76;
        public const int DiceOffMessageComposer = 77;
        public const int PresentOpenMessageComposer = 78;
        public const int CreditFurniRedeemMessageComposer = 183;
        public const int EnterOneWayDoorMessageComposer = 232;
        public const int SpinWheelOfFortuneMessageComposer = 247;
        public const int SetRandomStateMessageComposer = 314;
        public const int RoomDimmerGetPresetsMessageComposer = 341;
        public const int RoomDimmerSavePresetMessageComposer = 342;
        public const int RoomDimmerChangeStateMessageComposer = 343;
    }

    public static class RoomPets
    {
        public const int GetPetInfoMessageComposer = 3001;
        public const int RespectPetMessageComposer = 3005;
    }

    public static class RoomAction
    {
        public const int KickUserMessageComposer = 95;
        public const int AssignRightsMessageComposer = 96;
        public const int RemoveRightsMessageComposer = 97;
        public const int LetUserInMessageComposer = 98;
        public const int RemoveAllRightsMessageComposer = 155;
        public const int BanUserMessageComposer = 320;
        public const int CallGuideBotMessageComposer = 440;
        public const int KickBotMessageComposer = 441;
    }

    public static class Moderator
    {
        public const int ModeratorActionMessageComposer = 200;
        public const int PickIssuesMessageComposer = 450;
        public const int ReleaseIssuesMessageComposer = 451;
        public const int CloseIssuesMessageComposer = 452;
        public const int GetModeratorUserInfoMessageComposer = 454;
        public const int GetUserChatlogMessageComposer = 455;
        public const int GetRoomChatlogMessageComposer = 456;
        public const int GetCfhChatlogMessageComposer = 457;
        public const int GetRoomVisitsMessageComposer = 458;
        public const int GetModeratorRoomInfoMessageComposer = 459;
        public const int ModerateRoomMessageComposer = 460;
        public const int ModAlertMessageComposer = 461;
        public const int ModMessageMessageComposer = 462;
        public const int ModKickMessageComposer = 463;
        public const int ModBanMessageComposer = 464;
    }

    public static class Help
    {
        public const int GetPendingCallsForHelpMessageComposer = 237;
        public const int DeletePendingCallsForHelpMessageComposer = 238;
        public const int GetClientFaqsMessageComposer = 416;
        public const int GetFaqCategoriesMessageComposer = 417;
        public const int GetFaqTextMessageComposer = 418;
        public const int SearchFaqsMessageComposer = 419;
        public const int GetFaqCategoryMessageComposer = 420;
        public const int CallForHelpMessageComposer = 453;
    }

    public static class Catalog
    {
        public const int PurchaseFromCatalogComposer = 100;
        public const int GetCatalogIndexComposer = 101;
        public const int GetCatalogPageComposer = 102;
        public const int RedeemVoucherMessageComposer = 129;
        public const int PurchaseFromCatalogAsGiftComposer = 472;
        public const int GetGiftWrappingConfigurationComposer = 473;
        public const int GetClubGiftInfo = 474;
        public const int SelectClubGiftComposer = 475;
        public const int GetIsOfferGiftableComposer = 3030;
    }

    public static class Recycler
    {
        public const int GetRecyclerPrizesMessageComposer = 412;
        public const int GetRecyclerStatusMessageComposer = 413;
        public const int RecycleItemsMessageComposer = 414;
    }

    public static class Advertisement
    {
        public const int GetRoomAdMessageComposer = 126;
        public const int GetInterstitialMessageComposer = 182;
    }

    public static class Tracking
    {
        public const int LatencyPingRequestMessageComposer = 315;
        public const int LatencyPingReportMessageComposer = 316;
        public const int PerformanceLogMessageComposer = 421;
        public const int LagWarningReportMessageComposer = 422;
    }

    public static class Poll
    {
        public const int VoteAnswerMessageComposer = 112;
        public const int PollStartComposer = 234;
        public const int PollRejectComposer = 235;
        public const int PollAnswerComposer = 236;
    }

    public static class RoomPublicRoom
    {
        public const int ExitLockerRoomMessageComposer = 108;
        public const int ChangeRoomMessageComposer = 111;
        public const int TryBusMessageComposer = 113;
    }

    public static class Sound
    {
        public const int GetSongInfoMessageComposer = 221;
        public const int GetSoundSettingsComposer = 228;
        public const int SetSoundSettingsComposer = 229;
        public const int GetSoundMachinePlayListMessageComposer = 245;
    }

    public static class Avatar
    {
        public const int GetWardrobeMessageComposer = 375;
        public const int SaveWardrobeOutfitMessageComposer = 376;
        public const int ChangeUserNameMessageComposer = 470;
        public const int CheckUserNameMessageComposer = 471;
    }

    public static class Register
    {
        public const int UpdateFigureDataMessageComposer = 44;
    }

    public static class Idol
    {
        public const int StartPerformanceMessageComposer = 410;
        public const int VotePerformanceMessageComposer = 411;
    }
}

public static class OutgoingOpcodes
{
    public static class Handshake
    {
        public const int SecretKeyEvent = 1;
        public const int UserRightsMessageEvent = 2;
        public const int AuthenticationOKMessageEvent = 3;
        public const int UserObjectEvent = 5;
        public const int GenericErrorEvent = 33;
        public const int PingMessageEvent = 50;
        public const int SessionParamsMessageEvent = 257;
        public const int InitCryptoMessageEvent = 277;
        public const int DisconnectReasonEvent = 287;
        public const int UniqueMachineIDEvent = 439;
    }

    public static class Availability
    {
        public const int AvailabilityStatusMessageEvent = 290;
        public const int InfoHotelClosingMessageEvent = 291;
        public const int InfoHotelClosedMessageEvent = 292;
        public const int AvailabilityTimeMessageEvent = 293;
        public const int LoginFailedHotelClosedMessageEvent = 294;
    }

    public static class FriendList
    {
        public const int MessengerInitEvent = 12;
        public const int FriendListUpdateEvent = 13;
        public const int NewBuddyRequestEvent = 132;
        public const int NewConsoleMessageEvent = 134;
        public const int RoomInviteEvent = 135;
        public const int MessengerErrorEvent = 260;
        public const int InstantMessageErrorEvent = 261;
        public const int RoomInviteErrorEvent = 262;
        public const int BuddyRequestsEvent = 314;
        public const int AcceptBuddyResultEvent = 315;
        public const int FollowFriendFailedEvent = 349;
        public const int HabboSearchResultEvent = 435;
    }

    public static class InventoryPurse
    {
        public const int CreditBalanceEvent = 6;
    }

    public static class InventoryFurni
    {
        public const int FurniListInsertEvent = 98;
        public const int FurniListRemoveEvent = 99;
        public const int FurniListUpdateEvent = 101;
        public const int FurniListEvent = 140;
        public const int PostItPlacedEvent = 145;
    }

    public static class InventoryAvatarEffect
    {
        public const int AvatarEffectsMessageEvent = 460;
        public const int AvatarEffectAddedMessageEvent = 461;
        public const int AvatarEffectActivatedMessageEvent = 462;
        public const int AvatarEffectExpiredMessageEvent = 463;
        public const int AvatarEffectSelectedMessageEvent = 464;
    }

    public static class InventoryBadges
    {
        public const int BadgesEvent = 229;
    }

    public static class InventoryAchievements
    {
        public const int AchievementsEvent = 436;
    }

    public static class InventoryTrading
    {
        public const int TradingYouAreNotAllowedEvent = 102;
        public const int TradingOtherNotAllowedEvent = 103;
        public const int TradingOpenEvent = 104;
        public const int TradingAlreadyOpenEvent = 105;
        public const int TradingNotOpenEvent = 106;
        public const int TradingNoSuchItemEvent = 107;
        public const int TradingItemListEvent = 108;
        public const int TradingAcceptEvent = 109;
        public const int TradingCloseEvent = 110;
        public const int TradingConfirmationEvent = 111;
        public const int TradingCompletedEvent = 112;
    }

    public static class InventoryPets
    {
        public const int PetInventoryEvent = 600;
        public const int PetAddedToInventoryEvent = 603;
        public const int PetRemovedFromInventoryEvent = 604;
        public const int PetReceivedMessageEvent = 607;
    }

    public static class Marketplace
    {
        public const int MarketplaceMakeOfferResult = 610;
        public const int MarketplaceCanMakeOfferResult = 611;
        public const int MarketplaceConfigurationEvent = 612;
        public const int MarketplaceBuyOfferResultEvent = 613;
        public const int MarketplaceCancelOfferResultEvent = 614;
        public const int MarketPlaceOffersEvent = 615;
        public const int MarketPlaceOwnOffersEvent = 616;
        public const int MarketplaceItemStatsEvent = 617;
    }

    public static class Navigator
    {
        public const int FlatCreatedEvent = 59;
        public const int DoorbellMessageEvent = 91;
        public const int FlatAccessDeniedMessageEvent = 131;
        public const int UserFlatCatsEvent = 221;
        public const int FlatCatEvent = 222;
        public const int RoomRatingEvent = 345;
        public const int CanCreateRoomEventEvent = 367;
        public const int RoomEventEvent = 370;
        public const int OfficialRoomsEvent = 450;
        public const int GuestRoomSearchResultEvent = 451;
        public const int PopularRoomTagsResultEvent = 452;
        public const int PublicSpaceCastLibsEvent = 453;
        public const int GetGuestRoomResultEvent = 454;
        public const int NavigatorSettingsEvent = 455;
        public const int RoomInfoUpdatedEvent = 456;
        public const int RoomThumbnailUpdateResultEvent = 457;
        public const int FavouritesEvent = 458;
        public const int FavouriteChangedEvent = 459;
        public const int CanCreateRoomEvent = 512;
    }

    public static class RoomSettings
    {
        public const int NoSuchFlatEvent = 44;
        public const int RoomSettingsDataEvent = 465;
        public const int RoomSettingsErrorEvent = 466;
        public const int RoomSettingsSavedEvent = 467;
        public const int RoomSettingsSaveErrorEvent = 468;
        public const int FlatControllerAddedEvent = 510;
        public const int FlatControllerRemovedEvent = 511;
    }

    public static class RoomSession
    {
        public const int CloseConnectionMessageEvent = 18;
        public const int OpenConnectionMessageEvent = 19;
        public const int FlatAccessibleMessageEvent = 41;
        public const int RoomReadyMessageEvent = 69;
        public const int CantConnectMessageEvent = 224;
        public const int YouAreSpectatorMessageEvent = 254;
        public const int RoomQueueStatusMessageEvent = 259;
        public const int RoomForwardMessageEvent = 286;
    }

    public static class RoomChat
    {
        public const int ChatMessageEvent = 24;
        public const int WhisperMessageEvent = 25;
        public const int ShoutMessageEvent = 26;
        public const int UserTypingMessageEvent = 361;
    }

    public static class RoomEngine
    {
        public const int UsersMessageEvent = 28;
        public const int UserRemoveMessageEvent = 29;
        public const int PublicRoomObjectsMessageEvent = 30;
        public const int HeightMapMessageEvent = 31;
        public const int ObjectsMessageEvent = 32;
        public const int UserUpdateMessageEvent = 34;
        public const int ItemsMessageEvent = 45;
        public const int RoomPropertyMessageEvent = 46;
        public const int ItemDataUpdateMessageEvent = 48;
        public const int ItemAddMessageEvent = 83;
        public const int ItemRemoveMessageEvent = 84;
        public const int ItemUpdateMessageEvent = 85;
        public const int ObjectDataUpdateMessageEvent = 88;
        public const int ObjectAddMessageEvent = 93;
        public const int ObjectRemoveMessageEvent = 94;
        public const int ObjectUpdateMessageEvent = 95;
        public const int HeightMapUpdateMessageEvent = 219;
        public const int SlideObjectBundleMessageEvent = 230;
        public const int UserChangeMessageEvent = 266;
        public const int FurnitureAliasesMessageEvent = 297;
        public const int FloorHeightMapMessageEvent = 470;
        public const int RoomEntryInfoMessageEvent = 471;
        public const int PlaceObjectErrorMessageEvent = 516;
    }

    public static class RoomPets
    {
        public const int PetInfoMessageEvent_210 = 210;
        public const int PetInfoMessageEvent_601 = 601;
        public const int PetCommandsMessageEvent = 605;
        public const int PetPlacingErrorEvent = 608;
        public const int PetExperienceEvent = 609;
        public const int PetRespectFailedEvent = 621;
    }

    public static class RoomFurniture
    {
        public const int DoorOtherEndDeletedMessageEvent = 63;
        public const int DoorNotInstalledMessageEvent = 64;
        public const int DiceValueMessageEvent = 90;
        public const int PresentOpenedMessageEvent = 129;
        public const int OneWayDoorStatusMessageEvent = 312;
        public const int RoomDimmerPresetsMessageEvent = 365;
    }

    public static class Users
    {
        public const int ScrSendUserInfoEvent = 7;
        public const int ApproveNameMessageEvent = 36;
        public const int HabboUserBadgesMessageEvent = 228;
        public const int HabboGroupBadgesMessageEvent = 309;
        public const int HabboGroupDetailsMessageEvent = 311;
        public const int UserTagsMessageEvent = 350;
        public const int IgnoreResultMessageEvent = 419;
        public const int IgnoredUsersMessageEvent = 420;
        public const int RespectNotificationMessageEvent = 440;
        public const int UserNameChangedMessageEvent = 572;
        public const int PetRespectNotificationEvent = 606;
    }

    public static class RoomPermissions
    {
        public const int YouAreControllerMessageEvent = 42;
        public const int YouAreNotControllerMessageEvent = 43;
        public const int YouAreOwnerMessageEvent = 47;
    }

    public static class RoomAction
    {
        public const int DanceMessageEvent = 480;
        public const int WaveMessageEvent = 481;
        public const int CarryObjectMessageEvent = 482;
        public const int AvatarEffectMessageEvent = 485;
        public const int SleepMessageEvent = 486;
        public const int UseObjectMessageEvent = 488;
    }

    public static class Moderation
    {
        public const int UserBannedMessageEvent = 35;
        public const int ModMessageEvent = 161;
        public const int IssueDeletedMessageEvent = 273;
        public const int IssueInfoMessageEvent = 530;
        public const int ModeratorInitMessageEvent = 531;
        public const int IssuePickFailedMessageEvent = 532;
        public const int ModeratorUserInfoEvent = 533;
        public const int CfhChatlogEvent = 534;
        public const int RoomChatlogEvent = 535;
        public const int UserChatlogEvent = 536;
        public const int RoomVisitsEvent = 537;
        public const int ModeratorRoomInfoEvent = 538;
        public const int ModeratorActionResultMessageEvent = 539;
    }

    public static class Help
    {
        public const int CallForHelpReplyMessageEvent = 274;
        public const int CallForHelpPendingCallsMessageEvent = 319;
        public const int CallForHelpPendingCallsDeletedMessageEvent = 320;
        public const int CallForHelpResultMessageEvent = 321;
        public const int FaqClientFaqsMessageEvent = 518;
        public const int FaqCategoriesMessageEvent = 519;
        public const int FaqTextMessageEvent = 520;
        public const int FaqSearchResultsMessageEvent = 521;
        public const int FaqCategoryMessageEvent = 522;
        public const int IssueCloseNotificationMessageEvent = 540;
        public const int HotelMergeNameChangeEvent = 573;
        public const int TutorialStatusMessageEvent = 575;
    }

    public static class Error
    {
        public const int ErrorReportEvent = 299;
    }

    public static class Notifications
    {
        public const int HabboBroadcastMessageEvent = 139;
        public const int ClubGiftNotificationEvent = 280;
        public const int HabboAchievementNotificationMessageEvent = 437;
        public const int HabboActivityPointNotificationMessageEvent = 438;
        public const int InfoFeedEnableMessageEvent = 517;
        public const int PetLevelNotificationEvent = 602;
    }

    public static class Catalog
    {
        public const int PurchaseErrorMessageEvent = 65;
        public const int PurchaseOKMessageEvent = 67;
        public const int NotEnoughBalanceMessageEvent = 68;
        public const int GiftReceiverNotFoundEvent = 76;
        public const int CatalogIndexMessageEvent = 126;
        public const int CatalogPageMessageEvent = 127;
        public const int VoucherRedeemOkMessageEvent = 212;
        public const int VoucherRedeemErrorMessageEvent = 213;
        public const int PurchaseNotAllowedMessageEvent = 296;
        public const int CatalogPublishedMessageEvent = 441;
        public const int GiftWrappingConfigurationEvent = 620;
        public const int IsOfferGiftableMessageEvent = 622;
        public const int ClubGiftInfoEvent = 623;
        public const int ClubGiftSelectedEvent = 624;
    }

    public static class Recycler
    {
        public const int RecyclerPrizesMessageEvent = 506;
        public const int RecyclerStatusMessageEvent = 507;
        public const int RecyclerFinishedMessageEvent = 508;
    }

    public static class Advertisement
    {
        public const int RoomAdMessageEvent = 208;
        public const int InterstitialMessageEvent = 258;
    }

    public static class Tracking
    {
        public const int LatencyPingResponseMessageEvent = 354;
    }

    public static class Poll
    {
        public const int VoteQuestionMessageEvent = 79;
        public const int VoteResultMessageEvent = 80;
        public const int PollOfferEvent = 316;
        public const int PollContentsEvent = 317;
        public const int PollErrorEvent = 318;
    }

    public static class RoomPublicRoom
    {
        public const int ParkBusCannotEnterMessageEvent = 81;
        public const int OpenLockerRoomMessageEvent = 96;
        public const int ParkBusDoorMessageEvent = 503;
    }

    public static class Sound
    {
        public const int TraxSongInfoMessageEvent = 300;
        public const int SoundSettingsEvent = 308;
        public const int PlayListMessageEvent = 323;
        public const int PlayListSongAddedMessageEvent = 335;
    }

    public static class Avatar
    {
        public const int WardrobeMessageEvent = 267;
        public const int ChangeUserNameResultMessageEvent = 570;
        public const int CheckUserNameResultMessageEvent = 571;
    }

    public static class Idol
    {
        public const int JudgeGuiStatusMessageEvent = 490;
        public const int OpenPerformerGuiMessageEvent = 491;
        public const int ClosePerformerGuiMessageEvent = 492;
    }
}

public static class OpcodeNames
{
    private const string Unknown = "Unknown";

    private static readonly Dictionary<int, string> IncomingNames = Read(typeof(IncomingOpcodes));
    private static readonly Dictionary<int, string> OutgoingNames = Read(typeof(OutgoingOpcodes));

    public static string Incoming(int id) => IncomingNames.GetValueOrDefault(id, Unknown);

    public static string Outgoing(int id) => OutgoingNames.GetValueOrDefault(id, Unknown);

    private static Dictionary<int, string> Read(Type opcodes) => opcodes.GetNestedTypes()
        .SelectMany(group => group.GetFields(BindingFlags.Public | BindingFlags.Static))
        .Where(field => field.IsLiteral && field.FieldType == typeof(int))
        .GroupBy(field => (int)field.GetRawConstantValue()!)
        .ToDictionary(sameId => sameId.Key, sameId => string.Join("/", sameId.Select(field => field.Name)));
}
