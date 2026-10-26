using DragonEmu.Game.Rooms;
using DragonEmu.Protocol;
using DragonEmu.Protocol.Messages;
using DragonEmu.Protocol.Messages.Incoming.Advertisement;
using DragonEmu.Protocol.Messages.Incoming.Polls;
using DragonEmu.Protocol.Messages.Incoming.Avatar;
using DragonEmu.Protocol.Messages.Incoming.Catalog;
using DragonEmu.Protocol.Messages.Incoming.FriendList;
using DragonEmu.Protocol.Messages.Incoming.Handshake;
using DragonEmu.Protocol.Messages.Incoming.Help;
using DragonEmu.Protocol.Messages.Incoming.InventoryAchievements;
using DragonEmu.Protocol.Messages.Incoming.InventoryAvatarEffect;
using DragonEmu.Protocol.Messages.Incoming.InventoryBadges;
using DragonEmu.Protocol.Messages.Incoming.InventoryFurni;
using DragonEmu.Protocol.Messages.Incoming.InventoryPurse;
using DragonEmu.Protocol.Messages.Incoming.InventoryTrading;
using DragonEmu.Protocol.Messages.Incoming.Marketplace;
using DragonEmu.Protocol.Messages.Incoming.Moderator;
using DragonEmu.Protocol.Messages.Incoming.Navigator;
using DragonEmu.Protocol.Messages.Incoming.Pets;
using DragonEmu.Protocol.Messages.Incoming.Recycler;
using DragonEmu.Protocol.Messages.Incoming.Register;
using DragonEmu.Protocol.Messages.Incoming.RoomAction;
using DragonEmu.Protocol.Messages.Incoming.RoomAvatar;
using DragonEmu.Protocol.Messages.Incoming.RoomChat;
using DragonEmu.Protocol.Messages.Incoming.RoomEngine;
using DragonEmu.Protocol.Messages.Incoming.RoomFurniture;
using DragonEmu.Protocol.Messages.Incoming.RoomPublicRoom;
using DragonEmu.Protocol.Messages.Incoming.RoomSession;
using DragonEmu.Protocol.Messages.Incoming.RoomSettings;
using DragonEmu.Protocol.Messages.Incoming.Sound;
using DragonEmu.Protocol.Messages.Incoming.Users;
using Microsoft.Extensions.DependencyInjection;
using DragonEmu.Domain;

namespace DragonEmu.Gateway;

public sealed class GameMessageDispatcher : IPacketDispatcher
{
    private static readonly Dictionary<int, Func<IServiceProvider, IMessageComposer>> Factories = new()
    {
        [IncomingOpcodes.Handshake.InitCryptoMessageComposer] = _ => new InitCryptoMessageComposer(),
        [IncomingOpcodes.Handshake.PongMessageComposer] = _ => new PongMessageComposer(),
        [IncomingOpcodes.Handshake.DisconnectMessageComposer] = _ => new DisconnectMessageComposer(),
        [IncomingOpcodes.Handshake.TryLoginMessageComposer] = _ => new TryLoginMessageComposer(),
        [IncomingOpcodes.Handshake.UniqueIDMessageComposer] = _ => new UniqueIDMessageComposer(),
        [IncomingOpcodes.Handshake.VersionCheckMessageComposer] = _ => new VersionCheckMessageComposer(),
        [IncomingOpcodes.Handshake.GetSessionParametersMessageComposer] = _ => new GetSessionParametersMessageComposer(),
        [IncomingOpcodes.Handshake.GenerateSecretKeyMessageComposer] = _ => new GenerateSecretKeyMessageComposer(),
        [IncomingOpcodes.Handshake.SSOTicketMessageComposer] = sp => ActivatorUtilities.CreateInstance<SSOTicketMessageComposer>(sp),
        [IncomingOpcodes.Handshake.InfoRetrieveMessageComposer] = sp => ActivatorUtilities.CreateInstance<InfoRetrieveMessageComposer>(sp),
        [IncomingOpcodes.FriendList.MessengerInitMessageComposer] = sp => ActivatorUtilities.CreateInstance<MessengerInitMessageComposer>(sp),
        [IncomingOpcodes.FriendList.SendMsgMessageComposer] = sp => ActivatorUtilities.CreateInstance<SendMsgMessageComposer>(sp),
        [IncomingOpcodes.FriendList.SendRoomInviteMessageComposer] = sp => ActivatorUtilities.CreateInstance<SendRoomInviteMessageComposer>(sp),
        [IncomingOpcodes.FriendList.AcceptBuddyMessageComposer] = sp => ActivatorUtilities.CreateInstance<AcceptBuddyMessageComposer>(sp),
        [IncomingOpcodes.FriendList.DeclineBuddyMessageComposer] = sp => ActivatorUtilities.CreateInstance<DeclineBuddyMessageComposer>(sp),
        [IncomingOpcodes.FriendList.RequestBuddyMessageComposer] = sp => ActivatorUtilities.CreateInstance<RequestBuddyMessageComposer>(sp),
        [IncomingOpcodes.FriendList.RemoveBuddyMessageComposer] = sp => ActivatorUtilities.CreateInstance<RemoveBuddyMessageComposer>(sp),
        [IncomingOpcodes.FriendList.HabboSearchMessageComposer] = sp => ActivatorUtilities.CreateInstance<HabboSearchMessageComposer>(sp),
        [IncomingOpcodes.FriendList.GetBuddyRequestsMessageComposer] = sp => ActivatorUtilities.CreateInstance<GetBuddyRequestsMessageComposer>(sp),
        [IncomingOpcodes.FriendList.FollowFriendMessageComposer] = sp => ActivatorUtilities.CreateInstance<FollowFriendMessageComposer>(sp),
        [IncomingOpcodes.FriendList.FriendListUpdateMessageComposer] = _ => new FriendListUpdateMessageComposer(),
        [IncomingOpcodes.InventoryPurse.GetCreditsInfoComposer] = sp => ActivatorUtilities.CreateInstance<GetCreditsInfoComposer>(sp),
        [IncomingOpcodes.InventoryFurni.RequestFurniInventoryComposer] = sp => ActivatorUtilities.CreateInstance<RequestFurniInventoryComposer>(sp),
        [IncomingOpcodes.InventoryTrading.OpenTradingComposer] = sp => ActivatorUtilities.CreateInstance<OpenTradingComposer>(sp),
        [IncomingOpcodes.InventoryTrading.AddItemToTradeComposer] = sp => ActivatorUtilities.CreateInstance<AddItemToTradeComposer>(sp),
        [IncomingOpcodes.InventoryTrading.RemoveItemFromTradeComposer] = sp => ActivatorUtilities.CreateInstance<RemoveItemFromTradeComposer>(sp),
        [IncomingOpcodes.InventoryTrading.AcceptTradingComposer] = sp => ActivatorUtilities.CreateInstance<AcceptTradingComposer>(sp),
        [IncomingOpcodes.InventoryTrading.UnacceptTradingComposer] = sp => ActivatorUtilities.CreateInstance<UnacceptTradingComposer>(sp),
        [IncomingOpcodes.InventoryTrading.ConfirmAcceptTradingComposer] = sp => ActivatorUtilities.CreateInstance<ConfirmAcceptTradingComposer>(sp),
        [IncomingOpcodes.InventoryTrading.ConfirmDeclineTradingComposer] = sp => ActivatorUtilities.CreateInstance<ConfirmDeclineTradingComposer>(sp),
        [IncomingOpcodes.InventoryTrading.CloseTradingComposer] = sp => ActivatorUtilities.CreateInstance<CloseTradingComposer>(sp),
        [IncomingOpcodes.Users.ScrGetUserInfoMessageComposer] = sp => ActivatorUtilities.CreateInstance<ScrGetUserInfoMessageComposer>(sp),
        [IncomingOpcodes.Users.GetHabboGroupBadgesMessageComposer] = sp => ActivatorUtilities.CreateInstance<GetHabboGroupBadgesMessageComposer>(sp),
        [IncomingOpcodes.Users.GetHabboGroupDetailsMessageComposer] = sp => ActivatorUtilities.CreateInstance<GetHabboGroupDetailsMessageComposer>(sp),
        [IncomingOpcodes.Users.GetSelectedBadgesMessageComposer] = sp => ActivatorUtilities.CreateInstance<GetSelectedBadgesMessageComposer>(sp),
        [IncomingOpcodes.Users.GetUserTagsMessageComposer] = sp => ActivatorUtilities.CreateInstance<GetUserTagsMessageComposer>(sp),
        [IncomingOpcodes.Users.IgnoreUserMessageComposer] = sp => ActivatorUtilities.CreateInstance<IgnoreUserMessageComposer>(sp),
        [IncomingOpcodes.Users.UnignoreUserMessageComposer] = sp => ActivatorUtilities.CreateInstance<UnignoreUserMessageComposer>(sp),
        [IncomingOpcodes.Users.GetIgnoredUsersMessageComposer] = sp => ActivatorUtilities.CreateInstance<GetIgnoredUsersMessageComposer>(sp),
        [IncomingOpcodes.InventoryAchievements.GetAchievementsComposer] = sp => ActivatorUtilities.CreateInstance<GetAchievementsComposer>(sp),
        [IncomingOpcodes.InventoryBadges.GetBadgesComposer] = sp => ActivatorUtilities.CreateInstance<GetBadgesComposer>(sp),
        [IncomingOpcodes.InventoryBadges.SetActivatedBadgesComposer] = sp => ActivatorUtilities.CreateInstance<SetActivatedBadgesComposer>(sp),
        [IncomingOpcodes.InventoryAvatarEffect.AvatarEffectActivatedComposer] = sp => ActivatorUtilities.CreateInstance<AvatarEffectActivatedComposer>(sp),
        [IncomingOpcodes.InventoryAvatarEffect.AvatarEffectSelectedComposer] = sp => ActivatorUtilities.CreateInstance<AvatarEffectSelectedComposer>(sp),
        [IncomingOpcodes.RoomAction.CallGuideBotMessageComposer] = sp => ActivatorUtilities.CreateInstance<CallGuideBotMessageComposer>(sp),
        [IncomingOpcodes.RoomAction.KickBotMessageComposer] = sp => ActivatorUtilities.CreateInstance<KickBotMessageComposer>(sp),
        [IncomingOpcodes.Poll.VoteAnswerMessageComposer] = sp => ActivatorUtilities.CreateInstance<VoteAnswerMessageComposer>(sp),
        [IncomingOpcodes.Poll.PollStartComposer] = sp => ActivatorUtilities.CreateInstance<PollStartComposer>(sp),
        [IncomingOpcodes.Poll.PollRejectComposer] = sp => ActivatorUtilities.CreateInstance<PollRejectComposer>(sp),
        [IncomingOpcodes.Poll.PollAnswerComposer] = sp => ActivatorUtilities.CreateInstance<PollAnswerComposer>(sp),
        [IncomingOpcodes.Advertisement.GetInterstitialMessageComposer] = sp => ActivatorUtilities.CreateInstance<GetInterstitialMessageComposer>(sp),
        [IncomingOpcodes.Advertisement.GetRoomAdMessageComposer] = sp => ActivatorUtilities.CreateInstance<GetRoomAdMessageComposer>(sp),
        [IncomingOpcodes.Avatar.GetWardrobeMessageComposer] = sp => ActivatorUtilities.CreateInstance<GetWardrobeMessageComposer>(sp),
        [IncomingOpcodes.Avatar.CheckUserNameMessageComposer] = sp => ActivatorUtilities.CreateInstance<CheckUserNameMessageComposer>(sp),
        [IncomingOpcodes.Avatar.ChangeUserNameMessageComposer] = sp => ActivatorUtilities.CreateInstance<ChangeUserNameMessageComposer>(sp),
        [IncomingOpcodes.Avatar.SaveWardrobeOutfitMessageComposer] = sp => ActivatorUtilities.CreateInstance<SaveWardrobeOutfitMessageComposer>(sp),
        [IncomingOpcodes.Recycler.GetRecyclerPrizesMessageComposer] = sp => ActivatorUtilities.CreateInstance<GetRecyclerPrizesMessageComposer>(sp),
        [IncomingOpcodes.Recycler.GetRecyclerStatusMessageComposer] = sp => ActivatorUtilities.CreateInstance<GetRecyclerStatusMessageComposer>(sp),
        [IncomingOpcodes.Recycler.RecycleItemsMessageComposer] = sp => ActivatorUtilities.CreateInstance<RecycleItemsMessageComposer>(sp),
        [IncomingOpcodes.Navigator.GetUserFlatCatsMessageComposer] = sp => ActivatorUtilities.CreateInstance<GetUserFlatCatsMessageComposer>(sp),
        [IncomingOpcodes.Navigator.MyRoomsSearchMessageComposer] = sp => ActivatorUtilities.CreateInstance<MyRoomsSearchMessageComposer>(sp),
        [IncomingOpcodes.Navigator.CanCreateRoomMessageComposer] = sp => ActivatorUtilities.CreateInstance<CanCreateRoomMessageComposer>(sp),
        [IncomingOpcodes.Navigator.CanCreateEventMessageComposer] = sp => ActivatorUtilities.CreateInstance<CanCreateEventMessageComposer>(sp),
        [IncomingOpcodes.Navigator.CreateEventMessageComposer] = sp => ActivatorUtilities.CreateInstance<CreateEventMessageComposer>(sp),
        [IncomingOpcodes.Navigator.EditEventMessageComposer] = sp => ActivatorUtilities.CreateInstance<EditEventMessageComposer>(sp),
        [IncomingOpcodes.Navigator.CancelEventMessageComposer] = sp => ActivatorUtilities.CreateInstance<CancelEventMessageComposer>(sp),
        [IncomingOpcodes.Navigator.LatestEventsSearchMessageComposer] = sp => ActivatorUtilities.CreateInstance<LatestEventsSearchMessageComposer>(sp),
        [IncomingOpcodes.Navigator.CreateFlatMessageComposer] = sp => ActivatorUtilities.CreateInstance<CreateFlatMessageComposer>(sp),
        [IncomingOpcodes.Navigator.GetOfficialRoomsMessageComposer] = sp => ActivatorUtilities.CreateInstance<GetOfficialRoomsMessageComposer>(sp),
        [IncomingOpcodes.Navigator.RateFlatMessageComposer] = sp => ActivatorUtilities.CreateInstance<RateFlatMessageComposer>(sp),
        [IncomingOpcodes.Navigator.GetPublicSpaceCastLibsMessageComposer] = sp => ActivatorUtilities.CreateInstance<GetPublicSpaceCastLibsMessageComposer>(sp),
        [IncomingOpcodes.Navigator.UpdateNavigatorSettingsMessageComposer] = sp => ActivatorUtilities.CreateInstance<UpdateNavigatorSettingsMessageComposer>(sp),
        [IncomingOpcodes.Navigator.GetGuestRoomMessageComposer] = sp => ActivatorUtilities.CreateInstance<GetGuestRoomMessageComposer>(sp),
        [IncomingOpcodes.Navigator.PopularRoomsSearchMessageComposer] = sp => ActivatorUtilities.CreateInstance<PopularRoomsSearchMessageComposer>(sp),
        [IncomingOpcodes.Navigator.GetPopularRoomTagsMessageComposer] = sp => ActivatorUtilities.CreateInstance<GetPopularRoomTagsMessageComposer>(sp),
        [IncomingOpcodes.Navigator.UpdateRoomThumbnailMessageComposer] = sp => ActivatorUtilities.CreateInstance<UpdateRoomThumbnailMessageComposer>(sp),
        [IncomingOpcodes.Navigator.AddFavouriteRoomMessageComposer] = sp => ActivatorUtilities.CreateInstance<AddFavouriteRoomMessageComposer>(sp),
        [IncomingOpcodes.Navigator.DeleteFavouriteRoomMessageComposer] = sp => ActivatorUtilities.CreateInstance<DeleteFavouriteRoomMessageComposer>(sp),
        [IncomingOpcodes.Navigator.MyFriendsRoomsSearchMessageComposer] = sp => ActivatorUtilities.CreateInstance<MyFriendsRoomsSearchMessageComposer>(sp),
        [IncomingOpcodes.Navigator.RoomsWhereMyFriendsAreSearchMessageComposer] = sp => ActivatorUtilities.CreateInstance<RoomsWhereMyFriendsAreSearchMessageComposer>(sp),
        [IncomingOpcodes.Navigator.MyFavouriteRoomsSearchMessageComposer] = sp => ActivatorUtilities.CreateInstance<MyFavouriteRoomsSearchMessageComposer>(sp),
        [IncomingOpcodes.Navigator.MyRoomHistorySearchMessageComposer] = sp => ActivatorUtilities.CreateInstance<MyRoomHistorySearchMessageComposer>(sp),
        [IncomingOpcodes.Marketplace.MakeOfferMessageComposer] = sp => ActivatorUtilities.CreateInstance<MakeOfferMessageComposer>(sp),
        [IncomingOpcodes.Marketplace.GetMarketplaceConfigurationMessageComposer] = sp => ActivatorUtilities.CreateInstance<GetMarketplaceConfigurationMessageComposer>(sp),
        [IncomingOpcodes.Marketplace.GetMarketplaceCanMakeOfferComposer] = sp => ActivatorUtilities.CreateInstance<GetMarketplaceCanMakeOfferComposer>(sp),
        [IncomingOpcodes.Marketplace.BuyMarketplaceTokensMessageComposer] = sp => ActivatorUtilities.CreateInstance<BuyMarketplaceTokensMessageComposer>(sp),
        [IncomingOpcodes.Marketplace.BuyOfferMessageComposer] = sp => ActivatorUtilities.CreateInstance<BuyOfferMessageComposer>(sp),
        [IncomingOpcodes.Marketplace.CancelOfferMessageComposer] = sp => ActivatorUtilities.CreateInstance<CancelOfferMessageComposer>(sp),
        [IncomingOpcodes.Marketplace.RedeemOfferCreditsMessageComposer] = sp => ActivatorUtilities.CreateInstance<RedeemOfferCreditsMessageComposer>(sp),
        [IncomingOpcodes.Marketplace.GetOffersMessageComposer] = sp => ActivatorUtilities.CreateInstance<GetOffersMessageComposer>(sp),
        [IncomingOpcodes.Marketplace.GetOwnOffersMessageComposer] = sp => ActivatorUtilities.CreateInstance<GetOwnOffersMessageComposer>(sp),
        [IncomingOpcodes.Marketplace.GetMarketplaceItemStatsComposer] = sp => ActivatorUtilities.CreateInstance<GetMarketplaceItemStatsComposer>(sp),
        [IncomingOpcodes.Sound.GetSoundMachinePlayListMessageComposer] = sp => ActivatorUtilities.CreateInstance<GetSoundMachinePlayListMessageComposer>(sp),
        [IncomingOpcodes.Sound.GetSongInfoMessageComposer] = sp => ActivatorUtilities.CreateInstance<GetSongInfoMessageComposer>(sp),
        [IncomingOpcodes.RoomPublicRoom.ExitLockerRoomMessageComposer] = sp => ActivatorUtilities.CreateInstance<ExitLockerRoomMessageComposer>(sp),
        [IncomingOpcodes.Users.ApproveNameMessageComposer] = sp => ActivatorUtilities.CreateInstance<ApproveNameMessageComposer>(sp),
        [IncomingOpcodes.InventoryPets.GetPetInventoryComposer] = sp => ActivatorUtilities.CreateInstance<GetPetInventoryComposer>(sp),
        [IncomingOpcodes.RoomPets.GetPetInfoMessageComposer] = sp => ActivatorUtilities.CreateInstance<GetPetInfoMessageComposer>(sp),
        [IncomingOpcodes.RoomEngine.PlacePetMessageComposer] = sp => ActivatorUtilities.CreateInstance<PlacePetMessageComposer>(sp),
        [IncomingOpcodes.RoomEngine.RemovePetFromFlatMessageComposer] = sp => ActivatorUtilities.CreateInstance<RemovePetFromFlatMessageComposer>(sp),
        [IncomingOpcodes.RoomEngine.GetPetCommandsMessageComposer] = sp => ActivatorUtilities.CreateInstance<GetPetCommandsMessageComposer>(sp),
        [IncomingOpcodes.RoomPets.RespectPetMessageComposer] = sp => ActivatorUtilities.CreateInstance<RespectPetMessageComposer>(sp),
        [IncomingOpcodes.Help.CallForHelpMessageComposer] = sp => ActivatorUtilities.CreateInstance<CallForHelpMessageComposer>(sp),
        [IncomingOpcodes.Help.DeletePendingCallsForHelpMessageComposer] = sp => ActivatorUtilities.CreateInstance<DeletePendingCallsForHelpMessageComposer>(sp),
        [IncomingOpcodes.Help.GetClientFaqsMessageComposer] = sp => ActivatorUtilities.CreateInstance<GetClientFaqsMessageComposer>(sp),
        [IncomingOpcodes.Help.GetFaqCategoriesMessageComposer] = sp => ActivatorUtilities.CreateInstance<GetFaqCategoriesMessageComposer>(sp),
        [IncomingOpcodes.Help.GetFaqTextMessageComposer] = sp => ActivatorUtilities.CreateInstance<GetFaqTextMessageComposer>(sp),
        [IncomingOpcodes.Help.SearchFaqsMessageComposer] = sp => ActivatorUtilities.CreateInstance<SearchFaqsMessageComposer>(sp),
        [IncomingOpcodes.Help.GetFaqCategoryMessageComposer] = sp => ActivatorUtilities.CreateInstance<GetFaqCategoryMessageComposer>(sp),
        [IncomingOpcodes.Moderator.ModeratorActionMessageComposer] = sp => ActivatorUtilities.CreateInstance<ModeratorActionMessageComposer>(sp),
        [IncomingOpcodes.Moderator.PickIssuesMessageComposer] = sp => ActivatorUtilities.CreateInstance<PickIssuesMessageComposer>(sp),
        [IncomingOpcodes.Moderator.ReleaseIssuesMessageComposer] = sp => ActivatorUtilities.CreateInstance<ReleaseIssuesMessageComposer>(sp),
        [IncomingOpcodes.Moderator.CloseIssuesMessageComposer] = sp => ActivatorUtilities.CreateInstance<CloseIssuesMessageComposer>(sp),
        [IncomingOpcodes.Moderator.GetModeratorUserInfoMessageComposer] = sp => ActivatorUtilities.CreateInstance<GetModeratorUserInfoMessageComposer>(sp),
        [IncomingOpcodes.Moderator.GetUserChatlogMessageComposer] = sp => ActivatorUtilities.CreateInstance<GetUserChatlogMessageComposer>(sp),
        [IncomingOpcodes.Moderator.GetRoomChatlogMessageComposer] = sp => ActivatorUtilities.CreateInstance<GetRoomChatlogMessageComposer>(sp),
        [IncomingOpcodes.Moderator.GetCfhChatlogMessageComposer] = sp => ActivatorUtilities.CreateInstance<GetCfhChatlogMessageComposer>(sp),
        [IncomingOpcodes.Moderator.GetRoomVisitsMessageComposer] = sp => ActivatorUtilities.CreateInstance<GetRoomVisitsMessageComposer>(sp),
        [IncomingOpcodes.Moderator.GetModeratorRoomInfoMessageComposer] = sp => ActivatorUtilities.CreateInstance<GetModeratorRoomInfoMessageComposer>(sp),
        [IncomingOpcodes.Moderator.ModerateRoomMessageComposer] = sp => ActivatorUtilities.CreateInstance<ModerateRoomMessageComposer>(sp),
        [IncomingOpcodes.Moderator.ModAlertMessageComposer] = sp => ActivatorUtilities.CreateInstance<ModAlertMessageComposer>(sp),
        [IncomingOpcodes.Moderator.ModMessageMessageComposer] = sp => ActivatorUtilities.CreateInstance<ModMessageMessageComposer>(sp),
        [IncomingOpcodes.Moderator.ModKickMessageComposer] = sp => ActivatorUtilities.CreateInstance<ModKickMessageComposer>(sp),
        [IncomingOpcodes.Moderator.ModBanMessageComposer] = sp => ActivatorUtilities.CreateInstance<ModBanMessageComposer>(sp),
        [IncomingOpcodes.Navigator.RoomsWithHighestScoreSearchMessageComposer] = sp => ActivatorUtilities.CreateInstance<RoomsWithHighestScoreSearchMessageComposer>(sp),
        [IncomingOpcodes.Navigator.RoomTextSearchMessageComposer] = sp => ActivatorUtilities.CreateInstance<RoomTextSearchMessageComposer>(sp),
        [IncomingOpcodes.Navigator.RoomTagSearchMessageComposer] = sp => ActivatorUtilities.CreateInstance<RoomTagSearchMessageComposer>(sp),
        [IncomingOpcodes.Sound.GetSoundSettingsComposer] = sp => ActivatorUtilities.CreateInstance<GetSoundSettingsComposer>(sp),
        [IncomingOpcodes.Sound.SetSoundSettingsComposer] = sp => ActivatorUtilities.CreateInstance<SetSoundSettingsComposer>(sp),
        [IncomingOpcodes.RoomSession.OpenFlatConnectionMessageComposer] = sp => ActivatorUtilities.CreateInstance<OpenFlatConnectionMessageComposer>(sp),
        [IncomingOpcodes.RoomSession.OpenConnectionMessageComposer] = sp => ActivatorUtilities.CreateInstance<OpenConnectionMessageComposer>(sp),
        [IncomingOpcodes.RoomSession.GoToFlatMessageComposer] = sp => ActivatorUtilities.CreateInstance<GoToFlatMessageComposer>(sp),
        [IncomingOpcodes.RoomSession.QuitMessageComposer] = sp => ActivatorUtilities.CreateInstance<QuitRoomMessageComposer>(sp),
        [IncomingOpcodes.RoomEngine.GetFurnitureAliasesMessageComposer] = sp => ActivatorUtilities.CreateInstance<GetFurnitureAliasesMessageComposer>(sp),
        [IncomingOpcodes.RoomEngine.GetRoomEntryDataMessageComposer] = sp => ActivatorUtilities.CreateInstance<GetRoomEntryDataMessageComposer>(sp),
        [IncomingOpcodes.Catalog.PurchaseFromCatalogComposer] = sp => ActivatorUtilities.CreateInstance<PurchaseFromCatalogComposer>(sp),
        [IncomingOpcodes.Catalog.PurchaseFromCatalogAsGiftComposer] = sp => ActivatorUtilities.CreateInstance<PurchaseFromCatalogAsGiftComposer>(sp),
        [IncomingOpcodes.Catalog.RedeemVoucherMessageComposer] = sp => ActivatorUtilities.CreateInstance<RedeemVoucherMessageComposer>(sp),
        [IncomingOpcodes.Catalog.GetClubGiftInfo] = sp => ActivatorUtilities.CreateInstance<GetClubGiftInfoComposer>(sp),
        [IncomingOpcodes.Catalog.SelectClubGiftComposer] = sp => ActivatorUtilities.CreateInstance<SelectClubGiftMessageComposer>(sp),
        [IncomingOpcodes.Catalog.GetCatalogIndexComposer] = sp => ActivatorUtilities.CreateInstance<GetCatalogIndexMessageComposer>(sp),
        [IncomingOpcodes.Catalog.GetCatalogPageComposer] = sp => ActivatorUtilities.CreateInstance<GetCatalogPageMessageComposer>(sp),
        [IncomingOpcodes.Catalog.GetIsOfferGiftableComposer] = sp => ActivatorUtilities.CreateInstance<GetIsOfferGiftableMessageComposer>(sp),
        [IncomingOpcodes.Catalog.GetGiftWrappingConfigurationComposer] = sp => ActivatorUtilities.CreateInstance<GetGiftWrappingConfigurationComposer>(sp),
        [IncomingOpcodes.RoomFurniture.EnterOneWayDoorMessageComposer] = sp => ActivatorUtilities.CreateInstance<EnterOneWayDoorMessageComposer>(sp),
        [IncomingOpcodes.RoomFurniture.PresentOpenMessageComposer] = sp => ActivatorUtilities.CreateInstance<PresentOpenMessageComposer>(sp),
        [IncomingOpcodes.RoomFurniture.CreditFurniRedeemMessageComposer] = sp => ActivatorUtilities.CreateInstance<CreditFurniRedeemMessageComposer>(sp),
        [IncomingOpcodes.RoomFurniture.ThrowDiceMessageComposer] = sp => ActivatorUtilities.CreateInstance<ThrowDiceMessageComposer>(sp),
        [IncomingOpcodes.RoomFurniture.DiceOffMessageComposer] = sp => ActivatorUtilities.CreateInstance<DiceOffMessageComposer>(sp),
        [IncomingOpcodes.RoomFurniture.SpinWheelOfFortuneMessageComposer] = sp => ActivatorUtilities.CreateInstance<SpinWheelOfFortuneMessageComposer>(sp),
        [IncomingOpcodes.RoomFurniture.SetRandomStateMessageComposer] = sp => ActivatorUtilities.CreateInstance<SetRandomStateMessageComposer>(sp),
        [IncomingOpcodes.RoomFurniture.RoomDimmerGetPresetsMessageComposer] = sp => ActivatorUtilities.CreateInstance<RoomDimmerGetPresetsMessageComposer>(sp),
        [IncomingOpcodes.RoomFurniture.RoomDimmerSavePresetMessageComposer] = sp => ActivatorUtilities.CreateInstance<RoomDimmerSavePresetMessageComposer>(sp),
        [IncomingOpcodes.RoomFurniture.RoomDimmerChangeStateMessageComposer] = sp => ActivatorUtilities.CreateInstance<RoomDimmerChangeStateMessageComposer>(sp),
        [IncomingOpcodes.RoomEngine.MoveAvatarMessageComposer] = sp => ActivatorUtilities.CreateInstance<MoveAvatarMessageComposer>(sp),
        [IncomingOpcodes.RoomEngine.PlaceObjectMessageComposer] = sp => ActivatorUtilities.CreateInstance<PlaceObjectMessageComposer>(sp),
        [IncomingOpcodes.RoomEngine.GetItemDataMessageComposer] = sp => ActivatorUtilities.CreateInstance<GetItemDataMessageComposer>(sp),
        [IncomingOpcodes.RoomEngine.SetItemDataMessageComposer] = sp => ActivatorUtilities.CreateInstance<SetItemDataMessageComposer>(sp),
        [IncomingOpcodes.RoomEngine.RemoveItemMessageComposer] = sp => ActivatorUtilities.CreateInstance<RemoveItemMessageComposer>(sp),
        [IncomingOpcodes.RoomEngine.MoveObjectMessageComposer] = sp => ActivatorUtilities.CreateInstance<MoveObjectMessageComposer>(sp),
        [IncomingOpcodes.RoomEngine.PickupObjectMessageComposer] = sp => ActivatorUtilities.CreateInstance<PickupObjectMessageComposer>(sp),
        [IncomingOpcodes.RoomEngine.UseFurnitureMessageComposer] = sp => ActivatorUtilities.CreateInstance<UseFurnitureMessageComposer>(sp),
        [IncomingOpcodes.RoomEngine.UseWallItemMessageComposer] = sp => ActivatorUtilities.CreateInstance<UseWallItemMessageComposer>(sp),
        [IncomingOpcodes.RoomEngine.ApplyRoomEffectMessageComposer] = sp => ActivatorUtilities.CreateInstance<ApplyRoomEffectMessageComposer>(sp),
        [IncomingOpcodes.RoomAction.AssignRightsMessageComposer] = sp => ActivatorUtilities.CreateInstance<AssignRightsMessageComposer>(sp),
        [IncomingOpcodes.RoomAction.RemoveRightsMessageComposer] = sp => ActivatorUtilities.CreateInstance<RemoveRightsMessageComposer>(sp),
        [IncomingOpcodes.RoomAction.RemoveAllRightsMessageComposer] = sp => ActivatorUtilities.CreateInstance<RemoveAllRightsMessageComposer>(sp),
        [IncomingOpcodes.RoomAction.LetUserInMessageComposer] = sp => ActivatorUtilities.CreateInstance<LetUserInMessageComposer>(sp),
        [IncomingOpcodes.RoomAction.KickUserMessageComposer] = sp => ActivatorUtilities.CreateInstance<KickUserMessageComposer>(sp),
        [IncomingOpcodes.RoomAction.BanUserMessageComposer] = sp => ActivatorUtilities.CreateInstance<BanUserMessageComposer>(sp),
        [IncomingOpcodes.RoomSettings.GetRoomSettingsMessageComposer] = sp => ActivatorUtilities.CreateInstance<GetRoomSettingsMessageComposer>(sp),
        [IncomingOpcodes.RoomSettings.SaveRoomSettingsMessageComposer] = sp => ActivatorUtilities.CreateInstance<SaveRoomSettingsMessageComposer>(sp),
        [IncomingOpcodes.RoomSettings.DeleteRoomMessageComposer] = sp => ActivatorUtilities.CreateInstance<DeleteRoomMessageComposer>(sp),
        [IncomingOpcodes.RoomChat.ChatMessageComposer] = sp => ActivatorUtilities.CreateInstance<ChatMessageComposer>(sp),
        [IncomingOpcodes.RoomChat.ShoutMessageComposer] = sp => ActivatorUtilities.CreateInstance<ShoutMessageComposer>(sp),
        [IncomingOpcodes.RoomChat.WhisperMessageComposer] = sp => ActivatorUtilities.CreateInstance<WhisperMessageComposer>(sp),
        [IncomingOpcodes.RoomChat.StartTypingMessageComposer] = sp => ActivatorUtilities.CreateInstance<StartTypingMessageComposer>(sp),
        [IncomingOpcodes.RoomChat.CancelTypingMessageComposer] = sp => ActivatorUtilities.CreateInstance<CancelTypingMessageComposer>(sp),
        [IncomingOpcodes.Users.RespectUserMessageComposer] = sp => ActivatorUtilities.CreateInstance<RespectUserMessageComposer>(sp),
        [IncomingOpcodes.RoomAvatar.DanceMessageComposer] = sp => ActivatorUtilities.CreateInstance<DanceMessageComposer>(sp),
        [IncomingOpcodes.RoomAvatar.WaveMessageComposer] = sp => ActivatorUtilities.CreateInstance<WaveMessageComposer>(sp),
        [IncomingOpcodes.RoomAvatar.SignMessageComposer] = sp => ActivatorUtilities.CreateInstance<SignMessageComposer>(sp),
        [IncomingOpcodes.RoomAvatar.LookToMessageComposer] = sp => ActivatorUtilities.CreateInstance<LookToMessageComposer>(sp),
        [IncomingOpcodes.RoomPublicRoom.TryBusMessageComposer] = sp => ActivatorUtilities.CreateInstance<TryBusMessageComposer>(sp),
        [IncomingOpcodes.RoomPublicRoom.ChangeRoomMessageComposer] = sp => ActivatorUtilities.CreateInstance<ChangeRoomMessageComposer>(sp),
        [IncomingOpcodes.Register.UpdateFigureDataMessageComposer] = sp => ActivatorUtilities.CreateInstance<UpdateFigureDataMessageComposer>(sp),
    };

    private static readonly HashSet<int> BeforeLogin =
    [
        IncomingOpcodes.Handshake.InitCryptoMessageComposer,
        IncomingOpcodes.Handshake.SSOTicketMessageComposer,
        IncomingOpcodes.Handshake.PongMessageComposer,
        IncomingOpcodes.Handshake.DisconnectMessageComposer,
        IncomingOpcodes.Handshake.TryLoginMessageComposer,
        IncomingOpcodes.Handshake.UniqueIDMessageComposer,
        IncomingOpcodes.Handshake.VersionCheckMessageComposer,
        IncomingOpcodes.Handshake.GetSessionParametersMessageComposer,
        IncomingOpcodes.Handshake.GenerateSecretKeyMessageComposer,
    ];

    private static readonly HashSet<int> RoomActivity =
    [
        IncomingOpcodes.RoomEngine.MoveAvatarMessageComposer,
        IncomingOpcodes.RoomEngine.UseFurnitureMessageComposer,
        IncomingOpcodes.RoomEngine.UseWallItemMessageComposer,
        IncomingOpcodes.RoomFurniture.ThrowDiceMessageComposer,
        IncomingOpcodes.RoomFurniture.DiceOffMessageComposer,
        IncomingOpcodes.RoomFurniture.EnterOneWayDoorMessageComposer,
        IncomingOpcodes.RoomFurniture.SpinWheelOfFortuneMessageComposer,
        IncomingOpcodes.RoomChat.ChatMessageComposer,
        IncomingOpcodes.RoomChat.ShoutMessageComposer,
        IncomingOpcodes.RoomChat.WhisperMessageComposer,
    ];

    private readonly IServiceProvider _serviceProvider;
    private readonly RoomIdleService _idle;
    private readonly ILogger<GameMessageDispatcher> _logger;

    public GameMessageDispatcher(IServiceProvider serviceProvider, RoomIdleService idle, ILogger<GameMessageDispatcher> logger)
    {
        _serviceProvider = serviceProvider;
        _idle = idle;
        _logger = logger;
    }

    public static void EnsureEveryHandlerCanBeMade(IServiceProvider serviceProvider)
    {
        var broken = new List<string>();
        foreach ((int opcode, Func<IServiceProvider, IMessageComposer> factory) in Factories)
        {
            try
            {
                factory(serviceProvider);
            }
            catch (InvalidOperationException ex)
            {
                broken.Add($"{opcode}: {ex.Message}");
            }
        }

        if (broken.Count > 0)
        {
            throw new InvalidOperationException($"{broken.Count} message handler(s) can't be made:\n{string.Join("\n", broken)}");
        }
    }

    public async ValueTask DispatchAsync(ClientSession session, ClientPacket packet, CancellationToken cancellationToken)
    {
        bool handled = Factories.TryGetValue(packet.Id, out Func<IServiceProvider, IMessageComposer>? factory);
        _logger.LogIncoming(packet, handled);
        if (!handled || session.IsReplaced || (session.AuthenticatedUser is null && !BeforeLogin.Contains(packet.Id)))
        {
            return;
        }

        if (session.Repeats.IsRepeat(packet.Id, packet.Body, Environment.TickCount64))
        {
            _logger.LogDebug("Session {SessionId}: message {Id} repeated within {Window} ms. Thus it was not handled.",
                session.Id, packet.Id, RepeatFilter.Window.TotalMilliseconds);
            return;
        }

        if (RoomActivity.Contains(packet.Id))
        {
            _idle.MarkActive(session);
        }

        IMessageComposer message = factory!(_serviceProvider);
        message.Parse(packet);
        await message.HandleAsync(session, cancellationToken);
    }
}
