﻿using MixItUp.Base.Model;
using MixItUp.Base.Model.User;
using MixItUp.Base.Services;
using MixItUp.Base.Services.External;
using MixItUp.Base.Util;
using StreamingClient.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Twitch.Base.Models.Clients.Chat;
using Twitch.Base.Models.Clients.PubSub.Messages;
using Twitch.Base.Models.NewAPI.Chat;
using Twitch.Base.Models.NewAPI.Users;
using TwitchNewAPI = Twitch.Base.Models.NewAPI;
using TwitchV5API = Twitch.Base.Models.V5;

namespace MixItUp.Base.ViewModel.User
{
    public enum UserRoleEnum
    {
        Banned,
        User = 10,
        Pro = 20,
        Affiliate = 23,
        Partner = 25,
        Follower = 30,
        Regular = 35,
        Subscriber = 40,
        GlobalMod = 48,
        Mod = 50,
        ChannelEditor = 55,
        Staff = 60,
        Streamer = 70,

        Custom = 99,
    }

    public static class NewAPITwitchUserModelExtensions
    {
        public static bool IsAffiliate(this TwitchNewAPI.Users.UserModel twitchUser)
        {
            return twitchUser.broadcaster_type.Equals("affiliate");
        }

        public static bool IsPartner(this TwitchNewAPI.Users.UserModel twitchUser)
        {
            return twitchUser.broadcaster_type.Equals("partner");
        }

        public static bool IsStaff(this TwitchNewAPI.Users.UserModel twitchUser)
        {
            return twitchUser.type.Equals("staff") || twitchUser.type.Equals("admin");
        }

        public static bool IsGlobalMod(this TwitchNewAPI.Users.UserModel twitchUser)
        {
            return twitchUser.type.Equals("global_mod");
        }
    }

    public class UserViewModel : IEquatable<UserViewModel>, IComparable<UserViewModel>
    {
        public UserDataModel Data { get; private set; }

        public UserViewModel(string username)
        {
            this.SetUserData();

            this.UnassociatedUsername = username;
        }

        public UserViewModel(TwitchNewAPI.Users.UserModel twitchUser)
        {
            this.SetUserData(twitchID: twitchUser.id);

            this.TwitchID = twitchUser.id;
            this.TwitchUsername = twitchUser.login;
            this.TwitchDisplayName = (!string.IsNullOrEmpty(twitchUser.display_name)) ? twitchUser.display_name : this.TwitchUsername;
            this.TwitchAvatarLink = twitchUser.profile_image_url;

            this.SetTwitchRoles();
        }

        public UserViewModel(ChatMessagePacketModel twitchMessage)
        {
            this.SetUserData(twitchID: twitchMessage.UserID);

            this.TwitchID = twitchMessage.UserID;
            this.TwitchUsername = twitchMessage.UserLogin;
            this.TwitchDisplayName = (!string.IsNullOrEmpty(twitchMessage.UserDisplayName)) ? twitchMessage.UserDisplayName : this.TwitchUsername;

            this.SetTwitchRoles();
        }

        public UserViewModel(PubSubWhisperEventModel whisper)
        {
            this.SetUserData(twitchID: whisper.from_id.ToString());

            this.TwitchID = whisper.from_id.ToString();
            this.TwitchUsername = whisper.tags.login;
            this.TwitchDisplayName = (!string.IsNullOrEmpty(whisper.tags.display_name)) ? whisper.tags.display_name : this.TwitchUsername;

            this.SetTwitchRoles();
        }

        public UserViewModel(PubSubWhisperEventRecipientModel whisperRecipient)
        {
            this.SetUserData(twitchID: whisperRecipient.id.ToString());

            this.TwitchID = whisperRecipient.id.ToString();
            this.TwitchUsername = whisperRecipient.username;
            this.TwitchDisplayName = (!string.IsNullOrEmpty(whisperRecipient.display_name)) ? whisperRecipient.display_name : this.TwitchUsername;
            this.TwitchAvatarLink = whisperRecipient.profile_image;

            this.SetTwitchRoles();
        }

        public UserViewModel(ChatRawPacketModel packet)
        {
            this.SetUserData(twitchID: packet.Tags["user-id"]);

            this.TwitchID = packet.Tags["user-id"];
            this.TwitchUsername = packet.Tags["login"];
            this.TwitchDisplayName = (packet.Tags.ContainsKey("display-name") && !string.IsNullOrEmpty(packet.Tags["display-name"])) ? packet.Tags["display-name"] : this.TwitchUsername;

            this.SetTwitchRoles();
        }

        public UserViewModel(PubSubBitsEventV2Model packet)
        {
            this.SetUserData(twitchID: packet.user_id);

            this.TwitchID = packet.user_id;
            this.TwitchDisplayName = this.TwitchUsername = packet.user_name;

            this.SetTwitchRoles();
        }

        public UserViewModel(PubSubSubscriptionsEventModel packet)
        {
            this.SetUserData(twitchID: packet.user_id);

            this.TwitchID = packet.user_id;
            this.TwitchUsername = packet.user_name;
            this.TwitchDisplayName = (!string.IsNullOrEmpty(packet.display_name)) ? packet.display_name : this.TwitchUsername;

            this.SetTwitchRoles();
        }

        public UserViewModel(UserFollowModel follow)
        {
            this.SetUserData(twitchID: follow.from_id);

            this.TwitchID = follow.from_id;
            this.TwitchDisplayName = this.TwitchUsername = follow.from_name;

            this.SetTwitchRoles();
        }

        public UserViewModel(UserDataModel userData)
        {
            this.Data = userData;
        }

        private void SetUserData(string twitchID = null)
        {
            if (!string.IsNullOrEmpty(twitchID))
            {
                this.Data = ChannelSession.Settings.GetUserDataByTwitchID(twitchID);
                if (this.Data == null)
                {
                    this.Data = new UserDataModel() { TwitchID = twitchID };
                    ChannelSession.Settings.AddUserData(this.Data);
                }
            }
            else
            {
                this.Data = new UserDataModel();
            }
        }

        public Guid ID { get { return this.Data.ID; } }

        public StreamingPlatformTypeEnum Platform
        {
            get
            {
                if (!string.IsNullOrEmpty(this.TwitchID)) { return StreamingPlatformTypeEnum.Twitch; }
                return StreamingPlatformTypeEnum.None;
            }
        }

        public string PlatformID
        {
            get
            {
                if (this.Platform == StreamingPlatformTypeEnum.Twitch) { return this.TwitchID; }
                return null;
            }
        }

        public string Username
        {
            get
            {
                if (this.Platform == StreamingPlatformTypeEnum.Twitch) { return this.TwitchVisualName; }
                return this.UnassociatedUsername;
            }
        }

        public HashSet<UserRoleEnum> UserRoles
        {
            get
            {
                if (this.Platform == StreamingPlatformTypeEnum.Twitch) { return this.Data.TwitchUserRoles; }
                return new HashSet<UserRoleEnum>() { UserRoleEnum.User };
            }
        }

        public string AvatarLink
        {
            get
            {
                if (this.Platform == StreamingPlatformTypeEnum.Twitch) { return this.TwitchAvatarLink; }
                return string.Empty;
            }
        }

        public string ChannelLink
        {
            get
            {
                if (this.Platform == StreamingPlatformTypeEnum.Twitch) { return $"https://www.twitch.tv/{this.Username}"; }
                return string.Empty;
            }
        }

        public DateTimeOffset? AccountDate
        {
            get
            {
                if (this.Platform == StreamingPlatformTypeEnum.Twitch) { return this.Data.TwitchAccountDate; }
                return null;
            }
            set
            {
                if (this.Platform == StreamingPlatformTypeEnum.Twitch) { this.Data.TwitchAccountDate = value; }
            }
        }

        public DateTimeOffset? FollowDate
        {
            get
            {
                if (this.Platform == StreamingPlatformTypeEnum.Twitch) { return this.Data.TwitchFollowDate; }
                return null;
            }
            set
            {
                if (this.Platform == StreamingPlatformTypeEnum.Twitch) { this.Data.TwitchFollowDate = value; }

                if (this.FollowDate == null || this.FollowDate.GetValueOrDefault() == DateTimeOffset.MinValue)
                {
                    this.UserRoles.Remove(UserRoleEnum.Follower);
                }
                else
                {
                    this.UserRoles.Add(UserRoleEnum.Follower);
                }
            }
        }

        public DateTimeOffset? SubscribeDate
        {
            get
            {
                if (this.Platform == StreamingPlatformTypeEnum.Twitch) { return this.Data.TwitchSubscribeDate; }
                return null;
            }
            set
            {
                if (this.Platform == StreamingPlatformTypeEnum.Twitch) { this.Data.TwitchSubscribeDate = value; }
            }
        }

        public string PlatformBadgeLink
        {
            get
            {
                if (this.Platform == StreamingPlatformTypeEnum.Twitch) { return "/Assets/Images/Twitch-Small.png"; }
                return null;
            }
        }

        public string SubscriberBadgeLink
        {
            get
            {
                if (this.IsPlatformSubscriber)
                {
                    if (this.Platform == StreamingPlatformTypeEnum.Twitch && this.TwitchSubscriberBadge != null) { return this.TwitchSubscriberBadge.image_url_1x; }
                }
                return null;
            }
        }

        public bool IsAnonymous
        {
            get
            {
                if (this.Platform == StreamingPlatformTypeEnum.Twitch) { return string.IsNullOrEmpty(this.TwitchID); }
                return false;
            }
        }

        public DateTimeOffset LastSeen { get { return this.Data.LastSeen; } set { this.Data.LastSeen = value; } }

        public string UnassociatedUsername { get { return this.Data.UnassociatedUsername; } private set { this.Data.UnassociatedUsername = value; } }

        #region Mixer

        public uint MixerID { get { return this.Data.MixerID; } private set { if (value > 0) { this.Data.MixerID = value; } } }
        public string MixerUsername { get { return this.Data.MixerUsername; } private set { if (!string.IsNullOrEmpty(value)) { this.Data.MixerUsername = value; } } }
        public uint MixerChannelID { get { return this.Data.MixerChannelID; } private set { if (value > 0) { this.Data.MixerChannelID = value; } } }

        #endregion Mixer

        #region Twitch

        public string TwitchID { get { return this.Data.TwitchID; } private set { this.Data.TwitchID = value; } }
        public string TwitchUsername { get { return this.Data.TwitchUsername; } private set { this.Data.TwitchUsername = value; } }
        public string TwitchDisplayName { get { return this.Data.TwitchDisplayName; } private set { this.Data.TwitchDisplayName = value; } }
        public string TwitchAvatarLink { get { return this.Data.TwitchAvatarLink; } private set { this.Data.TwitchAvatarLink = value; } }

        public HashSet<UserRoleEnum> TwitchUserRoles { get { return this.Data.TwitchUserRoles; } private set { this.Data.TwitchUserRoles = value; } }

        public string TwitchVisualName { get { return (!string.IsNullOrEmpty(this.Data.TwitchDisplayName)) ? this.Data.TwitchDisplayName : this.Data.TwitchUsername; } }

        public int TwitchSubMonths
        {
            get
            {
                if (this.Data.TwitchBadgeInfo != null && this.Data.TwitchBadgeInfo.TryGetValue("subscriber", out int months))
                {
                    return months;
                }
                return 0;
            }
        }

        public bool IsTwitchSubscriber { get { return this.HasTwitchSubscriberBadge || this.HasTwitchSubscriberFounderBadge; } }

        public bool HasTwitchSubscriberBadge { get { return this.HasTwitchBadge("subscriber"); } }

        public bool HasTwitchSubscriberFounderBadge { get { return this.HasTwitchBadge("founder"); } }

        public ChatBadgeModel TwitchSubscriberBadge { get; private set; }

        public ChatBadgeModel TwitchBitsBadge { get; private set; }

        #endregion Twitch

        public DateTimeOffset LastUpdated { get { return this.Data.LastUpdated; } set { this.Data.LastUpdated = value; } }

        public DateTimeOffset LastActivity { get { return this.Data.LastActivity; } set { this.Data.LastActivity = value; } }

        public HashSet<string> CustomRoles { get { return this.Data.CustomRoles; } set { this.Data.CustomRoles = value; } }

        public bool IgnoreForQueries { get { return this.Data.IgnoreForQueries; } set { this.Data.IgnoreForQueries = value; } }

        public bool IsInChat { get { return this.Data.IsInChat; } set { this.Data.IsInChat = value; } }

        public string TwitterURL { get { return this.Data.TwitterURL; } set { this.Data.TwitterURL = value; } }

        public PatreonCampaignMember PatreonUser { get { return this.Data.PatreonUser; } set { this.Data.PatreonUser = value; } }

        public UserRoleEnum PrimaryRole { get { return (this.UserRoles.Count() > 0) ? this.UserRoles.Max() : UserRoleEnum.User; } }

        public string PrimaryRoleString { get { return EnumLocalizationHelper.GetLocalizedName(this.PrimaryRole); } }

        public string SortableID
        {
            get
            {
                UserRoleEnum role = this.PrimaryRole;
                if (role < UserRoleEnum.Subscriber)
                {
                    role = UserRoleEnum.User;
                }
                return (999 - role) + "-" + this.Username + "-" + this.Platform.ToString();
            }
        }

        public string Title
        {
            get
            {
                if (!string.IsNullOrEmpty(this.Data.CustomTitle))
                {
                    return this.Data.CustomTitle;
                }

                UserTitleModel title = ChannelSession.Settings.UserTitles.OrderByDescending(t => t.Role).ThenByDescending(t => t.Months).FirstOrDefault(t => t.MeetsTitle(this));
                if (title != null)
                {
                    return title.Name;
                }

                return "No Title";
            }
            set
            {
                this.Data.CustomTitle = value;
            }
        }

        public string RolesDisplayString
        {
            get
            {
                lock (this.rolesDisplayStringLock)
                {
                    if (this.Data.RolesDisplayString == null)
                    {
                        List<UserRoleEnum> userRoles = this.UserRoles.ToList();
                        if (this.Data.MixerUserRoles.Contains(UserRoleEnum.Banned))
                        {
                            userRoles.Clear();
                            userRoles.Add(UserRoleEnum.Banned);
                        }
                        else
                        {
                            if (this.Data.MixerUserRoles.Count() > 1)
                            {
                                userRoles.Remove(UserRoleEnum.User);
                            }

                            if (userRoles.Contains(UserRoleEnum.ChannelEditor))
                            {
                                userRoles.Remove(UserRoleEnum.Mod);
                            }

                            if (this.Data.MixerUserRoles.Contains(UserRoleEnum.Subscriber) || this.Data.MixerUserRoles.Contains(UserRoleEnum.Streamer))
                            {
                                userRoles.Remove(UserRoleEnum.Follower);
                            }

                            if (this.Data.MixerUserRoles.Contains(UserRoleEnum.Streamer))
                            {
                                userRoles.Remove(UserRoleEnum.ChannelEditor);
                                userRoles.Remove(UserRoleEnum.Subscriber);
                            }
                        }

                        List<string> displayRoles = userRoles.Select(r => EnumLocalizationHelper.GetLocalizedName(r)).ToList();
                        displayRoles.AddRange(this.CustomRoles);

                        this.Data.RolesDisplayString = string.Join(", ", userRoles.OrderByDescending(r => r));
                    }
                    return this.Data.RolesDisplayString;
                }
            }
            private set
            {
                lock (this.rolesDisplayStringLock)
                {
                    this.Data.RolesDisplayString = value;
                }
            }
        }
        private object rolesDisplayStringLock = new object();

        public bool IsFollower { get { return this.UserRoles.Contains(UserRoleEnum.Follower) || this.HasPermissionsTo(UserRoleEnum.Subscriber); } }
        public bool IsRegular { get { return this.UserRoles.Contains(UserRoleEnum.Regular); } }
        public bool IsPlatformSubscriber { get { return this.UserRoles.Contains(UserRoleEnum.Subscriber); } }
        public bool ShowSubscriberBadge { get { return this.IsPlatformSubscriber && !string.IsNullOrEmpty(this.SubscriberBadgeLink); } }

        public string AccountAgeString { get { return (this.AccountDate != null) ? this.AccountDate.GetValueOrDefault().GetAge() : "Unknown"; } }
        public string FollowAgeString { get { return (this.FollowDate != null) ? this.FollowDate.GetValueOrDefault().GetAge() : "Not Following"; } }
        public string SubscribeAgeString { get { return (this.SubscribeDate != null) ? this.SubscribeDate.GetValueOrDefault().GetAge() : "Not Subscribed"; } }
        public int SubscribeMonths
        {
            get
            {
                if (this.SubscribeDate != null)
                {
                    return this.SubscribeDate.GetValueOrDefault().TotalMonthsFromNow();
                }
                return 0;
            }
        }
        public string LastSeenString { get { return (this.LastSeen != DateTimeOffset.MinValue) ? this.LastSeen.ToFriendlyDateTimeString() : "Unknown"; } }

        public int WhispererNumber { get { return this.Data.WhispererNumber; } set { this.Data.WhispererNumber = value; } }
        public bool HasWhisperNumber { get { return this.WhispererNumber > 0; } }

        public string PrimaryRoleColorName
        {
            get
            {
                switch (this.PrimaryRole)
                {
                    case UserRoleEnum.Streamer:
                        return "UserStreamerRoleColor";
                    case UserRoleEnum.Staff:
                        return "UserStaffRoleColor";
                    case UserRoleEnum.ChannelEditor:
                    case UserRoleEnum.Mod:
                        return "UserModRoleColor";
                    case UserRoleEnum.GlobalMod:
                        return "UserGlobalModRoleColor";
                }

                if (this.UserRoles.Contains(UserRoleEnum.Pro))
                {
                    return "UserProRoleColor";
                }
                else
                {
                    return "UserDefaultRoleColor";
                }
            }
        }

        public PatreonTier PatreonTier
        {
            get
            {
                if (ChannelSession.Services.Patreon.IsConnected && this.PatreonUser != null)
                {
                    return ChannelSession.Services.Patreon.Campaign.GetTier(this.PatreonUser.TierID);
                }
                return null;
            }
        }

        public bool IsSubscriber { get { return this.UserRoles.Contains(UserRoleEnum.Subscriber) || this.IsEquivalentToSubscriber(); } }

        public bool HasPermissionsTo(UserRoleEnum checkRole)
        {
            Logger.Log($"Checking role permission for user: {this.PrimaryRole} - {checkRole}");

            if (checkRole == UserRoleEnum.Subscriber && this.IsEquivalentToSubscriber())
            {
                return true;
            }
            return this.PrimaryRole >= checkRole;
        }

        public bool ExceedsPermissions(UserRoleEnum checkRole) { return this.PrimaryRole > checkRole; }

        public bool IsEquivalentToSubscriber()
        {
            if (this.PatreonUser != null && ChannelSession.Services.Patreon.IsConnected && !string.IsNullOrEmpty(ChannelSession.Settings.PatreonTierMixerSubscriberEquivalent))
            {
                PatreonTier userTier = this.PatreonTier;
                PatreonTier equivalentTier = ChannelSession.Services.Patreon.Campaign.GetTier(ChannelSession.Settings.PatreonTierMixerSubscriberEquivalent);
                if (userTier != null && equivalentTier != null && userTier.Amount >= equivalentTier.Amount)
                {
                    return true;
                }
            }
            return false;
        }

        public void UpdateLastActivity() { this.LastActivity = DateTimeOffset.Now; }

        public async Task RefreshDetails(bool force = false)
        {
            if (!this.IsAnonymous)
            {
                if (!this.Data.UpdatedThisSession || force)
                {
                    DateTimeOffset refreshStart = DateTimeOffset.Now;

                    this.Data.UpdatedThisSession = true;
                    this.LastUpdated = DateTimeOffset.Now;

                    if (this.Platform.HasFlag(StreamingPlatformTypeEnum.Twitch))
                    {
                        await this.RefreshTwitchUserDetails();
                        await this.RefreshTwitchUserAccountDate();
                        await this.RefreshTwitchUserFollowDate();
                        await this.RefreshTwitchUserSubscribeDate();
                    }

                    this.SetCommonUserRoles();

                    await this.RefreshExternalServiceDetails();

                    double refreshTime = (DateTimeOffset.Now - refreshStart).TotalMilliseconds;
                    Logger.Log($"User refresh time: {refreshTime} ms");
                    if (refreshTime > 500)
                    {
                        Logger.Log(LogLevel.Error, string.Format("Long user rfresh time detected for the following user: {0} - {1} - {2} ms", this.ID, this.Username, refreshTime));
                    }
                }
            }
        }

        #region Twitch Data Setter Functions

        public void SetTwitchChatDetails(ChatMessagePacketModel message)
        {
            this.SetTwitchChatDetails(message.UserDisplayName, message.BadgeDictionary, message.BadgeInfoDictionary, message.Color);
        }

        public void SetTwitchChatDetails(ChatUserStatePacketModel userState)
        {
            this.SetTwitchChatDetails(userState.UserDisplayName, userState.BadgeDictionary, userState.BadgeInfoDictionary, userState.Color);
        }

        public void SetTwitchChatDetails(ChatUserNoticePacketModel userNotice)
        {
            this.SetTwitchChatDetails(userNotice.UserDisplayName, userNotice.BadgeDictionary, userNotice.BadgeInfoDictionary, userNotice.Color);
        }

        private void SetTwitchChatDetails(string displayName, Dictionary<string, int> badges, Dictionary<string, int> badgeInfo, string color)
        {
            this.TwitchDisplayName = displayName;
            this.Data.TwitchBadges = badges;
            this.Data.TwitchBadgeInfo = badgeInfo;
            this.Data.TwitchColor = color;

            if (this.Data.TwitchBadges != null)
            {
                if (this.HasTwitchBadge("admin") || this.HasTwitchBadge("staff")) { this.UserRoles.Add(UserRoleEnum.Staff); } else { this.UserRoles.Remove(UserRoleEnum.Staff); }
                if (this.HasTwitchBadge("global_mod")) { this.UserRoles.Add(UserRoleEnum.GlobalMod); } else { this.UserRoles.Remove(UserRoleEnum.GlobalMod); }
                if (this.HasTwitchBadge("moderator")) { this.TwitchUserRoles.Add(UserRoleEnum.Mod); } else { this.TwitchUserRoles.Remove(UserRoleEnum.Mod); }
                if (this.IsTwitchSubscriber || this.HasTwitchSubscriberFounderBadge) { this.TwitchUserRoles.Add(UserRoleEnum.Subscriber); } else { this.TwitchUserRoles.Remove(UserRoleEnum.Subscriber); }
                if (this.HasTwitchBadge("turbo") || this.HasTwitchBadge("premium")) { this.UserRoles.Add(UserRoleEnum.Pro); } else { this.UserRoles.Remove(UserRoleEnum.Pro); }

                if (ChannelSession.Services.Chat.TwitchChatService != null)
                {
                    string name = null;
                    if (this.HasTwitchSubscriberBadge)
                    {
                        name = "subscriber";
                    }
                    else if (this.HasTwitchSubscriberFounderBadge)
                    {
                        name = "founder";
                    }

                    if (!string.IsNullOrEmpty(name))
                    {
                        int versionID = this.GetTwitchBadgeVersion(name);
                        if (ChannelSession.Services.Chat.TwitchChatService.ChatBadges.ContainsKey(name) && ChannelSession.Services.Chat.TwitchChatService.ChatBadges[name].versions.ContainsKey(versionID.ToString()))
                        {
                            this.TwitchSubscriberBadge = ChannelSession.Services.Chat.TwitchChatService.ChatBadges[name].versions[versionID.ToString()];
                        }
                    }

                    if (this.HasTwitchBadge("bits"))
                    {
                        int versionID = this.GetTwitchBadgeVersion("bits");
                        if (ChannelSession.Services.Chat.TwitchChatService.ChatBadges.ContainsKey("bits") && ChannelSession.Services.Chat.TwitchChatService.ChatBadges["bits"].versions.ContainsKey(versionID.ToString()))
                        {
                            this.TwitchBitsBadge = ChannelSession.Services.Chat.TwitchChatService.ChatBadges["bits"].versions[versionID.ToString()];
                        }
                    }
                }
            }

            this.SetCommonUserRoles();

            this.RolesDisplayString = null;
        }

        private int GetTwitchBadgeVersion(string name)
        {
            if (this.Data.TwitchBadges != null && this.Data.TwitchBadges.TryGetValue(name, out int version))
            {
                return version;
            }
            return -1;
        }

        private bool HasTwitchBadge(string name) { return this.GetTwitchBadgeVersion(name) >= 0; }

        #endregion Twitch Data Setter Functions

        public async Task AddModerationStrike(string moderationReason = null)
        {
            Dictionary<string, string> extraSpecialIdentifiers = new Dictionary<string, string>();
            extraSpecialIdentifiers.Add(ModerationService.ModerationReasonSpecialIdentifier, moderationReason);

            this.Data.ModerationStrikes++;
            if (this.Data.ModerationStrikes == 1)
            {
                if (ChannelSession.Settings.ModerationStrike1Command != null)
                {
                    await ChannelSession.Settings.ModerationStrike1Command.Perform(this, extraSpecialIdentifiers: extraSpecialIdentifiers);
                }
            }
            else if (this.Data.ModerationStrikes == 2)
            {
                if (ChannelSession.Settings.ModerationStrike2Command != null)
                {
                    await ChannelSession.Settings.ModerationStrike2Command.Perform(this, extraSpecialIdentifiers: extraSpecialIdentifiers);
                }
            }
            else if (this.Data.ModerationStrikes >= 3)
            {
                if (ChannelSession.Settings.ModerationStrike3Command != null)
                {
                    await ChannelSession.Settings.ModerationStrike3Command.Perform(this, extraSpecialIdentifiers: extraSpecialIdentifiers);
                }
            }
        }

        public Task RemoveModerationStrike()
        {
            if (this.Data.ModerationStrikes > 0)
            {
                this.Data.ModerationStrikes--;
            }
            return Task.FromResult(0);
        }

        public void UpdateMinuteData()
        {
            if (ChannelSession.TwitchStreamIsLive)
            {
                this.Data.ViewingMinutes++;
            }
            else
            {
                this.Data.OfflineViewingMinutes++;
            }
            ChannelSession.Settings.UserData.ManualValueChanged(this.ID);

            if (ChannelSession.Settings.RegularUserMinimumHours > 0 && this.Data.ViewingHoursPart >= ChannelSession.Settings.RegularUserMinimumHours)
            {
                this.UserRoles.Add(UserRoleEnum.Regular);
            }
            else
            {
                this.UserRoles.Remove(UserRoleEnum.Regular);
            }
        }

        public TwitchV5API.Users.UserModel GetTwitchV5APIUserModel()
        {
            return new TwitchV5API.Users.UserModel()
            {
                id = this.TwitchID,
                name = this.TwitchUsername,
                display_name = this.TwitchDisplayName,
            };
        }

        public TwitchNewAPI.Users.UserModel GetTwitchNewAPIUserModel()
        {
            return new TwitchNewAPI.Users.UserModel()
            {
                id = this.TwitchID,
                login = this.TwitchUsername,
                display_name = this.TwitchDisplayName,
            };
        }

        public override bool Equals(object obj)
        {
            if (obj is UserViewModel)
            {
                return this.Equals((UserViewModel)obj);
            }
            return false;
        }

        public bool Equals(UserViewModel other) { return this.ID.Equals(other.ID); }

        public int CompareTo(UserViewModel other) { return this.Username.CompareTo(other.Username); }

        public override int GetHashCode() { return this.ID.GetHashCode(); }

        public override string ToString() { return this.Username; }

        #region Twitch Refresh

        private async Task RefreshTwitchUserDetails()
        {
            TwitchNewAPI.Users.UserModel twitchUser = (!string.IsNullOrEmpty(this.TwitchID)) ? await ChannelSession.TwitchUserConnection.GetNewAPIUserByID(this.TwitchID)
                : await ChannelSession.TwitchUserConnection.GetNewAPIUserByLogin(this.TwitchUsername);
            if (twitchUser != null)
            {
                this.TwitchID = twitchUser.id;
                this.TwitchUsername = twitchUser.login;
                this.TwitchDisplayName = (!string.IsNullOrEmpty(twitchUser.display_name)) ? twitchUser.display_name : this.TwitchDisplayName;
                this.TwitchAvatarLink = twitchUser.profile_image_url;

                if (twitchUser.IsPartner()) { this.UserRoles.Add(UserRoleEnum.Partner); } else { this.UserRoles.Remove(UserRoleEnum.Partner); }
                if (twitchUser.IsAffiliate()) { this.UserRoles.Add(UserRoleEnum.Affiliate); } else { this.UserRoles.Remove(UserRoleEnum.Affiliate); }
                if (twitchUser.IsStaff()) { this.UserRoles.Add(UserRoleEnum.Staff); } else { this.UserRoles.Remove(UserRoleEnum.Staff); }
                if (twitchUser.IsGlobalMod()) { this.UserRoles.Add(UserRoleEnum.GlobalMod); } else { this.UserRoles.Remove(UserRoleEnum.GlobalMod); }

                this.SetTwitchRoles();

                this.RolesDisplayString = null;
            }
        }

        private void SetTwitchRoles()
        {
            this.TwitchUserRoles.Add(UserRoleEnum.User);
            if (ChannelSession.TwitchChannelNewAPI != null && ChannelSession.TwitchChannelNewAPI.id.Equals(this.TwitchID))
            {
                this.TwitchUserRoles.Add(UserRoleEnum.Streamer);
            }
            this.IsInChat = true;
        }

        private async Task RefreshTwitchUserAccountDate()
        {
            TwitchV5API.Users.UserModel twitchV5User = await ChannelSession.TwitchUserConnection.GetV5APIUserByLogin(this.TwitchUsername);
            if (twitchV5User != null && !string.IsNullOrEmpty(twitchV5User.created_at) && DateTimeOffset.TryParse(twitchV5User.created_at, out DateTimeOffset createdDate))
            {
                this.AccountDate = createdDate;
            }
        }

        private async Task RefreshTwitchUserFollowDate()
        {
            UserFollowModel follow = await ChannelSession.TwitchUserConnection.CheckIfFollowsNewAPI(ChannelSession.TwitchChannelNewAPI, this.GetTwitchNewAPIUserModel());
            if (follow != null && !string.IsNullOrEmpty(follow.followed_at) && DateTimeOffset.TryParse(follow.followed_at, out DateTimeOffset followDate))
            {
                this.FollowDate = followDate;
            }
            else
            {
                this.FollowDate = null;
            }
        }

        private async Task RefreshTwitchUserSubscribeDate()
        {
            if (ChannelSession.TwitchUserNewAPI.IsAffiliate() || ChannelSession.TwitchUserNewAPI.IsPartner())
            {
                TwitchV5API.Users.UserSubscriptionModel subscription = await ChannelSession.TwitchUserConnection.CheckIfSubscribedV5(ChannelSession.TwitchChannelV5, this.GetTwitchV5APIUserModel());
                if (subscription != null && !string.IsNullOrEmpty(subscription.created_at) && DateTimeOffset.TryParse(subscription.created_at, out DateTimeOffset subDate))
                {
                    this.SubscribeDate = subDate;
                }
                else
                {
                    this.SubscribeDate = null;
                }
            }
        }

        #endregion Twitch Refresh

        private void SetCommonUserRoles()
        {
            if (this.UserRoles.Contains(UserRoleEnum.Streamer))
            {
                this.UserRoles.Add(UserRoleEnum.ChannelEditor);
                this.UserRoles.Add(UserRoleEnum.Mod);
                this.UserRoles.Add(UserRoleEnum.Subscriber);
                this.UserRoles.Add(UserRoleEnum.Follower);
            }

            if (this.UserRoles.Contains(UserRoleEnum.ChannelEditor))
            {
                this.UserRoles.Add(UserRoleEnum.Mod);
            }

            if (ChannelSession.Settings.RegularUserMinimumHours > 0 && this.Data.ViewingHoursPart >= ChannelSession.Settings.RegularUserMinimumHours)
            {
                this.UserRoles.Add(UserRoleEnum.Regular);
            }
            else
            {
                this.UserRoles.Remove(UserRoleEnum.Regular);
            }

            // Force re-build of roles display string
            this.RolesDisplayString = null;
        }

        private Task RefreshExternalServiceDetails()
        {
            this.CustomRoles.Clear();
            if (ChannelSession.Services.Patreon.IsConnected && this.PatreonUser == null)
            {
                IEnumerable<PatreonCampaignMember> campaignMembers = ChannelSession.Services.Patreon.CampaignMembers;

                PatreonCampaignMember patreonUser = null;
                if (!string.IsNullOrEmpty(this.Data.PatreonUserID))
                {
                    patreonUser = campaignMembers.FirstOrDefault(u => u.UserID.Equals(this.Data.PatreonUserID));
                }
                else
                {
                    patreonUser = campaignMembers.FirstOrDefault(u => u.User.LookupName.Equals(this.Username, StringComparison.CurrentCultureIgnoreCase));
                }

                this.PatreonUser = patreonUser;
                if (patreonUser != null)
                {
                    this.Data.PatreonUserID = patreonUser.UserID;
                }
                else
                {
                    this.Data.PatreonUserID = null;
                }
            }
            return Task.FromResult(0);
        }
    }
}
